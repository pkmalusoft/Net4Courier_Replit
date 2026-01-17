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

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
    }
}
