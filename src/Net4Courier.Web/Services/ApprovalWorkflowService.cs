using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class ApprovalWorkflowService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public ApprovalWorkflowService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ApprovalWorkflow>> GetWorkflowsAsync(long companyId, bool includeInactive = false)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.ApprovalWorkflows
            .Include(w => w.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .Where(w => w.CompanyId == companyId && !w.IsDeleted);

        if (!includeInactive)
            query = query.Where(w => w.IsActive);

        return await query.OrderBy(w => w.WorkflowType).ThenBy(w => w.Name).ToListAsync();
    }

    public async Task<ApprovalWorkflow?> GetWorkflowByTypeAsync(long companyId, ApprovalWorkflowType type)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.ApprovalWorkflows
            .Include(w => w.Steps.Where(s => !s.IsDeleted && s.IsActive).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(w => w.CompanyId == companyId && w.WorkflowType == type && w.IsActive && !w.IsDeleted);
    }

    public async Task<ApprovalWorkflow?> GetWorkflowByIdAsync(long workflowId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.ApprovalWorkflows
            .Include(w => w.Steps.Where(s => !s.IsDeleted).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(w => w.Id == workflowId && !w.IsDeleted);
    }

    public async Task<long> CreateWorkflowAsync(ApprovalWorkflow workflow)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.ApprovalWorkflows.Add(workflow);
        await context.SaveChangesAsync();
        return workflow.Id;
    }

    public async Task UpdateWorkflowAsync(ApprovalWorkflow workflow)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.ApprovalWorkflows.Update(workflow);
        await context.SaveChangesAsync();
    }

    public async Task DeleteWorkflowAsync(long workflowId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var workflow = await context.ApprovalWorkflows.FindAsync(workflowId);
        if (workflow != null)
        {
            workflow.IsDeleted = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<long> AddStepAsync(ApprovalStep step)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.ApprovalSteps.Add(step);
        await context.SaveChangesAsync();
        return step.Id;
    }

    public async Task UpdateStepAsync(ApprovalStep step)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.ApprovalSteps.Update(step);
        await context.SaveChangesAsync();
    }

    public async Task DeleteStepAsync(long stepId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var step = await context.ApprovalSteps.FindAsync(stepId);
        if (step != null)
        {
            step.IsDeleted = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasActiveWorkflowAsync(long companyId, ApprovalWorkflowType type)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.ApprovalWorkflows
            .AnyAsync(w => w.CompanyId == companyId && w.WorkflowType == type && w.IsActive && !w.IsDeleted);
    }

    public async Task<ApprovalRequest> SubmitForApprovalAsync(
        long companyId, long? branchId, ApprovalWorkflowType entityType,
        long entityId, string? entityCode, string? entityDescription,
        decimal? amount, string? currency,
        long requesterId, string? requesterName)
    {
        using var context = await _dbFactory.CreateDbContextAsync();

        var existing = await context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.EntityType == entityType && r.EntityId == entityId
                && r.Status != ApprovalStatus.Rejected && r.Status != ApprovalStatus.Cancelled
                && !r.IsDeleted);
        if (existing != null)
            throw new InvalidOperationException("An active approval request already exists for this document.");

        var workflow = await context.ApprovalWorkflows
            .Include(w => w.Steps.Where(s => !s.IsDeleted && s.IsActive).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(w => w.CompanyId == companyId && w.WorkflowType == entityType && w.IsActive && !w.IsDeleted);

        if (workflow == null || !workflow.Steps.Any())
            throw new InvalidOperationException($"No active approval workflow configured for {entityType}.");

        var firstStep = workflow.Steps.First();
        var totalSteps = workflow.Steps.Count;

        long? currentApproverId = firstStep.ApproverUserId;
        string? currentApproverName = firstStep.ApproverUserName;

        if (currentApproverId == null && firstStep.ApproverRoleId.HasValue)
        {
            var roleUser = await context.Users
                .Where(u => u.RoleId == firstStep.ApproverRoleId.Value && u.IsActive && !u.IsDeleted)
                .FirstOrDefaultAsync();
            if (roleUser != null)
            {
                currentApproverId = roleUser.Id;
                currentApproverName = roleUser.FullName;
            }
        }

        var request = new ApprovalRequest
        {
            CompanyId = companyId,
            BranchId = branchId,
            WorkflowId = workflow.Id,
            EntityType = entityType,
            EntityId = entityId,
            EntityCode = entityCode,
            EntityDescription = entityDescription,
            Amount = amount,
            Currency = currency,
            RequesterId = requesterId,
            RequesterName = requesterName,
            CurrentStepNumber = 1,
            TotalSteps = totalSteps,
            CurrentApproverId = currentApproverId,
            CurrentApproverName = currentApproverName,
            Status = ApprovalStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        context.ApprovalRequests.Add(request);

        context.ApprovalActions.Add(new ApprovalAction
        {
            RequestId = 0,
            StepNumber = 0,
            StepName = "Submission",
            ActionType = ApprovalActionType.Submitted,
            ActionById = requesterId,
            ActionByName = requesterName,
            ActionAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var action = context.ApprovalActions.Local.First();
        action.RequestId = request.Id;
        await context.SaveChangesAsync();

        return request;
    }

    public async Task<ApprovalRequest> ApproveAsync(long requestId, long userId, string? userName, string? comments)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var request = await context.ApprovalRequests
            .Include(r => r.Workflow)
                .ThenInclude(w => w.Steps.Where(s => !s.IsDeleted && s.IsActive).OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null)
            throw new InvalidOperationException("Approval request not found.");

        if (request.Status != ApprovalStatus.Pending && request.Status != ApprovalStatus.InProgress)
            throw new InvalidOperationException("This request is not pending approval.");

        var currentStep = request.Workflow.Steps
            .OrderBy(s => s.StepOrder)
            .Skip(request.CurrentStepNumber - 1)
            .FirstOrDefault();

        context.ApprovalActions.Add(new ApprovalAction
        {
            RequestId = requestId,
            StepNumber = request.CurrentStepNumber,
            StepName = currentStep?.StepName ?? $"Step {request.CurrentStepNumber}",
            ActionType = ApprovalActionType.Approved,
            ActionById = userId,
            ActionByName = userName,
            Comments = comments,
            ActionAt = DateTime.UtcNow
        });

        if (request.CurrentStepNumber >= request.TotalSteps)
        {
            request.Status = ApprovalStatus.Approved;
            request.CompletedAt = DateTime.UtcNow;
            request.CurrentApproverName = null;
            request.CurrentApproverId = null;
        }
        else
        {
            request.CurrentStepNumber++;
            request.Status = ApprovalStatus.InProgress;

            var nextStep = request.Workflow.Steps
                .OrderBy(s => s.StepOrder)
                .Skip(request.CurrentStepNumber - 1)
                .FirstOrDefault();

            if (nextStep != null)
            {
                request.CurrentApproverId = nextStep.ApproverUserId;
                request.CurrentApproverName = nextStep.ApproverUserName;

                if (request.CurrentApproverId == null && nextStep.ApproverRoleId.HasValue)
                {
                    var roleUser = await context.Users
                        .Where(u => u.RoleId == nextStep.ApproverRoleId.Value && u.IsActive && !u.IsDeleted)
                        .FirstOrDefaultAsync();
                    if (roleUser != null)
                    {
                        request.CurrentApproverId = roleUser.Id;
                        request.CurrentApproverName = roleUser.FullName;
                    }
                }
            }
        }

        request.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ApprovalRequest> RejectAsync(long requestId, long userId, string? userName, string? comments)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var request = await context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null)
            throw new InvalidOperationException("Approval request not found.");

        if (request.Status != ApprovalStatus.Pending && request.Status != ApprovalStatus.InProgress)
            throw new InvalidOperationException("This request is not pending approval.");

        context.ApprovalActions.Add(new ApprovalAction
        {
            RequestId = requestId,
            StepNumber = request.CurrentStepNumber,
            StepName = $"Step {request.CurrentStepNumber}",
            ActionType = ApprovalActionType.Rejected,
            ActionById = userId,
            ActionByName = userName,
            Comments = comments,
            ActionAt = DateTime.UtcNow
        });

        request.Status = ApprovalStatus.Rejected;
        request.CompletedAt = DateTime.UtcNow;
        request.CurrentApproverName = null;
        request.CurrentApproverId = null;
        request.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ApprovalRequest> ReturnAsync(long requestId, long userId, string? userName, string? comments)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var request = await context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null)
            throw new InvalidOperationException("Approval request not found.");

        context.ApprovalActions.Add(new ApprovalAction
        {
            RequestId = requestId,
            StepNumber = request.CurrentStepNumber,
            StepName = $"Step {request.CurrentStepNumber}",
            ActionType = ApprovalActionType.Returned,
            ActionById = userId,
            ActionByName = userName,
            Comments = comments,
            ActionAt = DateTime.UtcNow
        });

        request.Status = ApprovalStatus.Returned;
        request.CompletedAt = DateTime.UtcNow;
        request.CurrentApproverName = null;
        request.CurrentApproverId = null;
        request.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return request;
    }

    public async Task<List<ApprovalRequest>> GetPendingApprovalsAsync(long companyId, long userId, long? roleId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.ApprovalRequests
            .Where(r => r.CompanyId == companyId && !r.IsDeleted
                && (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        if (roleId.HasValue)
        {
            var workflowStepRoleIds = await context.ApprovalSteps
                .Where(s => s.ApproverRoleId == roleId.Value && s.IsActive && !s.IsDeleted)
                .Select(s => s.WorkflowId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => r.CurrentApproverId == userId
                || (r.CurrentApproverId == null && workflowStepRoleIds.Contains(r.WorkflowId)));
        }
        else
        {
            query = query.Where(r => r.CurrentApproverId == userId);
        }

        return await query.OrderByDescending(r => r.SubmittedAt).ToListAsync();
    }

    public async Task<List<ApprovalRequest>> GetMyRequestsAsync(long companyId, long userId, string? statusFilter = null)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.ApprovalRequests
            .Where(r => r.CompanyId == companyId && r.RequesterId == userId && !r.IsDeleted);

        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All" && Enum.TryParse<ApprovalStatus>(statusFilter, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query.OrderByDescending(r => r.SubmittedAt).ToListAsync();
    }

    public async Task<List<ApprovalAction>> GetActionsAsync(long requestId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.ApprovalActions
            .Where(a => a.RequestId == requestId)
            .OrderBy(a => a.ActionAt)
            .ToListAsync();
    }

    public async Task<int> GetPendingCountAsync(long companyId, long userId, long? roleId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.ApprovalRequests
            .Where(r => r.CompanyId == companyId && !r.IsDeleted
                && (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        if (roleId.HasValue)
        {
            var workflowStepRoleIds = await context.ApprovalSteps
                .Where(s => s.ApproverRoleId == roleId.Value && s.IsActive && !s.IsDeleted)
                .Select(s => s.WorkflowId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => r.CurrentApproverId == userId
                || (r.CurrentApproverId == null && workflowStepRoleIds.Contains(r.WorkflowId)));
        }
        else
        {
            query = query.Where(r => r.CurrentApproverId == userId);
        }

        return await query.CountAsync();
    }

    public string GetDocumentUrl(ApprovalWorkflowType entityType, long entityId)
    {
        return entityType switch
        {
            ApprovalWorkflowType.CustomerInvoice => $"/invoice-view/{entityId}",
            ApprovalWorkflowType.VendorBill => $"/vendor-bills",
            ApprovalWorkflowType.CourierExpense => $"/expense-approval",
            ApprovalWorkflowType.CreditNote => $"/credit-notes",
            ApprovalWorkflowType.DebitNote => $"/debit-notes",
            ApprovalWorkflowType.CustomerCredit => $"/customer-credit-approval",
            ApprovalWorkflowType.Receipt => $"/receipt-entry/{entityId}",
            ApprovalWorkflowType.VendorPayment => $"/vendor-payment-entry/{entityId}",
            ApprovalWorkflowType.JournalEntry => $"/gl/journals",
            ApprovalWorkflowType.CashVoucher => $"/cashbank/voucher",
            ApprovalWorkflowType.BankVoucher => $"/cashbank/voucher",
            _ => "#"
        };
    }
}
