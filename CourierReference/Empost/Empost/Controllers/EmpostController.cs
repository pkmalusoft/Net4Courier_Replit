using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Empost.Models;
using Server.Modules.Empost.Services;

namespace Server.Modules.Empost.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmpostController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostLicenseService _licenseService;
    private readonly IEmpostQuarterService _quarterService;
    private readonly IEmpostPeriodLockService _periodLockService;
    private readonly IEmpostFeeCalculationService _feeCalculationService;
    private readonly IEmpostSettlementService _settlementService;
    private readonly IEmpostReturnAdjustmentService _returnAdjustmentService;
    private readonly IEmpostReportingService _reportingService;
    private readonly IEmpostPdfExportService _pdfExportService;

    public EmpostController(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostLicenseService licenseService,
        IEmpostQuarterService quarterService,
        IEmpostPeriodLockService periodLockService,
        IEmpostFeeCalculationService feeCalculationService,
        IEmpostSettlementService settlementService,
        IEmpostReturnAdjustmentService returnAdjustmentService,
        IEmpostReportingService reportingService,
        IEmpostPdfExportService pdfExportService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _licenseService = licenseService;
        _quarterService = quarterService;
        _periodLockService = periodLockService;
        _feeCalculationService = feeCalculationService;
        _settlementService = settlementService;
        _returnAdjustmentService = returnAdjustmentService;
        _reportingService = reportingService;
        _pdfExportService = pdfExportService;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User context not available");
        }
        return userId;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst("name")?.Value 
            ?? "Unknown";
    }

    #region License Endpoints

    [HttpGet("license")]
    public async Task<ActionResult<EmpostLicense>> GetActiveLicense()
    {
        var license = await _licenseService.GetActiveLicenseAsync();
        if (license == null)
            return NotFound("No active Empost license found");
        return Ok(license);
    }

    [HttpGet("licenses")]
    public async Task<ActionResult<List<EmpostLicense>>> GetAllLicenses()
    {
        var licenses = await _licenseService.GetAllLicensesAsync();
        return Ok(licenses);
    }

    [HttpGet("license/{id}")]
    public async Task<ActionResult<EmpostLicense>> GetLicense(Guid id)
    {
        var license = await _licenseService.GetLicenseAsync(id);
        if (license == null)
            return NotFound();
        return Ok(license);
    }

    [HttpPost("license")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<EmpostLicense>> CreateLicense([FromBody] Services.CreateLicenseRequest request)
    {
        // Input validation with clear error messages
        var validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            validationErrors.Add("License Number is required");
        
        if (request.LicenseDate == default || request.LicenseDate < new DateTime(2000, 1, 1))
            validationErrors.Add("Please enter a valid License Date");
        
        if (request.MinimumAdvanceAmount.HasValue && request.MinimumAdvanceAmount.Value < 0)
            validationErrors.Add("Minimum Advance Amount cannot be negative");
        
        if (request.RoyaltyPercentage.HasValue && (request.RoyaltyPercentage.Value < 0 || request.RoyaltyPercentage.Value > 1))
            validationErrors.Add("Invalid Royalty Percentage value");
        
        if (request.WeightThresholdKg.HasValue && request.WeightThresholdKg.Value < 0)
            validationErrors.Add("Weight Threshold cannot be negative");
        
        if (validationErrors.Any())
            return BadRequest(string.Join("; ", validationErrors));
        
        try
        {
            var userId = GetCurrentUserId();
            var license = await _licenseService.CreateLicenseAsync(request, userId);
            return CreatedAtAction(nameof(GetLicense), new { id = license.Id }, license);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("An error occurred while creating the license. Please try again.");
        }
    }

    [HttpPut("license/{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<EmpostLicense>> UpdateLicense(Guid id, [FromBody] UpdateLicenseRequest request)
    {
        try
        {
            var license = await _licenseService.UpdateLicenseAsync(id, request);
            return Ok(license);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("license/{id}/advance-payment")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<EmpostAdvancePayment>> RecordAdvancePayment(Guid id, [FromBody] RecordAdvancePaymentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var payment = await _licenseService.RecordAdvancePaymentAsync(id, request, userId);
            return Ok(payment);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("license/{id}/advance-payments")]
    public async Task<ActionResult<List<EmpostAdvancePayment>>> GetAdvancePayments(Guid id)
    {
        var payments = await _licenseService.GetAdvancePaymentsAsync(id);
        return Ok(payments);
    }

    [HttpGet("license/{id}/fee-status")]
    public async Task<ActionResult<LicenseFeeStatus>> GetLicenseFeeStatus(Guid id)
    {
        try
        {
            var status = await _settlementService.GetLicenseFeeStatusAsync(id);
            return Ok(status);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    #endregion

    #region Quarter Endpoints

    [HttpGet("quarters")]
    public async Task<ActionResult<List<EmpostQuarter>>> GetQuarters([FromQuery] int? year = null)
    {
        var tenantId = GetCurrentTenantId();
        
        if (year.HasValue)
        {
            var quarters = await _quarterService.GetQuartersForYearAsync(year.Value);
            return Ok(quarters);
        }

        var allQuarters = await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId)
            .OrderByDescending(q => q.Year)
            .ThenByDescending(q => q.Quarter)
            .ToListAsync();

        return Ok(allQuarters);
    }

    [HttpGet("quarters/available-years")]
    public async Task<ActionResult<List<int>>> GetAvailableYears()
    {
        var tenantId = GetCurrentTenantId();
        var years = await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId)
            .Select(q => q.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();
        
        return Ok(years);
    }

    [HttpGet("quarter/current")]
    public async Task<ActionResult<EmpostQuarter>> GetCurrentQuarter()
    {
        var quarter = await _quarterService.GetCurrentQuarterAsync();
        if (quarter == null)
            return NotFound("No current quarter found. Please create a license first.");
        return Ok(quarter);
    }

    [HttpGet("quarter/{id}")]
    public async Task<ActionResult<EmpostQuarter>> GetQuarter(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var quarter = await _context.EmpostQuarters
            .Include(q => q.EmpostLicense)
            .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId);

        if (quarter == null)
            return NotFound();

        return Ok(quarter);
    }

    [HttpGet("quarter/{id}/breakdown")]
    public async Task<ActionResult<QuarterFeeBreakdown>> GetQuarterBreakdown(Guid id)
    {
        try
        {
            var breakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(id);
            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("quarters/{id}/calculate-fees")]
    public async Task<ActionResult<QuarterCalculationResult>> CalculateQuarterFees(Guid id)
    {
        try
        {
            var result = await _feeCalculationService.CalculateQuarterFeesAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("quarter/{id}/lock")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<EmpostQuarter>> LockQuarter(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var quarter = await _periodLockService.LockQuarterAsync(id, userId, userName);
            return Ok(quarter);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("quarter/{id}/unlock")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<EmpostQuarter>> UnlockQuarter(Guid id, [FromBody] UnlockQuarterRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var quarter = await _periodLockService.UnlockQuarterAsync(id, userId, userName, request.Reason);
            return Ok(quarter);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("quarter/{id}/submit")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<EmpostQuarter>> MarkQuarterAsSubmitted(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var quarter = await _periodLockService.MarkQuarterAsSubmittedAsync(id, userId, userName);
            return Ok(quarter);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("quarters/locked")]
    public async Task<ActionResult<List<EmpostQuarter>>> GetLockedQuarters()
    {
        var quarters = await _periodLockService.GetLockedQuartersAsync();
        return Ok(quarters);
    }

    [HttpGet("quarters/pending-submission")]
    public async Task<ActionResult<List<EmpostQuarter>>> GetQuartersPendingSubmission()
    {
        var quarters = await _periodLockService.GetQuartersPendingSubmissionAsync();
        return Ok(quarters);
    }

    #endregion

    #region Settlement Endpoints

    [HttpGet("quarter/{id}/settlement-calculation")]
    public async Task<ActionResult<QuarterSettlementCalculation>> GetQuarterSettlementCalculation(Guid id)
    {
        try
        {
            var calculation = await _settlementService.CalculateQuarterSettlementAsync(id);
            return Ok(calculation);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("quarter/{id}/settlement")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<EmpostQuarterlySettlement>> CreateSettlement(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var settlement = await _settlementService.CreateSettlementAsync(id, userId);
            return Ok(settlement);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("settlement/{id}/payment")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<EmpostQuarterlySettlement>> RecordSettlementPayment(
        Guid id, 
        [FromBody] RecordSettlementPaymentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var settlement = await _settlementService.RecordPaymentAsync(
                id, 
                request.AmountPaid, 
                request.PaymentMethod, 
                request.PaymentReference, 
                userId);
            return Ok(settlement);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("license/{id}/settlements")]
    public async Task<ActionResult<List<EmpostQuarterlySettlement>>> GetSettlements(Guid id)
    {
        var settlements = await _settlementService.GetSettlementsForLicensePeriodAsync(id);
        return Ok(settlements);
    }

    #endregion

    #region Return Adjustment Endpoints

    [HttpGet("adjustments/pending")]
    public async Task<ActionResult<List<EmpostReturnAdjustment>>> GetPendingAdjustments()
    {
        var adjustments = await _returnAdjustmentService.GetPendingAdjustmentsAsync();
        return Ok(adjustments);
    }

    [HttpGet("return-adjustments")]
    public async Task<ActionResult<List<object>>> GetAllReturnAdjustments()
    {
        var adjustments = await _returnAdjustmentService.GetAllAdjustmentsAsync();
        var result = adjustments.Select(a => new
        {
            a.Id,
            AWBNumber = a.AWBNumber ?? "",
            OriginalBookingDate = a.OriginalShipmentDate,
            ReturnDate = a.ReturnDate,
            QuarterName = a.EmpostQuarter != null ? $"{a.EmpostQuarter.QuarterName} {a.EmpostQuarter.Year}" : "",
            OriginalFeeAmount = a.OriginalFeeAmount,
            AdjustmentAmount = a.AdjustmentAmount,
            ReturnReason = a.Reason ?? "",
            Status = a.Status.ToString(),
            ProcessedAt = a.AppliedDate
        }).ToList();
        return Ok(result);
    }

    [HttpPost("return-adjustments/process-pending")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> ProcessPendingReturnAdjustments()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var count = await _returnAdjustmentService.ProcessPendingAdjustmentsAsync(userId, userName);
            return Ok(new { ProcessedCount = count, Message = $"Successfully processed {count} pending adjustments" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to process pending adjustments: {ex.Message}");
        }
    }

    [HttpGet("quarter/{id}/adjustments")]
    public async Task<ActionResult<List<EmpostReturnAdjustment>>> GetQuarterAdjustments(Guid id)
    {
        var adjustments = await _returnAdjustmentService.GetAdjustmentsForQuarterAsync(id);
        return Ok(adjustments);
    }

    [HttpPost("shipment/{shipmentId}/return-adjustment")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<EmpostReturnAdjustment>> CreateReturnAdjustment(
        Guid shipmentId, 
        [FromBody] CreateReturnAdjustmentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var adjustment = await _returnAdjustmentService.CreateReturnAdjustmentAsync(
                shipmentId, 
                request.Reason, 
                userId, 
                userName);

            if (adjustment == null)
                return BadRequest("Unable to create return adjustment. Shipment may be exempt or already adjusted.");

            return Ok(adjustment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Reporting Endpoints

    [HttpGet("quarter/{id}/report")]
    public async Task<ActionResult<EmpostQuarterlyReport>> GetQuarterlyReport(Guid id)
    {
        try
        {
            var report = await _reportingService.GenerateQuarterlyReportAsync(id);
            return Ok(report);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("license/{id}/annual-report/{year}")]
    public async Task<ActionResult<EmpostAnnualReport>> GetAnnualReport(Guid id, int year)
    {
        try
        {
            var report = await _reportingService.GenerateAnnualReportAsync(id, year);
            return Ok(report);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("quarter/{id}/shipments")]
    public async Task<ActionResult<List<EmpostShipmentFee>>> GetQuarterShipments(Guid id)
    {
        var shipments = await _reportingService.GetQuarterShipmentDetailsAsync(id);
        return Ok(shipments);
    }

    [HttpGet("quarter/{id}/reconciliation")]
    public async Task<ActionResult<EmpostReconciliationResult>> ReconcileQuarter(Guid id)
    {
        try
        {
            var result = await _reportingService.ReconcileWithGLAsync(id);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    #endregion

    #region Shipment Fee Endpoints

    [HttpGet("shipment/{shipmentId}/fee")]
    public async Task<ActionResult<EmpostShipmentFee>> GetShipmentFee(Guid shipmentId)
    {
        var fee = await _feeCalculationService.GetShipmentFeeAsync(shipmentId);
        if (fee == null)
            return NotFound();
        return Ok(fee);
    }

    [HttpPost("shipment/{shipmentId}/calculate-fee")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> RecalculateShipmentFee(Guid shipmentId)
    {
        try
        {
            await _feeCalculationService.RecalculateShipmentFeeAsync(shipmentId);
            return Ok(new { message = "Fee recalculated successfully" });
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Dashboard Endpoint

    [HttpGet("dashboard")]
    public async Task<ActionResult<EmpostDashboardData>> GetDashboard()
    {
        try
        {
            var license = await _licenseService.GetActiveLicenseAsync();
            if (license == null)
            {
                return Ok(new EmpostDashboardData
                {
                    HasActiveLicense = false
                });
            }

            var currentQuarter = await _quarterService.GetCurrentQuarterAsync();
            var quarterInfo = _quarterService.GetQuarterInfo(DateTime.UtcNow);
            var licenseStatus = await _settlementService.GetLicenseFeeStatusAsync(license.Id);

            QuarterFeeBreakdown? currentBreakdown = null;
            if (currentQuarter != null)
            {
                currentBreakdown = await _feeCalculationService.GetQuarterFeeBreakdownAsync(currentQuarter.Id);
            }

            var pendingSubmission = await _periodLockService.GetQuartersPendingSubmissionAsync();

            var dashboard = new EmpostDashboardData
            {
                HasActiveLicense = true,
                LicenseNumber = license.LicenseNumber,
                LicensePeriodStart = license.LicensePeriodStart,
                LicensePeriodEnd = license.LicensePeriodEnd,
                CurrentQuarterName = quarterInfo.QuarterName,
                CurrentYear = quarterInfo.Year,
                DaysUntilSubmission = _quarterService.GetDaysUntilSubmissionDeadline(quarterInfo.Year, quarterInfo.Quarter),
                SubmissionDeadline = quarterInfo.SubmissionDeadline,
                IsCurrentQuarterLocked = currentQuarter?.IsLocked ?? false,
                MinimumAdvanceAmount = license.MinimumAdvanceAmount,
                RoyaltyPercentage = license.RoyaltyPercentage,
                CumulativeFeeToDate = licenseStatus.CumulativeFeeToDate,
                AdvanceUtilized = licenseStatus.AdvanceUtilized,
                ExcessOverAdvance = licenseStatus.ExcessOverAdvance,
                BalanceDue = licenseStatus.BalanceDue,
                CurrentQuarterShipments = currentBreakdown?.TotalShipments ?? 0,
                CurrentQuarterTaxableShipments = currentBreakdown?.TaxableShipments ?? 0,
                CurrentQuarterGrossRevenue = currentBreakdown?.TotalGrossRevenue ?? 0,
                CurrentQuarterFee = currentBreakdown?.NetEmpostFee ?? 0,
                QuartersPendingSubmission = pendingSubmission.Count
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Audit Log Endpoint

    [HttpGet("audit-logs")]
    public async Task<ActionResult<List<EmpostAuditLog>>> GetAuditLogs(
        [FromQuery] Guid? licenseId = null,
        [FromQuery] Guid? quarterId = null,
        [FromQuery] int limit = 100)
    {
        var tenantId = GetCurrentTenantId();
        
        var query = _context.EmpostAuditLogs
            .Where(l => l.TenantId == tenantId)
            .AsQueryable();

        if (licenseId.HasValue)
        {
            query = query.Where(l => l.EmpostLicenseId == licenseId.Value);
        }

        if (quarterId.HasValue)
        {
            query = query.Where(l => l.EmpostQuarterId == quarterId.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.PerformedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(logs);
    }

    #endregion

    #region Report Export Endpoints

    [HttpGet("reports/quarterly/{quarterId}/pdf")]
    public async Task<IActionResult> GetQuarterlyReportPdf(Guid quarterId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound("Tenant not found");

            var report = await _reportingService.GenerateQuarterlyReportAsync(quarterId);
            var pdfBytes = _pdfExportService.GenerateQuarterlyFeeReportPdf(report, tenant);

            var fileName = $"Empost_Quarterly_Report_{report.QuarterName}_{report.Year}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reports/quarterly/{quarterId}/settlement-statement")]
    public async Task<IActionResult> GetSettlementStatementPdf(Guid quarterId, [FromQuery] decimal arrears = 0, [FromQuery] decimal advanceBalance = 0, [FromQuery] decimal delayFine = 0, [FromQuery] decimal otherFines = 0, [FromQuery] string? managerName = null)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound("Tenant not found");

            var quarter = await _context.EmpostQuarters
                .Include(q => q.EmpostLicense)
                .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId);
            if (quarter == null)
                return NotFound("Quarter not found");

            var report = await _reportingService.GenerateQuarterlyReportAsync(quarterId);
            
            var settlementData = new SettlementStatementData
            {
                LicenseNumber = quarter.EmpostLicense.LicenseNumber,
                LicenseeName = quarter.EmpostLicense.LicenseeName ?? tenant.Name,
                LicensePeriodStart = quarter.EmpostLicense.LicensePeriodStart,
                LicensePeriodEnd = quarter.EmpostLicense.LicensePeriodEnd,
                CompanyManagerName = managerName ?? "",
                Arrears = arrears,
                ForwardedAdvanceBalance = advanceBalance,
                DelayFine = delayFine,
                OtherFines = otherFines
            };

            var pdfBytes = _pdfExportService.GenerateSettlementStatementPdf(report, tenant, settlementData);

            var fileName = $"Empost_Settlement_Statement_Form9_{report.QuarterName}_{report.Year}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reports/quarterly/{quarterId}/excel")]
    public async Task<IActionResult> GetQuarterlyReportExcel(Guid quarterId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var report = await _reportingService.GenerateQuarterlyReportAsync(quarterId);
            var excelBytes = GenerateQuarterlyExcel(report);

            var fileName = $"Empost_Quarterly_Report_{report.QuarterName}_{report.Year}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reports/annual/{licenseId}/{year}/pdf")]
    public async Task<IActionResult> GetAnnualReportPdf(Guid licenseId, int year)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound("Tenant not found");

            var report = await _reportingService.GenerateAnnualReportAsync(licenseId, year);
            var pdfBytes = _pdfExportService.GenerateAnnualSummaryReportPdf(report, tenant);

            var fileName = $"Empost_Annual_Report_{year}-{year + 1}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private byte[] GenerateQuarterlyExcel(EmpostQuarterlyReport report)
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        
        using var package = new OfficeOpenXml.ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Quarterly Report");

        int row = 1;

        ws.Cells[row, 1].Value = "EMPOST 7X QUARTERLY FEE REPORT";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 14;
        row += 2;

        ws.Cells[row, 1].Value = "License Number:";
        ws.Cells[row, 2].Value = report.LicenseNumber;
        row++;
        ws.Cells[row, 1].Value = "Licensee:";
        ws.Cells[row, 2].Value = report.LicenseeName;
        row++;
        ws.Cells[row, 1].Value = "Quarter:";
        ws.Cells[row, 2].Value = $"{report.QuarterName} {report.Year}";
        row++;
        ws.Cells[row, 1].Value = "Period:";
        ws.Cells[row, 2].Value = $"{report.PeriodStart:dd MMM yyyy} - {report.PeriodEnd:dd MMM yyyy}";
        row += 2;

        ws.Cells[row, 1].Value = "SHIPMENT SUMMARY";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        ws.Cells[row, 1].Value = "Total Shipments";
        ws.Cells[row, 2].Value = report.TotalShipments;
        row++;
        ws.Cells[row, 1].Value = "Taxable Shipments (<=30kg)";
        ws.Cells[row, 2].Value = report.TaxableShipments;
        row++;
        ws.Cells[row, 1].Value = "Exempt Shipments (>30kg)";
        ws.Cells[row, 2].Value = report.ExemptShipments;
        row += 2;

        ws.Cells[row, 1].Value = "REVENUE SUMMARY";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        ws.Cells[row, 1].Value = "Total Gross Revenue";
        ws.Cells[row, 2].Value = report.TotalGrossRevenue;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Taxable Revenue";
        ws.Cells[row, 2].Value = report.TaxableGrossRevenue;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Exempt Revenue";
        ws.Cells[row, 2].Value = report.ExemptGrossRevenue;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row += 2;

        ws.Cells[row, 1].Value = "FEE CALCULATION";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        ws.Cells[row, 1].Value = "Gross Empost Fee (10%)";
        ws.Cells[row, 2].Value = report.GrossEmpostFee;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Return Adjustments";
        ws.Cells[row, 2].Value = report.ReturnAdjustments;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Net Empost Fee";
        ws.Cells[row, 2].Value = report.NetEmpostFee;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 1, row, 2].Style.Font.Bold = true;
        row += 2;

        ws.Cells[row, 1].Value = "SETTLEMENT";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        ws.Cells[row, 1].Value = "Cumulative Fee YTD";
        ws.Cells[row, 2].Value = report.CumulativeFeeToDate;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Advance Payment";
        ws.Cells[row, 2].Value = report.AdvancePaymentAmount;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Amount Payable";
        ws.Cells[row, 2].Value = report.AmountPayableThisQuarter;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "VAT";
        ws.Cells[row, 2].Value = report.VATOnFee;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        row++;
        ws.Cells[row, 1].Value = "Total Payable";
        ws.Cells[row, 2].Value = report.TotalPayable;
        ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 1, row, 2].Style.Font.Bold = true;

        ws.Cells[ws.Dimension.Address].AutoFitColumns();

        if (report.ShipmentDetails.Any())
        {
            var wsDetails = package.Workbook.Worksheets.Add("Shipment Details");
            wsDetails.Cells[1, 1].Value = "AWB Number";
            wsDetails.Cells[1, 2].Value = "Date";
            wsDetails.Cells[1, 3].Value = "Classification";
            wsDetails.Cells[1, 4].Value = "Weight";
            wsDetails.Cells[1, 5].Value = "Gross Amount";
            wsDetails.Cells[1, 6].Value = "Empost Fee";
            wsDetails.Cells[1, 1, 1, 6].Style.Font.Bold = true;

            row = 2;
            foreach (var detail in report.ShipmentDetails)
            {
                wsDetails.Cells[row, 1].Value = detail.AWBNumber;
                wsDetails.Cells[row, 2].Value = detail.ShipmentDate;
                wsDetails.Cells[row, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                wsDetails.Cells[row, 3].Value = detail.Classification;
                wsDetails.Cells[row, 4].Value = detail.Weight;
                wsDetails.Cells[row, 5].Value = detail.GrossAmount;
                wsDetails.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                wsDetails.Cells[row, 6].Value = detail.EmpostFee;
                wsDetails.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                row++;
            }

            wsDetails.Cells[wsDetails.Dimension.Address].AutoFitColumns();
        }

        return package.GetAsByteArray();
    }

    #endregion
}

public class UnlockQuarterRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RecordSettlementPaymentRequest
{
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
}

public class CreateReturnAdjustmentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class EmpostDashboardData
{
    public bool HasActiveLicense { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicensePeriodStart { get; set; }
    public DateTime? LicensePeriodEnd { get; set; }
    public string? CurrentQuarterName { get; set; }
    public int? CurrentYear { get; set; }
    public int? DaysUntilSubmission { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public bool IsCurrentQuarterLocked { get; set; }
    public decimal MinimumAdvanceAmount { get; set; }
    public decimal RoyaltyPercentage { get; set; }
    public decimal CumulativeFeeToDate { get; set; }
    public decimal AdvanceUtilized { get; set; }
    public decimal ExcessOverAdvance { get; set; }
    public decimal BalanceDue { get; set; }
    public int CurrentQuarterShipments { get; set; }
    public int CurrentQuarterTaxableShipments { get; set; }
    public decimal CurrentQuarterGrossRevenue { get; set; }
    public decimal CurrentQuarterFee { get; set; }
    public int QuartersPendingSubmission { get; set; }
}
