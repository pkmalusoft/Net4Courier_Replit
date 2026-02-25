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
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;
using QuestPDF.Infrastructure;

// Log startup immediately
var buildTimestamp = System.IO.File.Exists("build_timestamp.txt") 
    ? System.IO.File.ReadAllText("build_timestamp.txt").Trim() 
    : "unknown";
Console.WriteLine($"=====================================================");
Console.WriteLine($"  NET4COURIER BUILD INFO");
Console.WriteLine($"  Build: {buildTimestamp}");
Console.WriteLine($"  Started: {DateTime.UtcNow:O}");
Console.WriteLine($"  Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"  DATABASE_URL: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")) ? "NOT SET" : "SET")}");
Console.WriteLine($"=====================================================");

// Handle command-line utilities (runs without starting web server)
if (args.Length >= 3 && args[0] == "--reset-password")
{
    await HandlePasswordReset(args[1], args[2]);
    return;
}

if (args.Length >= 1 && args[0] == "--help")
{
    Console.WriteLine("Net4Courier Command Line Utilities:");
    Console.WriteLine("  --reset-password <username> <password>  Reset user password");
    Console.WriteLine("  --help                                   Show this help message");
    return;
}

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

var appDir = AppContext.BaseDirectory;
var projectDir = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", ".."));
var webRootDir = Path.Combine(projectDir, "wwwroot");

if (!Directory.Exists(webRootDir))
{
    projectDir = Directory.GetCurrentDirectory();
    webRootDir = Path.Combine(projectDir, "wwwroot");
}

if (!Directory.Exists(webRootDir))
{
    var candidate = Path.Combine(Directory.GetCurrentDirectory(), "src", "Net4Courier.Web", "wwwroot");
    if (Directory.Exists(candidate))
    {
        projectDir = Path.Combine(Directory.GetCurrentDirectory(), "src", "Net4Courier.Web");
        webRootDir = candidate;
    }
}

Console.WriteLine($"[{DateTime.UtcNow:O}] Resolved ContentRoot: {projectDir}");
Console.WriteLine($"[{DateTime.UtcNow:O}] Resolved WebRoot: {webRootDir}");
Console.WriteLine($"[{DateTime.UtcNow:O}] WebRoot exists: {Directory.Exists(webRootDir)}");

var options = new WebApplicationOptions
{
    Args = args,
    EnvironmentName = isProduction ? "Production" : null,
    ContentRootPath = projectDir,
    WebRootPath = webRootDir
};

var builder = WebApplication.CreateBuilder(options);

try
{
    builder.WebHost.UseStaticWebAssets();
    Console.WriteLine($"[{DateTime.UtcNow:O}] Static web assets configured successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] Static web assets not available (will use wwwroot copy): {ex.Message}");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor().AddCircuitOptions(options => 
{
    options.DetailedErrors = builder.Configuration.GetValue<bool>("DetailedErrors", false);
});

builder.Services.AddHttpContextAccessor();

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
        
        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};Timeout=30;Command Timeout=60";
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

builder.Services.AddScoped<IAuditContextProvider, AuditContextProvider>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null))
    .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null))
    .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()), ServiceLifetime.Scoped);


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());
builder.Services.AddScoped<ReportingService>();
builder.Services.AddScoped<ReportExportService>();
builder.Services.AddScoped<RatingEngineService>();
builder.Services.AddScoped<DRSReconciliationService>();
builder.Services.AddScoped<InvoicingService>();
builder.Services.AddHttpClient<ExchangeRateService>();
builder.Services.AddScoped<ApprovalWorkflowService>();
builder.Services.AddScoped<DatabaseBackupService>();
builder.Services.AddScoped<ShipmentStatusService>();
builder.Services.AddScoped<Net4Courier.Infrastructure.Services.SchemaAutoSyncService>();
builder.Services.AddScoped<StatusEventMappingService>();
builder.Services.AddScoped<MAWBService>();
builder.Services.AddScoped<AWBNumberService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ImportExcelService>();
builder.Services.AddScoped<ShipmentExcelService>();
builder.Services.AddSingleton<BarcodeService>();
builder.Services.AddScoped<AWBPrintService>();
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
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<IPageErrorHandler, PageErrorHandler>();
builder.Services.AddScoped<ISLAPdfService, SLAPdfService>();
builder.Services.AddScoped<RateCardImportService>();
builder.Services.AddScoped<AWBStockService>();
builder.Services.AddScoped<PrepaidService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGmailEmailService, GmailEmailService>();

// TrueBooks GL Module Services
builder.Services.AddScoped<Truebooks.Platform.Core.MultiTenancy.ITenantContext, Net4Courier.Web.Services.SingleTenantContext>();
builder.Services.AddDbContextFactory<Truebooks.Platform.Core.Infrastructure.PlatformDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)), ServiceLifetime.Scoped);

// Register TrueBooks Platform Finance services
Truebooks.Platform.Finance.Extensions.FinanceServiceExtensions.AddPlatformFinance(builder.Services);

// GL Services are provided by TrueBooks Platform Finance package (via AddPlatformFinance above)

// Register Cash & Bank and Journal Entry services
builder.Services.AddScoped<Net4Courier.Web.Services.CashBank.ICashBankTransactionService, Net4Courier.Web.Services.CashBank.CashBankTransactionService>();
builder.Services.AddScoped<Net4Courier.Web.Services.CashBank.IBankAccountService, Net4Courier.Web.Services.CashBank.BankAccountService>();
builder.Services.AddScoped<Net4Courier.Web.Services.CashBank.IPaymentAllocationService, Net4Courier.Web.Services.CashBank.PaymentAllocationService>();
builder.Services.AddScoped<Net4Courier.Web.Services.CashBank.IVoucherAttachmentService, Net4Courier.Web.Services.CashBank.VoucherAttachmentService>();
builder.Services.AddScoped<Net4Courier.Web.Services.CashBank.IJournalEntryService, Net4Courier.Web.Services.CashBank.JournalEntryService>();

// GL Module Services
builder.Services.AddScoped<Net4Courier.Web.Services.IFinancialYearService, Net4Courier.Web.Services.FinancialYearService>();
builder.Services.AddScoped<Net4Courier.Web.Services.IAccountHeadService, Net4Courier.Web.Services.AccountHeadService>();
builder.Services.AddScoped<Net4Courier.Web.Services.IPartyService, Net4Courier.Web.Services.PartyService>();

// GL Master Data Services (Legacy TrueBooks - kept for backward compatibility)
builder.Services.AddScoped<Net4Courier.Web.Interfaces.IChartOfAccountsService, Net4Courier.Web.Services.GL.ChartOfAccountsService>();
builder.Services.AddScoped<Net4Courier.Web.Interfaces.ICurrencyService, Net4Courier.Web.Services.GL.CurrencyService>();
builder.Services.AddScoped<Net4Courier.Web.Interfaces.ITaxCodeService, Net4Courier.Web.Services.GL.TaxCodeService>();
builder.Services.AddScoped<Net4Courier.Web.Interfaces.IVoucherNumberingService, Net4Courier.Web.Services.GL.VoucherNumberingService>();
builder.Services.AddScoped<Net4Courier.Web.Interfaces.IAccountClassificationService, Net4Courier.Web.Services.GL.AccountClassificationService>();

// GL Native Services (Net4Courier with long IDs)
builder.Services.AddScoped<Net4Courier.Web.Interfaces.IGLChartOfAccountsService, Net4Courier.Web.Services.GL.GLChartOfAccountsService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
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

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    if (path == "/health" || path == "/healthz")
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync($"{{\"status\":\"Healthy\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}");
        return;
    }
    if (path == "/" || path == "")
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var accept = context.Request.Headers["Accept"].ToString();
        var isHealthProbe = string.IsNullOrEmpty(userAgent) 
            || userAgent.Contains("GoogleHC", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("kube-probe", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Replit", StringComparison.OrdinalIgnoreCase)
            || (!accept.Contains("text/html") && !context.Request.Headers.ContainsKey("Cookie"));
        if (isHealthProbe)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"status\":\"Healthy\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}");
            return;
        }
    }
    await next();
});

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

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync($"{{\"status\":\"Healthy\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}");
        return;
    }
    if (!DatabaseInitializationService.IsReady && context.Request.Path == "/" 
        && !context.Request.Path.StartsWithSegments("/_blazor")
        && !context.Request.Path.StartsWithSegments("/_framework"))
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Net4Courier</title><meta http-equiv='refresh' content='5'><style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f5f5f5;}div{text-align:center;}h2{color:#333;}</style></head><body><div><h2>Net4Courier</h2><p>Application is starting up, please wait...</p></div></body></html>");
        return;
    }
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCookiePolicy();
app.UseStaticFiles();

var binWwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
if (Directory.Exists(binWwwroot) && binWwwroot != app.Environment.WebRootPath)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(binWwwroot),
        RequestPath = ""
    });
    startupLogger.LogInformation("Additional static files from: {Path}", binWwwroot);
}

var publishWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (Directory.Exists(publishWwwroot) && publishWwwroot != app.Environment.WebRootPath && publishWwwroot != binWwwroot)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(publishWwwroot),
        RequestPath = ""
    });
    startupLogger.LogInformation("Additional static files from: {Path}", publishWwwroot);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/api/company-logo", async (ApplicationDbContext db, IWebHostEnvironment env) =>
{
    try
    {
        var company = await db.Companies.FirstOrDefaultAsync();
        if (company?.Logo != null && !string.IsNullOrEmpty(company.Logo))
        {
            if (company.Logo.StartsWith("data:"))
            {
                var dataUriParts = company.Logo.Split(',', 2);
                if (dataUriParts.Length == 2)
                {
                    var header = dataUriParts[0];
                    var base64Data = dataUriParts[1];
                    var contentType = header.Replace("data:", "").Replace(";base64", "");
                    try
                    {
                        var bytes = Convert.FromBase64String(base64Data);
                        return Results.File(bytes, contentType);
                    }
                    catch (FormatException)
                    {
                        return Results.NotFound("Logo data is corrupted");
                    }
                }
            }
            else
            {
                var logoPath = Path.Combine(env.WebRootPath, company.Logo.TrimStart('/'));
                if (File.Exists(logoPath))
                {
                    var ext = Path.GetExtension(logoPath).ToLower();
                    var contentType = ext switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        ".svg" => "image/svg+xml",
                        ".webp" => "image/webp",
                        _ => "image/png"
                    };
                    var bytes = await File.ReadAllBytesAsync(logoPath);
                    return Results.File(bytes, contentType);
                }
            }
        }
        var fallbackPath = Path.Combine(env.WebRootPath, "images", "logo.png");
        if (File.Exists(fallbackPath))
        {
            var fallbackBytes = await File.ReadAllBytesAsync(fallbackPath);
            return Results.File(fallbackBytes, "image/png");
        }
        return Results.NotFound();
    }
    catch
    {
        return Results.NotFound();
    }
});

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

app.MapGet("/api/backup/download", (string token, DatabaseBackupService backupService, HttpContext httpContext) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(token))
            return Results.Content(BackupErrorPage("Invalid Request", "No download token was provided. Please go back to the Database Backup page and create a new backup."), "text/html");

        var (valid, filePath, fileName) = backupService.ValidateAndConsumeToken(token);
        if (!valid || string.IsNullOrEmpty(filePath))
            return Results.Content(BackupErrorPage("Download Unavailable", "This backup download link has expired, was already used, or is invalid. Backup links are single-use and expire after 10 minutes. Please go back to the Database Backup page and create a new backup."), "text/html");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 65536, options: FileOptions.DeleteOnClose | FileOptions.SequentialScan);

        return Results.File(stream, "application/sql", fileName ?? "backup.sql");
    }
    catch (Exception ex)
    {
        return Results.Content(BackupErrorPage("Download Error", $"An error occurred while preparing the backup download. Please try creating a new backup. Details: {ex.Message}"), "text/html");
    }
});

app.MapGet("/api/report/awb/{id:long}", async (long id, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FindAsync(id);
        if (awb == null) return Results.NotFound("AWB not found");
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? a5AccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var a5Cust = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            a5AccountNo = a5Cust?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateA5AWB(awb, companyName, logoData, website, branchCurrency, a5AccountNo);
        var fileName = inline == true ? null : $"AWB-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating AWB print for ID {Id}", id);
        return Results.Problem($"Error generating AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-duplex/{id:long}", async (long id, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FindAsync(id);
        if (awb == null) return Results.NotFound("AWB not found");
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? duplexAccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var duplexCust = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            duplexAccountNo = duplexCust?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateA4DuplexAWB(awb, companyName, logoData, website, branchCurrency, duplexAccountNo);
        var fileName = inline == true ? null : $"AWB-Duplex-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating duplex AWB print for ID {Id}", id);
        return Results.Problem($"Error generating duplex AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-duplex-by-awbno/{awbNo}", async (string awbNo, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
        if (awb == null)
        {
            var importShipment = await db.ImportShipments.FirstOrDefaultAsync(i => i.AWBNo == awbNo && !i.IsDeleted);
            if (importShipment == null) return Results.NotFound("AWB not found");
            awb = MapImportToInscanMaster(importShipment);
        }
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? duplexAccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var duplexCust = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            duplexAccountNo = duplexCust?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateA4DuplexAWB(awb, companyName, logoData, website, branchCurrency, duplexAccountNo);
        var fileName = inline == true ? null : $"AWB-Duplex-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating duplex AWB print for AWBNo {AWBNo}", awbNo);
        return Results.Problem($"Error generating duplex AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-label/{id:long}", async (long id, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FindAsync(id);
        if (awb == null) return Results.NotFound("AWB not found");
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? customerAccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var customer = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            customerAccountNo = customer?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateLabel(awb, companyName, logoData, branchCurrency, customerAccountNo);
        var fileName = inline == true ? null : $"Label-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating AWB label for ID {Id}", id);
        return Results.Problem($"Error generating AWB label: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-by-awbno/{awbNo}", async (string awbNo, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
        if (awb == null)
        {
            var importShipment = await db.ImportShipments.FirstOrDefaultAsync(i => i.AWBNo == awbNo && !i.IsDeleted);
            if (importShipment == null) return Results.NotFound("AWB not found");
            awb = MapImportToInscanMaster(importShipment);
        }
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? a5AccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var a5Cust = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            a5AccountNo = a5Cust?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateA5AWB(awb, companyName, logoData, website, branchCurrency, a5AccountNo);
        var fileName = inline == true ? null : $"AWB-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating AWB print for AWBNo {AWBNo}", awbNo);
        return Results.Problem($"Error generating AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-label-by-awbno/{awbNo}", async (string awbNo, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
        if (awb == null)
        {
            var importShipment = await db.ImportShipments.FirstOrDefaultAsync(i => i.AWBNo == awbNo && !i.IsDeleted);
            if (importShipment == null) return Results.NotFound("AWB not found");
            awb = MapImportToInscanMaster(importShipment);
        }
        if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
        {
            var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
            awb.BarcodeImage = h;
            awb.BarcodeImageVertical = v;
        }
        byte[]? logoData = null;
        string? companyName = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        await ResolveLocationCodes(awb, db);
        string? customerAccountNo = null;
        if (awb.CustomerId.HasValue)
        {
            var customer = await db.Parties.FirstOrDefaultAsync(p => p.Id == awb.CustomerId);
            customerAccountNo = customer?.CustomerAccountNo;
        }
        printService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = printService.GenerateLabel(awb, companyName, logoData, branchCurrency, customerAccountNo);
        var fileName = inline == true ? null : $"Label-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating AWB label for AWBNo {AWBNo}", awbNo);
        return Results.Problem($"Error generating AWB label: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-bulk/{ids}", async (string ids, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var idList = ids.Split(',').Select(long.Parse).ToList();
        var awbs = await db.InscanMasters.Where(i => idList.Contains(i.Id)).ToListAsync();
        if (!awbs.Any()) return Results.NotFound("No AWBs found");

        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        var firstAwb = awbs.First();
        if (firstAwb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == firstAwb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }

        foreach (var awb in awbs)
        {
            if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
            {
                var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
                awb.BarcodeImage = h;
                awb.BarcodeImageVertical = v;
            }
        }

        await ResolveLocationCodesBulk(awbs, db);
        var bulkA5CustIds = awbs.Where(a => a.CustomerId.HasValue).Select(a => a.CustomerId!.Value).Distinct().ToList();
        var bulkA5AcctNos = await db.Parties.Where(p => bulkA5CustIds.Contains(p.Id)).ToDictionaryAsync(p => (long)p.Id, p => p.CustomerAccountNo ?? "");
        var combinedPdf = printService.GenerateBulkA5AWB(awbs, companyName, logoData, website, branchCurrency, bulkA5AcctNos);
        var fileName = inline == true ? null : $"BulkAWB-{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return Results.File(combinedPdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating bulk AWB print");
        return Results.Problem($"Error generating bulk AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/awb-bulk-by-awbno/{awbNos}", async (string awbNos, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awbNoList = awbNos.Split(',').ToList();
        var awbs = new List<InscanMaster>();
        
        foreach (var awbNo in awbNoList)
        {
            var awb = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
            if (awb == null)
            {
                var importShipment = await db.ImportShipments.FirstOrDefaultAsync(i => i.AWBNo == awbNo && !i.IsDeleted);
                if (importShipment != null)
                    awb = MapImportToInscanMaster(importShipment);
            }
            if (awb != null) awbs.Add(awb);
        }
        
        if (!awbs.Any()) return Results.NotFound("No AWBs found");

        byte[]? logoData = null;
        string? companyName = null;
        string? website = null;
        string? branchCurrency = null;
        var firstAwb = awbs.First();
        if (firstAwb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == firstAwb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            website = branch?.Company?.Website;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
            website ??= company?.Website;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }

        foreach (var awb in awbs)
        {
            if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
            {
                var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
                awb.BarcodeImage = h;
                awb.BarcodeImageVertical = v;
            }
        }

        await ResolveLocationCodesBulk(awbs, db);
        var bulkA5CustIds = awbs.Where(a => a.CustomerId.HasValue).Select(a => a.CustomerId!.Value).Distinct().ToList();
        var bulkA5AcctNos = await db.Parties.Where(p => bulkA5CustIds.Contains(p.Id)).ToDictionaryAsync(p => (long)p.Id, p => p.CustomerAccountNo ?? "");
        var combinedPdf = printService.GenerateBulkA5AWB(awbs, companyName, logoData, website, branchCurrency, bulkA5AcctNos);
        var fileName = inline == true ? null : $"BulkAWB-{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return Results.File(combinedPdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating bulk AWB print by AWB numbers");
        return Results.Problem($"Error generating bulk AWB print: {ex.Message}");
    }
});

app.MapGet("/api/report/label-bulk/{ids}", async (string ids, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var idList = ids.Split(',').Select(long.Parse).ToList();
        var awbs = await db.InscanMasters.Where(i => idList.Contains(i.Id)).ToListAsync();
        if (!awbs.Any()) return Results.NotFound("No AWBs found");

        byte[]? logoData = null;
        string? companyName = null;
        string? branchCurrency = null;
        var firstAwb = awbs.First();
        if (firstAwb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == firstAwb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }

        foreach (var awb in awbs)
        {
            if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
            {
                var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
                awb.BarcodeImage = h;
                awb.BarcodeImageVertical = v;
            }
        }

        await ResolveLocationCodesBulk(awbs, db);
        var customerIds = awbs.Where(a => a.CustomerId.HasValue).Select(a => a.CustomerId!.Value).Distinct().ToList();
        var customerAccountNos = customerIds.Any()
            ? await db.Parties.Where(p => customerIds.Contains(p.Id) && p.CustomerAccountNo != null)
                .ToDictionaryAsync(p => p.Id, p => p.CustomerAccountNo!)
            : new Dictionary<long, string>();
        var combinedPdf = printService.GenerateBulkLabel(awbs, companyName, logoData, branchCurrency, customerAccountNos);
        var fileName = inline == true ? null : $"BulkLabels-{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return Results.File(combinedPdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating bulk label print");
        return Results.Problem($"Error generating bulk label print: {ex.Message}");
    }
});

app.MapGet("/api/report/label-bulk-by-awbno/{awbNos}", async (string awbNos, bool? inline, ApplicationDbContext db, AWBPrintService printService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awbNoList = awbNos.Split(',').ToList();
        var awbs = new List<InscanMaster>();
        
        foreach (var awbNo in awbNoList)
        {
            var awb = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
            if (awb == null)
            {
                var importShipment = await db.ImportShipments.FirstOrDefaultAsync(i => i.AWBNo == awbNo && !i.IsDeleted);
                if (importShipment != null)
                    awb = MapImportToInscanMaster(importShipment);
            }
            if (awb != null) awbs.Add(awb);
        }
        
        if (!awbs.Any()) return Results.NotFound("No AWBs found");

        byte[]? logoData = null;
        string? companyName = null;
        string? branchCurrency = null;
        var firstAwb = awbs.First();
        if (firstAwb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == firstAwb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            companyName = branch?.Company?.Name;
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            companyName ??= company?.Name;
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }

        foreach (var awb in awbs)
        {
            if (awb.BarcodeImage == null && !string.IsNullOrEmpty(awb.AWBNo))
            {
                var (h, v) = barcodeService.GenerateBothBarcodes(awb.AWBNo);
                awb.BarcodeImage = h;
                awb.BarcodeImageVertical = v;
            }
        }

        await ResolveLocationCodesBulk(awbs, db);
        var customerIds2 = awbs.Where(a => a.CustomerId.HasValue).Select(a => a.CustomerId!.Value).Distinct().ToList();
        var customerAccountNos2 = customerIds2.Any()
            ? await db.Parties.Where(p => customerIds2.Contains(p.Id) && p.CustomerAccountNo != null)
                .ToDictionaryAsync(p => p.Id, p => p.CustomerAccountNo!)
            : new Dictionary<long, string>();
        var combinedPdf = printService.GenerateBulkLabel(awbs, companyName, logoData, branchCurrency, customerAccountNos2);
        var fileName = inline == true ? null : $"BulkLabels-{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return Results.File(combinedPdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating bulk label print by AWB numbers");
        return Results.Problem($"Error generating bulk label print: {ex.Message}");
    }
});

app.MapGet("/api/report/shipment-invoice/{id:long}", async (long id, bool? inline, ApplicationDbContext db, AWBPrintService printService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters.FindAsync(id);
        if (awb == null) return Results.NotFound("AWB not found");
        
        byte[]? logoData = null;
        string? branchCurrency = null;
        if (awb.BranchId.HasValue)
        {
            var branch = await db.Branches.Include(b => b.Company).Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == awb.BranchId);
            logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            branchCurrency = branch?.Currency?.Code;
        }
        if (logoData == null)
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
        }
        if (branchCurrency == null)
        {
            var defaultBranch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => !b.IsDeleted);
            branchCurrency = defaultBranch?.Currency?.Code;
        }
        
        await ResolveLocationCodes(awb, db);
        var pdf = printService.GenerateShipmentInvoice(awb, logoData, $"INV-{awb.AWBNo}", branchCurrency);
        var fileName = inline == true ? null : $"ShipmentInvoice-{awb.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating shipment invoice for ID {Id}", id);
        return Results.Problem($"Error generating shipment invoice: {ex.Message}");
    }
});

app.MapGet("/api/report/tracking/{awbNo}", async (string awbNo, bool? inline, ApplicationDbContext db, AWBPrintService printService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var awb = await db.InscanMasters
            .FirstOrDefaultAsync(a => a.AWBNo == awbNo);
        
        if (awb != null)
        {
            var timeline = await db.ShipmentStatusHistories
                .Include(h => h.Status)
                .Where(h => h.InscanMasterId == awb.Id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            var existingLabels = timeline
                .Select(h => (h.Status?.Name ?? h.EventType ?? "").ToLowerInvariant())
                .ToHashSet();

            if (!existingLabels.Any(l => l.Contains("booked") || l.Contains("created") || l.Contains("awb entry")))
            {
                var bookedDate = awb.TransactionDate != default ? awb.TransactionDate : awb.CreatedAt;
                if (bookedDate != default)
                    timeline.Add(new ShipmentStatusHistory { EventType = "Booked", ChangedAt = bookedDate, InscanMasterId = awb.Id });
            }
            if (awb.PickedUpDate.HasValue && !existingLabels.Any(l => l.Contains("picked") || l.Contains("pickup")))
                timeline.Add(new ShipmentStatusHistory { EventType = "Picked Up", ChangedAt = awb.PickedUpDate.Value, InscanMasterId = awb.Id });
            if (awb.BaggedAt.HasValue && !existingLabels.Any(l => l.Contains("bagged") || l.Contains("outscan") || l.Contains("manifest")))
                timeline.Add(new ShipmentStatusHistory { EventType = "Outscanned / Bagged", ChangedAt = awb.BaggedAt.Value, InscanMasterId = awb.Id });
            if (awb.DeliveredDate.HasValue && !existingLabels.Any(l => l.Contains("delivered")))
                timeline.Add(new ShipmentStatusHistory { EventType = "Delivered", ChangedAt = awb.DeliveredDate.Value, InscanMasterId = awb.Id });

            var linkedImport = await db.ImportShipments
                .FirstOrDefaultAsync(i => i.AWBNo == awb.AWBNo && !i.IsDeleted);
            if (linkedImport != null)
            {
                timeline.Add(new ShipmentStatusHistory { EventType = "Import Shipment Created", ChangedAt = linkedImport.CreatedAt, InscanMasterId = awb.Id });
                if (linkedImport.InscannedAt.HasValue)
                    timeline.Add(new ShipmentStatusHistory { EventType = "Import Inscanned", ChangedAt = linkedImport.InscannedAt.Value, Remarks = linkedImport.InscannedByUserName != null ? $"By: {linkedImport.InscannedByUserName}" : null, InscanMasterId = awb.Id });
                if (linkedImport.CustomsClearedAt.HasValue)
                    timeline.Add(new ShipmentStatusHistory { EventType = "Customs Cleared", ChangedAt = linkedImport.CustomsClearedAt.Value, Remarks = linkedImport.CustomsClearedByUserName != null ? $"By: {linkedImport.CustomsClearedByUserName}" : null, InscanMasterId = awb.Id });
                if (linkedImport.ReleasedAt.HasValue)
                    timeline.Add(new ShipmentStatusHistory { EventType = "Released", ChangedAt = linkedImport.ReleasedAt.Value, Remarks = linkedImport.ReleasedByUserName != null ? $"By: {linkedImport.ReleasedByUserName}" : null, InscanMasterId = awb.Id });
                if (linkedImport.HandedOverAt.HasValue)
                    timeline.Add(new ShipmentStatusHistory { EventType = "Handed Over", ChangedAt = linkedImport.HandedOverAt.Value, Remarks = linkedImport.HandedOverToUserName != null ? $"By: {linkedImport.HandedOverToUserName}" : null, InscanMasterId = awb.Id });
            }

            timeline = timeline.OrderByDescending(t => t.ChangedAt).ToList();
            
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
                logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            }
            if (logoData == null)
            {
                var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
                logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            }
            
            await ResolveLocationCodes(awb, db);
            var pdf = printService.GenerateTrackingReport(awb, timeline, serviceTypeName, logoData);
            var fileName = inline == true ? null : $"Tracking-{awb.AWBNo}.pdf";
            return Results.File(pdf, "application/pdf", fileName);
        }
        
        var importShipment = await db.ImportShipments
            .Include(s => s.ImportMaster)
            .FirstOrDefaultAsync(a => a.AWBNo == awbNo);
        
        if (importShipment != null)
        {
            byte[]? logoData = null;
            var branchId = importShipment.ImportMaster?.BranchId;
            if (branchId.HasValue)
            {
                var branch = await db.Branches.Include(b => b.Company).FirstOrDefaultAsync(b => b.Id == branchId);
                logoData = ResolveLogoBytes(branch?.Company?.Logo, env.WebRootPath);
            }
            if (logoData == null)
            {
                var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
                logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
            }
            
            var transactionDate = importShipment.ImportMaster?.TransactionDate ?? importShipment.CreatedAt;
            var pdf = printService.GenerateImportTrackingReport(importShipment, transactionDate, logoData);
            var fileName = inline == true ? null : $"Tracking-{importShipment.AWBNo}.pdf";
            return Results.File(pdf, "application/pdf", fileName);
        }
        
        return Results.NotFound("AWB not found");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating tracking report for AWB {AwbNo}", awbNo);
        return Results.Problem($"Error generating tracking report: {ex.Message}");
    }
});

app.MapGet("/api/report/invoice/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var invoice = await db.Invoices.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == id);
    if (invoice == null) return Results.NotFound();
    
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath ?? env.ContentRootPath);
    
    Net4Courier.Operations.Entities.InscanMaster? shipment = null;
    if (invoice.Details.Any())
    {
        var firstDetail = invoice.Details.First();
        if (firstDetail.InscanId > 0)
        {
            shipment = await db.InscanMasters.FirstOrDefaultAsync(i => i.Id == firstDetail.InscanId);
        }
        else if (!string.IsNullOrEmpty(firstDetail.AWBNo))
        {
            shipment = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == firstDetail.AWBNo && !i.IsDeleted);
        }
    }
    
    string? invAccountNo = null;
    if (invoice.CustomerId.HasValue)
    {
        var invCust = await db.Parties.FirstOrDefaultAsync(p => p.Id == invoice.CustomerId);
        invAccountNo = invCust?.CustomerAccountNo;
    }
    byte[] pdf;
    if (shipment != null)
    {
        var currency = shipment.Currency ?? "AED";
        pdf = reportService.GenerateShipmentInvoicePdf(invoice, shipment, currency, logoData);
    }
    else
    {
        pdf = reportService.GenerateInvoicePdf(invoice, logoData, company?.Name, invAccountNo);
    }
    return Results.File(pdf, "application/pdf", $"Invoice-{invoice.InvoiceNo}.pdf");
});

app.MapGet("/api/report/domestic-invoice/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var invoice = await db.Invoices.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == id);
    if (invoice == null) return Results.NotFound();
    
    var company = await db.Companies.Include(c => c.Currency).FirstOrDefaultAsync(c => !c.IsDeleted);
    var branch = invoice.BranchId.HasValue 
        ? await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == invoice.BranchId) 
        : null;
    var currency = branch?.Currency?.Code ?? company?.Currency?.Code ?? "AED";
    
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath ?? env.ContentRootPath);
    
    string? domAcctNo = null;
    if (invoice.CustomerId.HasValue)
    {
        var domCust = await db.Parties.FirstOrDefaultAsync(p => p.Id == invoice.CustomerId);
        domAcctNo = domCust?.CustomerAccountNo;
    }
    var pdf = reportService.GenerateDomesticInvoicePdf(invoice, currency, logoData, company?.Name, company?.TaxNumber, domAcctNo);
    return Results.File(pdf, "application/pdf", $"DomesticInvoice-{invoice.InvoiceNo}.pdf");
});

app.MapGet("/api/report/duty-receipt/{id:long}", async (long id, bool? inline, ApplicationDbContext db, ReportingService reportService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var shipment = await db.InscanMasters.FindAsync(id);
        if (shipment == null)
        {
            var importShipment = await db.ImportShipments
                .Include(i => i.ImportMaster)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (importShipment == null) return Results.NotFound("Shipment not found");
            shipment = MapImportToInscanMaster(importShipment);
            if (importShipment.ImportMaster != null)
            {
                shipment.BranchId = importShipment.ImportMaster.BranchId;
                shipment.CompanyId = importShipment.ImportMaster.CompanyId;
            }
        }

        try
        {
            if (shipment.BarcodeImage == null && !string.IsNullOrEmpty(shipment.AWBNo))
            {
                var (h, _) = barcodeService.GenerateBothBarcodes(shipment.AWBNo);
                shipment.BarcodeImage = h;
            }
        }
        catch (Exception bex)
        {
            logger.LogWarning(bex, "Failed to generate barcode for duty receipt, AWB: {AWBNo}", shipment.AWBNo);
        }

        var branch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == shipment.BranchId);
        var company = branch != null ? await db.Companies.Include(c => c.City).Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == branch.CompanyId) : null;
        if (company == null)
            company = await db.Companies.Include(c => c.City).Include(c => c.Country).FirstOrDefaultAsync(c => !c.IsDeleted);
        var currency = branch?.Currency?.Code ?? "AED";
        
        byte[]? logoData = null;
        try
        {
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath ?? env.ContentRootPath);
        }
        catch (Exception lex)
        {
            logger.LogWarning(lex, "Failed to resolve logo for duty receipt");
        }
        
        var companyAddress = company != null ? $"{company.Address}, {company.City?.Name}, {company.Country?.Name}" : null;
        string? dutyAcctNo = null;
        if (shipment.CustomerId.HasValue)
        {
            var dutyCust = await db.Parties.FirstOrDefaultAsync(p => p.Id == shipment.CustomerId);
            dutyAcctNo = dutyCust?.CustomerAccountNo;
        }
        var otherCharges = await db.AWBOtherCharges
            .Include(c => c.OtherChargeType)
            .Where(c => c.InscanId == shipment.Id && !c.IsDeleted)
            .ToListAsync();
        var codAmt = shipment.CODAmount ?? 0;
        decimal importVatAmt = 0;
        if (shipment.MovementTypeId == MovementType.InternationalImport && (shipment.CustomsValue ?? 0) > 0 && branch != null)
        {
            importVatAmt = (shipment.CustomsValue ?? 0) * (branch.VatPercentage ?? 0) / 100;
        }
        reportService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = reportService.GenerateDutyReceiptPdf(shipment, currency, logoData, company?.Name, companyAddress, company?.Phone, company?.Email, company?.TaxNumber, dutyAcctNo, otherCharges, importVatAmt, codAmt);
        var fileName = inline == true ? null : $"DutyReceipt-{shipment.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating duty receipt for ID {Id}. Exception: {Message}, Stack: {Stack}", id, ex.Message, ex.StackTrace);
        return Results.Problem($"Error generating duty receipt: {ex.Message}");
    }
});

app.MapGet("/api/report/duty-receipt-by-awbno/{awbNo}", async (string awbNo, bool? inline, ApplicationDbContext db, ReportingService reportService, BarcodeService barcodeService, IWebHostEnvironment env, ILogger<Program> logger) =>
{
    try
    {
        var shipment = await db.InscanMasters.FirstOrDefaultAsync(i => i.AWBNo == awbNo);
        if (shipment == null)
        {
            var importShipment = await db.ImportShipments
                .Include(i => i.ImportMaster)
                .FirstOrDefaultAsync(i => i.AWBNo == awbNo);
            if (importShipment == null) return Results.NotFound("Shipment not found");
            shipment = MapImportToInscanMaster(importShipment);
            if (importShipment.ImportMaster != null)
            {
                shipment.BranchId = importShipment.ImportMaster.BranchId;
                shipment.CompanyId = importShipment.ImportMaster.CompanyId;
            }
        }

        try
        {
            if (shipment.BarcodeImage == null && !string.IsNullOrEmpty(shipment.AWBNo))
            {
                var (h, _) = barcodeService.GenerateBothBarcodes(shipment.AWBNo);
                shipment.BarcodeImage = h;
            }
        }
        catch (Exception bex)
        {
            logger.LogWarning(bex, "Failed to generate barcode for duty receipt, AWB: {AWBNo}", shipment.AWBNo);
        }

        var branch = await db.Branches.Include(b => b.Currency).FirstOrDefaultAsync(b => b.Id == shipment.BranchId);
        var company = branch != null ? await db.Companies.Include(c => c.City).Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == branch.CompanyId) : null;
        if (company == null)
            company = await db.Companies.Include(c => c.City).Include(c => c.Country).FirstOrDefaultAsync(c => !c.IsDeleted);
        var currency = branch?.Currency?.Code ?? "AED";

        byte[]? logoData = null;
        try
        {
            logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath ?? env.ContentRootPath);
        }
        catch (Exception lex)
        {
            logger.LogWarning(lex, "Failed to resolve logo for duty receipt");
        }

        var companyAddress = company != null ? $"{company.Address}, {company.City?.Name}, {company.Country?.Name}" : null;
        string? dutyAcctNo2 = null;
        if (shipment.CustomerId.HasValue)
        {
            var dutyCust2 = await db.Parties.FirstOrDefaultAsync(p => p.Id == shipment.CustomerId);
            dutyAcctNo2 = dutyCust2?.CustomerAccountNo;
        }
        var otherCharges2 = await db.AWBOtherCharges
            .Include(c => c.OtherChargeType)
            .Where(c => c.InscanId == shipment.Id && !c.IsDeleted)
            .ToListAsync();
        var codAmt2 = shipment.CODAmount ?? 0;
        decimal importVatAmt2 = 0;
        if (shipment.MovementTypeId == MovementType.InternationalImport && (shipment.CustomsValue ?? 0) > 0 && branch != null)
        {
            importVatAmt2 = (shipment.CustomsValue ?? 0) * (branch.VatPercentage ?? 0) / 100;
        }
        reportService.SetCountryCodeLookup(await LoadCountryCodeLookup(db));
        var pdf = reportService.GenerateDutyReceiptPdf(shipment, currency, logoData, company?.Name, companyAddress, company?.Phone, company?.Email, company?.TaxNumber, dutyAcctNo2, otherCharges2, importVatAmt2, codAmt2);
        var fileName = inline == true ? null : $"DutyReceipt-{shipment.AWBNo}.pdf";
        return Results.File(pdf, "application/pdf", fileName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error generating duty receipt for AWBNo {AWBNo}. Exception: {Message}, Stack: {Stack}", awbNo, ex.Message, ex.StackTrace);
        return Results.Problem($"Error generating duty receipt: {ex.Message}");
    }
});

app.MapGet("/api/report/receipt/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var receipt = await db.Receipts.Include(r => r.Allocations).FirstOrDefaultAsync(r => r.Id == id);
    if (receipt == null) return Results.NotFound();
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = reportService.GenerateReceiptPdf(receipt, logoData, company?.Name);
    return Results.File(pdf, "application/pdf", $"Receipt-{receipt.ReceiptNo}.pdf");
});

app.MapGet("/api/report/cash-receipt/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var submission = await db.CourierCashSubmissions.Include(s => s.DRS).FirstOrDefaultAsync(s => s.Id == id);
    if (submission == null) return Results.NotFound();
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = reportService.GenerateCashReceiptPdf(submission, submission.DRS, company?.Name ?? "Net4Courier", logoData);
    return Results.File(pdf, "application/pdf", $"CashReceipt-{submission.ReceiptNo ?? id.ToString()}.pdf");
});

app.MapPost("/api/report/cash-receipt/{id:long}/email", async (long id, string email, ApplicationDbContext db, ReportingService reportService, IGmailEmailService emailService, IWebHostEnvironment env) =>
{
    var submission = await db.CourierCashSubmissions.Include(s => s.DRS).FirstOrDefaultAsync(s => s.Id == id);
    if (submission == null) return Results.NotFound();
    
    if (string.IsNullOrEmpty(email))
    {
        return Results.BadRequest("Email address is required");
    }
    
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    var companyName = company?.Name ?? "Net4Courier";
    var fromEmail = company?.Email ?? "noreply@net4courier.com";
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    
    var pdf = reportService.GenerateCashReceiptPdf(submission, submission.DRS, companyName, logoData);
    
    var subject = $"Cash Receipt - {submission.ReceiptNo ?? "Receipt"} - {submission.SubmissionDate:dd-MMM-yyyy}";
    var htmlBody = $@"
        <p>Dear {submission.CourierName},</p>
        <p>Please find attached your cash receipt for DRS {submission.DRS?.DRSNo ?? "-"}.</p>
        <p><strong>Receipt No:</strong> {submission.ReceiptNo ?? "-"}<br/>
        <strong>Date:</strong> {submission.SubmissionDate:dd-MMM-yyyy}<br/>
        <strong>Amount Received:</strong> {(submission.ReceivedAmount ?? 0):N2}</p>
        <p>Thank you.</p>
        <p>Regards,<br/>{companyName}</p>
    ";
    
    var success = await emailService.SendEmailWithAttachmentAsync(
        email, submission.CourierName ?? "Courier",
        subject, htmlBody,
        pdf, $"CashReceipt-{submission.ReceiptNo ?? id.ToString()}.pdf", "application/pdf",
        fromEmail, companyName
    );
    
    return success ? Results.Ok(new { message = "Email sent successfully" }) : Results.Problem("Failed to send email");
});

app.MapGet("/api/report/mawb/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = await reportService.GenerateMAWBManifest(id, logoData, company?.Name);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"MAWB-Manifest-{id}.pdf");
});

app.MapGet("/api/report/awb-print/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = reportService.GenerateAirWaybillPdf(awb, logoData, company?.Name);
    return Results.File(pdf, "application/pdf", $"AirWaybill-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/manifest-label/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var awb = await db.InscanMasters.FindAsync(id);
    if (awb == null) return Results.NotFound();
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = reportService.GenerateManifestLabel(awb, awb.BagNo, awb.MAWBNo, logoData, company?.Name);
    return Results.File(pdf, "application/pdf", $"Label-{awb.AWBNo}.pdf");
});

app.MapGet("/api/report/mawb-labels/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = await reportService.GenerateManifestLabels(id, logoData, company?.Name);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"MAWB-Labels-{id}.pdf");
});

app.MapGet("/api/report/export-manifest/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = await reportService.GenerateExportManifest(id, logoData, company?.Name);
    if (pdf.Length == 0) return Results.NotFound();
    return Results.File(pdf, "application/pdf", $"Export-Manifest-{id}.pdf");
});

app.MapGet("/api/report/domestic-manifest/{id:long}", async (long id, ApplicationDbContext db, ReportingService reportService, IWebHostEnvironment env) =>
{
    var company = await db.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
    byte[]? logoData = ResolveLogoBytes(company?.Logo, env.WebRootPath);
    var pdf = await reportService.GenerateDomesticManifest(id, logoData, company?.Name);
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

app.MapGet("/api/export/audit-logs", async (
    HttpContext context,
    ApplicationDbContext db,
    DateTime? fromDate,
    DateTime? toDate,
    string? entityName,
    int? action,
    long? userId) =>
{
    try
    {
        var query = db.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp < toUtc);
        }

        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (action.HasValue)
        {
            var auditAction = (Net4Courier.Kernel.Entities.AuditAction)action.Value;
            query = query.Where(a => a.Action == auditAction);
        }

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        var logs = await query.OrderByDescending(a => a.Timestamp).Take(5000).ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Audit Logs");

        worksheet.Cell(1, 1).Value = "Timestamp";
        worksheet.Cell(1, 2).Value = "Entity";
        worksheet.Cell(1, 3).Value = "Record ID";
        worksheet.Cell(1, 4).Value = "Action";
        worksheet.Cell(1, 5).Value = "User";
        worksheet.Cell(1, 6).Value = "Branch";
        worksheet.Cell(1, 7).Value = "IP Address";
        worksheet.Cell(1, 8).Value = "Old Values";
        worksheet.Cell(1, 9).Value = "New Values";

        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

        int row = 2;
        foreach (var log in logs)
        {
            worksheet.Cell(row, 1).Value = log.Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
            worksheet.Cell(row, 2).Value = log.EntityName;
            worksheet.Cell(row, 3).Value = log.EntityId;
            worksheet.Cell(row, 4).Value = log.Action.ToString();
            worksheet.Cell(row, 5).Value = log.UserName ?? "";
            worksheet.Cell(row, 6).Value = log.BranchName ?? "";
            worksheet.Cell(row, 7).Value = log.IPAddress ?? "";
            worksheet.Cell(row, 8).Value = log.OldValues ?? "";
            worksheet.Cell(row, 9).Value = log.NewValues ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return Results.File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Export failed: {ex.Message}");
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

var urlsArgIndex = Array.IndexOf(args, "--urls");
var listenUrl = urlsArgIndex >= 0 && urlsArgIndex + 1 < args.Length 
    ? args[urlsArgIndex + 1]
    : args.FirstOrDefault(a => a.StartsWith("--urls="))?.Split('=', 2).LastOrDefault()
      ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS") 
      ?? "http://0.0.0.0:5000";
Console.WriteLine($"[{DateTime.UtcNow:O}] Starting HTTP server on {listenUrl}");
Console.WriteLine($"[{DateTime.UtcNow:O}] All middleware and routes configured successfully");

Console.WriteLine();
Console.WriteLine("=====================================================");
Console.WriteLine("  NET4COURIER - APPLICATION READY");
Console.WriteLine($"  Build Timestamp : {buildTimestamp}");
Console.WriteLine($"  Server Started  : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"  Listening On    : {listenUrl}");
Console.WriteLine($"  Environment     : {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"  Database        : {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")) ? "NOT SET" : "Connected")}");
Console.WriteLine("=====================================================");
Console.WriteLine();

string BackupErrorPage(string title, string message)
{
    return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Backup Download - {title}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; }}
        .card {{ background: white; border-radius: 8px; box-shadow: 0 2px 12px rgba(0,0,0,0.1); padding: 40px; max-width: 500px; text-align: center; }}
        .icon {{ font-size: 48px; margin-bottom: 16px; }}
        h1 {{ color: #d32f2f; font-size: 22px; margin: 0 0 12px; }}
        p {{ color: #555; line-height: 1.6; margin: 0 0 24px; }}
        a {{ display: inline-block; padding: 10px 24px; background: #1976d2; color: white; text-decoration: none; border-radius: 4px; font-weight: 500; }}
        a:hover {{ background: #1565c0; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon"">&#9888;</div>
        <h1>{title}</h1>
        <p>{message}</p>
        <a href=""/database-backup"">Back to Database Backup</a>
    </div>
</body>
</html>";
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] FATAL ERROR starting server: {ex.Message}");
    Console.WriteLine($"[{DateTime.UtcNow:O}] Stack trace: {ex.StackTrace}");
    throw;
}

static byte[]? ResolveLogoBytes(string? logo, string webRootPath)
{
    if (string.IsNullOrEmpty(logo)) return null;
    if (logo.StartsWith("data:"))
    {
        var parts = logo.Split(',', 2);
        if (parts.Length == 2)
        {
            try { return Convert.FromBase64String(parts[1]); }
            catch { return null; }
        }
        return null;
    }
    var filePath = Path.Combine(webRootPath, logo.TrimStart('/'));
    return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
}

static async Task<Dictionary<string, string>> LoadCountryCodeLookup(ApplicationDbContext db)
{
    var countries = await db.Countries
        .Where(c => !string.IsNullOrEmpty(c.Code))
        .Select(c => new { c.Name, c.Code })
        .ToListAsync();
    var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var c in countries)
    {
        if (!string.IsNullOrWhiteSpace(c.Name) && !string.IsNullOrWhiteSpace(c.Code))
            lookup[c.Name.Trim()] = c.Code.Trim();
    }
    return lookup;
}

static async Task ResolveLocationCodes(InscanMaster awb, ApplicationDbContext db)
{
    db.Entry(awb).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

    var cityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var countryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (!string.IsNullOrWhiteSpace(awb.ConsignorCity)) cityNames.Add(awb.ConsignorCity);
    if (!string.IsNullOrWhiteSpace(awb.ConsigneeCity)) cityNames.Add(awb.ConsigneeCity);
    if (!string.IsNullOrWhiteSpace(awb.ConsignorCountry)) countryNames.Add(awb.ConsignorCountry);
    if (!string.IsNullOrWhiteSpace(awb.ConsigneeCountry)) countryNames.Add(awb.ConsigneeCountry);

    if (cityNames.Count > 0)
    {
        var cities = await db.Cities.AsNoTracking().Where(c => c.Code != null && cityNames.Contains(c.Name)).ToListAsync();
        var cityCodeMap = cities.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Code!, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCity) && cityCodeMap.TryGetValue(awb.ConsignorCity, out var cc1))
            awb.ConsignorCity = cc1;
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCity) && cityCodeMap.TryGetValue(awb.ConsigneeCity, out var cc2))
            awb.ConsigneeCity = cc2;
    }

    if (countryNames.Count > 0)
    {
        var countries = await db.Countries.AsNoTracking().Where(c => c.Code != null && countryNames.Contains(c.Name)).ToListAsync();
        var countryCodeMap = countries.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Code!, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCountry) && countryCodeMap.TryGetValue(awb.ConsignorCountry, out var co1))
            awb.ConsignorCountry = co1;
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCountry) && countryCodeMap.TryGetValue(awb.ConsigneeCountry, out var co2))
            awb.ConsigneeCountry = co2;
    }
}

static async Task ResolveLocationCodesBulk(List<InscanMaster> awbs, ApplicationDbContext db)
{
    foreach (var awb in awbs)
    {
        try { db.Entry(awb).State = Microsoft.EntityFrameworkCore.EntityState.Detached; } catch { }
    }

    var cityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var countryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var awb in awbs)
    {
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCity)) cityNames.Add(awb.ConsignorCity);
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCity)) cityNames.Add(awb.ConsigneeCity);
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCountry)) countryNames.Add(awb.ConsignorCountry);
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCountry)) countryNames.Add(awb.ConsigneeCountry);
    }

    Dictionary<string, string> cityCodeMap = new(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, string> countryCodeMap = new(StringComparer.OrdinalIgnoreCase);

    if (cityNames.Count > 0)
    {
        var cities = await db.Cities.AsNoTracking().Where(c => c.Code != null && cityNames.Contains(c.Name)).ToListAsync();
        cityCodeMap = cities.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Code!, StringComparer.OrdinalIgnoreCase);
    }
    if (countryNames.Count > 0)
    {
        var countries = await db.Countries.AsNoTracking().Where(c => c.Code != null && countryNames.Contains(c.Name)).ToListAsync();
        countryCodeMap = countries.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Code!, StringComparer.OrdinalIgnoreCase);
    }

    foreach (var awb in awbs)
    {
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCity) && cityCodeMap.TryGetValue(awb.ConsignorCity, out var cc1))
            awb.ConsignorCity = cc1;
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCity) && cityCodeMap.TryGetValue(awb.ConsigneeCity, out var cc2))
            awb.ConsigneeCity = cc2;
        if (!string.IsNullOrWhiteSpace(awb.ConsignorCountry) && countryCodeMap.TryGetValue(awb.ConsignorCountry, out var co1))
            awb.ConsignorCountry = co1;
        if (!string.IsNullOrWhiteSpace(awb.ConsigneeCountry) && countryCodeMap.TryGetValue(awb.ConsigneeCountry, out var co2))
            awb.ConsigneeCountry = co2;
    }
}

static InscanMaster MapImportToInscanMaster(ImportShipment import)
{
    return new InscanMaster
    {
        Id = import.Id,
        AWBNo = import.AWBNo,
        ReferenceNo = import.ReferenceNo,
        TransactionDate = import.CreatedAt,
        Consignor = import.ShipperName,
        ConsignorAddress1 = import.ShipperAddress,
        ConsignorCity = import.ShipperCity,
        ConsignorCountry = import.ShipperCountry,
        ConsignorPhone = import.ShipperPhone,
        OriginPortCode = import.ShipperCountry,
        Consignee = import.ConsigneeName,
        ConsigneeAddress1 = import.ConsigneeAddress,
        ConsigneeCity = import.ConsigneeCity,
        ConsigneeState = import.ConsigneeState,
        ConsigneeCountry = import.ConsigneeCountry,
        DestinationPortCode = import.ConsigneeCountry,
        ConsigneePostalCode = import.ConsigneePostalCode,
        ConsigneePhone = import.ConsigneePhone,
        ConsigneeMobile = import.ConsigneeMobile,
        Pieces = import.Pieces,
        Weight = import.Weight,
        ChargeableWeight = import.ChargeableWeight ?? import.Weight,
        CargoDescription = import.ContentsDescription,
        MovementTypeId = MovementType.InternationalImport,
        DocumentTypeId = import.ShipmentType == ImportShipmentType.Document ? DocumentType.Document : DocumentType.ParcelUpto30Kg,
        PaymentModeId = import.PaymentMode,
        CustomsValue = import.DeclaredValue,
        DutyVatAmount = (import.DutyAmount ?? 0) + (import.VATAmount ?? 0),
        OtherCharge = import.OtherCharges,
        NetTotal = import.TotalCustomsCharges,
        Currency = import.Currency,
        IsCOD = import.IsCOD,
        CODAmount = import.CODAmount,
    };
}

static async Task HandlePasswordReset(string username, string password)
{
    Console.WriteLine($"[{DateTime.UtcNow:O}] Password Reset Utility");
    
    var setupKey = Environment.GetEnvironmentVariable("SETUP_KEY");
    if (string.IsNullOrEmpty(setupKey))
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR: SETUP_KEY environment variable is required for password reset");
        Console.WriteLine($"[{DateTime.UtcNow:O}] Set SETUP_KEY environment variable to enable this utility");
        Environment.Exit(1);
        return;
    }
    
    Console.WriteLine($"[{DateTime.UtcNow:O}] SETUP_KEY verified");
    Console.WriteLine($"[{DateTime.UtcNow:O}] Resetting password for user: {username}");
    
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR: DATABASE_URL environment variable is not set");
        Environment.Exit(1);
        return;
    }
    
    string connectionString;
    if (databaseUrl.StartsWith("postgresql://"))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/').Split('?')[0];
        var dbUsername = userInfo[0];
        var dbPassword = userInfo.Length > 1 ? userInfo[1] : "";
        connectionString = $"Host={host};Port={port};Database={database};Username={dbUsername};Password={dbPassword}";
    }
    else
    {
        connectionString = databaseUrl;
    }
    
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    
    using var dbContext = new ApplicationDbContext(optionsBuilder.Options);
    
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null)
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR: User '{username}' not found");
        Environment.Exit(1);
        return;
    }
    
    var newHash = BCrypt.Net.BCrypt.HashPassword(password);
    user.PasswordHash = newHash;
    user.ModifiedAt = DateTime.UtcNow;
    
    await dbContext.SaveChangesAsync();
    
    Console.WriteLine($"[{DateTime.UtcNow:O}] SUCCESS: Password updated for user '{username}'");
    Console.WriteLine($"[{DateTime.UtcNow:O}] New password hash: {newHash.Substring(0, 20)}...");
}

public class DatabaseInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;
    public static bool IsReady { get; private set; } = false;

    public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);
        
        // Check if database is configured before attempting initialization
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
        {
            _logger.LogWarning("DATABASE_URL not set. Skipping database initialization. Application running in limited mode.");
            IsReady = true;
            return;
        }
        
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schemaSync = scope.ServiceProvider.GetRequiredService<Net4Courier.Infrastructure.Services.SchemaAutoSyncService>();

        try
        {
            var isProductionMode = Environment.GetEnvironmentVariable("PRODUCTION_MODE")?.ToLower() == "true";
            var schemaAutoApply = Environment.GetEnvironmentVariable("SCHEMA_AUTO_APPLY")?.ToLower() != "false"; // default true for dev

            // Step 1: Check if database already has tables
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(stoppingToken);
            
            bool databaseHasTables = false;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Companies')";
                var result = await cmd.ExecuteScalarAsync(stoppingToken);
                databaseHasTables = result is bool b && b;
            }

            if (!databaseHasTables)
            {
                _logger.LogInformation("Fresh database detected. Creating initial schema...");
                await dbContext.Database.EnsureCreatedAsync(stoppingToken);
            }
            else
            {
                _logger.LogInformation("Existing database detected. Skipping EnsureCreated to protect data.");
            }

            // Step 2: Auto-sync schema (creates missing tables/columns - never drops anything)
            if (isProductionMode && !schemaAutoApply)
            {
                _logger.LogInformation("PRODUCTION_MODE=true, SCHEMA_AUTO_APPLY=false: Running schema sync in preview-only mode...");
                var previewResult = await schemaSync.PreviewSchemaChangesAsync(dbContext, stoppingToken);
                if (previewResult.DetectedMissing.Count > 0)
                {
                    _logger.LogWarning("=== SCHEMA CHANGES PENDING ({Count} changes) ===", previewResult.DetectedMissing.Count);
                    foreach (var missing in previewResult.DetectedMissing)
                    {
                        _logger.LogWarning("  PENDING: {Change}", missing);
                    }
                    _logger.LogWarning("Set SCHEMA_AUTO_APPLY=true environment variable to apply these changes on next restart.");
                    _logger.LogWarning("=== END SCHEMA PREVIEW ===");
                }
                else
                {
                    _logger.LogInformation("Schema is up to date. No pending changes.");
                }
            }
            else
            {
                var autoSyncSuccess = await schemaSync.SyncSchemaAsync(dbContext, stoppingToken);
                
                if (!autoSyncSuccess)
                {
                    _logger.LogWarning("Auto-sync incomplete, attempting fallback to schema_sync_full.sql...");
                    var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "schema_sync_full.sql");
                    if (!File.Exists(scriptPath))
                    {
                        scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "scripts", "schema_sync_full.sql");
                    }
                    await schemaSync.ExecuteSchemaScriptAsync(dbContext, scriptPath, stoppingToken);
                }
            }

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    DO $$
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'UserFavorites_Id_seq') THEN
                            CREATE SEQUENCE ""UserFavorites_Id_seq"" OWNED BY ""UserFavorites"".""Id"";
                            PERFORM setval('""UserFavorites_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""UserFavorites""), 0) + 1, false);
                            ALTER TABLE ""UserFavorites"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""UserFavorites_Id_seq""');
                        END IF;
                    END $$;
                ", stoppingToken);
            }
            catch (Exception seqEx)
            {
                _logger.LogWarning(seqEx, "UserFavorites sequence fix skipped");
            }

            // Seed PlatformAdmin early - before GL tables and geographic data to ensure it always runs
            try
            {
                var earlyAuthService = scope.ServiceProvider.GetRequiredService<AuthService>();
                await earlyAuthService.SeedPlatformAdminAsync();
            }
            catch (Exception paEx)
            {
                _logger.LogError(paEx, "Early PlatformAdmin seeding failed - will retry later in startup");
            }

            // GL Module Tables - Native long-based IDs
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""GLAccountClassifications"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""CompanyId"" BIGINT,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""Description"" TEXT,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GLAccountClassifications_CompanyId_Name"" ON ""GLAccountClassifications"" (""CompanyId"", ""Name"");
            ", stoppingToken);
            _logger.LogInformation("GLAccountClassifications table ensured");

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""GLChartOfAccounts"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""CompanyId"" BIGINT,
                    ""AccountCode"" VARCHAR(50) NOT NULL,
                    ""AccountName"" VARCHAR(200) NOT NULL,
                    ""AccountType"" VARCHAR(50),
                    ""ParentId"" BIGINT REFERENCES ""GLChartOfAccounts""(""Id"") ON DELETE SET NULL,
                    ""IsSystemAccount"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""DisplayOrder"" INT NOT NULL DEFAULT 0,
                    ""Description"" TEXT,
                    ""AllowPosting"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""Level"" INT NOT NULL DEFAULT 0,
                    ""AccountClassificationId"" BIGINT REFERENCES ""GLAccountClassifications""(""Id"") ON DELETE SET NULL,
                    ""ControlAccountType"" INT,
                    ""DeactivatedDate"" TIMESTAMP WITH TIME ZONE,
                    ""DeactivationReason"" TEXT,
                    ""CreatedByUser"" VARCHAR(100),
                    ""UpdatedByUser"" VARCHAR(100),
                    ""DeactivatedByUserId"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GLChartOfAccounts_CompanyId_AccountCode"" ON ""GLChartOfAccounts"" (""CompanyId"", ""AccountCode"");
            ", stoppingToken);
            _logger.LogInformation("GLChartOfAccounts table ensured");

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""GLTaxCodes"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""CompanyId"" BIGINT,
                    ""Code"" VARCHAR(20) NOT NULL,
                    ""Description"" VARCHAR(200),
                    ""Rate"" DECIMAL(18, 4) NOT NULL DEFAULT 0,
                    ""TaxType"" VARCHAR(20),
                    ""CreatedByUser"" VARCHAR(100),
                    ""UpdatedByUser"" VARCHAR(100),
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GLTaxCodes_CompanyId_Code"" ON ""GLTaxCodes"" (""CompanyId"", ""Code"");
            ", stoppingToken);
            _logger.LogInformation("GLTaxCodes table ensured");

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""GLVoucherNumberings"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""CompanyId"" BIGINT,
                    ""TransactionType"" VARCHAR(50) NOT NULL,
                    ""Prefix"" VARCHAR(20),
                    ""Suffix"" VARCHAR(20),
                    ""Separator"" VARCHAR(10) DEFAULT '-',
                    ""NextNumber"" INT NOT NULL DEFAULT 1,
                    ""NumberLength"" INT NOT NULL DEFAULT 6,
                    ""IsLocked"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""FinancialYearId"" BIGINT,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_GLVoucherNumberings_CompanyId_TransactionType_FinancialYearId"" ON ""GLVoucherNumberings"" (""CompanyId"", ""TransactionType"", ""FinancialYearId"");
            ", stoppingToken);
            _logger.LogInformation("GLVoucherNumberings table ensured");

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

            await dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""ServiceTypeId"" BIGINT;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""ShipmentModeId"" BIGINT;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""DocumentType"" INT;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""MinWeight"" NUMERIC NOT NULL DEFAULT 1;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""MaxWeight"" NUMERIC NOT NULL DEFAULT 5;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""TaxPercent"" NUMERIC NOT NULL DEFAULT 0;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""AdditionalWeight"" NUMERIC NOT NULL DEFAULT 1;
                ALTER TABLE ""RateCardZones"" ADD COLUMN IF NOT EXISTS ""AdditionalRate"" NUMERIC NOT NULL DEFAULT 0;
                ALTER TABLE ""RateCards"" ADD COLUMN IF NOT EXISTS ""ServiceTypeId"" BIGINT;
                ALTER TABLE ""RateCards"" ADD COLUMN IF NOT EXISTS ""ShipmentModeId"" BIGINT;
                ALTER TABLE ""RateCardSlabRules"" ADD COLUMN IF NOT EXISTS ""FlatRate"" NUMERIC;
                ALTER TABLE ""RateCardSlabRules"" ADD COLUMN IF NOT EXISTS ""CostFlatRate"" NUMERIC;
                ALTER TABLE ""RateCardSlabRules"" ADD COLUMN IF NOT EXISTS ""CostPerKgRate"" NUMERIC;
                ALTER TABLE ""RateCardSlabRules"" ADD COLUMN IF NOT EXISTS ""Additional1KgRate"" NUMERIC;
                ALTER TABLE ""SlabRuleTemplateDetails"" ADD COLUMN IF NOT EXISTS ""FlatRate"" NUMERIC NOT NULL DEFAULT 0;
                ALTER TABLE ""SlabRuleTemplateDetails"" ADD COLUMN IF NOT EXISTS ""CostFlatRate"" NUMERIC NOT NULL DEFAULT 0;
                ALTER TABLE ""SlabRuleTemplateDetails"" ADD COLUMN IF NOT EXISTS ""CostPerKgRate"" NUMERIC NOT NULL DEFAULT 0;
                ALTER TABLE ""SlabRuleTemplateDetails"" ADD COLUMN IF NOT EXISTS ""Additional1KgRate"" NUMERIC;
                ALTER TABLE ""ZoneMatrixDetails"" ADD COLUMN IF NOT EXISTS ""StateId"" BIGINT;
                ALTER TABLE ""ZoneMatrixDetails"" ADD COLUMN IF NOT EXISTS ""PostalCodeFrom"" VARCHAR(20);
                ALTER TABLE ""ZoneMatrixDetails"" ADD COLUMN IF NOT EXISTS ""PostalCodeTo"" VARCHAR(20);
                ALTER TABLE ""RateCards"" ADD COLUMN IF NOT EXISTS ""Description"" TEXT;
                ALTER TABLE ""RateCards"" ADD COLUMN IF NOT EXISTS ""ForwardingAgentId"" BIGINT;
                ALTER TABLE ""CustomerRateAssignments"" ADD COLUMN IF NOT EXISTS ""Remarks"" TEXT;
                ALTER TABLE ""CustomerRateAssignments"" ADD COLUMN IF NOT EXISTS ""CompanyId"" BIGINT;
            ", stoppingToken);

            // Create Currencies table
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""Currencies"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""Code"" VARCHAR(10) NOT NULL,
                    ""Name"" VARCHAR(100) NOT NULL,
                    ""Symbol"" VARCHAR(10),
                    ""DecimalPlaces"" INT NOT NULL DEFAULT 2,
                    ""IsBaseCurrency"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""ExchangeRate"" NUMERIC NOT NULL DEFAULT 1.0,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP WITH TIME ZONE,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT
                );
                ALTER TABLE ""Currencies"" ADD COLUMN IF NOT EXISTS ""IsBaseCurrency"" BOOLEAN NOT NULL DEFAULT FALSE;
                ALTER TABLE ""Currencies"" ADD COLUMN IF NOT EXISTS ""ExchangeRate"" NUMERIC NOT NULL DEFAULT 1.0;
                ALTER TABLE ""Currencies"" ADD COLUMN IF NOT EXISTS ""IsDemo"" BOOLEAN NOT NULL DEFAULT FALSE;
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Currencies_Code"" ON ""Currencies"" (""Code"");
            ", stoppingToken);

            // Seed Currencies
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'AED', 'UAE Dirham', 'د.إ', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'AED');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'SAR', 'Saudi Riyal', '﷼', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'SAR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'QAR', 'Qatari Riyal', '﷼', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'QAR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'KWD', 'Kuwaiti Dinar', 'د.ك', 3, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'KWD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'BHD', 'Bahraini Dinar', '.د.ب', 3, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'BHD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'OMR', 'Omani Rial', 'ر.ع.', 3, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'OMR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'JOD', 'Jordanian Dinar', 'د.ا', 3, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'JOD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'USD', 'US Dollar', '$', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'USD');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'INR', 'Indian Rupee', '₹', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'INR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'EUR', 'Euro', '€', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'EUR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'PKR', 'Pakistani Rupee', 'Rs', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'PKR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'BDT', 'Bangladeshi Taka', '৳', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'BDT');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'MYR', 'Malaysian Ringgit', 'RM', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'MYR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'IDR', 'Indonesian Rupiah', 'Rp', 0, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'IDR');
                INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""DecimalPlaces"", ""IsBaseCurrency"", ""ExchangeRate"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'PHP', 'Philippine Peso', '₱', 2, FALSE, 1.0, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Currencies"" WHERE ""Code"" = 'PHP');
            ", stoppingToken);

            // Create DRS Reconciliation tables
            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""CourierCashSubmissions"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""DRSId"" BIGINT NOT NULL,
                    ""CourierId"" INT NOT NULL,
                    ""CourierName"" VARCHAR(255),
                    ""SubmissionDate"" TIMESTAMP NOT NULL,
                    ""CashSubmittedAmount"" DECIMAL(18,2) DEFAULT 0,
                    ""SubmissionTime"" TIMESTAMP NOT NULL,
                    ""ReceivedById"" INT,
                    ""ReceivedByName"" VARCHAR(255),
                    ""ReceivedAt"" TIMESTAMP,
                    ""ReceivedAmount"" DECIMAL(18,2),
                    ""ReceiptNo"" VARCHAR(50),
                    ""ReceiptVoucherId"" BIGINT,
                    ""Remarks"" TEXT,
                    ""IsAcknowledged"" BOOLEAN DEFAULT FALSE,
                    ""IsActive"" BOOLEAN DEFAULT TRUE,
                    ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""CreatedByName"" TEXT,
                    ""ModifiedByName"" TEXT,
                    ""IsDeleted"" BOOLEAN DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN DEFAULT FALSE
                );
                CREATE INDEX IF NOT EXISTS ""IX_CourierCashSubmissions_DRSId"" ON ""CourierCashSubmissions""(""DRSId"");
                CREATE INDEX IF NOT EXISTS ""IX_CourierCashSubmissions_CourierId"" ON ""CourierCashSubmissions""(""CourierId"");
                CREATE INDEX IF NOT EXISTS ""IX_CourierCashSubmissions_SubmissionDate"" ON ""CourierCashSubmissions""(""SubmissionDate"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""DRSReconciliationStatements"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""DRSId"" BIGINT NOT NULL,
                    ""DRSNo"" VARCHAR(50),
                    ""CashSubmissionId"" BIGINT NOT NULL,
                    ""ReceiptNo"" VARCHAR(50),
                    ""StatementDate"" TIMESTAMP NOT NULL,
                    ""CourierId"" INT NOT NULL,
                    ""CourierName"" VARCHAR(255),
                    ""TotalMaterialCost"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalCODAmount"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalOtherCharges"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalCollectible"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalCollected"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalDiscount"" DECIMAL(18,2) DEFAULT 0,
                    ""CashSubmitted"" DECIMAL(18,2) DEFAULT 0,
                    ""ExpenseBills"" DECIMAL(18,2) DEFAULT 0,
                    ""Balance"" DECIMAL(18,2) DEFAULT 0,
                    ""IsSettled"" BOOLEAN DEFAULT FALSE,
                    ""SettledAt"" TIMESTAMP,
                    ""SettledById"" INT,
                    ""SettledByName"" VARCHAR(255),
                    ""Remarks"" TEXT,
                    ""IsActive"" BOOLEAN DEFAULT TRUE,
                    ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""IsDeleted"" BOOLEAN DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN DEFAULT FALSE
                );
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationStatements_DRSId"" ON ""DRSReconciliationStatements""(""DRSId"");
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationStatements_CashSubmissionId"" ON ""DRSReconciliationStatements""(""CashSubmissionId"");
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationStatements_StatementDate"" ON ""DRSReconciliationStatements""(""StatementDate"");
            ", stoppingToken);

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""DRSReconciliationLines"" (
                    ""Id"" BIGSERIAL PRIMARY KEY,
                    ""DRSId"" BIGINT NOT NULL,
                    ""CashSubmissionId"" BIGINT NOT NULL,
                    ""InscanId"" BIGINT NOT NULL,
                    ""AWBNo"" VARCHAR(50),
                    ""MaterialCost"" DECIMAL(18,2) DEFAULT 0,
                    ""CODAmount"" DECIMAL(18,2) DEFAULT 0,
                    ""OtherCharges"" DECIMAL(18,2) DEFAULT 0,
                    ""TotalCollectible"" DECIMAL(18,2) DEFAULT 0,
                    ""MaterialCollected"" DECIMAL(18,2) DEFAULT 0,
                    ""CODCollected"" DECIMAL(18,2) DEFAULT 0,
                    ""OtherCollected"" DECIMAL(18,2) DEFAULT 0,
                    ""AmountCollected"" DECIMAL(18,2) DEFAULT 0,
                    ""DiscountAmount"" DECIMAL(18,2) DEFAULT 0,
                    ""DiscountReason"" TEXT,
                    ""DiscountApproved"" BOOLEAN DEFAULT FALSE,
                    ""DiscountApprovedById"" INT,
                    ""DiscountApprovedByName"" VARCHAR(255),
                    ""DiscountApprovedAt"" TIMESTAMP,
                    ""Status"" INT DEFAULT 1,
                    ""Remarks"" TEXT,
                    ""IsActive"" BOOLEAN DEFAULT TRUE,
                    ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ""ModifiedAt"" TIMESTAMP,
                    ""CreatedBy"" INT,
                    ""ModifiedBy"" INT,
                    ""IsDeleted"" BOOLEAN DEFAULT FALSE,
                    ""IsDemo"" BOOLEAN DEFAULT FALSE
                );
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationLines_DRSId"" ON ""DRSReconciliationLines""(""DRSId"");
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationLines_CashSubmissionId"" ON ""DRSReconciliationLines""(""CashSubmissionId"");
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationLines_InscanId"" ON ""DRSReconciliationLines""(""InscanId"");
                CREATE INDEX IF NOT EXISTS ""IX_DRSReconciliationLines_AWBNo"" ON ""DRSReconciliationLines""(""AWBNo"");
            ", stoppingToken);

            _logger.LogInformation("DRS Reconciliation tables created");

            // Seed Countries
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'United Arab Emirates', 'AE', 'AE', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'AE');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Saudi Arabia', 'SA', 'SA', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'SA');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Qatar', 'QA', 'QA', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'QA');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kuwait', 'KW', 'KW', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'KW');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bahrain', 'BH', 'BH', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'BH');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Oman', 'OM', 'OM', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'OM');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Jordan', 'JO', 'JO', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'JO');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'India', 'IN', 'IN', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'IN');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Pakistan', 'PK', 'PK', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'PK');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bangladesh', 'BD', 'BD', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'BD');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Malaysia', 'MY', 'MY', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'MY');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Indonesia', 'ID', 'ID', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'ID');
                INSERT INTO ""Countries"" (""Name"", ""Code"", ""IATACode"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Philippines', 'PH', 'PH', TRUE, FALSE, FALSE, CURRENT_TIMESTAMP WHERE NOT EXISTS (SELECT 1 FROM ""Countries"" WHERE ""Code"" = 'PH');
            ", stoppingToken);

            // Seed States - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dubai', 'DXB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DXB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Abu Dhabi', 'AUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sharjah', 'SHJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SHJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Ajman', 'AJM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AJM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Fujairah', 'FUJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'FUJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Ras Al Khaimah', 'RAK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RAK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Umm Al Quwain', 'UAQ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'UAQ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'));
            ", stoppingToken);

            // Seed States - Saudi Arabia
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Riyadh', 'RUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Makkah', 'MKH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MKH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Madinah', 'MDN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MDN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Eastern Province', 'EP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'EP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Asir', 'ASR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ASR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'));
            ", stoppingToken);

            // Seed States - Qatar, Kuwait, Bahrain
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Doha', 'DOH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DOH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Al Rayyan', 'RYN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RYN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Al Wakrah', 'WKR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WKR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Al Asimah', 'ASM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ASM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Hawalli', 'HWL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'HWL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Farwaniya', 'FRW', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'FRW' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Capital', 'CAP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CAP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Muharraq', 'MUH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MUH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Northern', 'NOR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NOR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'));
            ", stoppingToken);

            // Seed States - Oman, Jordan
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Muscat', 'MSC', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MSC' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dhofar', 'DHF', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DHF' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'North Al Batinah', 'NAB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NAB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Amman', 'AMN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AMN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Irbid', 'IRB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'IRB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Zarqa', 'ZRQ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ZRQ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'));
            ", stoppingToken);

            // Seed States - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Maharashtra', 'MH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Delhi', 'DL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Karnataka', 'KA', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KA' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Tamil Nadu', 'TN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'TN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Gujarat', 'GJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'GJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kerala', 'KL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'West Bengal', 'WB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Uttar Pradesh', 'UP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'UP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Rajasthan', 'RJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Punjab', 'PB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Telangana', 'TG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'TG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Andhra Pradesh', 'AP', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'AP' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'));
            ", stoppingToken);

            // Seed States - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Punjab', 'PJB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PJB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sindh', 'SND', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SND' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'KPK', 'KPK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KPK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Balochistan', 'BLN', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'BLN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Islamabad', 'ISB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'ISB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dhaka', 'DHK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DHK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Chittagong', 'CTG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CTG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Rajshahi', 'RAJ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'RAJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Khulna', 'KHU', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KHU' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sylhet', 'SYL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SYL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'));
            ", stoppingToken);

            // Seed States - Malaysia, Indonesia, Philippines
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Selangor', 'SEL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SEL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kuala Lumpur', 'KL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Penang', 'PNG', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'PNG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Johor', 'JHR', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'JHR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sabah', 'SBH', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SBH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sarawak', 'SRK', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'SRK' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Jakarta', 'JKT', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'JKT' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'West Java', 'WJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'WJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'East Java', 'EJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'EJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Central Java', 'CJV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bali', 'BAL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'BAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'North Sumatra', 'NSM', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'NSM' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Metro Manila', 'MNL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'MNL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Cebu', 'CEB', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CEB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Davao', 'DAV', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'DAV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Calabarzon', 'CAL', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
                INSERT INTO ""States"" (""Name"", ""Code"", ""CountryId"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Central Luzon', 'CLZ', (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""States"" WHERE ""Code"" = 'CLZ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'));
            ", stoppingToken);

            // Seed Cities - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dubai City', 'DXBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DXB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DXBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Jebel Ali', 'JBL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DXB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JBL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Abu Dhabi City', 'AUHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AUHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Al Ain', 'AAC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AAC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Sharjah City', 'SHJC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'SHJ'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SHJC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Ajman City', 'AJMC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AJM'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AJMC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Fujairah City', 'FUJC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'FUJ'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'FUJC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Ras Al Khaimah City', 'RAKC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'RAK'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'AE'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'RAKC');
            ", stoppingToken);

            // Seed Cities - Saudi Arabia, Qatar, Kuwait
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Riyadh City', 'RUHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'RUH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'RUHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Makkah City', 'MKHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MKH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MKHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Jeddah', 'JED', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MKH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JED');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Madinah City', 'MDNC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MDN'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MDNC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dammam', 'DMM', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'EP'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'SA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DMM');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Doha City', 'DOHC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DOH'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'QA'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DOHC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kuwait City', 'KWC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'ASM'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'KW'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KWC');
            ", stoppingToken);

            // Seed Cities - Bahrain, Oman, Jordan
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Manama', 'MAN', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CAP'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BH'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MAN');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Muscat City', 'MSCC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MSC'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MSCC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Salalah', 'SLL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DHF'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'OM'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SLL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Amman City', 'AMNC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'AMN'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'JO'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AMNC');
            ", stoppingToken);

            // Seed Cities - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Mumbai', 'BOM', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'BOM');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Pune', 'PNQ', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MH' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'PNQ');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'New Delhi', 'DEL', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DEL');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bangalore', 'BLR', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KA' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'BLR');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Chennai', 'MAA', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'TN' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MAA');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Ahmedabad', 'AMD', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'GJ' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'AMD');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kolkata', 'CCU', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'WB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CCU');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Hyderabad', 'HYD', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'TG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'HYD');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kochi', 'COK', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'IN'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'COK');
            ", stoppingToken);

            // Seed Cities - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Lahore', 'LHE', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'PJB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'LHE');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Karachi', 'KHI', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'SND'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KHI');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Islamabad City', 'ISBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'ISB'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PK'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'ISBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Dhaka City', 'DHKC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DHK'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DHKC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Chittagong City', 'CTGC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CTG'), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'BD'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CTGC');
            ", stoppingToken);

            // Seed Cities - Malaysia, Indonesia, Philippines
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Kuala Lumpur City', 'KLC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'KL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'KLC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'George Town', 'PEN', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'PNG' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'PEN');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Johor Bahru', 'JHB', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'JHR' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'MY'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JHB');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Jakarta City', 'JKTC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'JKT' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'JKTC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Surabaya', 'SUB', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'EJV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'SUB');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Denpasar', 'DPS', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'BAL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'ID'), FALSE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DPS');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Manila', 'MLA', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'MNL' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'MLA');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Cebu City', 'CEBC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'CEB' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'CEBC');
                INSERT INTO ""Cities"" (""Name"", ""Code"", ""StateId"", ""CountryId"", ""IsHub"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Davao City', 'DAVC', (SELECT ""Id"" FROM ""States"" WHERE ""Code"" = 'DAV' AND ""CountryId"" = (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH') LIMIT 1), (SELECT ""Id"" FROM ""Countries"" WHERE ""Code"" = 'PH'), TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Cities"" WHERE ""Code"" = 'DAVC');
            ", stoppingToken);

            // Seed Locations - UAE
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Deira', 'DEIRA', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DXBC'), '00000', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'DEIRA');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bur Dubai', 'BURDXB', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DXBC'), '00000', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BURDXB');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Abu Dhabi Downtown', 'AUHDTN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'AUHC'), '00000', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'AUHDTN');
            ", stoppingToken);

            // Seed Locations - India
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Andheri', 'ANDH', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BOM'), '400069', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'ANDH');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Bandra', 'BNDR', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BOM'), '400050', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BNDR');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Connaught Place', 'CNPL', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DEL'), '110001', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'CNPL');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Whitefield', 'WTFL', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BLR'), '560066', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'WTFL');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Koramangala', 'KRMN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'BLR'), '560034', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'KRMN');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'T Nagar', 'TNGR', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MAA'), '600017', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'TNGR');
            ", stoppingToken);

            // Seed Locations - Pakistan, Bangladesh
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Gulberg', 'GULB', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'LHE'), '54000', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'GULB');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Clifton', 'CLFT', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'KHI'), '75600', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'CLFT');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Gulshan', 'GLSN', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DHKC'), '1212', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'GLSN');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Banani', 'BNNI', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'DHKC'), '1213', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BNNI');
            ", stoppingToken);

            // Seed Locations - Southeast Asia
            await dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'KLCC', 'KLCC', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'KLC'), '50088', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'KLCC');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Menteng', 'MNTG', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'JKTC'), '10310', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'MNTG');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'Makati', 'MKAT', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MLA'), '1200', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'MKAT');
                INSERT INTO ""Locations"" (""Name"", ""Code"", ""CityId"", ""Pincode"", ""IsServiceable"", ""IsActive"", ""IsDeleted"", ""IsDemo"", ""CreatedAt"")
                SELECT 'BGC', 'BGC', (SELECT ""Id"" FROM ""Cities"" WHERE ""Code"" = 'MLA'), '1630', TRUE, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP 
                WHERE NOT EXISTS (SELECT 1 FROM ""Locations"" WHERE ""Code"" = 'BGC');
            ", stoppingToken);

            _logger.LogInformation("Geographical data seeding completed");

            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            await authService.SeedAdminUserAsync();
            await authService.SeedPlatformAdminAsync();

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

            if (!await dbContext.AccountTypes.AnyAsync(stoppingToken))
            {
                var accountTypes = new[]
                {
                    new Net4Courier.Masters.Entities.AccountType { Code = "CREDIT", Name = "Credit", Description = "Credit account - payment after delivery", SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.AccountType { Code = "COD", Name = "COD", Description = "Cash on Delivery", SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.AccountType { Code = "PREPAID", Name = "Pre-paid", Description = "Payment before service", SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.AccountType { Code = "CASH", Name = "Cash", Description = "Cash payment", SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Net4Courier.Masters.Entities.AccountType { Code = "TRANS", Name = "Trans-shipment", Description = "Trans-shipment account", SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                dbContext.AccountTypes.AddRange(accountTypes);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Seeded Customer Account Types");
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

            try
            {
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var companies = await dbContext.Companies.ToListAsync(stoppingToken);
                foreach (var company in companies)
                {
                    if (!string.IsNullOrEmpty(company.Logo) && !company.Logo.StartsWith("data:"))
                    {
                        var logoPath = Path.Combine(env.WebRootPath, company.Logo.TrimStart('/'));
                        if (File.Exists(logoPath))
                        {
                            var bytes = await File.ReadAllBytesAsync(logoPath, stoppingToken);
                            var ext = Path.GetExtension(logoPath).ToLower();
                            var contentType = ext switch
                            {
                                ".png" => "image/png",
                                ".jpg" or ".jpeg" => "image/jpeg",
                                ".gif" => "image/gif",
                                ".svg" => "image/svg+xml",
                                _ => "image/png"
                            };
                            company.Logo = $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
                            _logger.LogInformation("Migrated company '{Name}' logo from file path to database storage", company.Name);
                        }
                        else
                        {
                            _logger.LogWarning("Company '{Name}' logo file not found at '{Path}', clearing logo reference", company.Name, logoPath);
                            company.Logo = null;
                        }
                    }
                }
                if (dbContext.ChangeTracker.HasChanges())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Company logo migration completed");
                }
            }
            catch (Exception logoEx)
            {
                _logger.LogWarning(logoEx, "Company logo migration encountered an issue");
            }

            _logger.LogInformation("Database initialization completed successfully");
            IsReady = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
            IsReady = true;
        }
    }
}
