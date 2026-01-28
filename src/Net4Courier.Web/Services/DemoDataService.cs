using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;
using Net4Courier.Masters.Entities;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public interface IDemoDataService
{
    Task<DemoDataStats> GetDemoDataStatsAsync();
    Task<bool> CreateMasterDataAsync();
    Task<bool> CreateTransactionDataAsync();
    Task<bool> DeleteAllDemoDataAsync();
}

public class DemoDataStats
{
    public int Parties { get; set; }
    public int PickupRequests { get; set; }
    public int AWBs { get; set; }
    public int Invoices { get; set; }
    public int Receipts { get; set; }
    public int Journals { get; set; }
    public int DRS { get; set; }
}

public class DemoDataService : IDemoDataService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public DemoDataService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<DemoDataStats> GetDemoDataStatsAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        return new DemoDataStats
        {
            Parties = await context.Parties.CountAsync(p => p.IsDemo),
            PickupRequests = await context.PickupRequests.CountAsync(p => p.IsDemo),
            AWBs = await context.InscanMasters.CountAsync(a => a.IsDemo),
            Invoices = await context.Invoices.CountAsync(i => i.IsDemo),
            Receipts = await context.Receipts.CountAsync(r => r.IsDemo),
            Journals = await context.Journals.CountAsync(j => j.IsDemo),
            DRS = await context.DRSs.CountAsync(d => d.IsDemo)
        };
    }

    public async Task<bool> CreateMasterDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var uaeAddresses = new[]
        {
            new { Building = "Al Quoz Business Center", Street = "Sheikh Zayed Road", Area = "Al Quoz", City = "Dubai", PostalCode = "12345" },
            new { Building = "Jumeirah Business Tower", Street = "Jumeirah Lake Towers", Area = "JLT", City = "Dubai", PostalCode = "23456" },
            new { Building = "Abu Dhabi Trade Centre", Street = "Corniche Road", Area = "Al Markaziyah", City = "Abu Dhabi", PostalCode = "34567" },
            new { Building = "Sharjah Industrial Area", Street = "Industrial Area 6", Area = "Sharjah Industrial", City = "Sharjah", PostalCode = "45678" },
            new { Building = "Ajman Free Zone Office", Street = "Al Jurf Industrial", Area = "Al Jurf", City = "Ajman", PostalCode = "56789" }
        };

        var customerNames = new[]
        {
            "Emirates Trading Co.",
            "Gulf Logistics LLC",
            "Desert Star Enterprises",
            "Arabian Nights Imports",
            "Palm City Distribution"
        };

        var parties = new List<Party>();
        for (int i = 0; i < 5; i++)
        {
            var party = new Party
            {
                CompanyId = 1,
                Code = $"DEMO-CUST-{(i + 1):D3}",
                Name = customerNames[i],
                PartyType = PartyType.Customer,
                AccountNature = PartyAccountNature.Receivable,
                ContactPerson = $"Contact Person {i + 1}",
                Phone = $"+971 4 {100 + i:D3} {1000 + i:D4}",
                Mobile = $"+971 50 {500 + i:D3} {2000 + i:D4}",
                Email = $"demo{i + 1}@example.com",
                ClientAddress = $"{uaeAddresses[i].Building}, {uaeAddresses[i].Street}, {uaeAddresses[i].Area}, {uaeAddresses[i].City}",
                CreditLimit = 10000 + (i * 5000),
                CreditDays = 30,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            parties.Add(party);
        }

        context.Parties.AddRange(parties);
        await context.SaveChangesAsync();

        foreach (var party in parties)
        {
            var idx = parties.IndexOf(party);
            var addr = uaeAddresses[idx];
            var partyAddress = new PartyAddress
            {
                PartyId = party.Id,
                AddressType = "Primary",
                BuildingName = addr.Building,
                Street = addr.Street,
                Area = addr.Area,
                City = addr.City,
                State = addr.City,
                Country = "United Arab Emirates",
                PostalCode = addr.PostalCode,
                IsDefault = true,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            context.PartyAddresses.Add(partyAddress);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateTransactionDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var consignorData = new[]
        {
            new { Name = "Al Futtaim Electronics", Contact = "Ahmed Hassan", Phone = "+971 4 345 6789", Address = "Al Quoz Industrial 3", City = "Dubai" },
            new { Name = "Majid Al Futtaim Retail", Contact = "Sara Mohamed", Phone = "+971 4 456 7890", Address = "Mall of the Emirates", City = "Dubai" },
            new { Name = "Etisalat Store", Contact = "Khalid Ahmed", Phone = "+971 4 567 8901", Address = "Deira City Centre", City = "Dubai" },
            new { Name = "Lulu Hypermarket", Contact = "Fatima Ali", Phone = "+971 6 678 9012", Address = "Al Wahda Mall", City = "Sharjah" },
            new { Name = "Carrefour UAE", Contact = "Omar Ibrahim", Phone = "+971 2 789 0123", Address = "Yas Mall", City = "Abu Dhabi" }
        };

        var consigneeData = new[]
        {
            new { Name = "Customer Residence A", Contact = "Mohammed Abdullah", Phone = "+971 50 111 2222", Address = "Villa 25, Arabian Ranches", City = "Dubai", PostalCode = "11111" },
            new { Name = "Corporate Office B", Contact = "Aisha Khan", Phone = "+971 55 222 3333", Address = "Office 1502, Burj Khalifa", City = "Dubai", PostalCode = "22222" },
            new { Name = "Apartment Complex C", Contact = "Youssef Hassan", Phone = "+971 56 333 4444", Address = "Apt 801, Marina Walk", City = "Dubai", PostalCode = "33333" },
            new { Name = "Industrial Warehouse D", Contact = "Nadia Rashid", Phone = "+971 50 444 5555", Address = "Warehouse 12, Jebel Ali", City = "Dubai", PostalCode = "44444" },
            new { Name = "Business Centre E", Contact = "Tariq Mahmoud", Phone = "+971 52 555 6666", Address = "Suite 3A, Corniche Tower", City = "Abu Dhabi", PostalCode = "55555" }
        };

        var demoCustomers = await context.Parties
            .Where(p => p.IsDemo && p.PartyType == PartyType.Customer)
            .OrderBy(p => p.Id)
            .Take(5)
            .ToListAsync();

        var baseDate = DateTime.UtcNow.AddDays(-7);

        for (int i = 0; i < 5; i++)
        {
            var pickupDate = baseDate.AddDays(i);
            var customerId = demoCustomers.Count > i ? demoCustomers[i].Id : (long?)null;

            var pickupRequest = new PickupRequest
            {
                PickupNo = $"DEMO-PKP-{(i + 1):D3}",
                RequestDate = pickupDate,
                ScheduledDate = pickupDate.AddHours(2),
                CompanyId = 1,
                CustomerId = customerId,
                CustomerName = consignorData[i].Name,
                ContactPerson = consignorData[i].Contact,
                Phone = consignorData[i].Phone,
                Mobile = consignorData[i].Phone,
                PickupAddress = consignorData[i].Address,
                City = consignorData[i].City,
                Country = "United Arab Emirates",
                EstimatedPieces = i + 1,
                EstimatedWeight = (i + 1) * 2.5m,
                ActualPieces = i + 1,
                ActualWeight = (i + 1) * 2.5m,
                Status = PickupStatus.Inscanned,
                InscannedAt = pickupDate.AddHours(4),
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.PickupRequests.Add(pickupRequest);
            await context.SaveChangesAsync();

            var pickupShipment = new PickupRequestShipment
            {
                PickupRequestId = pickupRequest.Id,
                LineNo = 1,
                Consignee = consigneeData[i].Name,
                ConsigneeContact = consigneeData[i].Contact,
                ConsigneePhone = consigneeData[i].Phone,
                ConsigneeMobile = consigneeData[i].Phone,
                ConsigneeAddress1 = consigneeData[i].Address,
                ConsigneeCity = consigneeData[i].City,
                ConsigneeCountry = "United Arab Emirates",
                ConsigneePostalCode = consigneeData[i].PostalCode,
                Pieces = i + 1,
                Weight = (i + 1) * 2.5m,
                CargoDescription = $"Demo shipment contents {i + 1}",
                PaymentModeId = PaymentMode.Account,
                DocumentTypeId = DocumentType.ParcelUpto30Kg,
                Status = ShipmentLineStatus.Booked,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.PickupRequestShipments.Add(pickupShipment);
            await context.SaveChangesAsync();

            var awb = new InscanMaster
            {
                AWBNo = $"DEMO-AWB-{(i + 1):D3}",
                TransactionDate = pickupDate,
                CompanyId = 1,
                CustomerId = customerId,
                PickupRequestId = pickupRequest.Id,
                PickupRequestShipmentId = pickupShipment.Id,
                Consignor = consignorData[i].Name,
                ConsignorContact = consignorData[i].Contact,
                ConsignorPhone = consignorData[i].Phone,
                ConsignorMobile = consignorData[i].Phone,
                ConsignorAddress1 = consignorData[i].Address,
                ConsignorCity = consignorData[i].City,
                ConsignorCountry = "United Arab Emirates",
                Consignee = consigneeData[i].Name,
                ConsigneeContact = consigneeData[i].Contact,
                ConsigneePhone = consigneeData[i].Phone,
                ConsigneeMobile = consigneeData[i].Phone,
                ConsigneeAddress1 = consigneeData[i].Address,
                ConsigneeCity = consigneeData[i].City,
                ConsigneeCountry = "United Arab Emirates",
                ConsigneePostalCode = consigneeData[i].PostalCode,
                Pieces = i + 1,
                Weight = (i + 1) * 2.5m,
                ChargeableWeight = (i + 1) * 2.5m,
                CargoDescription = $"Demo shipment contents {i + 1}",
                CourierStatusId = CourierStatus.Delivered,
                PaymentModeId = PaymentMode.Account,
                MovementTypeId = MovementType.Domestic,
                DocumentTypeId = DocumentType.ParcelUpto30Kg,
                CourierCharge = 50 + (i * 10),
                NetTotal = 50 + (i * 10),
                DeliveredDate = pickupDate.AddDays(1),
                DeliveredTo = consigneeData[i].Contact,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.InscanMasters.Add(awb);
            await context.SaveChangesAsync();

            pickupShipment.AWBId = awb.Id;
            pickupShipment.AWBNo = awb.AWBNo;
            pickupShipment.BookedAt = pickupDate.AddHours(4);
            await context.SaveChangesAsync();

            var trackingEvents = new List<(CourierStatus Status, string Location, string Remarks, int HoursOffset)>
            {
                (CourierStatus.PickedUp, consignorData[i].City, "Package picked up from sender", 0),
                (CourierStatus.InscanAtOrigin, consignorData[i].City, "Package received at origin facility", 2),
                (CourierStatus.InTransit, "Dubai Hub", "Package in transit to destination", 6),
                (CourierStatus.OutForDelivery, consigneeData[i].City, "Package out for delivery", 20),
                (CourierStatus.Delivered, consigneeData[i].City, $"Delivered to {consigneeData[i].Contact}", 24)
            };

            foreach (var (status, location, remarks, hoursOffset) in trackingEvents)
            {
                var tracking = new AWBTracking
                {
                    InscanId = awb.Id,
                    EventDateTime = pickupDate.AddHours(hoursOffset),
                    StatusId = status,
                    Location = location,
                    City = location.Contains("Hub") ? "Dubai" : (status == CourierStatus.Delivered ? consigneeData[i].City : consignorData[i].City),
                    Country = "United Arab Emirates",
                    Remarks = remarks,
                    UpdatedByName = "Demo System",
                    IsPublic = true,
                    IsPODCaptured = status == CourierStatus.Delivered,
                    ReceivedBy = status == CourierStatus.Delivered ? consigneeData[i].Contact : null,
                    DeliveryDateTime = status == CourierStatus.Delivered ? pickupDate.AddHours(hoursOffset) : null,
                    IsActive = true,
                    IsDemo = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.AWBTrackings.Add(tracking);
            }

            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAllDemoDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var demoInscanIds = await context.InscanMasters
            .Where(a => a.IsDemo)
            .Select(a => a.Id)
            .ToListAsync();

        if (demoInscanIds.Any())
        {
            var trackings = await context.AWBTrackings
                .Where(t => demoInscanIds.Contains(t.InscanId))
                .ToListAsync();
            context.AWBTrackings.RemoveRange(trackings);
            await context.SaveChangesAsync();

            var items = await context.InscanMasterItems
                .Where(i => demoInscanIds.Contains(i.InscanId))
                .ToListAsync();
            context.InscanMasterItems.RemoveRange(items);
            await context.SaveChangesAsync();
        }

        var demoPickupIds = await context.PickupRequests
            .Where(p => p.IsDemo)
            .Select(p => p.Id)
            .ToListAsync();

        if (demoPickupIds.Any())
        {
            var shipments = await context.PickupRequestShipments
                .Where(s => demoPickupIds.Contains(s.PickupRequestId))
                .ToListAsync();
            context.PickupRequestShipments.RemoveRange(shipments);
            await context.SaveChangesAsync();
        }

        var pickupRequests = await context.PickupRequests.Where(p => p.IsDemo).ToListAsync();
        context.PickupRequests.RemoveRange(pickupRequests);
        await context.SaveChangesAsync();

        var inscanMasters = await context.InscanMasters.Where(a => a.IsDemo).ToListAsync();
        context.InscanMasters.RemoveRange(inscanMasters);
        await context.SaveChangesAsync();

        var demoPartyIds = await context.Parties
            .Where(p => p.IsDemo)
            .Select(p => p.Id)
            .ToListAsync();

        if (demoPartyIds.Any())
        {
            var addresses = await context.PartyAddresses
                .Where(a => demoPartyIds.Contains(a.PartyId))
                .ToListAsync();
            context.PartyAddresses.RemoveRange(addresses);
            await context.SaveChangesAsync();
        }

        var parties = await context.Parties.Where(p => p.IsDemo).ToListAsync();
        context.Parties.RemoveRange(parties);
        await context.SaveChangesAsync();

        return true;
    }
}
