using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Infrastructure.Services;
using Net4Courier.Web.Components;
using Net4Courier.Web.Services;
using Net4Courier.Operations.Services;
using QuestPDF.Infrastructure;

// Log startup immediately
Console.WriteLine($"[{DateTime.UtcNow:O}] Net4Courier starting...");
Console.WriteLine($"[{DateTime.UtcNow:O}] Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"[{DateTime.UtcNow:O}] DATABASE_URL: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")) ? "NOT SET" : "SET")}");

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

var options = new WebApplicationOptions
{
    Args = args,
    EnvironmentName = isProduction ? "Production" : null
};

var builder = WebApplication.CreateBuilder(options);

// Only enable static web assets in development - they're not available in production
if (!isProduction)
{
    try
    {
        builder.WebHost.UseStaticWebAssets();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] Static web assets not available: {ex.Message}");
    }
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
        // No database configured - use a placeholder to allow app to start
        connectionString = "Host=localhost;Database=placeholder";
        databaseConfigured = false;
        Console.WriteLine($"[{DateTime.UtcNow:O}] WARNING: DATABASE_URL not set. Application will start in limited mode.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR parsing DATABASE_URL: {ex.Message}");
    connectionString = "Host=localhost;Database=placeholder";
    databaseConfigured = false;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());
builder.Services.AddScoped<ReportingService>();
builder.Services.AddScoped<ReportExportService>();
builder.Services.AddScoped<RatingEngineService>();
builder.Services.AddScoped<DRSReconciliationService>();
builder.Services.AddScoped<InvoicingService>();
builder.Services.AddScoped<ShipmentStatusService>();
builder.Services.AddScoped<MAWBService>();
builder.Services.AddScoped<AWBNumberService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ImportExcelService>();
builder.Services.AddScoped<ShipmentExcelService>();
builder.Services.AddSingleton<BarcodeService>();
builder.Services.AddScoped<AWBPrintService>(sp => new AWBPrintService(sp.GetRequiredService<IWebHostEnvironment>()));
builder.Services.AddScoped<IDemoDataService, DemoDataService>();
builder.Services.AddScoped<PODUpdateService>();
builder.Services.AddScoped<PODExcelService>();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<BookingWebhookService>();
builder.Services.AddScoped<EmpostFeeReportService>();
builder.Services.AddScoped<IEmpostService, EmpostService>();
builder.Services.AddScoped<IBankReconciliationService, BankReconciliationService>();
builder.Services.AddScoped<IBankStatementImportService, BankStatementImportService>();
builder.Services.AddScoped<CODRemittanceService>();
builder.Services.AddScoped<PickupCommitmentService>();
builder.Services.AddScoped<PickupIncentiveService>();
builder.Services.AddScoped<TransferOrderService>();
builder.Services.AddScoped<GlobalSearchService>();
builder.Services.AddScoped<IPageErrorHandler, PageErrorHandler>();
builder.Services.AddScoped<ISLAPdfService, SLAPdfService>();
builder.Services.AddScoped<RateCardImportService>();
builder.Services.AddScoped<AWBStockService>();
builder.Services.AddScoped<PrepaidService>();
builder.Services.AddScoped<ISetupService, SetupService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGmailEmailService, GmailEmailService>();
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

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    if (!path.StartsWith("/setup") && !path.StartsWith("/_") && !path.StartsWith("/health") && 
        !path.Contains(".") && !path.StartsWith("/api"))
    {
        using var scope = context.RequestServices.CreateScope();
        var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();
        
        try
        {
            var setupRequired = await setupService.IsSetupRequiredAsync();
            if (setupRequired)
            {
                context.Response.Redirect("/setup");
                return;
            }
        }
        catch
        {
        }
    }
    
    await next();
});

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

app.MapGet("/api/report/awb/{id:long}", async (long id, ApplicationDbContext db, AWBPrintService printService) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var pdf = printService.GenerateA5AWB(awb);
    return Results.File(pdf, "application/pdf", $"AWB-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/awb-label/{id:long}", async (long id, ApplicationDbContext db, AWBPrintService printService) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var pdf = printService.GenerateLabel(awb);
    return Results.File(pdf, "application/pdf", $"Label-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/shipment-invoice/{id:long}", async (long id, ApplicationDbContext db, AWBPrintService printService, IWebHostEnvironment env) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    
    byte[]? logoData = null;
    if (awb.BranchId.HasValue)
    {
        var branch = await db.Branches.Include(b => b.Company).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
        if (branch?.Company?.Logo != null)
        {
            var logoPath = Path.Combine(env.WebRootPath, branch.Company.Logo.TrimStart('/'));
            if (File.Exists(logoPath))
            {
                logoData = await File.ReadAllBytesAsync(logoPath);
            }
        }
    }
    
    var pdf = printService.GenerateShipmentInvoice(awb, logoData, $"INV-{awb.AWBNo}");
    return Results.File(pdf, "application/pdf", $"ShipmentInvoice-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/tracking/{awbNo}", async (string awbNo, ApplicationDbContext db, AWBPrintService printService, IWebHostEnvironment env) =>
{
    var awb = await db.InscanMasters
        .FirstOrDefaultAsync(a => a.AWBNo == awbNo);
    if (awb == null) return Results.NotFound("AWB not found");
    
    var timeline = await db.ShipmentStatusHistories
        .Include(h => h.Status)
        .Where(h => h.InscanMasterId == awb.Id)
        .OrderByDescending(h => h.ChangedAt)
        .ToListAsync();
    
    string? serviceTypeName = null;
    if (awb.ShipmentModeId.HasValue)
    {
        var shipmentMode = await db.ShipmentModes.FindAsync(awb.ShipmentModeId.Value);
        serviceTypeName = shipmentMode?.Name;
    }
    
    byte[]? logoData = null;
    if (awb.BranchId.HasValue)
    {
        var branch = await db.Branches.Include(b => b.Company).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
        if (branch?.Company?.Logo != null)
        {
            var logoPath = Path.Combine(env.WebRootPath, branch.Company.Logo.TrimStart('/'));
            if (File.Exists(logoPath))
            {
                logoData = await File.ReadAllBytesAsync(logoPath);
            }
        }
    }
    
    var pdf = printService.GenerateTrackingReport(awb, timeline, serviceTypeName, logoData);
    return Results.File(pdf, "application/pdf", $"Tracking-{awb.AWBNo}.pdf");
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

app.MapGet("/api/report/sla-agreement/{id:long}", async (long id, ISLAPdfService slaPdfService) =>
{
    try
    {
        var pdf = await slaPdfService.GenerateSLAAgreementPdfAsync(id);
        return Results.File(pdf, "application/pdf", $"SLA-Agreement-{id}.pdf");
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
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
Console.WriteLine($"[{DateTime.UtcNow:O}] All middleware and routes configured successfully");

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
                CREATE TABLE IF NOT EXISTS ""ShipmentModes"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(50) NOT NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" TEXT,
                    ""SortOrder"" INT NOT NULL DEFAULT 0,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ShipmentModes_Code"" ON ""ShipmentModes"" (""Code"");
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

            // Create Currencies table
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""Currencies"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(10) NOT NULL,
                    ""Name"" VARCHAR(100) NOT NULL,
                    ""Symbol"" VARCHAR(10),
                    ""DecimalPlaces"" INT NOT NULL DEFAULT 2,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Currencies_Code"" ON ""Currencies"" (""Code"");
            ", stoppingToken);

            // Seed Currencies
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'AED', 'UAE Dirham', 'د.إ', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'AED');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'SAR', 'Saudi Riyal', '﷼', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'SAR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'QAR', 'Qatari Riyal', '﷼', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'QAR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'KWD', 'Kuwaiti Dinar', 'د.ك', 3, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'KWD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'BHD', 'Bahraini Dinar', '.د.ب', 3, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'BHD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'OMR', 'Omani Rial', 'ر.ع.', 3, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'OMR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'JOD', 'Jordanian Dinar', 'د.ا', 3, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'JOD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'USD', 'US Dollar', '$', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'USD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'INR', 'Indian Rupee', '₹', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'INR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'EUR', 'Euro', '€', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'EUR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'PKR', 'Pakistani Rupee', 'Rs', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'PKR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'BDT', 'Bangladeshi Taka', '৳', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'BDT');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'MYR', 'Malaysian Ringgit', 'RM', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'MYR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'IDR', 'Indonesian Rupiah', 'Rp', 0, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'IDR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsActive"", ""CreatedAt"")
                SELECT 'PHP', 'Philippine Peso', '₱', 2, TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'PHP');
            ", stoppingToken);

            // Seed Countries
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'United Arab Emirates', 'AE', 'AE', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'AE');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Saudi Arabia', 'SA', 'SA', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'SA');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Qatar', 'QA', 'QA', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'QA');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kuwait', 'KW', 'KW', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'KW');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bahrain', 'BH', 'BH', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'BH');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Oman', 'OM', 'OM', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'OM');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Jordan', 'JO', 'JO', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'JO');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'India', 'IN', 'IN', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'IN');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Pakistan', 'PK', 'PK', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'PK');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bangladesh', 'BD', 'BD', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'BD');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Malaysia', 'MY', 'MY', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'MY');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Indonesia', 'ID', 'ID', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'ID');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""CreatedAt"")
                SELECT 'Philippines', 'PH', 'PH', TRUE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'PH');
            ", stoppingToken);

            // Seed States - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dubai', 'DXB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DXB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Abu Dhabi', 'AUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sharjah', 'SHJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SHJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Ajman', 'AJM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AJM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Fujairah', 'FUJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'FUJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Ras Al Khaimah', 'RAK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RAK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Umm Al Quwain', 'UAQ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'UAQ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
            ", stoppingToken);

            // Seed States - Saudi Arabia
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Riyadh', 'RUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Makkah', 'MKH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MKH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Madinah', 'MDN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MDN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Eastern Province', 'EP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'EP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Asir', 'ASR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ASR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
            ", stoppingToken);

            // Seed States - Qatar, Kuwait, Bahrain
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Doha', 'DOH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DOH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Al Rayyan', 'RYN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RYN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Al Wakrah', 'WKR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WKR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Al Asimah', 'ASM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ASM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Hawalli', 'HWL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'HWL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Farwaniya', 'FRW', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'FRW' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Capital', 'CAP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CAP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Muharraq', 'MUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Northern', 'NOR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NOR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
            ", stoppingToken);

            // Seed States - Oman, Jordan
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Muscat', 'MSC', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MSC' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dhofar', 'DHF', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DHF' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'North Al Batinah', 'NAB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NAB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Amman', 'AMN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AMN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Irbid', 'IRB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'IRB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Zarqa', 'ZRQ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ZRQ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
            ", stoppingToken);

            // Seed States - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Maharashtra', 'MH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Delhi', 'DL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Karnataka', 'KA', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KA' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Tamil Nadu', 'TN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'TN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Gujarat', 'GJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'GJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kerala', 'KL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'West Bengal', 'WB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Uttar Pradesh', 'UP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'UP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Rajasthan', 'RJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Punjab', 'PB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Telangana', 'TG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'TG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Andhra Pradesh', 'AP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
            ", stoppingToken);

            // Seed States - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Punjab', 'PJB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PJB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sindh', 'SND', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SND' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'KPK', 'KPK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KPK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Balochistan', 'BLN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'BLN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Islamabad', 'ISB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ISB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dhaka', 'DHK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DHK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Chittagong', 'CTG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CTG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Rajshahi', 'RAJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RAJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Khulna', 'KHU', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KHU' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sylhet', 'SYL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SYL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
            ", stoppingToken);

            // Seed States - Malaysia, Indonesia, Philippines
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Selangor', 'SEL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SEL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kuala Lumpur', 'KL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Penang', 'PNG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PNG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Johor', 'JHR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'JHR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sabah', 'SBH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SBH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sarawak', 'SRK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SRK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Jakarta', 'JKT', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'JKT' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'West Java', 'WJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'East Java', 'EJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'EJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Central Java', 'CJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bali', 'BAL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'BAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'North Sumatra', 'NSM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NSM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Metro Manila', 'MNL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MNL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Cebu', 'CEB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CEB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Davao', 'DAV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DAV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Calabarzon', 'CAL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""CreatedAt"")
                SELECT 'Central Luzon', 'CLZ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CLZ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
            ", stoppingToken);

            // Seed Cities - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dubai City', 'DXBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DXB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DXBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Jebel Ali', 'JBL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DXB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JBL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Abu Dhabi City', 'AUHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AUHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Al Ain', 'AAC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AAC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Sharjah City', 'SHJC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'SHJ'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SHJC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Ajman City', 'AJMC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AJM'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AJMC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Fujairah City', 'FUJC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'FUJ'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'FUJC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Ras Al Khaimah City', 'RAKC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'RAK'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'RAKC');
            ", stoppingToken);

            // Seed Cities - Saudi Arabia, Qatar, Kuwait
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Riyadh City', 'RUHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'RUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'RUHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Makkah City', 'MKHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MKH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MKHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Jeddah', 'JED', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MKH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JED');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Madinah City', 'MDNC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MDN'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MDNC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dammam', 'DMM', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'EP'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DMM');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Doha City', 'DOHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DOH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DOHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kuwait City', 'KWC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'ASM'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KWC');
            ", stoppingToken);

            // Seed Cities - Bahrain, Oman, Jordan
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Manama', 'MAN', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CAP'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MAN');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Muscat City', 'MSCC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MSC'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MSCC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Salalah', 'SLL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DHF'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SLL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Amman City', 'AMNC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AMN'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AMNC');
            ", stoppingToken);

            // Seed Cities - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Mumbai', 'BOM', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'BOM');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Pune', 'PNQ', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'PNQ');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'New Delhi', 'DEL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DEL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bangalore', 'BLR', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KA' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'BLR');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Chennai', 'MAA', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'TN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MAA');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Ahmedabad', 'AMD', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'GJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AMD');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kolkata', 'CCU', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'WB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CCU');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Hyderabad', 'HYD', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'TG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'HYD');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kochi', 'COK', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'COK');
            ", stoppingToken);

            // Seed Cities - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Lahore', 'LHE', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'PJB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'LHE');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Karachi', 'KHI', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'SND'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KHI');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Islamabad City', 'ISBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'ISB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'ISBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Dhaka City', 'DHKC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DHK'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DHKC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Chittagong City', 'CTGC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CTG'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CTGC');
            ", stoppingToken);

            // Seed Cities - Malaysia, Indonesia, Philippines
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Kuala Lumpur City', 'KLC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KLC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'George Town', 'PEN', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'PNG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'PEN');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Johor Bahru', 'JHB', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'JHR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JHB');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Jakarta City', 'JKTC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'JKT' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JKTC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Surabaya', 'SUB', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'EJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SUB');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Denpasar', 'DPS', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'BAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), FALSE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DPS');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Manila', 'MLA', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MNL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MLA');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Cebu City', 'CEBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CEB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CEBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""CreatedAt"")
                SELECT 'Davao City', 'DAVC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DAV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DAVC');
            ", stoppingToken);

            // Seed Locations - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Deira', 'DEIRA', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DXBC'), '00000', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'DEIRA');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bur Dubai', 'BURDXB', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DXBC'), '00000', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BURDXB');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Abu Dhabi Downtown', 'AUHDTN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'AUHC'), '00000', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'AUHDTN');
            ", stoppingToken);

            // Seed Locations - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Andheri', 'ANDH', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BOM'), '400069', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'ANDH');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Bandra', 'BNDR', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BOM'), '400050', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BNDR');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Connaught Place', 'CNPL', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DEL'), '110001', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'CNPL');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Whitefield', 'WTFL', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BLR'), '560066', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'WTFL');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Koramangala', 'KRMN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BLR'), '560034', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'KRMN');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'T Nagar', 'TNGR', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MAA'), '600017', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'TNGR');
            ", stoppingToken);

            // Seed Locations - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Gulberg', 'GULB', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'LHE'), '54000', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'GULB');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Clifton', 'CLFT', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'KHI'), '75600', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'CLFT');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Gulshan', 'GLSN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DHKC'), '1212', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'GLSN');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Banani', 'BNNI', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DHKC'), '1213', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BNNI');
            ", stoppingToken);

            // Seed Locations - Southeast Asia
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'KLCC', 'KLCC', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'KLC'), '50088', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'KLCC');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Menteng', 'MNTG', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'JKTC'), '10310', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'MNTG');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'Makati', 'MKAT', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MLA'), '1200', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'MKAT');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""CreatedAt"")
                SELECT 'BGC', 'BGC', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MLA'), '1630', TRUE, TRUE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BGC');
            ", stoppingToken);

            _logger.LogInformation("Geographical data seeding completed");

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

            // Seed Vehicles
            if (!await dbContext.Vehicles.AnyAsync(stoppingToken))
            {
                var branch = await dbContext.Branches.FirstOrDefaultAsync(b => !b.IsDeleted && b.IsActive, stoppingToken);
                var company = await dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted, stoppingToken);
                if (branch != null && company != null)
                {
                    var vehicles = new[]
                    {
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-1001", VehicleType = "Motorcycle", Make = "Honda", Model = "CBR150", Year = 2022, Capacity = 20, DriverName = "Ahmed Khan", DriverPhone = "+971-50-111-0001", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-1002", VehicleType = "Motorcycle", Make = "Yamaha", Model = "FZ25", Year = 2023, Capacity = 25, DriverName = "Mohammad Ali", DriverPhone = "+971-50-111-0002", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-2001", VehicleType = "Van", Make = "Toyota", Model = "Hiace", Year = 2021, Capacity = 1000, DriverName = "Rashid Saeed", DriverPhone = "+971-50-222-0001", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-2002", VehicleType = "Van", Make = "Nissan", Model = "Urvan", Year = 2022, Capacity = 1200, DriverName = "Faisal Ahmed", DriverPhone = "+971-50-222-0002", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-2003", VehicleType = "Van", Make = "Ford", Model = "Transit", Year = 2023, Capacity = 1500, DriverName = "Salim Omar", DriverPhone = "+971-50-222-0003", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-3001", VehicleType = "Truck", Make = "Isuzu", Model = "NPR", Year = 2020, Capacity = 3000, DriverName = "Khalid Hassan", DriverPhone = "+971-50-333-0001", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-3002", VehicleType = "Truck", Make = "Mitsubishi", Model = "Fuso", Year = 2021, Capacity = 5000, DriverName = "Yusuf Ibrahim", DriverPhone = "+971-50-333-0002", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-4001", VehicleType = "Pickup", Make = "Toyota", Model = "Hilux", Year = 2022, Capacity = 800, DriverName = "Samir Abbas", DriverPhone = "+971-50-444-0001", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-4002", VehicleType = "Pickup", Make = "Nissan", Model = "Navara", Year = 2023, Capacity = 900, DriverName = "Tariq Mahmoud", DriverPhone = "+971-50-444-0002", CreatedAt = DateTime.UtcNow },
                        new Net4Courier.Operations.Entities.Vehicle { CompanyId = company.Id, BranchId = branch.Id, VehicleNo = "DXB-5001", VehicleType = "Bike", Make = "TVS", Model = "Apache", Year = 2023, Capacity = 15, DriverName = "Imran Shaikh", DriverPhone = "+971-50-555-0001", CreatedAt = DateTime.UtcNow }
                    };
                    dbContext.Vehicles.AddRange(vehicles);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Seeded Vehicles");
                }
            }

            if (!await dbContext.Ports.AnyAsync(stoppingToken))
            {
                var ports = new[]
                {
                    // UAE Airports
                    new Net4Courier.Masters.Entities.Port { Code = "DXB", Name = "Dubai International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "DXB", ICAOCode = "OMDB", City = "Dubai", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "DWC", Name = "Al Maktoum International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "DWC", ICAOCode = "OMDW", City = "Dubai", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AUH", Name = "Abu Dhabi International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "AUH", ICAOCode = "OMAA", City = "Abu Dhabi", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "SHJ", Name = "Sharjah International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "SHJ", ICAOCode = "OMSJ", City = "Sharjah", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "RKT", Name = "Ras Al Khaimah International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "RKT", ICAOCode = "OMRK", City = "Ras Al Khaimah", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    // UAE Seaports
                    new Net4Courier.Masters.Entities.Port { Code = "AEJEA", Name = "Jebel Ali Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AEJEA", City = "Dubai", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 10, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AERSM", Name = "Port Rashid", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AERSM", City = "Dubai", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 11, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AEKHL", Name = "Khalifa Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AEKHL", City = "Abu Dhabi", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 12, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AEZAY", Name = "Zayed Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AEZAY", City = "Abu Dhabi", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 13, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AESHJ", Name = "Sharjah Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AESHJ", City = "Sharjah", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 14, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "AEFUJ", Name = "Fujairah Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "AEFUJ", City = "Fujairah", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 15, IsActive = true, CreatedAt = DateTime.UtcNow },
                    // UAE Land Borders
                    new Net4Courier.Masters.Entities.Port { Code = "HATTA", Name = "Hatta Border Crossing", PortType = Net4Courier.Masters.Entities.PortType.LandBorder, City = "Hatta", State = "Dubai", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 20, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "ALAIN", Name = "Al Ain Border (Hili)", PortType = Net4Courier.Masters.Entities.PortType.LandBorder, City = "Al Ain", State = "Abu Dhabi", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 21, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "GHUWAIFAT", Name = "Ghuwaifat Border (Saudi Arabia)", PortType = Net4Courier.Masters.Entities.PortType.LandBorder, City = "Ghuwaifat", State = "Abu Dhabi", Country = "United Arab Emirates", CountryCode = "AE", TimeZone = "Asia/Dubai", SortOrder = 22, IsActive = true, CreatedAt = DateTime.UtcNow },
                    // Major International Airports
                    new Net4Courier.Masters.Entities.Port { Code = "LHR", Name = "London Heathrow Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "LHR", ICAOCode = "EGLL", City = "London", Country = "United Kingdom", CountryCode = "GB", TimeZone = "Europe/London", SortOrder = 100, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "JFK", Name = "John F. Kennedy International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "JFK", ICAOCode = "KJFK", City = "New York", Country = "United States", CountryCode = "US", TimeZone = "America/New_York", SortOrder = 101, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "SIN", Name = "Singapore Changi Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "SIN", ICAOCode = "WSSS", City = "Singapore", Country = "Singapore", CountryCode = "SG", TimeZone = "Asia/Singapore", SortOrder = 102, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "HKG", Name = "Hong Kong International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "HKG", ICAOCode = "VHHH", City = "Hong Kong", Country = "Hong Kong", CountryCode = "HK", TimeZone = "Asia/Hong_Kong", SortOrder = 103, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "FRA", Name = "Frankfurt Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "FRA", ICAOCode = "EDDF", City = "Frankfurt", Country = "Germany", CountryCode = "DE", TimeZone = "Europe/Berlin", SortOrder = 104, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "CDG", Name = "Paris Charles de Gaulle Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "CDG", ICAOCode = "LFPG", City = "Paris", Country = "France", CountryCode = "FR", TimeZone = "Europe/Paris", SortOrder = 105, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "BOM", Name = "Chhatrapati Shivaji Maharaj International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "BOM", ICAOCode = "VABB", City = "Mumbai", Country = "India", CountryCode = "IN", TimeZone = "Asia/Kolkata", SortOrder = 106, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "DEL", Name = "Indira Gandhi International Airport", PortType = Net4Courier.Masters.Entities.PortType.Airport, IATACode = "DEL", ICAOCode = "VIDP", City = "New Delhi", Country = "India", CountryCode = "IN", TimeZone = "Asia/Kolkata", SortOrder = 107, IsActive = true, CreatedAt = DateTime.UtcNow },
                    // Major International Seaports
                    new Net4Courier.Masters.Entities.Port { Code = "SGSIN", Name = "Port of Singapore", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "SGSIN", City = "Singapore", Country = "Singapore", CountryCode = "SG", TimeZone = "Asia/Singapore", SortOrder = 200, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "CNSHA", Name = "Port of Shanghai", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "CNSHA", City = "Shanghai", Country = "China", CountryCode = "CN", TimeZone = "Asia/Shanghai", SortOrder = 201, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "NLRTM", Name = "Port of Rotterdam", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "NLRTM", City = "Rotterdam", Country = "Netherlands", CountryCode = "NL", TimeZone = "Europe/Amsterdam", SortOrder = 202, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.Port { Code = "INNSA", Name = "Jawaharlal Nehru Port", PortType = Net4Courier.Masters.Entities.PortType.Seaport, UNLocode = "INNSA", City = "Navi Mumbai", Country = "India", CountryCode = "IN", TimeZone = "Asia/Kolkata", SortOrder = 203, IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                dbContext.Ports.AddRange(ports);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Seeded Ports data with {Count} ports", ports.Length);
            }

            if (!await dbContext.ShipmentModes.AnyAsync(stoppingToken))
            {
                var shipmentModes = new[]
                {
                    new Net4Courier.Masters.Entities.ShipmentMode { Code = "AIR", Name = "Air", Description = "Air freight shipments", SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ShipmentMode { Code = "SEA", Name = "Sea", Description = "Sea freight shipments", SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ShipmentMode { Code = "ROAD", Name = "Road/Surface", Description = "Road/surface freight shipments", SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ShipmentMode { Code = "RAIL", Name = "Rail", Description = "Rail freight shipments", SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.ShipmentMode { Code = "MULTI", Name = "Multimodal", Description = "Combined transport modes", SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                dbContext.ShipmentModes.AddRange(shipmentModes);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Seeded Shipment Modes");
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
