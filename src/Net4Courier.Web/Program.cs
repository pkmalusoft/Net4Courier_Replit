using Microsoft.AspNetCore.Components.Authorization;
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        
        // Create ShipmentStatus tables if they don't exist (for incremental schema updates)
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
        ");
        
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
        ");
        
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
        ");
        
        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        await authService.SeedAdminUserAsync();
        
        if (!await dbContext.OtherChargeTypes.AnyAsync())
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
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Seeded OtherChargeTypes data");
        }
        
        var shipmentStatusService = scope.ServiceProvider.GetRequiredService<ShipmentStatusService>();
        await shipmentStatusService.SeedDefaultStatuses();
        
        if (!await dbContext.Parties.AnyAsync(p => p.PartyType == Net4Courier.Masters.Entities.PartyType.ForwardingAgent))
        {
            var company = await dbContext.Companies.FirstOrDefaultAsync(c => !c.IsDeleted);
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
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Seeded Forwarding Agents (DHL, FedEx, Aramex, UPS, TNT) - Account Payables");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run("http://0.0.0.0:5000");
