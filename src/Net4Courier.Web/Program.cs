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

// Enable static web assets - wrapped in try-catch for production deployments
// where manifest files may not be present
try
{
    builder.WebHost.UseStaticWebAssets();
}
catch (InvalidOperationException)
{
    // Static web assets manifest not found - this is expected in some deployment scenarios
    // Static files will be served from wwwroot instead
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

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;
bool databaseConfigured = true;

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
else if (!string.IsNullOrEmpty(databaseUrl))
{
    connectionString = databaseUrl;
}
else
{
    // No database configured - use a placeholder to allow app to start
    // This enables diagnostics endpoints to work
    connectionString = "Host=localhost;Database=placeholder";
    databaseConfigured = false;
    Console.WriteLine("WARNING: DATABASE_URL not set. Application will start in limited mode.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

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

// Global error handling middleware for detailed error tracking
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

// Log startup diagnostics
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Application starting...");
startupLogger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);
startupLogger.LogInformation("ContentRootPath: {Path}", app.Environment.ContentRootPath);
startupLogger.LogInformation("WebRootPath: {Path}", app.Environment.WebRootPath);

// Check if wwwroot exists and has files
var webRootPath = app.Environment.WebRootPath;
if (Directory.Exists(webRootPath))
{
    var files = Directory.GetFiles(webRootPath, "*", SearchOption.AllDirectories);
    startupLogger.LogInformation("WebRoot contains {Count} files", files.Length);
}
else
{
    startupLogger.LogWarning("WebRoot directory does not exist: {Path}", webRootPath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCookiePolicy();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok("Healthy"));

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
        DatabaseUrl = !string.IsNullOrEmpty(dbUrl) ? "Set" : "NOT SET - This is the problem!",
        DatabaseUrlLength = dbUrl?.Length ?? 0,
        CurrentDirectory = Environment.CurrentDirectory,
        ProcessPath = Environment.ProcessPath,
        Message = string.IsNullOrEmpty(dbUrl) 
            ? "DATABASE_URL is not set. You need to connect a production database in Replit's Database panel."
            : "Database URL is configured."
    });
});

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
        
        // Check if database is configured before attempting initialization
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
        {
            _logger.LogWarning("DATABASE_URL not set. Skipping database initialization. Application running in limited mode.");
            return;
        }
        
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
                ALTER TABLE ""PickupRequests"" 
                ADD COLUMN IF NOT EXISTS ""StatusId"" BIGINT REFERENCES ""ShipmentStatuses""(""Id""),
                ADD COLUMN IF NOT EXISTS ""StatusGroupId"" BIGINT REFERENCES ""ShipmentStatusGroups""(""Id"");
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
                    ""AddedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""AddedByUserId"" BIGINT,
                    ""AddedByUserName"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" VARCHAR(200),
                    ""ModifiedByName"" VARCHAR(200)
                );
                CREATE INDEX IF NOT EXISTS ""IX_ImportShipmentNotes_ImportShipmentId"" ON ""ImportShipmentNotes"" (""ImportShipmentId"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ImportDocuments"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""ImportMasterId"" BIGINT NOT NULL REFERENCES ""ImportMasters""(""Id"") ON DELETE CASCADE,
                    ""DocumentType"" INT NOT NULL DEFAULT 1,
                    ""DocumentTypeName"" VARCHAR(100) NOT NULL,
                    ""OriginalFileName"" VARCHAR(255) NOT NULL,
                    ""StoredFileName"" VARCHAR(255) NOT NULL,
                    ""FilePath"" VARCHAR(500) NOT NULL,
                    ""ContentType"" VARCHAR(100),
                    ""FileSize"" BIGINT NOT NULL DEFAULT 0,
                    ""Description"" VARCHAR(500),
                    ""UploadedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UploadedByUserId"" BIGINT,
                    ""UploadedByUserName"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE INDEX IF NOT EXISTS ""IX_ImportDocuments_ImportMasterId"" ON ""ImportDocuments"" (""ImportMasterId"");
                CREATE INDEX IF NOT EXISTS ""IX_ImportDocuments_DocumentType"" ON ""ImportDocuments"" (""DocumentType"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBPrefix"" VARCHAR(50);
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBStartingNumber"" BIGINT NOT NULL DEFAULT 1;
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBIncrement"" INT NOT NULL DEFAULT 1;
                ALTER TABLE ""Branches"" ADD COLUMN IF NOT EXISTS ""AWBLastUsedNumber"" BIGINT NOT NULL DEFAULT 0;
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""AWBOtherCharges"" ADD COLUMN IF NOT EXISTS ""Notes"" VARCHAR(500);
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

            if (!await dbContext.ImportMasters.AnyAsync(stoppingToken))
            {
                var branch = await dbContext.Branches.FirstOrDefaultAsync(stoppingToken);
                var company = await dbContext.Companies.FirstOrDefaultAsync(stoppingToken);
                
                if (branch != null && company != null)
                {
                    var imports = new[]
                    {
                        new Net4Courier.Operations.Entities.ImportMaster 
                        { 
                            ImportRefNo = "IMP20260121001",
                            TransactionDate = DateTime.UtcNow,
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            ImportMode = Net4Courier.Kernel.Enums.ImportMode.Air,
                            MasterReferenceType = Net4Courier.Kernel.Enums.MasterReferenceType.MAWB,
                            MasterReferenceNumber = "176-12345678",
                            OriginCountryName = "United Kingdom",
                            OriginCityName = "London",
                            OriginPortCode = "LHR",
                            DestinationCountryName = "United Arab Emirates",
                            DestinationCityName = "Dubai",
                            DestinationPortCode = "DXB",
                            CarrierName = "Emirates",
                            CarrierCode = "EK",
                            FlightNo = "EK002",
                            FlightDate = DateTime.UtcNow.AddDays(-1),
                            ETA = DateTime.UtcNow,
                            TotalBags = 3,
                            TotalShipments = 15,
                            TotalGrossWeight = 125.5m,
                            TotalChargeableWeight = 150.0m,
                            Status = Net4Courier.Kernel.Enums.ImportMasterStatus.Arrived,
                            Remarks = "Sample import from London",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByName = "System"
                        },
                        new Net4Courier.Operations.Entities.ImportMaster 
                        { 
                            ImportRefNo = "IMP20260121002",
                            TransactionDate = DateTime.UtcNow.AddDays(-2),
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            ImportMode = Net4Courier.Kernel.Enums.ImportMode.Sea,
                            MasterReferenceType = Net4Courier.Kernel.Enums.MasterReferenceType.BL,
                            MasterReferenceNumber = "MAEU123456789",
                            OriginCountryName = "China",
                            OriginCityName = "Shanghai",
                            OriginPortCode = "SHA",
                            DestinationCountryName = "United Arab Emirates",
                            DestinationCityName = "Dubai",
                            DestinationPortCode = "JEA",
                            CarrierName = "Maersk Line",
                            VesselName = "MSC Oscar",
                            VoyageNumber = "VY2601",
                            ETA = DateTime.UtcNow.AddDays(5),
                            TotalBags = 10,
                            TotalShipments = 45,
                            TotalGrossWeight = 2500.0m,
                            TotalChargeableWeight = 2500.0m,
                            Status = Net4Courier.Kernel.Enums.ImportMasterStatus.InTransit,
                            Remarks = "Sea freight from Shanghai",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            CreatedByName = "System"
                        },
                        new Net4Courier.Operations.Entities.ImportMaster 
                        { 
                            ImportRefNo = "IMP20260121003",
                            TransactionDate = DateTime.UtcNow.AddDays(-1),
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            ImportMode = Net4Courier.Kernel.Enums.ImportMode.Land,
                            MasterReferenceType = Net4Courier.Kernel.Enums.MasterReferenceType.TruckWaybill,
                            MasterReferenceNumber = "TRK-2026-0001",
                            OriginCountryName = "Oman",
                            OriginCityName = "Muscat",
                            DestinationCountryName = "United Arab Emirates",
                            DestinationCityName = "Dubai",
                            TruckNumber = "DXB-12345",
                            DriverName = "Ahmed Hassan",
                            DriverPhone = "+971501234567",
                            ETA = DateTime.UtcNow,
                            TotalBags = 5,
                            TotalShipments = 25,
                            TotalGrossWeight = 800.0m,
                            TotalChargeableWeight = 800.0m,
                            Status = Net4Courier.Kernel.Enums.ImportMasterStatus.Arrived,
                            Remarks = "Land shipment from Oman",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            CreatedByName = "System"
                        }
                    };
                    dbContext.ImportMasters.AddRange(imports);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Seeded Import Masters");
                }
            }

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
    }
}
