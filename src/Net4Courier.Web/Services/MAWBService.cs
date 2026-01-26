using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class ScanValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public InscanMaster? Shipment { get; set; }
}

public class MAWBService
{
    private readonly ApplicationDbContext _context;
    private readonly ShipmentStatusService _statusService;

    public MAWBService(ApplicationDbContext context, ShipmentStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<string> GenerateMAWBNo(long? branchId)
    {
        var prefix = "MAWB";
        var date = DateTime.Now.ToString("yyyyMMdd");
        
        var lastMawb = await _context.MasterAirwaybills
            .Where(m => m.BranchId == branchId && m.MAWBNo.StartsWith($"{prefix}{date}"))
            .OrderByDescending(m => m.MAWBNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastMawb != null)
        {
            var lastSeq = lastMawb.MAWBNo.Substring(prefix.Length + date.Length);
            if (int.TryParse(lastSeq, out int parsed))
                sequence = parsed + 1;
        }

        return $"{prefix}{date}{sequence:D4}";
    }

    public async Task<MasterAirwaybill> CreateMAWB(
        long? companyId,
        long? branchId,
        long? financialYearId,
        long? originCityId,
        string? originCityName,
        long? originCountryId,
        string? originCountryName,
        string? originAirportCode,
        long? destinationCityId,
        string? destinationCityName,
        long? destinationCountryId,
        string? destinationCountryName,
        string? destinationAirportCode,
        string? carrierCode,
        string? carrierName,
        string? flightNo,
        DateTime? departureDate,
        DateTime? arrivalDate,
        TimeSpan? departureTime,
        TimeSpan? arrivalTime,
        long? coLoaderId = null,
        string? coLoaderName = null,
        string? remarks = null)
    {
        var mawbNo = await GenerateMAWBNo(branchId);
        
        var mawb = new MasterAirwaybill
        {
            MAWBNo = mawbNo,
            TransactionDate = DateTime.UtcNow,
            CompanyId = companyId,
            BranchId = branchId,
            FinancialYearId = financialYearId,
            OriginCityId = originCityId,
            OriginCityName = originCityName,
            OriginCountryId = originCountryId,
            OriginCountryName = originCountryName,
            OriginAirportCode = originAirportCode,
            DestinationCityId = destinationCityId,
            DestinationCityName = destinationCityName,
            DestinationCountryId = destinationCountryId,
            DestinationCountryName = destinationCountryName,
            DestinationAirportCode = destinationAirportCode,
            CarrierCode = carrierCode,
            CarrierName = carrierName,
            FlightNo = flightNo,
            DepartureDate = departureDate.HasValue ? DateTime.SpecifyKind(departureDate.Value, DateTimeKind.Utc) : null,
            ArrivalDate = arrivalDate.HasValue ? DateTime.SpecifyKind(arrivalDate.Value, DateTimeKind.Utc) : null,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            CoLoaderId = coLoaderId,
            CoLoaderName = coLoaderName,
            Remarks = remarks,
            Status = MAWBStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.MasterAirwaybills.Add(mawb);
        await _context.SaveChangesAsync();

        return mawb;
    }

    public async Task<MAWBBag> CreateBag(long mawbId, string? bagType = null, string? remarks = null)
    {
        var mawb = await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);

        if (mawb == null)
            throw new InvalidOperationException("MAWB not found");

        if (mawb.Status != MAWBStatus.Draft)
            throw new InvalidOperationException("Cannot add bags to finalized MAWB");

        var sequenceNo = (mawb.Bags?.Count ?? 0) + 1;
        var bagNo = $"{mawb.MAWBNo}-B{sequenceNo:D3}";

        var bag = new MAWBBag
        {
            MAWBId = mawbId,
            BagNo = bagNo,
            SequenceNo = sequenceNo,
            BagType = bagType,
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow
        };

        _context.MAWBBags.Add(bag);
        await _context.SaveChangesAsync();

        return bag;
    }

    public async Task<ScanValidationResult> ValidateShipmentForBagging(
        string awbNo,
        long mawbId,
        long bagId)
    {
        var shipment = await _context.InscanMasters
            .FirstOrDefaultAsync(i => i.AWBNo == awbNo);

        if (shipment == null)
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid AWB – Shipment does not exist"
            };
        }

        var mawb = await _context.MasterAirwaybills.FindAsync(mawbId);
        if (mawb == null)
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = "MAWB not found"
            };
        }

        if (mawb.Status != MAWBStatus.Draft)
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = "Cannot add shipments to finalized MAWB"
            };
        }

        if (shipment.IsOnHold)
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = "Shipment is on hold and cannot be manifested. Resolve hold before bagging."
            };
        }

        if (shipment.MAWBId.HasValue)
        {
            if (shipment.MAWBId == mawbId)
            {
                return new ScanValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Shipment already assigned to Bag {shipment.BagNo}"
                };
            }
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = "Shipment already manifested in another MAWB"
            };
        }

        if (shipment.MAWBBagId.HasValue)
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Shipment already assigned to Bag {shipment.BagNo}"
            };
        }

        if (!string.IsNullOrEmpty(mawb.OriginCityName) && 
            !string.Equals(shipment.ConsignorCity, mawb.OriginCityName, StringComparison.OrdinalIgnoreCase))
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Origin mismatch – Shipment origin ({shipment.ConsignorCity}) doesn't match MAWB origin ({mawb.OriginCityName})"
            };
        }

        if (!string.IsNullOrEmpty(mawb.DestinationCityName) && 
            !string.Equals(shipment.ConsigneeCity, mawb.DestinationCityName, StringComparison.OrdinalIgnoreCase))
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Destination mismatch – Shipment destination ({shipment.ConsigneeCity}) doesn't match MAWB destination ({mawb.DestinationCityName})"
            };
        }

        var eligibleStatuses = new[]
        {
            CourierStatus.PickedUp,
            CourierStatus.InscanAtOrigin,
            CourierStatus.Pending
        };

        if (!eligibleStatuses.Contains(shipment.CourierStatusId))
        {
            return new ScanValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Shipment status ({shipment.CourierStatusId}) is not eligible for bagging"
            };
        }

        return new ScanValidationResult
        {
            IsValid = true,
            Shipment = shipment
        };
    }

    public async Task<InscanMaster?> AddShipmentToBag(
        long bagId,
        long shipmentId,
        long? userId = null,
        string? userName = null)
    {
        var bag = await _context.MAWBBags
            .Include(b => b.MAWB)
            .FirstOrDefaultAsync(b => b.Id == bagId);

        if (bag == null)
            throw new InvalidOperationException("Bag not found");

        if (bag.MAWB?.Status != MAWBStatus.Draft)
            throw new InvalidOperationException("Cannot add shipments to finalized MAWB");

        var shipment = await _context.InscanMasters.FindAsync(shipmentId);
        if (shipment == null)
            throw new InvalidOperationException("Shipment not found");

        shipment.MAWBId = bag.MAWBId;
        shipment.MAWBBagId = bagId;
        shipment.MAWBNo = bag.MAWB?.MAWBNo;
        shipment.BagNo = bag.BagNo;
        shipment.BaggedAt = DateTime.UtcNow;
        shipment.BaggedByUserId = userId;
        shipment.BaggedByUserName = userName;
        shipment.ModifiedAt = DateTime.UtcNow;

        bag.PieceCount += shipment.Pieces ?? 1;
        bag.GrossWeight += shipment.Weight ?? 0;
        bag.ChargeableWeight += shipment.ChargeableWeight ?? 0;
        bag.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _statusService.SetStatus(
            shipmentId,
            "BAGGED",
            "MAWB_BAGGING",
            bagId,
            "MAWBBag",
            null,
            bag.MAWB?.OriginCityName,
            userId,
            userName,
            $"Added to Bag {bag.BagNo}, MAWB {bag.MAWB?.MAWBNo}",
            true);

        return shipment;
    }

    public async Task<bool> RemoveShipmentFromBag(long shipmentId, long? userId = null, string? userName = null)
    {
        var shipment = await _context.InscanMasters
            .Include(s => s.MAWBBag)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);

        if (shipment == null || !shipment.MAWBBagId.HasValue)
            return false;

        var bag = shipment.MAWBBag;
        var mawb = await _context.MasterAirwaybills.FindAsync(shipment.MAWBId);

        if (mawb?.Status != MAWBStatus.Draft)
            throw new InvalidOperationException("Cannot remove shipments from finalized MAWB");

        var oldBagNo = shipment.BagNo;
        var oldMAWBNo = shipment.MAWBNo;

        if (bag != null)
        {
            bag.PieceCount -= shipment.Pieces ?? 1;
            bag.GrossWeight -= shipment.Weight ?? 0;
            bag.ChargeableWeight -= shipment.ChargeableWeight ?? 0;
            bag.ModifiedAt = DateTime.UtcNow;
        }

        shipment.MAWBId = null;
        shipment.MAWBBagId = null;
        shipment.MAWBNo = null;
        shipment.BagNo = null;
        shipment.BaggedAt = null;
        shipment.BaggedByUserId = null;
        shipment.BaggedByUserName = null;
        shipment.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _statusService.SetStatus(
            shipmentId,
            "UNBAGGED",
            "MAWB_UNBAGGING",
            bag?.Id,
            "MAWBBag",
            null,
            mawb?.OriginCityName,
            userId,
            userName,
            $"Removed from Bag {oldBagNo}, MAWB {oldMAWBNo}",
            true);

        return true;
    }

    public async Task<(bool Success, string? ErrorMessage)> FinalizeMAWB(
        long mawbId,
        long? userId = null,
        string? userName = null)
    {
        var mawb = await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);

        if (mawb == null)
            return (false, "MAWB not found");

        if (mawb.Status != MAWBStatus.Draft)
            return (false, "MAWB is already finalized");

        var shipmentsInMawb = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId)
            .ToListAsync();

        if (!shipmentsInMawb.Any())
            return (false, "Cannot finalize MAWB without any shipments");

        var onHoldShipments = shipmentsInMawb.Where(s => s.IsOnHold).ToList();
        if (onHoldShipments.Any())
        {
            var awbNos = string.Join(", ", onHoldShipments.Take(5).Select(s => s.AWBNo));
            return (false, $"Cannot finalize – {onHoldShipments.Count} shipment(s) are on hold: {awbNos}");
        }

        mawb.TotalBags = mawb.Bags?.Count ?? 0;
        mawb.TotalPieces = shipmentsInMawb.Sum(s => s.Pieces ?? 0);
        mawb.TotalGrossWeight = shipmentsInMawb.Sum(s => s.Weight ?? 0);
        mawb.TotalChargeableWeight = shipmentsInMawb.Sum(s => s.ChargeableWeight ?? 0);
        mawb.Status = MAWBStatus.Finalized;
        mawb.FinalizedAt = DateTime.UtcNow;
        mawb.FinalizedByUserId = userId;
        mawb.FinalizedByUserName = userName;
        mawb.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        foreach (var shipment in shipmentsInMawb)
        {
            await _statusService.SetStatus(
                shipment.Id,
                "MANIFESTED",
                "MAWB_FINALIZED",
                mawbId,
                "MasterAirwaybill",
                null,
                mawb.OriginCityName,
                userId,
                userName,
                $"MAWB {mawb.MAWBNo} finalized for {mawb.DestinationCityName}",
                true);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DispatchMAWB(
        long mawbId,
        long? userId = null,
        string? userName = null)
    {
        var mawb = await _context.MasterAirwaybills.FindAsync(mawbId);

        if (mawb == null)
            return (false, "MAWB not found");

        if (mawb.Status != MAWBStatus.Finalized)
            return (false, "MAWB must be finalized before dispatch");

        mawb.Status = MAWBStatus.Dispatched;
        mawb.DispatchedAt = DateTime.UtcNow;
        mawb.DispatchedByUserId = userId;
        mawb.DispatchedByUserName = userName;
        mawb.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var shipmentsInMawb = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId)
            .ToListAsync();

        foreach (var shipment in shipmentsInMawb)
        {
            await _statusService.SetStatus(
                shipment.Id,
                "IN_TRANSIT",
                "MAWB_DISPATCHED",
                mawbId,
                "MasterAirwaybill",
                null,
                mawb.OriginCityName,
                userId,
                userName,
                $"MAWB {mawb.MAWBNo} dispatched via {mawb.CarrierName} {mawb.FlightNo}",
                true);
        }

        return (true, null);
    }

    public async Task<List<InscanMaster>> GetEligibleShipments(
        long mawbId,
        string? originCity = null,
        string? destinationCity = null)
    {
        var mawb = await _context.MasterAirwaybills.FindAsync(mawbId);
        if (mawb == null)
            return new List<InscanMaster>();

        var eligibleStatuses = new[]
        {
            CourierStatus.PickedUp,
            CourierStatus.InscanAtOrigin,
            CourierStatus.Pending
        };

        var query = _context.InscanMasters
            .Where(s => !s.MAWBId.HasValue)
            .Where(s => !s.IsOnHold)
            .Where(s => eligibleStatuses.Contains(s.CourierStatusId));

        if (!string.IsNullOrEmpty(mawb.OriginCityName))
            query = query.Where(s => s.ConsignorCity == mawb.OriginCityName);

        if (!string.IsNullOrEmpty(mawb.DestinationCityName))
            query = query.Where(s => s.ConsigneeCity == mawb.DestinationCityName);

        return await query
            .OrderByDescending(s => s.TransactionDate)
            .Take(500)
            .ToListAsync();
    }

    public async Task<List<MasterAirwaybill>> GetMAWBList(
        long? branchId = null,
        MAWBStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.MasterAirwaybills
            .Include(m => m.Bags)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(m => m.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(m => m.Status == status);

        if (fromDate.HasValue)
            query = query.Where(m => m.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.TransactionDate <= toDate.Value);

        return await query
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.Id)
            .Take(200)
            .ToListAsync();
    }

    public async Task<MasterAirwaybill?> GetMAWBWithDetails(long mawbId)
    {
        return await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);
    }

    public async Task<List<InscanMaster>> GetShipmentsInMAWB(long mawbId)
    {
        return await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId)
            .OrderBy(s => s.BagNo)
            .ThenBy(s => s.AWBNo)
            .ToListAsync();
    }

    public async Task<List<InscanMaster>> GetShipmentsInBag(long bagId)
    {
        return await _context.InscanMasters
            .Where(s => s.MAWBBagId == bagId)
            .OrderBy(s => s.AWBNo)
            .ToListAsync();
    }

    public async Task SealBag(long bagId, string sealNo, long? userId = null, string? userName = null)
    {
        var bag = await _context.MAWBBags
            .Include(b => b.MAWB)
            .FirstOrDefaultAsync(b => b.Id == bagId);

        if (bag == null)
            throw new InvalidOperationException("Bag not found");

        if (bag.MAWB?.Status != MAWBStatus.Draft)
            throw new InvalidOperationException("Cannot seal bag in finalized MAWB");

        bag.SealNo = sealNo;
        bag.IsSealed = true;
        bag.SealedAt = DateTime.UtcNow;
        bag.SealedByUserId = userId;
        bag.SealedByUserName = userName;
        bag.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateMAWB(MasterAirwaybill mawb)
    {
        var existing = await _context.MasterAirwaybills.FindAsync(mawb.Id);
        if (existing == null)
            return false;

        if (existing.Status != MAWBStatus.Draft)
            throw new InvalidOperationException("Cannot modify finalized MAWB");

        existing.OriginCityId = mawb.OriginCityId;
        existing.OriginCityName = mawb.OriginCityName;
        existing.OriginCountryId = mawb.OriginCountryId;
        existing.OriginCountryName = mawb.OriginCountryName;
        existing.OriginAirportCode = mawb.OriginAirportCode;
        existing.DestinationCityId = mawb.DestinationCityId;
        existing.DestinationCityName = mawb.DestinationCityName;
        existing.DestinationCountryId = mawb.DestinationCountryId;
        existing.DestinationCountryName = mawb.DestinationCountryName;
        existing.DestinationAirportCode = mawb.DestinationAirportCode;
        existing.CarrierCode = mawb.CarrierCode;
        existing.CarrierName = mawb.CarrierName;
        existing.FlightNo = mawb.FlightNo;
        existing.DepartureDate = mawb.DepartureDate.HasValue ? DateTime.SpecifyKind(mawb.DepartureDate.Value, DateTimeKind.Utc) : null;
        existing.ArrivalDate = mawb.ArrivalDate.HasValue ? DateTime.SpecifyKind(mawb.ArrivalDate.Value, DateTimeKind.Utc) : null;
        existing.DepartureTime = mawb.DepartureTime;
        existing.ArrivalTime = mawb.ArrivalTime;
        existing.CoLoaderId = mawb.CoLoaderId;
        existing.CoLoaderName = mawb.CoLoaderName;
        existing.Remarks = mawb.Remarks;
        existing.CustomsDeclarationNo = mawb.CustomsDeclarationNo;
        existing.ExportPermitNo = mawb.ExportPermitNo;
        existing.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
