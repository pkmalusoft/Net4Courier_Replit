using Net4Courier.Kernel.Enums;

namespace Net4Courier.Kernel.Entities;

public class ApprovalWorkflow : BaseEntity
{
    public long CompanyId { get; set; }
    public ApprovalWorkflowType WorkflowType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequireAllApprovers { get; set; }

    public virtual ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
}

public class ApprovalStep : BaseEntity
{
    public long WorkflowId { get; set; }
    public long CompanyId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public long? ApproverUserId { get; set; }
    public string? ApproverUserName { get; set; }
    public long? ApproverRoleId { get; set; }
    public string? ApproverRoleName { get; set; }
    public bool CanDelegate { get; set; }
    public int? TimeoutHours { get; set; }

    public virtual ApprovalWorkflow Workflow { get; set; } = null!;
}

public class ApprovalRequest : AuditableEntity
{
    public long CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long WorkflowId { get; set; }
    public ApprovalWorkflowType EntityType { get; set; }
    public long EntityId { get; set; }
    public string? EntityCode { get; set; }
    public string? EntityDescription { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public long RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public int CurrentStepNumber { get; set; }
    public int TotalSteps { get; set; }
    public long? CurrentApproverId { get; set; }
    public string? CurrentApproverName { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public virtual ApprovalWorkflow Workflow { get; set; } = null!;
    public virtual ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();
}

public class ApprovalAction : BaseEntity
{
    public long RequestId { get; set; }
    public int StepNumber { get; set; }
    public string? StepName { get; set; }
    public ApprovalActionType ActionType { get; set; }
    public long ActionById { get; set; }
    public string? ActionByName { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;

    public virtual ApprovalRequest Request { get; set; } = null!;
}
