using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Infrastructure.Services;
using Net4Courier.Web.Components;
using Net4Courier.Web.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
// --- ADD THIS BLOCK ---

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.PopoverOptions.ThrowOnDuplicateProvider = false;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgresql://"))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/').Split('?')[0];
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    
    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
}
else
{
    connectionString = databaseUrl ?? "";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());
builder.Services.AddScoped<ReportingService>();
builder.Services.AddScoped<RatingEngineService>();
builder.Services.AddScoped<DRSReconciliationService>();
builder.Services.AddScoped<InvoicingService>();
builder.Services.AddScoped<ShipmentStatusService>();
builder.Services.AddScoped<MAWBService>();
builder.Services.AddScoped<AWBNumberService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

var keysPath = Path.Combine(Environment.CurrentDirectory, "data-protection-keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("Net4Courier");

builder.Services.AddHostedService<DatabaseInitializationService>();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCookiePolicy();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapGet("/api/report/awb/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var pdf = reportService.GenerateAWBLabel(awb);
    return Results.File(pdf, "application/pdf", $"AWB-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/invoice/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService) =>
{
    var invoice = await db.Invoices.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == id);
    if (invoice == null) return Results.NotFound();
    var pdf = reportService.GenerateInvoicePdf(invoice);
    return Results.File(pdf, "application/pdf", $"Invoice-{invoice.InvoiceNo}.pdf");
});

app.MapGet("/api/report/receipt/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService) =>
{
    var receipt = await db.Receipts.Include(r => r.Allocations).FirstOrDefaultAsync(r => r.Id == id);
    if (receipt == null) return Results.NotFound();
    var pdf = reportService.GenerateReceiptPdf(receipt);
    return Results.File(pdf, "application/pdf", $"Receipt-{receipt.ReceiptNo}.pdf");
});

app.MapGet("/api/report/mawb/{id:long}", async (long id, ReportingService reportService) =>
{
    var pdf = await reportService.GenerateMAWBManifest(id);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"MAWB-Manifest-{id}.pdf");
});

app.MapGet("/api/report/awb-print/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var pdf = reportService.GenerateAirWaybillPdf(awb);
    return Results.File(pdf, "application/pdf", $"AirWaybill-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/manifest-label/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var pdf = reportService.GenerateManifestLabel(awb, awb.BagNo, awb.MAWBNo);
    return Results.File(pdf, "application/pdf", $"Label-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/mawb-labels/{id:long}", async (long id, ReportingService reportService) =>
{
    var pdf = await reportService.GenerateManifestLabels(id);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"MAWB-Labels-{id}.pdf");
});

app.MapGet("/api/report/export-manifest/{id:long}", async (long id, ReportingService reportService) =>
{
    var pdf = await reportService.GenerateExportManifest(id);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"Export-Manifest-{id}.pdf");
});

app.MapGet("/api/report/domestic-manifest/{id:long}", async (long id, ReportingService reportService) =>
{
    var pdf = await reportService.GenerateDomesticManifest(id);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"Domestic-Manifest-{id}.pdf");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run("http://0.0.0.0:5000");

public class DatabaseInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);
        
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.EnsureCreatedAsync(stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ShipmentStatusGroups"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(50) NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" TEXT,
                    ""SequenceNo"" INT NOT NULL DEFAULT 0,
                    ""IconName"" VARCHAR(50),
                    ""ColorCode"" VARCHAR(20),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ShipmentStatusGroups_Code"" ON ""ShipmentStatusGroups"" (""Code"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ShipmentStatuses"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""StatusGroupId"" BIGINT NOT NULL REFERENCES ""ShipmentStatusGroups""(""Id"") ON DELETE CASCADE,
                    ""Code"" VARCHAR(50) NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""TimelineDescription"" TEXT,
                    ""SequenceNo"" INT NOT NULL DEFAULT 0,
                    ""IsTerminal"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsException"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""MapsToCourierStatus"" INT,
                    ""IconName"" VARCHAR(50),
                    ""ColorCode"" VARCHAR(20),
                    ""RequiresPOD"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""RequiresLocation"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""RequiresRemarks"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ShipmentStatuses_Code"" ON ""ShipmentStatuses"" (""Code"");
                CREATE INDEX IF NOT EXISTS ""IX_ShipmentStatuses_GroupId"" ON ""ShipmentStatuses"" (""StatusGroupId"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ShipmentStatusHistories"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""InscanMasterId"" BIGINT NOT NULL REFERENCES ""InscanMasters""(""Id"") ON DELETE CASCADE,
                    ""StatusId"" BIGINT NOT NULL REFERENCES ""ShipmentStatuses""(""Id"") ON DELETE CASCADE,
                    ""StatusGroupId"" BIGINT NOT NULL REFERENCES ""ShipmentStatusGroups""(""Id"") ON DELETE CASCADE,
                    ""EventType"" VARCHAR(100) NOT NULL,
                    ""EventRefId"" BIGINT,
                    ""EventRefType"" VARCHAR(100),
                    ""BranchId"" BIGINT,
                    ""LocationName"" VARCHAR(200),
                    ""UserId"" BIGINT,
                    ""UserName"" VARCHAR(200),
                    ""Remarks"" TEXT,
                    ""ChangedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""IsAutomatic"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""DeviceInfo"" VARCHAR(500),
                    ""Latitude"" DECIMAL(10,7),
                    ""Longitude"" DECIMAL(10,7),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE INDEX IF NOT EXISTS ""IX_ShipmentStatusHistories_InscanMasterId"" ON ""ShipmentStatusHistories"" (""InscanMasterId"");
                CREATE INDEX IF NOT EXISTS ""IX_ShipmentStatusHistories_Timeline"" ON ""ShipmentStatusHistories"" (""InscanMasterId"", ""ChangedAt"" DESC);
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ServiceTypes"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(50) NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" TEXT,
                    ""TransitDays"" INT,
                    ""IsExpress"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDefault"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""SortOrder"" INT NOT NULL DEFAULT 0,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ServiceTypes_Code"" ON ""ServiceTypes"" (""Code"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBPrefix"" VARCHAR(50);
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBStartingNumber"" BIGINT NOT NULL DEFAULT 1;
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBIncrement"" INT NOT NULL DEFAULT 1;
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBLastUsedNumber"" BIGINT NOT NULL DEFAULT 0;
            ", stoppingToken);

            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            await authService.SeedAdminUserAsync();

            if (!await dbContext.OtherChargeTypes.AnyAsync(stoppingToken))
            {
                var chargeTypes = new[]
                {
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Handling Charges", Code = "HDL", Description = "Charges for handling special shipments", DefaultAmount = 50, SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Insurance", Code = "INS", Description = "Insurance coverage for shipment value", DefaultAmount = 100, SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Packaging", Code = "PKG", Description = "Special packaging materials and service", DefaultAmount = 25, SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Documentation", Code = "DOC", Description = "Documentation and paperwork charges", DefaultAmount = 30, SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Customs Clearance", Code = "CUS", Description = "Customs clearance and processing fees", DefaultAmount = 200, SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Pickup Charges", Code = "PUP", Description = "Door-to-door pickup service", DefaultAmount = 75, SortOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Warehousing", Code = "WHS", Description = "Storage and warehousing fees", DefaultAmount = 40, SortOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Operations.Entities.OtherChargeType { Name = "Priority Handling", Code = "PRI", Description = "Express/priority processing", DefaultAmount = 150, SortOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                dbContext.OtherChargeTypes.AddRange(chargeTypes);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Seeded OtherChargeTypes data");
            }

            var shipmentStatusService = scope.ServiceProvider.GetRequiredService<ShipmentStatusService>();
            await shipmentStatusService.SeedDefaultStatuses();
            await shipmentStatusService.SeedRTSStatuses();

            if (!await dbContext.Parties.AnyAsync(p => p.PartyType == Net4Courier.Masters.Entities.PartyType.ForwardingAgent, stoppingToken))
            {
                var company = await dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted, stoppingToken);
                if (company != null)
                {
                    var forwardingAgents = new[]
                    {
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "DHL Express", Code = "DHL", PartyType = Net4Courier.Masters.Entities.PartyType.ForwardingAgent, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable, ContactPerson = "DHL Support", Phone = "+1-800-225-5345", Email = "support@dhl.com", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "FedEx", Code = "FEDEX", PartyType = Net4Courier.Masters.Entities.PartyType.ForwardingAgent, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable, ContactPerson = "FedEx Support", Phone = "+1-800-463-3339", Email = "support@fedex.com", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "Aramex", Code = "ARAMEX", PartyType = Net4Courier.Masters.Entities.PartyType.ForwardingAgent, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable, ContactPerson = "Aramex Support", Phone = "+971-600-544000", Email = "support@aramex.com", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "UPS", Code = "UPS", PartyType = Net4Courier.Masters.Entities.PartyType.ForwardingAgent, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable, ContactPerson = "UPS Support", Phone = "+1-800-742-5877", Email = "support@ups.com", IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "TNT Express", Code = "TNT", PartyType = Net4Courier.Masters.Entities.PartyType.ForwardingAgent, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable, ContactPerson = "TNT Support", Phone = "+31-88-393-9393", Email = "support@tnt.com", IsActive = true, CreatedAt = DateTime.UtcNow }
                    };
                    dbContext.Parties.AddRange(forwardingAgents);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Seeded Forwarding Agents");
                }
            }

            if (!await dbContext.Parties.AnyAsync(p => p.PartyType == Net4Courier.Masters.Entities.PartyType.CoLoader, stoppingToken))
            {
                var company = await dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted, stoppingToken);
                if (company != null)
                {
                    var coloaders = new[]
                    {
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "FastTrack Logistics", Code = "FTL", PartyType = Net4Courier.Masters.Entities.PartyType.CoLoader, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable, ContactPerson = "FastTrack Operations", Phone = "+1-800-555-0101", Email = "ops@fasttrack.com", CreditLimit = 50000, CreditDays = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "Global Freight Partners", Code = "GFP", PartyType = Net4Courier.Masters.Entities.PartyType.CoLoader, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable, ContactPerson = "GFP Sales", Phone = "+1-800-555-0102", Email = "sales@gfp.com", CreditLimit = 75000, CreditDays = 45, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "Swift Cargo Solutions", Code = "SCS", PartyType = Net4Courier.Masters.Entities.PartyType.CoLoader, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable, ContactPerson = "Swift Support", Phone = "+1-800-555-0103", Email = "support@swiftcargo.com", CreditLimit = 40000, CreditDays = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "TransWorld Shipping", Code = "TWS", PartyType = Net4Courier.Masters.Entities.PartyType.CoLoader, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable, ContactPerson = "TW Operations", Phone = "+1-800-555-0104", Email = "operations@transworld.com", CreditLimit = 60000, CreditDays = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Masters.Entities.Party { CompanyId = company.Id, Name = "Pacific Logistics", Code = "PACLOG", PartyType = Net4Courier.Masters.Entities.PartyType.CoLoader, AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable, ContactPerson = "Pacific Logistics Team", Phone = "+1-800-555-0105", Email = "team@pacificlog.com", CreditLimit = 80000, CreditDays = 60, IsActive = true, CreatedAt = DateTime.UtcNow }
                    };
                    dbContext.Parties.AddRange(coloaders);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Seeded Co-Loaders");
                }
            }

            if (!await dbContext.ServiceTypes.AnyAsync(stoppingToken))
            {
                var serviceTypes = new[]
                {
                    new Net4Courier.Masters.Entities.ServiceType { Code = "STD", Name = "Standard Delivery", Description = "Regular delivery service with standard transit times", TransitDays = 5, IsExpress = false, IsDefault = true, SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "EXP", Name = "Express Delivery", Description = "Fast delivery with priority handling", TransitDays = 2, IsExpress = true, IsDefault = false, SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "OVN", Name = "Overnight", Description = "Next business day delivery", TransitDays = 1, IsExpress = true, IsDefault = false, SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "SDD", Name = "Same Day Delivery", Description = "Delivery within the same business day", TransitDays = 0, IsExpress = true, IsDefault = false, SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "ECO", Name = "Economy", Description = "Cost-effective delivery with extended transit", TransitDays = 7, IsExpress = false, IsDefault = false, SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "DOC", Name = "Document Express", Description = "Specialized service for document shipments", TransitDays = 1, IsExpress = true, IsDefault = false, SortOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "FRT", Name = "Freight", Description = "Heavy cargo and pallet shipments", TransitDays = 10, IsExpress = false, IsDefault = false, SortOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ServiceType { Code = "COD", Name = "Cash on Delivery", Description = "Delivery with payment collection", TransitDays = 3, IsExpress = false, IsDefault = false, SortOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                dbContext.ServiceTypes.AddRange(serviceTypes);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Seeded Service Types");
            }

            await SeedDemoPickupRequestsAndShipments(dbContext, stoppingToken);

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
    }

    private async Task SeedDemoPickupRequestsAndShipments(ApplicationDbContext dbContext, CancellationToken stoppingToken)
    {
        if (await dbContext.PickupRequests.AnyAsync(stoppingToken))
            return;

        var company = await dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted, stoppingToken);
        var branch = await dbContext.Branches.FirstOrDefaultAsync(b => !b.IsDeleted, stoppingToken);
        if (company == null || branch == null)
        {
            _logger.LogWarning("Cannot seed demo data: Company or Branch not found");
            return;
        }

        var customer = await dbContext.Parties.FirstOrDefaultAsync(p => p.PartyType == Net4Courier.Masters.Entities.PartyType.Customer && !p.IsDeleted, stoppingToken);
        if (customer == null)
        {
            customer = new Net4Courier.Masters.Entities.Party
            {
                CompanyId = company.Id,
                Name = "Demo Customer Ltd",
                Code = "DEMO",
                PartyType = Net4Courier.Masters.Entities.PartyType.Customer,
                AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Receivable,
                ContactPerson = "John Smith",
                Phone = "+91-9876543210",
                Email = "demo@customer.com",
                CreditLimit = 100000,
                CreditDays = 30,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Parties.Add(customer);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        var courier = await dbContext.Parties.FirstOrDefaultAsync(p => p.PartyType == Net4Courier.Masters.Entities.PartyType.DeliveryAgent && !p.IsDeleted, stoppingToken);
        if (courier == null)
        {
            courier = new Net4Courier.Masters.Entities.Party
            {
                CompanyId = company.Id,
                Name = "Rajesh Kumar",
                Code = "COURIER01",
                PartyType = Net4Courier.Masters.Entities.PartyType.DeliveryAgent,
                AccountNature = Net4Courier.Masters.Entities.PartyAccountNature.Payable,
                ContactPerson = "Rajesh Kumar",
                Phone = "+91-9988776655",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Parties.Add(courier);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        var pickupRequests = new List<Net4Courier.Operations.Entities.PickupRequest>();
        var now = DateTime.UtcNow;
        int pickupCounter = 1;

        var destinations = new[]
        {
            ("Tech Solutions Pvt Ltd", "Mumbai", "Maharashtra", "400001"),
            ("Global Exports Inc", "Delhi", "Delhi", "110001"),
            ("Sunrise Traders", "Bangalore", "Karnataka", "560001"),
            ("Eastern Distributors", "Kolkata", "West Bengal", "700001"),
            ("Southern Logistics", "Chennai", "Tamil Nadu", "600001"),
            ("Western Supplies Co", "Ahmedabad", "Gujarat", "380001"),
            ("Northern Goods Ltd", "Lucknow", "Uttar Pradesh", "226001"),
            ("Central Hub Corp", "Nagpur", "Maharashtra", "440001")
        };

        for (int i = 0; i < 2; i++)
        {
            var pickup = new Net4Courier.Operations.Entities.PickupRequest
            {
                PickupNo = $"PU{now:yyyyMMdd}{pickupCounter++:D4}",
                RequestDate = now.AddDays(-i),
                ScheduledDate = now.AddDays(1),
                CompanyId = company.Id,
                BranchId = branch.Id,
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                ContactPerson = "John Smith",
                Phone = "+91-9876543210",
                Mobile = "+91-9876543210",
                PickupAddress = "123 Business Park, Tech Zone",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                PostalCode = "500081",
                EstimatedPieces = 2,
                EstimatedWeight = 5.5m,
                Status = Net4Courier.Operations.Entities.PickupStatus.PickupRequest,
                CreatedAt = now.AddDays(-i)
            };
            pickup.Shipments.Add(new Net4Courier.Operations.Entities.PickupRequestShipment
            {
                LineNo = 1,
                Consignee = destinations[i].Item1,
                ConsigneeCity = destinations[i].Item2,
                ConsigneeState = destinations[i].Item3,
                ConsigneePostalCode = destinations[i].Item4,
                ConsigneeCountry = "India",
                Pieces = 2,
                Weight = 5.5m,
                CargoDescription = "Electronics",
                PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Account,
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
                CreatedAt = now
            });
            pickupRequests.Add(pickup);
        }

        for (int i = 2; i < 4; i++)
        {
            var pickup = new Net4Courier.Operations.Entities.PickupRequest
            {
                PickupNo = $"PU{now:yyyyMMdd}{pickupCounter++:D4}",
                RequestDate = now.AddDays(-1),
                ScheduledDate = now,
                CompanyId = company.Id,
                BranchId = branch.Id,
                CustomerId = customer.Id,
                CourierId = courier.Id,
                CustomerName = customer.Name,
                ContactPerson = "John Smith",
                Phone = "+91-9876543210",
                Mobile = "+91-9876543210",
                PickupAddress = "456 Industrial Area, Sector 12",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                PostalCode = "500032",
                EstimatedPieces = 1,
                EstimatedWeight = 3.0m,
                Status = Net4Courier.Operations.Entities.PickupStatus.AssignedForCollection,
                AssignedAt = now.AddHours(-2),
                CourierName = courier.Name,
                CourierPhone = courier.Phone,
                CreatedAt = now.AddDays(-1)
            };
            pickup.Shipments.Add(new Net4Courier.Operations.Entities.PickupRequestShipment
            {
                LineNo = 1,
                Consignee = destinations[i].Item1,
                ConsigneeCity = destinations[i].Item2,
                ConsigneeState = destinations[i].Item3,
                ConsigneePostalCode = destinations[i].Item4,
                ConsigneeCountry = "India",
                Pieces = 1,
                Weight = 3.0m,
                CargoDescription = "Documents",
                PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Prepaid,
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Document,
                CreatedAt = now
            });
            pickupRequests.Add(pickup);
        }

        for (int i = 4; i < 6; i++)
        {
            var pickup = new Net4Courier.Operations.Entities.PickupRequest
            {
                PickupNo = $"PU{now:yyyyMMdd}{pickupCounter++:D4}",
                RequestDate = now.AddDays(-2),
                ScheduledDate = now.AddDays(-1),
                CompanyId = company.Id,
                BranchId = branch.Id,
                CustomerId = customer.Id,
                CourierId = courier.Id,
                CustomerName = customer.Name,
                ContactPerson = "John Smith",
                Phone = "+91-9876543210",
                Mobile = "+91-9876543210",
                PickupAddress = "789 Commerce Hub, Ring Road",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                PostalCode = "500018",
                EstimatedPieces = 3,
                EstimatedWeight = 8.0m,
                ActualPieces = 3,
                ActualWeight = 7.8m,
                Status = Net4Courier.Operations.Entities.PickupStatus.ShipmentCollected,
                AssignedAt = now.AddDays(-1),
                CollectedAt = now.AddHours(-4),
                CourierName = courier.Name,
                CourierPhone = courier.Phone,
                CollectionRemarks = "Collected successfully",
                CreatedAt = now.AddDays(-2)
            };
            pickup.Shipments.Add(new Net4Courier.Operations.Entities.PickupRequestShipment
            {
                LineNo = 1,
                Consignee = destinations[i].Item1,
                ConsigneeCity = destinations[i].Item2,
                ConsigneeState = destinations[i].Item3,
                ConsigneePostalCode = destinations[i].Item4,
                ConsigneeCountry = "India",
                Pieces = 3,
                Weight = 7.8m,
                CargoDescription = "Garments",
                PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.COD,
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
                CreatedAt = now
            });
            pickupRequests.Add(pickup);
        }

        for (int i = 6; i < 8; i++)
        {
            var pickup = new Net4Courier.Operations.Entities.PickupRequest
            {
                PickupNo = $"PU{now:yyyyMMdd}{pickupCounter++:D4}",
                RequestDate = now.AddDays(-3),
                ScheduledDate = now.AddDays(-2),
                CompanyId = company.Id,
                BranchId = branch.Id,
                CustomerId = customer.Id,
                CourierId = courier.Id,
                CustomerName = customer.Name,
                ContactPerson = "John Smith",
                Phone = "+91-9876543210",
                Mobile = "+91-9876543210",
                PickupAddress = "321 Export Zone, IT Park",
                City = "Hyderabad",
                State = "Telangana",
                Country = "India",
                PostalCode = "500084",
                EstimatedPieces = 2,
                EstimatedWeight = 4.5m,
                ActualPieces = 2,
                ActualWeight = 4.3m,
                Status = Net4Courier.Operations.Entities.PickupStatus.Inscanned,
                AssignedAt = now.AddDays(-2),
                CollectedAt = now.AddDays(-1).AddHours(-6),
                InscannedAt = now.AddDays(-1).AddHours(-3),
                InscannedBy = "admin",
                CourierName = courier.Name,
                CourierPhone = courier.Phone,
                CollectionRemarks = "Collected and inscanned",
                CreatedAt = now.AddDays(-3)
            };
            pickup.Shipments.Add(new Net4Courier.Operations.Entities.PickupRequestShipment
            {
                LineNo = 1,
                Consignee = destinations[i].Item1,
                ConsigneeCity = destinations[i].Item2,
                ConsigneeState = destinations[i].Item3,
                ConsigneePostalCode = destinations[i].Item4,
                ConsigneeCountry = "India",
                Pieces = 2,
                Weight = 4.3m,
                CargoDescription = "Machinery Parts",
                PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Account,
                DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
                CreatedAt = now
            });
            pickupRequests.Add(pickup);
        }

        dbContext.PickupRequests.AddRange(pickupRequests);
        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Seeded 8 demo Pickup Requests");

        int awbCounter = 1;
        var shipments = new List<Net4Courier.Operations.Entities.InscanMaster>();

        var awb1 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now.AddDays(-2),
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "123 Business Park",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500081",
            Consignee = "ABC Corp",
            ConsigneeAddress1 = "456 Commercial Area",
            ConsigneeCity = "Pune",
            ConsigneeState = "Maharashtra",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "411001",
            Pieces = 2,
            Weight = 5.0m,
            ChargeableWeight = 5.0m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.PickedUp,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Account,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
            CargoDescription = "Electronics",
            CourierCharge = 250,
            NetTotal = 250,
            CreatedAt = now.AddDays(-2),
            CreatedByName = "admin"
        };
        shipments.Add(awb1);

        var awb2 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now.AddDays(-2),
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "789 Tech Hub",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500032",
            Consignee = "XYZ Ltd",
            ConsigneeAddress1 = "123 Business Park",
            ConsigneeCity = "Jaipur",
            ConsigneeState = "Rajasthan",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "302001",
            Pieces = 1,
            Weight = 2.5m,
            ChargeableWeight = 2.5m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.PickedUp,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Prepaid,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Document,
            CargoDescription = "Documents",
            CourierCharge = 150,
            NetTotal = 150,
            CreatedAt = now.AddDays(-2),
            CreatedByName = "admin"
        };
        shipments.Add(awb2);

        var awb3 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now.AddDays(-1),
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "456 Export Zone",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500084",
            Consignee = "Quick Mart",
            ConsigneeAddress1 = "789 Retail Park",
            ConsigneeCity = "Surat",
            ConsigneeState = "Gujarat",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "395001",
            Pieces = 3,
            Weight = 8.0m,
            ChargeableWeight = 8.0m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.InscanAtOrigin,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.COD,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
            CargoDescription = "Garments",
            CODAmount = 5000,
            CourierCharge = 350,
            NetTotal = 350,
            CreatedAt = now.AddDays(-1),
            CreatedByName = "admin"
        };
        shipments.Add(awb3);

        var awb4 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now.AddDays(-1),
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "321 Industrial Hub",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500018",
            Consignee = "Tech Supplies",
            ConsigneeAddress1 = "567 IT Park",
            ConsigneeCity = "Indore",
            ConsigneeState = "Madhya Pradesh",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "452001",
            Pieces = 2,
            Weight = 4.0m,
            ChargeableWeight = 4.0m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.InscanAtOrigin,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Account,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
            CargoDescription = "Computer Parts",
            CourierCharge = 200,
            NetTotal = 200,
            CreatedAt = now.AddDays(-1),
            CreatedByName = "admin"
        };
        shipments.Add(awb4);

        var awb5 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now,
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "999 Commerce Center",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500081",
            Consignee = "City Stores",
            ConsigneeAddress1 = "123 Main Road",
            ConsigneeCity = "Hyderabad",
            ConsigneeState = "Telangana",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "500001",
            Pieces = 1,
            Weight = 1.5m,
            ChargeableWeight = 1.5m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.OutForDelivery,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.Prepaid,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Document,
            CargoDescription = "Legal Documents",
            CourierCharge = 100,
            NetTotal = 100,
            CreatedAt = now,
            CreatedByName = "admin"
        };
        shipments.Add(awb5);

        var awb6 = new Net4Courier.Operations.Entities.InscanMaster
        {
            AWBNo = $"AWB{now:yyyyMMdd}{awbCounter++:D6}",
            TransactionDate = now,
            CompanyId = company.Id,
            BranchId = branch.Id,
            CustomerId = customer.Id,
            Consignor = customer.Name,
            ConsignorAddress1 = "777 Business Hub",
            ConsignorCity = "Hyderabad",
            ConsignorState = "Telangana",
            ConsignorCountry = "India",
            ConsignorPostalCode = "500032",
            Consignee = "Metro Distributors",
            ConsigneeAddress1 = "456 Market Street",
            ConsigneeCity = "Secunderabad",
            ConsigneeState = "Telangana",
            ConsigneeCountry = "India",
            ConsigneePostalCode = "500003",
            Pieces = 4,
            Weight = 12.0m,
            ChargeableWeight = 12.0m,
            CourierStatusId = Net4Courier.Kernel.Enums.CourierStatus.OutForDelivery,
            PaymentModeId = Net4Courier.Kernel.Enums.PaymentMode.COD,
            DocumentTypeId = Net4Courier.Kernel.Enums.DocumentType.Parcel,
            CargoDescription = "Retail Goods",
            CODAmount = 15000,
            CourierCharge = 450,
            NetTotal = 450,
            CreatedAt = now,
            CreatedByName = "admin"
        };
        shipments.Add(awb6);

        dbContext.InscanMasters.AddRange(shipments);
        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Seeded 6 demo AWB/Shipments");
    }
}
