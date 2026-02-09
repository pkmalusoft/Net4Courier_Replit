using Microsoft.EntityFrameworkCore;
using Net4Courier.Finance.Entities;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class EmpostFeeReportService
{
    private readonly ApplicationDbContext _dbContext;

    public EmpostFeeReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<EmpostFeeReportItem>> GetEmpostFeeReportAsync(
        DateTime fromDate, DateTime toDate, long? branchId = null)
    {
        var fromDateUtc = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Utc);
        var toDateUtc = DateTime.SpecifyKind(toDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var query = _dbContext.InscanMasters
            .Where(i => !i.IsDeleted && 
                        i.TransactionDate >= fromDateUtc && 
                        i.TransactionDate <= toDateUtc);

        if (branchId.HasValue)
        {
            query = query.Where(i => i.BranchId == branchId.Value);
        }

        var groupedData = await query
            .GroupBy(i => new 
            { 
                ParcelTypeId = (int)i.DocumentTypeId,
                MovementId = (int)i.MovementTypeId
            })
            .Select(g => new
            {
                g.Key.ParcelTypeId,
                g.Key.MovementId,
                Quantity = g.Count(),
                CourierCharge = g.Sum(x => x.CourierCharge ?? 0),
                OtherCharge = g.Sum(x => x.OtherCharge ?? 0),
                FuelSurcharge = g.Sum(x => x.FuelSurcharge ?? 0),
                NetTotal = g.Sum(x => x.NetTotal ?? 0),
                TaxAmount = g.Sum(x => ((x.CourierCharge ?? 0) + (x.FuelSurcharge ?? 0) + (x.OtherCharge ?? 0)) * (x.TaxPercent ?? 0) / 100),
                VatPercent = g.Where(x => (x.TaxPercent ?? 0) > 0).Select(x => x.TaxPercent ?? 0).FirstOrDefault()
            })
            .ToListAsync();

        var result = groupedData.Select(g => new EmpostFeeReportItem
        {
            ParcelTypeId = g.ParcelTypeId,
            ParcelType = ((DocumentType)g.ParcelTypeId).ToString(),
            MovementId = g.MovementId,
            MovementType = GetMovementTypeName((MovementType)g.MovementId),
            Quantity = g.Quantity,
            CourierCharge = g.CourierCharge + g.FuelSurcharge,
            OtherCharge = g.OtherCharge,
            NetTotal = g.NetTotal,
            Tax = g.TaxAmount,
            VatPercent = g.VatPercent,
            VatAmount = g.TaxAmount
        }).OrderBy(x => x.ParcelTypeId).ThenBy(x => x.MovementId).ToList();

        return result;
    }

    private string GetMovementTypeName(MovementType movementType)
    {
        return movementType switch
        {
            MovementType.Domestic => "Domestic",
            MovementType.InternationalExport => "International Export",
            MovementType.InternationalImport => "International Import",
            MovementType.Transhipment => "Transhipment",
            _ => movementType.ToString()
        };
    }
}
