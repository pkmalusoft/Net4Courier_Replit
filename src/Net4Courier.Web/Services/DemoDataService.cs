using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;
using Net4Courier.Masters.Entities;
using Net4Courier.Operations.Entities;
using Net4Courier.Finance.Entities;
using TruebooksPlatform = Truebooks.Platform.Core.Infrastructure;

namespace Net4Courier.Web.Services;

public interface IDemoDataService
{
    Task<DemoDataStats> GetDemoDataStatsAsync();
    Task<AllDataStats> GetAllDataStatsAsync();
    Task<bool> CreateGLDataAsync();
    Task<bool> CreateMasterDataAsync();
    Task<bool> CreateAWBStockDataAsync();
    Task<bool> CreateCRMDataAsync();
    Task<bool> CreateTransactionDataAsync();
    Task<bool> CreateFinanceDataAsync();
    Task<bool> DeleteAllDemoDataAsync();
    Task<bool> DeleteAllDataAsync();
}

public class AllDataStats
{
    public int Parties { get; set; }
    public int Employees { get; set; }
    public int Vehicles { get; set; }
    public int AWBStocks { get; set; }
    public int PrepaidDocuments { get; set; }
    public int Tickets { get; set; }
    public int PickupRequests { get; set; }
    public int AWBs { get; set; }
    public int DRS { get; set; }
    public int Invoices { get; set; }
    public int Receipts { get; set; }
    public int Journals { get; set; }
    public int BankAccounts { get; set; }
    public int BankReconciliations { get; set; }
    public int CashBankTransactions { get; set; }
}

public class DemoDataStats
{
    public int Customers { get; set; }
    public int Vendors { get; set; }
    public int Agents { get; set; }
    public int Couriers { get; set; }
    public int Employees { get; set; }
    public int Vehicles { get; set; }
    public int AWBStocks { get; set; }
    public int PrepaidDocuments { get; set; }
    public int Tickets { get; set; }
    public int PickupRequests { get; set; }
    public int AWBs { get; set; }
    public int DRS { get; set; }
    public int Invoices { get; set; }
    public int Receipts { get; set; }
    public int Journals { get; set; }
    public int BankAccounts { get; set; }
    public int BankReconciliations { get; set; }
    public int ChartOfAccounts { get; set; }
    public int Currencies { get; set; }
    public int TaxCodes { get; set; }
    public int CashBankTransactions { get; set; }
}

public class DemoDataService : IDemoDataService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IDbContextFactory<TruebooksPlatform.PlatformDbContext> _platformDbFactory;
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public DemoDataService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IDbContextFactory<TruebooksPlatform.PlatformDbContext> platformDbFactory)
    {
        _dbFactory = dbFactory;
        _platformDbFactory = platformDbFactory;
    }

    public async Task<DemoDataStats> GetDemoDataStatsAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        await using var platformContext = await _platformDbFactory.CreateDbContextAsync();

        int ticketCount = 0;
        try
        {
            ticketCount = await context.Tickets.CountAsync(t => t.IsDemo);
        }
        catch { }

        return new DemoDataStats
        {
            Customers = await context.Parties.CountAsync(p => p.IsDemo && p.PartyType == PartyType.Customer),
            Vendors = await context.Parties.CountAsync(p => p.IsDemo && p.PartyType == PartyType.Supplier),
            Agents = await context.Parties.CountAsync(p => p.IsDemo && p.PartyType == PartyType.ForwardingAgent),
            Couriers = await context.Parties.CountAsync(p => p.IsDemo && p.PartyType == PartyType.DeliveryAgent),
            Employees = await context.Employees.CountAsync(e => e.IsDemo),
            Vehicles = await context.Vehicles.CountAsync(v => v.IsDemo),
            AWBStocks = await context.AWBStocks.CountAsync(a => a.IsDemo),
            PrepaidDocuments = await context.PrepaidDocuments.CountAsync(p => p.IsDemo),
            Tickets = ticketCount,
            PickupRequests = await context.PickupRequests.CountAsync(p => p.IsDemo),
            AWBs = await context.InscanMasters.CountAsync(a => a.IsDemo),
            DRS = await context.DRSs.CountAsync(d => d.IsDemo),
            Invoices = await context.Invoices.CountAsync(i => i.IsDemo),
            Receipts = await context.Receipts.CountAsync(r => r.IsDemo),
            Journals = await context.Journals.CountAsync(j => j.IsDemo),
            BankAccounts = await context.BankAccounts.CountAsync(b => b.Notes != null && b.Notes.Contains("[DEMO]")),
            BankReconciliations = await context.BankReconciliations.CountAsync(r => r.IsDemo),
            ChartOfAccounts = await platformContext.ChartOfAccounts.CountAsync(c => c.TenantId == DemoTenantId && c.AccountName.StartsWith("DEMO-")),
            Currencies = await platformContext.Currencies.CountAsync(c => c.TenantId == DemoTenantId && c.Name.StartsWith("DEMO-")),
            TaxCodes = await platformContext.TaxCodes.CountAsync(t => t.TenantId == DemoTenantId && t.Description != null && t.Description.StartsWith("DEMO-")),
            CashBankTransactions = await context.CashBankTransactions.CountAsync(c => c.IsDemo)
        };
    }

    public async Task<bool> CreateGLDataAsync()
    {
        await using var platformContext = await _platformDbFactory.CreateDbContextAsync();

        var existingAccounts = await platformContext.ChartOfAccounts
            .Where(c => c.TenantId == DemoTenantId && c.AccountName.StartsWith("DEMO-"))
            .AnyAsync();

        if (existingAccounts)
        {
            return true;
        }

        var now = DateTime.UtcNow;
        var accounts = new List<TruebooksPlatform.ChartOfAccount>();
        var accountMappings = new Dictionary<string, Guid>();

        var chartData = new List<(string Code, string Name, TruebooksPlatform.AccountType Type, bool AllowPosting, string? ParentCode)>
        {
            ("1000", "DEMO-Assets", TruebooksPlatform.AccountType.Asset, false, null),
            ("1100", "DEMO-Cash & Bank", TruebooksPlatform.AccountType.Asset, false, "1000"),
            ("1110", "DEMO-Petty Cash", TruebooksPlatform.AccountType.Asset, true, "1100"),
            ("1120", "DEMO-Bank - Dubai Islamic Bank", TruebooksPlatform.AccountType.Asset, true, "1100"),
            ("1130", "DEMO-Bank - Emirates NBD", TruebooksPlatform.AccountType.Asset, true, "1100"),
            ("1200", "DEMO-Accounts Receivable", TruebooksPlatform.AccountType.Asset, false, "1000"),
            ("1210", "DEMO-Trade Receivables", TruebooksPlatform.AccountType.Asset, true, "1200"),
            ("1220", "DEMO-Staff Advances", TruebooksPlatform.AccountType.Asset, true, "1200"),
            ("1300", "DEMO-Prepaid & Deposits", TruebooksPlatform.AccountType.Asset, false, "1000"),
            ("1310", "DEMO-Prepaid AWB Deposits", TruebooksPlatform.AccountType.Asset, true, "1300"),
            ("1320", "DEMO-Security Deposits", TruebooksPlatform.AccountType.Asset, true, "1300"),
            ("1400", "DEMO-Fixed Assets", TruebooksPlatform.AccountType.Asset, false, "1000"),
            ("1410", "DEMO-Vehicles", TruebooksPlatform.AccountType.Asset, true, "1400"),
            ("1420", "DEMO-Office Equipment", TruebooksPlatform.AccountType.Asset, true, "1400"),
            ("1430", "DEMO-Computer Equipment", TruebooksPlatform.AccountType.Asset, true, "1400"),

            ("2000", "DEMO-Liabilities", TruebooksPlatform.AccountType.Liability, false, null),
            ("2100", "DEMO-Accounts Payable", TruebooksPlatform.AccountType.Liability, false, "2000"),
            ("2110", "DEMO-Trade Payables", TruebooksPlatform.AccountType.Liability, true, "2100"),
            ("2120", "DEMO-Agent Payables", TruebooksPlatform.AccountType.Liability, true, "2100"),
            ("2200", "DEMO-Accrued Expenses", TruebooksPlatform.AccountType.Liability, false, "2000"),
            ("2210", "DEMO-Accrued Salaries", TruebooksPlatform.AccountType.Liability, true, "2200"),
            ("2220", "DEMO-Accrued Rent", TruebooksPlatform.AccountType.Liability, true, "2200"),
            ("2300", "DEMO-Customer Deposits", TruebooksPlatform.AccountType.Liability, false, "2000"),
            ("2310", "DEMO-COD Payable to Customers", TruebooksPlatform.AccountType.Liability, true, "2300"),
            ("2320", "DEMO-Prepaid Balance Liability", TruebooksPlatform.AccountType.Liability, true, "2300"),
            ("2400", "DEMO-Tax Liabilities", TruebooksPlatform.AccountType.Liability, false, "2000"),
            ("2410", "DEMO-VAT Payable", TruebooksPlatform.AccountType.Liability, true, "2400"),

            ("3000", "DEMO-Equity", TruebooksPlatform.AccountType.Equity, false, null),
            ("3100", "DEMO-Capital", TruebooksPlatform.AccountType.Equity, false, "3000"),
            ("3110", "DEMO-Owner's Capital", TruebooksPlatform.AccountType.Equity, true, "3100"),
            ("3200", "DEMO-Retained Earnings", TruebooksPlatform.AccountType.Equity, false, "3000"),
            ("3210", "DEMO-Retained Earnings Account", TruebooksPlatform.AccountType.Equity, true, "3200"),
            ("3220", "DEMO-Current Year Earnings", TruebooksPlatform.AccountType.Equity, true, "3200"),

            ("4000", "DEMO-Revenue", TruebooksPlatform.AccountType.Revenue, false, null),
            ("4100", "DEMO-Courier Income", TruebooksPlatform.AccountType.Revenue, false, "4000"),
            ("4110", "DEMO-Domestic Courier Revenue", TruebooksPlatform.AccountType.Revenue, true, "4100"),
            ("4120", "DEMO-International Courier Revenue", TruebooksPlatform.AccountType.Revenue, true, "4100"),
            ("4130", "DEMO-Express Delivery Revenue", TruebooksPlatform.AccountType.Revenue, true, "4100"),
            ("4200", "DEMO-Other Income", TruebooksPlatform.AccountType.Revenue, false, "4000"),
            ("4210", "DEMO-COD Collection Fees", TruebooksPlatform.AccountType.Revenue, true, "4200"),
            ("4220", "DEMO-Insurance Fees", TruebooksPlatform.AccountType.Revenue, true, "4200"),
            ("4230", "DEMO-Packaging Income", TruebooksPlatform.AccountType.Revenue, true, "4200"),

            ("5000", "DEMO-Expenses", TruebooksPlatform.AccountType.Expense, false, null),
            ("5100", "DEMO-Operations", TruebooksPlatform.AccountType.Expense, false, "5000"),
            ("5110", "DEMO-Fuel & Transport", TruebooksPlatform.AccountType.Expense, true, "5100"),
            ("5120", "DEMO-Vehicle Maintenance", TruebooksPlatform.AccountType.Expense, true, "5100"),
            ("5130", "DEMO-Courier Subcontracting", TruebooksPlatform.AccountType.Expense, true, "5100"),
            ("5200", "DEMO-Staff Costs", TruebooksPlatform.AccountType.Expense, false, "5000"),
            ("5210", "DEMO-Salaries & Wages", TruebooksPlatform.AccountType.Expense, true, "5200"),
            ("5220", "DEMO-Staff Benefits", TruebooksPlatform.AccountType.Expense, true, "5200"),
            ("5230", "DEMO-Overtime", TruebooksPlatform.AccountType.Expense, true, "5200"),
            ("5300", "DEMO-Admin Expenses", TruebooksPlatform.AccountType.Expense, false, "5000"),
            ("5310", "DEMO-Rent", TruebooksPlatform.AccountType.Expense, true, "5300"),
            ("5320", "DEMO-Utilities", TruebooksPlatform.AccountType.Expense, true, "5300"),
            ("5330", "DEMO-Office Supplies", TruebooksPlatform.AccountType.Expense, true, "5300"),
            ("5340", "DEMO-Communication", TruebooksPlatform.AccountType.Expense, true, "5300")
        };

        foreach (var (code, name, type, allowPosting, parentCode) in chartData)
        {
            var id = Guid.NewGuid();
            accountMappings[code] = id;
            accounts.Add(new TruebooksPlatform.ChartOfAccount
            {
                Id = id,
                TenantId = DemoTenantId,
                AccountCode = code,
                AccountName = name,
                AccountType = type,
                AllowPosting = allowPosting,
                IsActive = true,
                IsSystemAccount = false,
                CreatedAt = now
            });
        }

        foreach (var account in accounts)
        {
            var chartItem = chartData.First(c => c.Code == account.AccountCode);
            if (chartItem.ParentCode != null && accountMappings.TryGetValue(chartItem.ParentCode, out var parentId))
            {
                account.ParentAccountId = parentId;
            }
        }

        platformContext.ChartOfAccounts.AddRange(accounts);

        var currencies = new List<TruebooksPlatform.Currency>
        {
            new TruebooksPlatform.Currency { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "AED", Name = "DEMO-UAE Dirham", Symbol = "AED", DecimalPlaces = 2, IsBaseCurrency = true, ExchangeRate = 1m, IsActive = true, CreatedAt = now },
            new TruebooksPlatform.Currency { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "USD", Name = "DEMO-US Dollar", Symbol = "$", DecimalPlaces = 2, IsBaseCurrency = false, ExchangeRate = 3.67m, IsActive = true, CreatedAt = now },
            new TruebooksPlatform.Currency { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "INR", Name = "DEMO-Indian Rupee", Symbol = "₹", DecimalPlaces = 2, IsBaseCurrency = false, ExchangeRate = 0.044m, IsActive = true, CreatedAt = now },
            new TruebooksPlatform.Currency { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "SAR", Name = "DEMO-Saudi Riyal", Symbol = "SAR", DecimalPlaces = 2, IsBaseCurrency = false, ExchangeRate = 0.98m, IsActive = true, CreatedAt = now }
        };
        platformContext.Currencies.AddRange(currencies);

        var taxCodes = new List<TruebooksPlatform.TaxCode>
        {
            new TruebooksPlatform.TaxCode { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "VAT5", Description = "DEMO-5% VAT", Rate = 5m, IsActive = true, CreatedAt = now },
            new TruebooksPlatform.TaxCode { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "VAT0", Description = "DEMO-Zero Rated", Rate = 0m, IsActive = true, CreatedAt = now },
            new TruebooksPlatform.TaxCode { Id = Guid.NewGuid(), TenantId = DemoTenantId, Code = "EXEMPT", Description = "DEMO-Exempt from VAT", Rate = 0m, IsActive = true, CreatedAt = now }
        };
        platformContext.TaxCodes.AddRange(taxCodes);

        await platformContext.SaveChangesAsync();
        return true;
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

        var vendorNames = new[]
        {
            "Quick Transport LLC",
            "Al Noor Fuel Station",
            "Emirates Vehicle Services"
        };

        for (int i = 0; i < 3; i++)
        {
            var vendor = new Party
            {
                CompanyId = 1,
                Code = $"DEMO-VEND-{(i + 1):D3}",
                Name = vendorNames[i],
                PartyType = PartyType.Supplier,
                AccountNature = PartyAccountNature.Payable,
                ContactPerson = $"Vendor Contact {i + 1}",
                Phone = $"+971 4 {200 + i:D3} {3000 + i:D4}",
                Mobile = $"+971 55 {600 + i:D3} {4000 + i:D4}",
                Email = $"vendor{i + 1}@example.com",
                ClientAddress = $"Vendor Address {i + 1}, Dubai Industrial Area",
                CreditLimit = 50000 + (i * 10000),
                CreditDays = 45,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            parties.Add(vendor);
        }

        var agentNames = new[]
        {
            "India Express Logistics",
            "Saudi Cargo Partners"
        };

        for (int i = 0; i < 2; i++)
        {
            var agent = new Party
            {
                CompanyId = 1,
                Code = $"DEMO-AGNT-{(i + 1):D3}",
                Name = agentNames[i],
                PartyType = PartyType.ForwardingAgent,
                AccountNature = PartyAccountNature.Payable,
                ContactPerson = $"Agent Contact {i + 1}",
                Phone = $"+{(i == 0 ? "91 22" : "966 11")} {300 + i:D3} {5000 + i:D4}",
                Mobile = $"+{(i == 0 ? "91 98" : "966 50")} {700 + i:D3} {6000 + i:D4}",
                Email = $"agent{i + 1}@example.com",
                ClientAddress = i == 0 ? "Mumbai, India" : "Riyadh, Saudi Arabia",
                CreditDays = 60,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            parties.Add(agent);
        }

        var courierNames = new[]
        {
            "Speedex Courier Services",
            "FastTrack Delivery",
            "Reliable Express"
        };

        for (int i = 0; i < 3; i++)
        {
            var courier = new Party
            {
                CompanyId = 1,
                Code = $"DEMO-COUR-{(i + 1):D3}",
                Name = courierNames[i],
                PartyType = PartyType.DeliveryAgent,
                AccountNature = PartyAccountNature.Payable,
                ContactPerson = $"Courier Contact {i + 1}",
                Phone = $"+971 4 {400 + i:D3} {7000 + i:D4}",
                Mobile = $"+971 52 {800 + i:D3} {8000 + i:D4}",
                Email = $"courier{i + 1}@example.com",
                ClientAddress = $"Courier Hub {i + 1}, Dubai",
                CreditDays = 30,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            parties.Add(courier);
        }

        context.Parties.AddRange(parties);
        await context.SaveChangesAsync();

        foreach (var party in parties.Where(p => p.PartyType == PartyType.Customer))
        {
            var idx = parties.IndexOf(party);
            if (idx < uaeAddresses.Length)
            {
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
        }

        var employeeData = new[]
        {
            new { Code = "DEMO-EMP-001", Name = "Ahmed Al Rashid", Role = "Driver", Phone = "+971 50 111 1111" },
            new { Code = "DEMO-EMP-002", Name = "Mohammed Hassan", Role = "Driver", Phone = "+971 50 222 2222" },
            new { Code = "DEMO-EMP-003", Name = "Khalid Ibrahim", Role = "Driver", Phone = "+971 50 333 3333" },
            new { Code = "DEMO-EMP-004", Name = "Sara Ahmed", Role = "Dispatcher", Phone = "+971 50 444 4444" },
            new { Code = "DEMO-EMP-005", Name = "Fatima Khan", Role = "Dispatcher", Phone = "+971 50 555 5555" }
        };

        foreach (var emp in employeeData)
        {
            var employee = new Employee
            {
                Code = emp.Code,
                Name = emp.Name,
                FirstName = emp.Name.Split(' ')[0],
                LastName = emp.Name.Contains(' ') ? emp.Name.Split(' ')[1] : null,
                Mobile = emp.Phone,
                Email = $"{emp.Code.ToLower().Replace("-", "")}@example.com",
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddMonths(-6),
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Employees.Add(employee);
        }

        var vehicleData = new[]
        {
            new { VehicleNo = "DEMO-DXB-1234", Type = "Van", Make = "Toyota", Model = "Hiace", Capacity = 1500m },
            new { VehicleNo = "DEMO-DXB-5678", Type = "Motorcycle", Make = "Honda", Model = "PCX", Capacity = 50m },
            new { VehicleNo = "DEMO-DXB-9012", Type = "Truck", Make = "Isuzu", Model = "NPR", Capacity = 3000m }
        };

        foreach (var veh in vehicleData)
        {
            var vehicle = new Vehicle
            {
                CompanyId = 1,
                BranchId = 1,
                VehicleNo = veh.VehicleNo,
                VehicleType = veh.Type,
                Make = veh.Make,
                Model = veh.Model,
                Year = 2023,
                Capacity = veh.Capacity,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Vehicles.Add(vehicle);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateAWBStockDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var awbStocks = new[]
        {
            new AWBStock
            {
                CompanyId = 1,
                BranchId = 1,
                StockDate = DateTime.UtcNow.AddDays(-30),
                ReferenceNo = "DEMO-STOCK-001",
                ItemName = "AWB Booklet - Domestic",
                ItemType = "Domestic",
                Qty = 100,
                Rate = 5m,
                Amount = 500m,
                AWBCount = 100,
                AWBNoFrom = "DEM1000001",
                AWBNoTo = "DEM1000100",
                AllocatedCount = 10,
                AvailableCount = 90,
                Status = StockStatus.PartiallyAllocated,
                Remarks = "Demo stock for domestic shipments",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            },
            new AWBStock
            {
                CompanyId = 1,
                BranchId = 1,
                StockDate = DateTime.UtcNow.AddDays(-25),
                ReferenceNo = "DEMO-STOCK-002",
                ItemName = "AWB Booklet - International",
                ItemType = "International",
                Qty = 50,
                Rate = 10m,
                Amount = 500m,
                AWBCount = 50,
                AWBNoFrom = "DEM2000001",
                AWBNoTo = "DEM2000050",
                AllocatedCount = 5,
                AvailableCount = 45,
                Status = StockStatus.PartiallyAllocated,
                Remarks = "Demo stock for international shipments",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AWBStocks.AddRange(awbStocks);

        var demoCustomers = await context.Parties
            .Where(p => p.IsDemo && p.PartyType == PartyType.Customer)
            .OrderBy(p => p.Id)
            .Take(2)
            .ToListAsync();

        if (demoCustomers.Count >= 2)
        {
            var prepaidDocs = new[]
            {
                new PrepaidDocument
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentNo = "DEMO-PREP-001",
                    DocumentDate = DateTime.UtcNow.AddDays(-20),
                    CustomerId = demoCustomers[0].Id,
                    CustomerName = demoCustomers[0].Name,
                    Origin = "Dubai",
                    Destination = "Abu Dhabi",
                    NoOfAWBs = 10,
                    CourierCharge = 50m,
                    AWBNoFrom = "DPRP100001",
                    AWBNoTo = "DPRP100010",
                    PaymentMode = PrepaidPaymentMode.Bank,
                    TotalPrepaidAmount = 500m,
                    UsedAmount = 100m,
                    BalanceAmount = 400m,
                    Status = PrepaidDocumentStatus.PartiallyUsed,
                    Remarks = "Demo prepaid document 1",
                    IsActive = true,
                    IsDemo = true,
                    CreatedAt = DateTime.UtcNow
                },
                new PrepaidDocument
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentNo = "DEMO-PREP-002",
                    DocumentDate = DateTime.UtcNow.AddDays(-15),
                    CustomerId = demoCustomers[1].Id,
                    CustomerName = demoCustomers[1].Name,
                    Origin = "Dubai",
                    Destination = "Sharjah",
                    NoOfAWBs = 5,
                    CourierCharge = 30m,
                    AWBNoFrom = "DPRP200001",
                    AWBNoTo = "DPRP200005",
                    PaymentMode = PrepaidPaymentMode.Cash,
                    TotalPrepaidAmount = 150m,
                    UsedAmount = 0m,
                    BalanceAmount = 150m,
                    Status = PrepaidDocumentStatus.Active,
                    Remarks = "Demo prepaid document 2",
                    IsActive = true,
                    IsDemo = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.PrepaidDocuments.AddRange(prepaidDocs);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateCRMDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var existingCategories = await context.TicketCategories.AnyAsync(c => c.Name.StartsWith("DEMO-"));
        if (!existingCategories)
        {
            var categories = new List<TicketCategory>
            {
                new TicketCategory { Name = "DEMO-Delivery Issue", Description = "Issues related to delivery", Icon = "mdi-truck-delivery", Color = "#FF5722", SortOrder = 1, IsActive = true, IsDemo = true },
                new TicketCategory { Name = "DEMO-Billing Query", Description = "Billing and invoice queries", Icon = "mdi-receipt", Color = "#2196F3", SortOrder = 2, IsActive = true, IsDemo = true },
                new TicketCategory { Name = "DEMO-Tracking", Description = "Shipment tracking issues", Icon = "mdi-map-marker-path", Color = "#4CAF50", SortOrder = 3, IsActive = true, IsDemo = true },
                new TicketCategory { Name = "DEMO-Damage/Loss", Description = "Damaged or lost shipments", Icon = "mdi-package-variant-remove", Color = "#F44336", SortOrder = 4, IsActive = true, IsDemo = true },
                new TicketCategory { Name = "DEMO-General Inquiry", Description = "General questions", Icon = "mdi-help-circle", Color = "#9C27B0", SortOrder = 5, IsActive = true, IsDemo = true }
            };
            context.TicketCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        var demoCategories = await context.TicketCategories
            .Where(c => c.Name.StartsWith("DEMO-"))
            .ToListAsync();

        var demoCustomers = await context.Parties
            .Where(p => p.IsDemo && p.PartyType == PartyType.Customer)
            .OrderBy(p => p.Id)
            .Take(5)
            .ToListAsync();

        var demoAWBs = await context.InscanMasters
            .Where(a => a.IsDemo)
            .OrderBy(a => a.Id)
            .Take(5)
            .ToListAsync();

        var ticketData = new[]
        {
            new { Subject = "Delayed Delivery", Status = TicketStatus.Open, Priority = TicketPriority.High },
            new { Subject = "Invoice Discrepancy", Status = TicketStatus.InProgress, Priority = TicketPriority.Medium },
            new { Subject = "Tracking Not Updating", Status = TicketStatus.PendingCustomer, Priority = TicketPriority.Low },
            new { Subject = "Package Damaged", Status = TicketStatus.Resolved, Priority = TicketPriority.Urgent },
            new { Subject = "Rate Inquiry", Status = TicketStatus.Closed, Priority = TicketPriority.Low }
        };

        for (int i = 0; i < 5; i++)
        {
            var category = demoCategories.Count > i ? demoCategories[i] : demoCategories.FirstOrDefault();
            var customer = demoCustomers.Count > i ? demoCustomers[i] : demoCustomers.FirstOrDefault();
            var awb = demoAWBs.Count > i ? demoAWBs[i] : null;

            var ticket = new Ticket
            {
                TicketNo = $"DEMO-TKT-{(i + 1):D4}",
                Subject = $"DEMO - {ticketData[i].Subject}",
                Description = $"Demo ticket for testing: {ticketData[i].Subject}. This is auto-generated demo data.",
                CategoryId = category?.Id ?? 0,
                CategoryName = category?.Name,
                PartyId = customer?.Id ?? 0,
                PartyName = customer?.Name,
                AWBId = awb?.Id,
                AWBNo = awb?.AWBNo,
                Status = ticketData[i].Status,
                Priority = ticketData[i].Priority,
                BranchId = 1,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow.AddDays(-i - 1)
            };

            if (ticketData[i].Status == TicketStatus.Resolved || ticketData[i].Status == TicketStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.UtcNow.AddHours(-12);
                ticket.ResolutionNotes = "Demo resolution - issue addressed";
            }

            if (ticketData[i].Status == TicketStatus.Closed)
            {
                ticket.ClosedAt = DateTime.UtcNow.AddHours(-6);
                ticket.CustomerRating = 4;
                ticket.CustomerFeedback = "Good service";
            }

            context.Tickets.Add(ticket);
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

        var demoEmployees = await context.Employees
            .Where(e => e.IsDemo)
            .OrderBy(e => e.Id)
            .Take(3)
            .ToListAsync();

        var demoVehicles = await context.Vehicles
            .Where(v => v.IsDemo)
            .OrderBy(v => v.Id)
            .Take(3)
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
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.ParcelUpto30Kg,
                Status = ShipmentLineStatus.Booked,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.PickupRequestShipments.Add(pickupShipment);
            await context.SaveChangesAsync();

            var courierCharge = 50 + (i * 10);
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
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.ParcelUpto30Kg,
                CourierCharge = courierCharge,
                NetTotal = courierCharge,
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

            var employee = demoEmployees.Count > (i % 3) ? demoEmployees[i % 3] : null;
            var vehicle = demoVehicles.Count > (i % 3) ? demoVehicles[i % 3] : null;

            var drs = new DRS
            {
                DRSNo = $"DEMO-DRS-{(i + 1):D3}",
                DRSDate = pickupDate.AddHours(18),
                CompanyId = 1,
                BranchId = 1,
                DeliveryEmployeeId = employee != null ? (int)employee.Id : null,
                DeliveryEmployeeName = employee?.Name,
                VehicleId = vehicle != null ? (int)vehicle.Id : null,
                VehicleNo = vehicle?.VehicleNo,
                TotalAWBs = 1,
                DeliveredCount = 1,
                PendingCount = 0,
                ReturnedCount = 0,
                TotalCOD = 0,
                CollectedCOD = 0,
                TotalCourierCharges = courierCharge,
                Status = DRSStatus.Closed,
                ClosedAt = pickupDate.AddDays(1),
                Remarks = "Demo DRS completed",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.DRSs.Add(drs);
            await context.SaveChangesAsync();

            var drsDetail = new DRSDetail
            {
                DRSId = drs.Id,
                InscanId = awb.Id,
                Sequence = 1,
                AttemptNo = 1,
                Status = "Delivered",
                Remarks = "Successfully delivered",
                AttemptedAt = pickupDate.AddHours(22),
                CODAmount = 0,
                CollectedAmount = 0,
                ReceivedBy = consigneeData[i].Contact,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.DRSDetails.Add(drsDetail);
            await context.SaveChangesAsync();

            var invoice = new Invoice
            {
                InvoiceNo = $"DEMO-INV-{(i + 1):D4}",
                InvoiceDate = pickupDate.AddDays(2),
                CompanyId = 1,
                BranchId = 1,
                CustomerId = customerId,
                CustomerName = demoCustomers.Count > i ? demoCustomers[i].Name : consignorData[i].Name,
                TotalAWBs = 1,
                SubTotal = courierCharge,
                TaxPercent = 5,
                TaxAmount = courierCharge * 0.05m,
                NetTotal = courierCharge * 1.05m,
                PaidAmount = i < 3 ? courierCharge * 1.05m : 0,
                BalanceAmount = i < 3 ? 0 : courierCharge * 1.05m,
                Status = i < 3 ? InvoiceStatus.Paid : InvoiceStatus.Generated,
                DueDate = pickupDate.AddDays(32),
                Remarks = "Demo invoice",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var invoiceDetail = new InvoiceDetail
            {
                InvoiceId = invoice.Id,
                InscanId = awb.Id,
                AWBNo = awb.AWBNo,
                AWBDate = awb.TransactionDate,
                Origin = consignorData[i].City,
                Destination = consigneeData[i].City,
                Pieces = awb.Pieces,
                Weight = awb.Weight,
                CourierCharge = courierCharge,
                TaxAmount = courierCharge * 0.05m,
                Total = courierCharge * 1.05m,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.InvoiceDetails.Add(invoiceDetail);

            if (i < 3)
            {
                var receipt = new Receipt
                {
                    ReceiptNo = $"DEMO-RCT-{(i + 1):D4}",
                    ReceiptDate = pickupDate.AddDays(5),
                    CompanyId = 1,
                    BranchId = 1,
                    CustomerId = customerId,
                    CustomerName = demoCustomers.Count > i ? demoCustomers[i].Name : consignorData[i].Name,
                    Amount = courierCharge * 1.05m,
                    PaymentMode = i == 0 ? "Cash" : (i == 1 ? "Bank Transfer" : "Cheque"),
                    BankName = i > 0 ? "Emirates NBD" : null,
                    TransactionRef = i == 1 ? $"TRF-{100000 + i}" : null,
                    ChequeNo = i == 2 ? $"CHQ-{200000 + i}" : null,
                    ChequeDate = i == 2 ? pickupDate.AddDays(3) : null,
                    Remarks = "Demo receipt",
                    IsAllocated = true,
                    IsActive = true,
                    IsDemo = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Receipts.Add(receipt);
                await context.SaveChangesAsync();

                var allocation = new ReceiptAllocation
                {
                    ReceiptId = receipt.Id,
                    InvoiceId = invoice.Id,
                    AllocatedAmount = courierCharge * 1.05m,
                    Remarks = "Auto allocated",
                    IsActive = true,
                    IsDemo = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.ReceiptAllocations.Add(allocation);
            }

            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> CreateFinanceDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var accountHeads = await context.AccountHeads
            .Where(a => a.IsActive)
            .Take(10)
            .ToListAsync();

        if (accountHeads.Count < 2)
        {
            return true;
        }

        for (int i = 0; i < 3; i++)
        {
            var journal = new Journal
            {
                VoucherNo = $"DEMO-JV-{(i + 1):D4}",
                VoucherDate = DateTime.UtcNow.AddDays(-10 + i),
                CompanyId = 1,
                BranchId = 1,
                VoucherType = "JV",
                Narration = $"Demo journal entry {i + 1} - Manual adjustment",
                TotalDebit = 1000 + (i * 500),
                TotalCredit = 1000 + (i * 500),
                IsPosted = true,
                PostedAt = DateTime.UtcNow.AddDays(-9 + i),
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Journals.Add(journal);
            await context.SaveChangesAsync();

            var debitAccount = accountHeads[i % accountHeads.Count];
            var creditAccount = accountHeads[(i + 1) % accountHeads.Count];

            var debitEntry = new Net4Courier.Finance.Entities.JournalEntry
            {
                JournalId = journal.Id,
                AccountHeadId = debitAccount.Id,
                AccountCode = debitAccount.Code,
                AccountName = debitAccount.Name,
                Debit = 1000 + (i * 500),
                Credit = 0,
                Narration = "Demo debit entry",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            var creditEntry = new Net4Courier.Finance.Entities.JournalEntry
            {
                JournalId = journal.Id,
                AccountHeadId = creditAccount.Id,
                AccountCode = creditAccount.Code,
                AccountName = creditAccount.Name,
                Debit = 0,
                Credit = 1000 + (i * 500),
                Narration = "Demo credit entry",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.JournalEntries.AddRange(debitEntry, creditEntry);
        }

        await context.SaveChangesAsync();

        var cashAccountHead = accountHeads.FirstOrDefault(a => a.Name?.Contains("Cash") == true) ?? accountHeads[0];

        for (int i = 0; i < 3; i++)
        {
            var cashTransaction = new Net4Courier.Finance.Entities.CashBankTransaction
            {
                VoucherNo = $"DEMO-CR-{(i + 1):D4}",
                VoucherDate = DateTime.UtcNow.AddDays(-5 + i),
                TransactionType = TransactionType.Cash,
                RecPayType = RecPayType.Receipt,
                TransactionCategory = TransactionCategory.GL,
                SourceAccountId = cashAccountHead.Id,
                TotalAmount = 500 + (i * 250),
                Status = CashBankStatus.Posted,
                PostedDate = DateTime.UtcNow.AddDays(-4 + i),
                Narration = $"Demo cash receipt {i + 1}",
                FiscalYear = DateTime.UtcNow.Year,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.CashBankTransactions.Add(cashTransaction);
        }

        for (int i = 0; i < 3; i++)
        {
            var bankTransaction = new Net4Courier.Finance.Entities.CashBankTransaction
            {
                VoucherNo = $"DEMO-BR-{(i + 1):D4}",
                VoucherDate = DateTime.UtcNow.AddDays(-3 + i),
                TransactionType = TransactionType.Bank,
                RecPayType = i < 2 ? RecPayType.Receipt : RecPayType.Payment,
                TransactionCategory = TransactionCategory.GL,
                SourceAccountId = accountHeads.Count > 1 ? accountHeads[1].Id : accountHeads[0].Id,
                TotalAmount = 2000 + (i * 1000),
                BankName = "Emirates NBD",
                BranchName = "Dubai Main Branch",
                ReferenceNo = $"REF-{300000 + i}",
                Status = CashBankStatus.Posted,
                PostedDate = DateTime.UtcNow.AddDays(-2 + i),
                Narration = $"Demo bank {(i < 2 ? "receipt" : "payment")} {i + 1}",
                FiscalYear = DateTime.UtcNow.Year,
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.CashBankTransactions.Add(bankTransaction);
        }

        await context.SaveChangesAsync();

        var demoBankAccounts = await context.BankAccounts
            .Where(b => b.Notes != null && b.Notes.Contains("[DEMO]"))
            .ToListAsync();

        if (!demoBankAccounts.Any())
        {
            var bankAccount = new Net4Courier.Finance.Entities.BankAccount
            {
                AccountNumber = "DEMO-1234567890",
                AccountName = "DEMO Operations Account",
                BankName = "Emirates NBD",
                BranchName = "Dubai Main Branch",
                SwiftCode = "EABORAEDXXX",
                IbanNumber = "AE070331234567890123456",
                OpeningBalance = 50000m,
                OpeningBalanceDate = DateTime.UtcNow.AddMonths(-6).Date,
                Notes = "[DEMO] Demo bank account for testing",
                IsActive = true,
                AccountHeadId = accountHeads.Count > 1 ? accountHeads[1].Id : accountHeads[0].Id,
                CompanyId = 1,
                BranchId = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.BankAccounts.Add(bankAccount);
            await context.SaveChangesAsync();

            var reconciliation = new Net4Courier.Finance.Entities.BankReconciliation
            {
                CompanyId = 1,
                BranchId = 1,
                BankAccountId = bankAccount.Id,
                ReconciliationNumber = "DEMO-RECON-001",
                StatementDate = DateTime.UtcNow.AddDays(-1).Date,
                StatementOpeningBalance = 50000m,
                StatementClosingBalance = 55000m,
                BookOpeningBalance = 50000m,
                BookClosingBalance = 54500m,
                DifferenceAmount = 500m,
                Status = ReconciliationStatus.Draft,
                Notes = "Demo reconciliation",
                IsActive = true,
                IsDemo = true,
                CreatedAt = DateTime.UtcNow
            };

            context.BankReconciliations.Add(reconciliation);
            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAllDemoDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var demoBankReconciliationIds = await context.BankReconciliations
            .Where(r => r.IsDemo)
            .Select(r => r.Id)
            .ToListAsync();

        if (demoBankReconciliationIds.Any())
        {
            var matches = await context.ReconciliationMatches
                .Where(m => demoBankReconciliationIds.Contains(m.BankReconciliationId))
                .ToListAsync();
            context.ReconciliationMatches.RemoveRange(matches);

            var adjustments = await context.ReconciliationAdjustments
                .Where(a => demoBankReconciliationIds.Contains(a.BankReconciliationId))
                .ToListAsync();
            context.ReconciliationAdjustments.RemoveRange(adjustments);

            var statementImports = await context.BankStatementImports
                .Where(s => demoBankReconciliationIds.Contains(s.BankReconciliationId))
                .ToListAsync();

            foreach (var import in statementImports)
            {
                var lines = await context.BankStatementLines
                    .Where(l => l.BankStatementImportId == import.Id)
                    .ToListAsync();
                context.BankStatementLines.RemoveRange(lines);
            }

            context.BankStatementImports.RemoveRange(statementImports);

            var reconciliations = await context.BankReconciliations.Where(r => r.IsDemo).ToListAsync();
            context.BankReconciliations.RemoveRange(reconciliations);
            await context.SaveChangesAsync();
        }

        var demoBankAccounts = await context.BankAccounts
            .Where(b => b.Notes != null && b.Notes.Contains("[DEMO]"))
            .ToListAsync();
        context.BankAccounts.RemoveRange(demoBankAccounts);
        await context.SaveChangesAsync();

        var demoCashBankTransactions = await context.CashBankTransactions.Where(c => c.IsDemo).ToListAsync();
        foreach (var transaction in demoCashBankTransactions)
        {
            var lines = await context.CashBankTransactionLines
                .Where(l => l.CashBankTransactionId == transaction.Id)
                .ToListAsync();
            context.CashBankTransactionLines.RemoveRange(lines);

            var attachments = await context.VoucherAttachments
                .Where(a => a.CashBankTransactionId == transaction.Id)
                .ToListAsync();
            context.VoucherAttachments.RemoveRange(attachments);
        }
        context.CashBankTransactions.RemoveRange(demoCashBankTransactions);
        await context.SaveChangesAsync();

        var demoJournalIds = await context.Journals
            .Where(j => j.IsDemo)
            .Select(j => j.Id)
            .ToListAsync();

        if (demoJournalIds.Any())
        {
            var journalEntries = await context.JournalEntries
                .Where(e => demoJournalIds.Contains(e.JournalId))
                .ToListAsync();
            context.JournalEntries.RemoveRange(journalEntries);
        }

        var journals = await context.Journals.Where(j => j.IsDemo).ToListAsync();
        context.Journals.RemoveRange(journals);
        await context.SaveChangesAsync();

        var demoReceiptIds = await context.Receipts
            .Where(r => r.IsDemo)
            .Select(r => r.Id)
            .ToListAsync();

        if (demoReceiptIds.Any())
        {
            var allocations = await context.ReceiptAllocations
                .Where(a => demoReceiptIds.Contains(a.ReceiptId))
                .ToListAsync();
            context.ReceiptAllocations.RemoveRange(allocations);
        }

        var receipts = await context.Receipts.Where(r => r.IsDemo).ToListAsync();
        context.Receipts.RemoveRange(receipts);
        await context.SaveChangesAsync();

        var demoInvoiceIds = await context.Invoices
            .Where(i => i.IsDemo)
            .Select(i => i.Id)
            .ToListAsync();

        if (demoInvoiceIds.Any())
        {
            var invoiceDetails = await context.InvoiceDetails
                .Where(d => demoInvoiceIds.Contains(d.InvoiceId))
                .ToListAsync();
            context.InvoiceDetails.RemoveRange(invoiceDetails);

            var specialCharges = await context.InvoiceSpecialCharges
                .Where(s => demoInvoiceIds.Contains(s.InvoiceId))
                .ToListAsync();
            context.InvoiceSpecialCharges.RemoveRange(specialCharges);
        }

        var invoices = await context.Invoices.Where(i => i.IsDemo).ToListAsync();
        context.Invoices.RemoveRange(invoices);
        await context.SaveChangesAsync();

        var demoDRSIds = await context.DRSs
            .Where(d => d.IsDemo)
            .Select(d => d.Id)
            .ToListAsync();

        if (demoDRSIds.Any())
        {
            var drsDetails = await context.DRSDetails
                .Where(d => demoDRSIds.Contains(d.DRSId))
                .ToListAsync();
            context.DRSDetails.RemoveRange(drsDetails);

            var cashSubmissions = await context.CourierCashSubmissions
                .Where(c => demoDRSIds.Contains(c.DRSId))
                .ToListAsync();
            context.CourierCashSubmissions.RemoveRange(cashSubmissions);

            var expenses = await context.CourierExpenses
                .Where(e => demoDRSIds.Contains(e.DRSId))
                .ToListAsync();
            context.CourierExpenses.RemoveRange(expenses);
        }

        var drss = await context.DRSs.Where(d => d.IsDemo).ToListAsync();
        context.DRSs.RemoveRange(drss);
        await context.SaveChangesAsync();

        var tickets = await context.Tickets.Where(t => t.IsDemo).ToListAsync();
        foreach (var ticket in tickets)
        {
            var comments = await context.TicketComments
                .Where(c => c.TicketId == ticket.Id)
                .ToListAsync();
            context.TicketComments.RemoveRange(comments);
        }
        context.Tickets.RemoveRange(tickets);

        var ticketCategories = await context.TicketCategories
            .Where(c => c.Name.StartsWith("DEMO-"))
            .ToListAsync();
        context.TicketCategories.RemoveRange(ticketCategories);
        await context.SaveChangesAsync();

        var demoImportMasterIds = await context.ImportMasters
            .Where(m => m.IsDemo)
            .Select(m => m.Id)
            .ToListAsync();

        var demoImportShipmentIds = await context.ImportShipments
            .Where(s => s.IsDemo)
            .Select(s => s.Id)
            .ToListAsync();

        if (demoImportShipmentIds.Any())
        {
            var importNotes = await context.ImportShipmentNotes
                .Where(n => demoImportShipmentIds.Contains(n.ImportShipmentId))
                .ToListAsync();
            context.ImportShipmentNotes.RemoveRange(importNotes);
            await context.SaveChangesAsync();
        }

        if (demoImportMasterIds.Any())
        {
            var importDocs = await context.ImportDocuments
                .Where(d => demoImportMasterIds.Contains(d.ImportMasterId))
                .ToListAsync();
            context.ImportDocuments.RemoveRange(importDocs);
            await context.SaveChangesAsync();
        }

        var importShipments = await context.ImportShipments.Where(s => s.IsDemo).ToListAsync();
        context.ImportShipments.RemoveRange(importShipments);
        await context.SaveChangesAsync();

        var importBags = await context.ImportBags.Where(b => b.IsDemo).ToListAsync();
        context.ImportBags.RemoveRange(importBags);
        await context.SaveChangesAsync();

        var importMasters = await context.ImportMasters.Where(m => m.IsDemo).ToListAsync();
        context.ImportMasters.RemoveRange(importMasters);
        await context.SaveChangesAsync();

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

            var shipmentNotes = await context.ShipmentNotes
                .Where(n => demoInscanIds.Contains(n.ShipmentId))
                .ToListAsync();
            foreach (var note in shipmentNotes)
            {
                var mentions = await context.ShipmentNoteMentions
                    .Where(m => m.NoteId == note.Id)
                    .ToListAsync();
                context.ShipmentNoteMentions.RemoveRange(mentions);
            }
            context.ShipmentNotes.RemoveRange(shipmentNotes);
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

        var prepaidDocs = await context.PrepaidDocuments.Where(p => p.IsDemo).ToListAsync();
        foreach (var doc in prepaidDocs)
        {
            var awbs = await context.PrepaidAWBs
                .Where(a => a.PrepaidDocumentId == doc.Id)
                .ToListAsync();
            context.PrepaidAWBs.RemoveRange(awbs);
        }
        context.PrepaidDocuments.RemoveRange(prepaidDocs);

        var awbStocks = await context.AWBStocks.Where(s => s.IsDemo).ToListAsync();
        context.AWBStocks.RemoveRange(awbStocks);
        await context.SaveChangesAsync();

        var vehicles = await context.Vehicles.Where(v => v.IsDemo).ToListAsync();
        context.Vehicles.RemoveRange(vehicles);

        var employees = await context.Employees.Where(e => e.IsDemo).ToListAsync();
        context.Employees.RemoveRange(employees);
        await context.SaveChangesAsync();

        var demoPartyIds = await context.Parties
            .Where(p => p.IsDemo)
            .Select(p => p.Id)
            .ToListAsync();

        if (demoPartyIds.Any())
        {
            var slaAgreements = await context.SLAAgreements
                .Where(s => demoPartyIds.Contains(s.CustomerId))
                .ToListAsync();
            context.SLAAgreements.RemoveRange(slaAgreements);

            var customerBranches = await context.CustomerBranches
                .Where(cb => demoPartyIds.Contains(cb.PartyId))
                .ToListAsync();
            context.CustomerBranches.RemoveRange(customerBranches);

            var addresses = await context.PartyAddresses
                .Where(a => demoPartyIds.Contains(a.PartyId))
                .ToListAsync();
            context.PartyAddresses.RemoveRange(addresses);
            await context.SaveChangesAsync();
        }

        var parties = await context.Parties.Where(p => p.IsDemo).ToListAsync();
        context.Parties.RemoveRange(parties);
        await context.SaveChangesAsync();

        await using var platformContext = await _platformDbFactory.CreateDbContextAsync();

        var demoTaxCodes = await platformContext.TaxCodes
            .Where(t => t.TenantId == DemoTenantId && t.Description != null && t.Description.StartsWith("DEMO-"))
            .ToListAsync();
        platformContext.TaxCodes.RemoveRange(demoTaxCodes);

        var demoCurrencies = await platformContext.Currencies
            .Where(c => c.TenantId == DemoTenantId && c.Name.StartsWith("DEMO-"))
            .ToListAsync();
        platformContext.Currencies.RemoveRange(demoCurrencies);

        var demoAccounts = await platformContext.ChartOfAccounts
            .Where(c => c.TenantId == DemoTenantId && c.AccountName.StartsWith("DEMO-"))
            .ToListAsync();
        platformContext.ChartOfAccounts.RemoveRange(demoAccounts);

        await platformContext.SaveChangesAsync();

        return true;
    }

    public async Task<AllDataStats> GetAllDataStatsAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        int ticketCount = 0;
        try { ticketCount = await context.Tickets.CountAsync(); } catch { }

        return new AllDataStats
        {
            Parties = await context.Parties.CountAsync(),
            Employees = await context.Employees.CountAsync(),
            Vehicles = await context.Vehicles.CountAsync(),
            AWBStocks = await context.AWBStocks.CountAsync(),
            PrepaidDocuments = await context.PrepaidDocuments.CountAsync(),
            Tickets = ticketCount,
            PickupRequests = await context.PickupRequests.CountAsync(),
            AWBs = await context.InscanMasters.CountAsync(),
            DRS = await context.DRSs.CountAsync(),
            Invoices = await context.Invoices.CountAsync(),
            Receipts = await context.Receipts.CountAsync(),
            Journals = await context.Journals.CountAsync(),
            BankAccounts = await context.BankAccounts.CountAsync(),
            BankReconciliations = await context.BankReconciliations.CountAsync(),
            CashBankTransactions = await context.CashBankTransactions.CountAsync()
        };
    }

    public async Task<bool> DeleteAllDataAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        // Delete in correct order to respect foreign key constraints
        // Finance - delete child records first
        try
        {
            var reconciliationAdjustments = await context.ReconciliationAdjustments.ToListAsync();
            context.ReconciliationAdjustments.RemoveRange(reconciliationAdjustments);
            await context.SaveChangesAsync();

            var reconciliationMatches = await context.ReconciliationMatches.ToListAsync();
            context.ReconciliationMatches.RemoveRange(reconciliationMatches);
            await context.SaveChangesAsync();

            var bankReconciliations = await context.BankReconciliations.ToListAsync();
            context.BankReconciliations.RemoveRange(bankReconciliations);
            await context.SaveChangesAsync();
        }
        catch { }

        try
        {
            var voucherAttachments = await context.VoucherAttachments.ToListAsync();
            context.VoucherAttachments.RemoveRange(voucherAttachments);
            await context.SaveChangesAsync();

            var cashBankLines = await context.CashBankTransactionLines.ToListAsync();
            context.CashBankTransactionLines.RemoveRange(cashBankLines);
            await context.SaveChangesAsync();

            var cashBankTxns = await context.CashBankTransactions.ToListAsync();
            context.CashBankTransactions.RemoveRange(cashBankTxns);
            await context.SaveChangesAsync();
        }
        catch { }

        try
        {
            var bankAccounts = await context.BankAccounts.ToListAsync();
            context.BankAccounts.RemoveRange(bankAccounts);
            await context.SaveChangesAsync();
        }
        catch { }

        // Journal entries
        try
        {
            var journalEntries = await context.JournalEntries.ToListAsync();
            context.JournalEntries.RemoveRange(journalEntries);
            await context.SaveChangesAsync();

            var journals = await context.Journals.ToListAsync();
            context.Journals.RemoveRange(journals);
            await context.SaveChangesAsync();
        }
        catch { }

        // Receipts and Receipt Allocations
        try
        {
            var receiptAllocations = await context.ReceiptAllocations.ToListAsync();
            context.ReceiptAllocations.RemoveRange(receiptAllocations);
            await context.SaveChangesAsync();

            var receipts = await context.Receipts.ToListAsync();
            context.Receipts.RemoveRange(receipts);
            await context.SaveChangesAsync();
        }
        catch { }

        // Invoices and related
        try
        {
            var invoiceSpecialCharges = await context.InvoiceSpecialCharges.ToListAsync();
            context.InvoiceSpecialCharges.RemoveRange(invoiceSpecialCharges);
            await context.SaveChangesAsync();

            var invoiceDetails = await context.InvoiceDetails.ToListAsync();
            context.InvoiceDetails.RemoveRange(invoiceDetails);
            await context.SaveChangesAsync();

            var invoices = await context.Invoices.ToListAsync();
            context.Invoices.RemoveRange(invoices);
            await context.SaveChangesAsync();
        }
        catch { }

        // DRS and DRS Details
        try
        {
            var drsDetails = await context.DRSDetails.ToListAsync();
            context.DRSDetails.RemoveRange(drsDetails);
            await context.SaveChangesAsync();

            var drss = await context.DRSs.ToListAsync();
            context.DRSs.RemoveRange(drss);
            await context.SaveChangesAsync();
        }
        catch { }

        // CRM - Tickets
        try
        {
            var ticketComments = await context.TicketComments.ToListAsync();
            context.TicketComments.RemoveRange(ticketComments);
            await context.SaveChangesAsync();

            var tickets = await context.Tickets.ToListAsync();
            context.Tickets.RemoveRange(tickets);
            await context.SaveChangesAsync();

            var ticketCategories = await context.TicketCategories.ToListAsync();
            context.TicketCategories.RemoveRange(ticketCategories);
            await context.SaveChangesAsync();
        }
        catch { }

        // Import data
        try
        {
            var importDocuments = await context.ImportDocuments.ToListAsync();
            context.ImportDocuments.RemoveRange(importDocuments);
            await context.SaveChangesAsync();

            var importShipments = await context.ImportShipments.ToListAsync();
            context.ImportShipments.RemoveRange(importShipments);
            await context.SaveChangesAsync();

            var importBags = await context.ImportBags.ToListAsync();
            context.ImportBags.RemoveRange(importBags);
            await context.SaveChangesAsync();

            var importMasters = await context.ImportMasters.ToListAsync();
            context.ImportMasters.RemoveRange(importMasters);
            await context.SaveChangesAsync();
        }
        catch { }

        // AWB Tracking and items
        try
        {
            var awbTrackings = await context.AWBTrackings.ToListAsync();
            context.AWBTrackings.RemoveRange(awbTrackings);
            await context.SaveChangesAsync();

            var inscanItems = await context.InscanMasterItems.ToListAsync();
            context.InscanMasterItems.RemoveRange(inscanItems);
            await context.SaveChangesAsync();

            var shipmentNoteMentions = await context.ShipmentNoteMentions.ToListAsync();
            context.ShipmentNoteMentions.RemoveRange(shipmentNoteMentions);
            await context.SaveChangesAsync();

            var shipmentNotes = await context.ShipmentNotes.ToListAsync();
            context.ShipmentNotes.RemoveRange(shipmentNotes);
            await context.SaveChangesAsync();
        }
        catch { }

        // Pickup request shipments
        try
        {
            var pickupShipments = await context.PickupRequestShipments.ToListAsync();
            context.PickupRequestShipments.RemoveRange(pickupShipments);
            await context.SaveChangesAsync();

            var pickupRequests = await context.PickupRequests.ToListAsync();
            context.PickupRequests.RemoveRange(pickupRequests);
            await context.SaveChangesAsync();
        }
        catch { }

        // Inscan Masters (AWBs)
        try
        {
            var inscanMasters = await context.InscanMasters.ToListAsync();
            context.InscanMasters.RemoveRange(inscanMasters);
            await context.SaveChangesAsync();
        }
        catch { }

        // Prepaid Documents
        try
        {
            var prepaidAwbs = await context.PrepaidAWBs.ToListAsync();
            context.PrepaidAWBs.RemoveRange(prepaidAwbs);
            await context.SaveChangesAsync();

            var prepaidDocs = await context.PrepaidDocuments.ToListAsync();
            context.PrepaidDocuments.RemoveRange(prepaidDocs);
            await context.SaveChangesAsync();
        }
        catch { }

        // AWB Stocks
        try
        {
            var awbStocks = await context.AWBStocks.ToListAsync();
            context.AWBStocks.RemoveRange(awbStocks);
            await context.SaveChangesAsync();
        }
        catch { }

        // Vehicles
        try
        {
            var vehicles = await context.Vehicles.ToListAsync();
            context.Vehicles.RemoveRange(vehicles);
            await context.SaveChangesAsync();
        }
        catch { }

        // Employees
        try
        {
            var employees = await context.Employees.ToListAsync();
            context.Employees.RemoveRange(employees);
            await context.SaveChangesAsync();
        }
        catch { }

        // Parties and related
        try
        {
            var slaAgreements = await context.SLAAgreements.ToListAsync();
            context.SLAAgreements.RemoveRange(slaAgreements);
            await context.SaveChangesAsync();

            var customerBranches = await context.CustomerBranches.ToListAsync();
            context.CustomerBranches.RemoveRange(customerBranches);
            await context.SaveChangesAsync();

            var partyAddresses = await context.PartyAddresses.ToListAsync();
            context.PartyAddresses.RemoveRange(partyAddresses);
            await context.SaveChangesAsync();

            var parties = await context.Parties.ToListAsync();
            context.Parties.RemoveRange(parties);
            await context.SaveChangesAsync();
        }
        catch { }

        return true;
    }
}
