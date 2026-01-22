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
using Microsoft.Extensions.FileProviders; 

// Log startup immediately
Console.WriteLine($"[{DateTime.UtcNow:O}] Net4Courier starting...");
Console.WriteLine($"[{DateTime.UtcNow:O}] Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");

try
{
    QuestPDF.Settings.License = LicenseType.Community;
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] Warning: QuestPDF license setup failed: {ex.Message}");
}

var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
    || Environment.GetEnvironmentVariable("REPLIT_DEPLOYMENT") == "1";

Console.WriteLine($"[{DateTime.UtcNow:O}] IsProduction: {isProduction}");

// FIXED: Explicitly set WebRootPath to ensure it finds the wwwroot folder
var options = new WebApplicationOptions
{
    Args = args,
    EnvironmentName = isProduction ? "Production" : null,
    WebRootPath = "wwwroot" 
};

var builder = WebApplication.CreateBuilder(options);

// FIXED: Try to enable static web assets in ALL environments
try
{
    builder.WebHost.UseStaticWebAssets();
    Console.WriteLine($"[{DateTime.UtcNow:O}] StaticWebAssets enabled.");
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] Note: StaticWebAssets could not be enabled: {ex.Message}");
}

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

// Database Configuration
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

try
{
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
        Console.WriteLine($"[{DateTime.UtcNow:O}] Database connection configured: Host={host}, Database={database}");
    }
    else if (!string.IsNullOrEmpty(databaseUrl))
    {
        connectionString = databaseUrl;
        Console.WriteLine($"[{DateTime.UtcNow:O}] Using raw DATABASE_URL connection string");
    }
    else
    {
        connectionString = "Host=localhost;Database=placeholder";
        Console.WriteLine($"[{DateTime.UtcNow:O}] WARNING: DATABASE_URL not set. Application will start in limited mode.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR parsing DATABASE_URL: {ex.Message}");
    connectionString = "Host=localhost;Database=placeholder";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

// Service Registrations
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
builder.Services.AddScoped<ImportExcelService>();
builder.Services.AddScoped<ShipmentExcelService>();
builder.Services.AddScoped<PODUpdateService>();
builder.Services.AddScoped<PODExcelService>();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<BookingWebhookService>();
builder.Services.AddScoped<EmpostFeeReportService>();
builder.Services.AddScoped<IEmpostService, EmpostService>();
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

// Global error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception for request {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        if (app.Environment.IsDevelopment())
        {
            await context.Response.WriteAsync($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
        }
        else
        {
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCookiePolicy();

// FIXED: Ensure Static Files are served before Authentication/Routing
app.UseStaticFiles();

app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok("Healthy"));

// Diagnostics Endpoint
app.MapGet("/api/diagnostics", (IWebHostEnvironment env) => 
{
    var webRootExists = Directory.Exists(env.WebRootPath);
    var webRootFiles = webRootExists ? Directory.GetFiles(env.WebRootPath, "*", SearchOption.AllDirectories).Length : 0;
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    return Results.Ok(new
    {
        Environment = env.EnvironmentName,
        ContentRootPath = env.ContentRootPath,
        WebRootPath = env.WebRootPath,
        WebRootExists = webRootExists,
        WebRootFileCount = webRootFiles,
        DatabaseUrl = !string.IsNullOrEmpty(dbUrl) ? "Set" : "NOT SET",
        Message = "Diagnostics Check Complete"
    });
});

// Reporting Endpoints
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

app.MapPost("/api/bookings/webhook/{integrationId}", async (
    string integrationId, 
    BookingWebhookPayload payload, 
    HttpContext context,
    BookingWebhookService webhookService) =>
{
    var webhookSecret = context.Request.Headers["X-Webhook-Secret"].FirstOrDefault();

    if (string.IsNullOrEmpty(webhookSecret))
    {
        return Results.Unauthorized();
    }

    var isValid = await webhookService.ValidateWebhookSecretAsync(integrationId, webhookSecret);
    if (!isValid)
    {
        return Results.Unauthorized();
    }

    var result = await webhookService.ProcessBookingAsync(payload, integrationId);

    if (result.Success)
    {
        return Results.Ok(new { 
            success = true, 
            message = result.Message, 
            pickupRequestId = result.PickupRequestId 
        });
    }

    return Results.BadRequest(new { success = false, message = result.Message });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine($"[{DateTime.UtcNow:O}] Starting HTTP server on http://0.0.0.0:5000");

try
{
    app.Run("http://0.0.0.0:5000");
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] FATAL ERROR starting server: {ex.Message}");
    Console.WriteLine($"[{DateTime.UtcNow:O}] Stack trace: {ex.StackTrace}");
    throw;
}

// Background Database Initialization Service
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

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
        {
            _logger.LogWarning("DATABASE_URL not set. Skipping database initialization.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Restore full initialization logic here
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
                ALTER TABLE ""PickupRequests"" 
                ADD COLUMN IF NOT EXISTS ""StatusId"" BIGINT REFERENCES ""ShipmentStatuses""(""Id""),
                ADD COLUMN IF NOT EXISTS ""StatusGroupId"" BIGINT REFERENCES ""ShipmentStatusGroups""(""Id""),
                ADD COLUMN IF NOT EXISTS ""BookingVehicle"" INT;
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""PickupStatusHistories"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""PickupRequestId"" BIGINT NOT NULL REFERENCES ""PickupRequests""(""Id"") ON DELETE CASCADE,
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
                CREATE INDEX IF NOT EXISTS ""IX_PickupStatusHistories_PickupRequestId"" ON ""PickupStatusHistories"" (""PickupRequestId"");
                CREATE INDEX IF NOT EXISTS ""IX_PickupStatusHistories_Timeline"" ON ""PickupStatusHistories"" (""PickupRequestId"", ""ChangedAt"" DESC);
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
                CREATE TABLE IF NOT EXISTS ""Ports"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(50) NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""PortType"" INT NOT NULL DEFAULT 1,
                    ""IATACode"" VARCHAR(10),
                    ""ICAOCode"" VARCHAR(10),
                    ""UNLocode"" VARCHAR(20),
                    ""City"" VARCHAR(100),
                    ""State"" VARCHAR(100),
                    ""Country"" VARCHAR(100),
                    ""CountryCode"" VARCHAR(10),
                    ""Latitude"" DECIMAL(10,7),
                    ""Longitude"" DECIMAL(10,7),
                    ""TimeZone"" VARCHAR(100),
                    ""Description"" TEXT,
                    ""SortOrder"" INT NOT NULL DEFAULT 0,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Ports_Code"" ON ""Ports"" (""Code"");
                CREATE INDEX IF NOT EXISTS ""IX_Ports_PortType"" ON ""Ports"" (""PortType"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ApiSettings"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" TEXT,
                    ""IntegrationType"" INT NOT NULL DEFAULT 0,
                    ""BaseUrl"" VARCHAR(500),
                    ""ApiKey"" TEXT,
                    ""ApiSecret"" TEXT,
                    ""Username"" VARCHAR(200),
                    ""Password"" TEXT,
                    ""BearerToken"" TEXT,
                    ""AuthType"" INT NOT NULL DEFAULT 0,
                    ""WebhookSecret"" TEXT,
                    ""WebhookEndpoint"" VARCHAR(500),
                    ""Headers"" TEXT,
                    ""CustomFields"" TEXT,
                    ""TokenExpiry"" TIMESTAMP WITH TIME ZONE,
                    ""SyncIntervalMinutes"" INT,
                    ""LastSyncAt"" TIMESTAMP WITH TIME ZONE,
                    ""LastSyncStatus"" VARCHAR(100),
                    ""LastSyncError"" TEXT,
                    ""BranchId"" BIGINT,
                    ""IsEnabled"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" VARCHAR(200),
                    ""ModifiedByName"" VARCHAR(200)
                );
                CREATE INDEX IF NOT EXISTS ""IX_ApiSettings_Name"" ON ""ApiSettings"" (""Name"");
                CREATE INDEX IF NOT EXISTS ""IX_ApiSettings_IntegrationType"" ON ""ApiSettings"" (""IntegrationType"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ImportMasters"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""ImportRefNo"" VARCHAR(50) NOT NULL,
                    ""TransactionDate"" TIMESTAMP WITH TIME ZONE NOT NULL,
                    ""FinancialYearId"" BIGINT,
                    ""CompanyId"" BIGINT,
                    ""BranchId"" BIGINT,
                    ""ImportMode"" INT NOT NULL DEFAULT 0,
                    ""MasterReferenceType"" INT NOT NULL DEFAULT 0,
                    ""MasterReferenceNumber"" VARCHAR(50) NOT NULL,
                    ""OriginCountryId"" BIGINT,
                    ""OriginCountryName"" VARCHAR(100),
                    ""OriginCityId"" BIGINT,
                    ""OriginCityName"" VARCHAR(100),
                    ""OriginPortCode"" VARCHAR(20),
                    ""DestinationCountryId"" BIGINT,
                    ""DestinationCountryName"" VARCHAR(100),
                    ""DestinationCityId"" BIGINT,
                    ""DestinationCityName"" VARCHAR(100),
                    ""DestinationPortCode"" VARCHAR(20),
                    ""ETD"" TIMESTAMP WITH TIME ZONE,
                    ""ETA"" TIMESTAMP WITH TIME ZONE,
                    ""ActualArrivalDate"" TIMESTAMP WITH TIME ZONE,
                    ""CarrierName"" VARCHAR(100),
                    ""CarrierCode"" VARCHAR(20),
                    ""FlightNo"" VARCHAR(20),
                    ""FlightDate"" TIMESTAMP WITH TIME ZONE,
                    ""VesselName"" VARCHAR(100),
                    ""VoyageNumber"" VARCHAR(50),
                    ""TruckNumber"" VARCHAR(50),
                    ""DriverName"" VARCHAR(100),
                    ""DriverPhone"" VARCHAR(20),
                    ""ManifestNumber"" VARCHAR(50),
                    ""CargoType"" INT NOT NULL DEFAULT 0,
                    ""TotalBags"" INT NOT NULL DEFAULT 0,
                    ""TotalShipments"" INT NOT NULL DEFAULT 0,
                    ""TotalGrossWeight"" DECIMAL(18,3),
                    ""TotalChargeableWeight"" DECIMAL(18,3),
                    ""TotalPieces"" INT,
                    ""ImportWarehouseId"" BIGINT,
                    ""ImportWarehouseName"" VARCHAR(100),
                    ""Status"" INT NOT NULL DEFAULT 0,
                    ""Remarks"" VARCHAR(500),
                    ""CoLoaderName"" VARCHAR(200),
                    ""CoLoaderId"" BIGINT,
                    ""CoLoaderRefNo"" VARCHAR(50),
                    ""CustomsDeclarationNo"" VARCHAR(50),
                    ""ExportPermitNo"" VARCHAR(50),
                    ""InscannedAt"" TIMESTAMP WITH TIME ZONE,
                    ""InscannedByUserId"" BIGINT,
                    ""InscannedByUserName"" VARCHAR(100),
                    ""ClosedAt"" TIMESTAMP WITH TIME ZONE,
                    ""ClosedByUserId"" BIGINT,
                    ""ClosedByUserName"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" VARCHAR(200),
                    ""ModifiedByName"" VARCHAR(200)
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ImportMasters_ImportRefNo"" ON ""ImportMasters"" (""ImportRefNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportMasters_MasterReferenceNumber"" ON ""ImportMasters"" (""MasterReferenceNumber"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportMasters_BranchId_TransactionDate"" ON ""ImportMasters"" (""BranchId"", ""TransactionDate"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ImportBags"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""ImportMasterId"" BIGINT NOT NULL REFERENCES ""ImportMasters""(""Id"") ON DELETE CASCADE,
                    ""BagNumber"" VARCHAR(50) NOT NULL,
                    ""SealNumber"" VARCHAR(50),
                    ""HandlingCode"" VARCHAR(20),
                    ""TotalShipments"" INT NOT NULL DEFAULT 0,
                    ""GrossWeight"" DECIMAL(18,3),
                    ""ChargeableWeight"" DECIMAL(18,3),
                    ""Length"" DECIMAL(18,2),
                    ""Width"" DECIMAL(18,2),
                    ""Height"" DECIMAL(18,2),
                    ""Status"" INT NOT NULL DEFAULT 0,
                    ""Remarks"" VARCHAR(500),
                    ""InscannedAt"" TIMESTAMP WITH TIME ZONE,
                    ""InscannedByUserId"" BIGINT,
                    ""InscannedByUserName"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" VARCHAR(200),
                    ""ModifiedByName"" VARCHAR(200)
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ImportBags_ImportMasterId_BagNumber"" ON ""ImportBags"" (""ImportMasterId"", ""BagNumber"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportBags_BagNumber"" ON ""ImportBags"" (""BagNumber"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ImportShipments"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""ImportMasterId"" BIGINT NOT NULL REFERENCES ""ImportMasters""(""Id"") ON DELETE RESTRICT,
                    ""ImportBagId"" BIGINT REFERENCES ""ImportBags""(""Id"") ON DELETE RESTRICT,
                    ""AWBNo"" VARCHAR(50) NOT NULL,
                    ""ReferenceNo"" VARCHAR(50),
                    ""ShipperName"" VARCHAR(200),
                    ""ShipperAddress"" VARCHAR(500),
                    ""ShipperCity"" VARCHAR(100),
                    ""ShipperCountry"" VARCHAR(100),
                    ""ShipperPhone"" VARCHAR(20),
                    ""ConsigneeName"" VARCHAR(200) NOT NULL,
                    ""ConsigneeAddress"" VARCHAR(500),
                    ""ConsigneeCity"" VARCHAR(100),
                    ""ConsigneeState"" VARCHAR(100),
                    ""ConsigneeCountry"" VARCHAR(100) NOT NULL,
                    ""ConsigneePostalCode"" VARCHAR(20),
                    ""ConsigneePhone"" VARCHAR(20),
                    ""ConsigneeMobile"" VARCHAR(20),
                    ""Pieces"" INT NOT NULL DEFAULT 1,
                    ""Weight"" DECIMAL(18,3),
                    ""VolumetricWeight"" DECIMAL(18,3),
                    ""ChargeableWeight"" DECIMAL(18,3),
                    ""ContentsDescription"" VARCHAR(500),
                    ""SpecialInstructions"" VARCHAR(500),
                    ""DeclaredValue"" DECIMAL(18,2),
                    ""Currency"" VARCHAR(10),
                    ""HSCode"" VARCHAR(20),
                    ""DutyAmount"" DECIMAL(18,2),
                    ""VATAmount"" DECIMAL(18,2),
                    ""OtherCharges"" DECIMAL(18,2),
                    ""TotalCustomsCharges"" DECIMAL(18,2),
                    ""CODAmount"" DECIMAL(18,2),
                    ""IsCOD"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""Status"" INT NOT NULL DEFAULT 0,
                    ""CustomsStatus"" INT NOT NULL DEFAULT 0,
                    ""HoldReason"" INT NOT NULL DEFAULT 0,
                    ""HoldReasonDetails"" VARCHAR(500),
                    ""ImporterOfRecord"" VARCHAR(200),
                    ""CustomsEntryNumber"" VARCHAR(50),
                    ""ExaminationRemarks"" VARCHAR(500),
                    ""InscannedAt"" TIMESTAMP WITH TIME ZONE,
                    ""InscannedByUserId"" BIGINT,
                    ""InscannedByUserName"" VARCHAR(100),
                    ""CustomsClearedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CustomsClearedByUserId"" BIGINT,
                    ""CustomsClearedByUserName"" VARCHAR(100),
                    ""ReleasedAt"" TIMESTAMP WITH TIME ZONE,
                    ""ReleasedByUserId"" BIGINT,
                    ""ReleasedByUserName"" VARCHAR(100),
                    ""HandedOverAt"" TIMESTAMP WITH TIME ZONE,
                    ""HandedOverToUserId"" BIGINT,
                    ""HandedOverToUserName"" VARCHAR(100),
                    ""ConvertedToAWBId"" BIGINT,
                    ""Remarks"" VARCHAR(500),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" VARCHAR(200),
                    ""ModifiedByName"" VARCHAR(200)
                );
                CREATE INDEX IF NOT EXISTS ""IX_ImportShipments_AWBNo"" ON ""ImportShipments"" (""AWBNo"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ImportShipments_ImportMasterId_AWBNo"" ON ""ImportShipments"" (""ImportMasterId"", ""AWBNo"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportShipments_ImportBagId"" ON ""ImportShipments"" (""ImportBagId"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportShipments_Status"" ON ""ImportShipments"" (""Status"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ImportShipmentNotes"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""ImportShipmentId"" BIGINT NOT NULL REFERENCES ""ImportShipments""(""Id"") ON DELETE CASCADE,
                    ""NoteText"" VARCHAR(2000) NOT NULL,
                    ""AddedAt"" TIMESTAMP WITH TIME ZONE NOT NULL