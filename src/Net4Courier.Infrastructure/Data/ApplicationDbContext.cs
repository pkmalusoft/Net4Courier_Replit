using Microsoft.EntityFrameworkCore;
using Net4Courier.Masters.Entities;
using Net4Courier.Operations.Entities;
using Net4Courier.Finance.Entities;

namespace Net4Courier.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<FeaturePermission> FeaturePermissions => Set<FeaturePermission>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyAddress> PartyAddresses => Set<PartyAddress>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Location> Locations => Set<Location>();
    
    public DbSet<InscanMaster> InscanMasters => Set<InscanMaster>();
    public DbSet<InscanMasterItem> InscanMasterItems => Set<InscanMasterItem>();
    public DbSet<AWBTracking> AWBTrackings => Set<AWBTracking>();
    public DbSet<QuickInscanMaster> QuickInscanMasters => Set<QuickInscanMaster>();
    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<DRS> DRSs => Set<DRS>();
    public DbSet<DRSDetail> DRSDetails => Set<DRSDetail>();
    public DbSet<CourierCashSubmission> CourierCashSubmissions => Set<CourierCashSubmission>();
    public DbSet<CourierExpense> CourierExpenses => Set<CourierExpense>();
    public DbSet<CourierLedger> CourierLedgers => Set<CourierLedger>();
    public DbSet<OtherChargeType> OtherChargeTypes => Set<OtherChargeType>();
    public DbSet<AWBOtherCharge> AWBOtherCharges => Set<AWBOtherCharge>();
    public DbSet<PickupRequest> PickupRequests => Set<PickupRequest>();
    public DbSet<PickupRequestShipment> PickupRequestShipments => Set<PickupRequestShipment>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<MasterAirwaybill> MasterAirwaybills => Set<MasterAirwaybill>();
    public DbSet<MAWBBag> MAWBBags => Set<MAWBBag>();
    public DbSet<ShipmentNote> ShipmentNotes => Set<ShipmentNote>();
    public DbSet<ShipmentNoteMention> ShipmentNoteMentions => Set<ShipmentNoteMention>();
    
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<SpecialCharge> SpecialCharges => Set<SpecialCharge>();
    public DbSet<InvoiceSpecialCharge> InvoiceSpecialCharges => Set<InvoiceSpecialCharge>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptAllocation> ReceiptAllocations => Set<ReceiptAllocation>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<AccountHead> AccountHeads => Set<AccountHead>();
    public DbSet<ControlAccountSetting> ControlAccountSettings => Set<ControlAccountSetting>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<DebitNote> DebitNotes => Set<DebitNote>();
    
    public DbSet<RateCard> RateCards => Set<RateCard>();
    public DbSet<ZoneCategory> ZoneCategories => Set<ZoneCategory>();
    public DbSet<ZoneMatrix> ZoneMatrices => Set<ZoneMatrix>();
    public DbSet<ZoneMatrixDetail> ZoneMatrixDetails => Set<ZoneMatrixDetail>();
    public DbSet<RateCardZone> RateCardZones => Set<RateCardZone>();
    public DbSet<RateCardSlabRule> RateCardSlabRules => Set<RateCardSlabRule>();
    public DbSet<CustomerRateAssignment> CustomerRateAssignments => Set<CustomerRateAssignment>();
    public DbSet<SlabRuleTemplate> SlabRuleTemplates => Set<SlabRuleTemplate>();
    public DbSet<SlabRuleTemplateDetail> SlabRuleTemplateDetails => Set<SlabRuleTemplateDetail>();
    
    public DbSet<ShipmentStatusGroup> ShipmentStatusGroups => Set<ShipmentStatusGroup>();
    public DbSet<ShipmentStatus> ShipmentStatuses => Set<ShipmentStatus>();
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistories => Set<ShipmentStatusHistory>();
    
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    
    public DbSet<ImportMaster> ImportMasters => Set<ImportMaster>();
    public DbSet<ImportBag> ImportBags => Set<ImportBag>();
    public DbSet<ImportShipment> ImportShipments => Set<ImportShipment>();
    public DbSet<ImportShipmentNote> ImportShipmentNotes => Set<ImportShipmentNote>();
    
    public DbSet<ApiSetting> ApiSettings => Set<ApiSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureMastersModule(modelBuilder);
        ConfigureOperationsModule(modelBuilder);
        ConfigureFinanceModule(modelBuilder);
    }

    private void ConfigureMastersModule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.State).WithMany().HasForeignKey(e => e.StateId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10).HasDefaultValue("USD");
            entity.Property(e => e.CurrencySymbol).HasMaxLength(10);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Branches)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.State).WithMany().HasForeignKey(e => e.StateId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => new { e.BranchId, e.Code }).IsUnique();
            entity.HasOne(e => e.Branch)
                  .WithMany(b => b.Warehouses)
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<UserBranch>(entity =>
        {
            entity.ToTable("UserBranches");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.BranchId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserBranches)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Branch)
                  .WithMany(b => b.UserBranches)
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
            entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
            entity.HasOne(e => e.UserType).WithMany(ut => ut.Users).HasForeignKey(e => e.UserTypeId);
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("UserTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Role).WithMany(r => r.Permissions).HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<FeaturePermission>(entity =>
        {
            entity.ToTable("FeaturePermissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeatureCode).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => new { e.RoleId, e.FeatureCode }).IsUnique();
            entity.HasOne(e => e.Role).WithMany(r => r.FeaturePermissions).HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<FinancialYear>(entity =>
        {
            entity.ToTable("FinancialYears");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
            entity.HasOne(e => e.Company).WithMany(c => c.FinancialYears).HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<FinancialPeriod>(entity =>
        {
            entity.ToTable("FinancialPeriods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PeriodName).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.FinancialYearId, e.PeriodNumber }).IsUnique();
            entity.HasOne(e => e.FinancialYear).WithMany(fy => fy.Periods).HasForeignKey(e => e.FinancialYearId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("Parties");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<PartyAddress>(entity =>
        {
            entity.ToTable("PartyAddresses");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Party).WithMany(p => p.Addresses).HasForeignKey(e => e.PartyId);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.IATACode).HasMaxLength(10);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("States");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.HasIndex(e => new { e.CountryId, e.Code }).IsUnique();
            entity.HasOne(e => e.Country).WithMany(c => c.States).HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("Cities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.HasIndex(e => new { e.StateId, e.Code }).IsUnique();
            entity.HasOne(e => e.Country).WithMany(c => c.Cities).HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.State).WithMany(s => s.Cities).HasForeignKey(e => e.StateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Pincode).HasMaxLength(20);
            entity.HasIndex(e => new { e.CityId, e.Pincode });
            entity.HasOne(e => e.City).WithMany(c => c.Locations).HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApiSetting>(entity =>
        {
            entity.ToTable("ApiSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BaseUrl).HasMaxLength(500);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.ApiSecret).HasMaxLength(500);
            entity.Property(e => e.WebhookSecret).HasMaxLength(100);
            entity.Property(e => e.WebhookEndpoint).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.BearerToken).HasMaxLength(2000);
            entity.Property(e => e.LastSyncStatus).HasMaxLength(50);
            entity.Property(e => e.LastSyncError).HasMaxLength(2000);
            entity.Property(e => e.Headers).HasMaxLength(4000);
            entity.Property(e => e.CustomFields).HasMaxLength(4000);
            entity.HasIndex(e => new { e.IntegrationType, e.BranchId });
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<RateCard>(entity =>
        {
            entity.ToTable("RateCards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RateCardName).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.RateCardName);
            entity.HasIndex(e => new { e.CompanyId, e.Status });
            entity.HasIndex(e => new { e.MovementTypeId, e.PaymentModeId });
        });

        modelBuilder.Entity<ZoneCategory>(entity =>
        {
            entity.ToTable("ZoneCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.ForwardingAgent).WithMany().HasForeignKey(e => e.ForwardingAgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ZoneMatrix>(entity =>
        {
            entity.ToTable("ZoneMatrices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ZoneCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ZoneName).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => new { e.ZoneCategoryId, e.ZoneCode }).IsUnique();
            entity.HasIndex(e => new { e.CountryId, e.CityId });
            entity.HasOne(e => e.ZoneCategory).WithMany(z => z.ZoneMatrices).HasForeignKey(e => e.ZoneCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Country).WithMany().HasForeignKey(e => e.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ZoneMatrixDetail>(entity =>
        {
            entity.ToTable("ZoneMatrixDetails");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ZoneMatrixId, e.CountryId, e.CityId });
            entity.HasOne(e => e.ZoneMatrix).WithMany(z => z.Details).HasForeignKey(e => e.ZoneMatrixId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RateCardZone>(entity =>
        {
            entity.ToTable("RateCardZones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseWeight).HasPrecision(18, 3);
            entity.Property(e => e.BaseRate).HasPrecision(18, 4);
            entity.Property(e => e.CostBaseRate).HasPrecision(18, 4);
            entity.Property(e => e.CostPerKg).HasPrecision(18, 4);
            entity.Property(e => e.SalesBaseRate).HasPrecision(18, 4);
            entity.Property(e => e.SalesPerKg).HasPrecision(18, 4);
            entity.Property(e => e.MarginPercentage).HasPrecision(5, 2);
            entity.Property(e => e.MinCharge).HasPrecision(18, 4);
            entity.Property(e => e.MaxCharge).HasPrecision(18, 4);
            entity.Ignore(e => e.MarginBaseRate);
            entity.Ignore(e => e.MarginPerKg);
            entity.HasIndex(e => new { e.RateCardId, e.ZoneMatrixId });
            entity.HasIndex(e => new { e.RateCardId, e.ForwardingAgentId });
            entity.HasOne(e => e.RateCard).WithMany(r => r.RateCardZones).HasForeignKey(e => e.RateCardId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ZoneMatrix).WithMany(z => z.RateCardZones).HasForeignKey(e => e.ZoneMatrixId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RateCardSlabRule>(entity =>
        {
            entity.ToTable("RateCardSlabRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromWeight).HasPrecision(18, 3);
            entity.Property(e => e.ToWeight).HasPrecision(18, 3);
            entity.Property(e => e.IncrementWeight).HasPrecision(18, 3);
            entity.Property(e => e.IncrementRate).HasPrecision(18, 4);
            entity.HasIndex(e => new { e.RateCardZoneId, e.FromWeight, e.ToWeight });
            entity.HasOne(e => e.RateCardZone).WithMany(r => r.SlabRules).HasForeignKey(e => e.RateCardZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerRateAssignment>(entity =>
        {
            entity.ToTable("CustomerRateAssignments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerId, e.EffectiveFrom, e.Priority });
            entity.HasIndex(e => new { e.RateCardId, e.CustomerId });
            entity.HasOne(e => e.RateCard).WithMany(r => r.CustomerAssignments).HasForeignKey(e => e.RateCardId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SlabRuleTemplate>(entity =>
        {
            entity.ToTable("SlabRuleTemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BaseWeight).HasPrecision(18, 3);
            entity.Property(e => e.BaseRate).HasPrecision(18, 4);
            entity.HasIndex(e => new { e.CompanyId, e.TemplateName }).IsUnique();
        });

        modelBuilder.Entity<SlabRuleTemplateDetail>(entity =>
        {
            entity.ToTable("SlabRuleTemplateDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromWeight).HasPrecision(18, 3);
            entity.Property(e => e.ToWeight).HasPrecision(18, 3);
            entity.Property(e => e.IncrementWeight).HasPrecision(18, 3);
            entity.Property(e => e.IncrementRate).HasPrecision(18, 4);
            entity.HasIndex(e => new { e.TemplateId, e.SortOrder });
            entity.HasOne(e => e.Template).WithMany(t => t.Details).HasForeignKey(e => e.TemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureOperationsModule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InscanMaster>(entity =>
        {
            entity.ToTable("InscanMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MAWBNo).HasMaxLength(50);
            entity.Property(e => e.BagNo).HasMaxLength(50);
            entity.Property(e => e.BaggedByUserName).HasMaxLength(100);
            entity.Property(e => e.HoldReason).HasMaxLength(500);
            entity.Property(e => e.HoldByUserName).HasMaxLength(100);
            entity.Property(e => e.HoldReleasedByUserName).HasMaxLength(100);
            
            entity.HasIndex(e => e.AWBNo);
            entity.HasIndex(e => new { e.BranchId, e.TransactionDate });
            entity.HasIndex(e => new { e.CustomerId, e.TransactionDate });
            entity.HasIndex(e => new { e.AWBNo, e.CourierStatusId });
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.FinancialYearId);
            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.DRSId);
            entity.HasIndex(e => e.MAWBId);
            entity.HasIndex(e => e.MAWBBagId);
            entity.HasIndex(e => e.IsOnHold);
            
            entity.HasOne(e => e.MAWB).WithMany().HasForeignKey(e => e.MAWBId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.MAWBBag).WithMany(b => b.Shipments).HasForeignKey(e => e.MAWBBagId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InscanMasterItem>(entity =>
        {
            entity.ToTable("InscanMasterItems");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Inscan).WithMany(i => i.Items).HasForeignKey(e => e.InscanId);
        });

        modelBuilder.Entity<AWBTracking>(entity =>
        {
            entity.ToTable("AWBTrackings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InscanId, e.EventDateTime });
            entity.HasOne(e => e.Inscan).WithMany(i => i.TrackingHistory).HasForeignKey(e => e.InscanId);
        });

        modelBuilder.Entity<QuickInscanMaster>(entity =>
        {
            entity.ToTable("QuickInscanMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InscanSheetNumber).HasMaxLength(50);
            entity.HasIndex(e => e.InscanSheetNumber);
        });

        modelBuilder.Entity<Manifest>(entity =>
        {
            entity.ToTable("Manifests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ManifestNo).HasMaxLength(50);
            entity.HasIndex(e => e.ManifestNo);
        });

        modelBuilder.Entity<DRS>(entity =>
        {
            entity.ToTable("DRS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DRSNo).HasMaxLength(50);
            entity.Property(e => e.ExpectedTotal).HasPrecision(18, 2);
            entity.Property(e => e.ActualReceived).HasPrecision(18, 2);
            entity.Property(e => e.ApprovedExpenses).HasPrecision(18, 2);
            entity.Property(e => e.Variance).HasPrecision(18, 2);
            entity.Property(e => e.TotalCourierCharges).HasPrecision(18, 2);
            entity.Property(e => e.TotalMaterialCost).HasPrecision(18, 2);
            entity.Property(e => e.PickupCash).HasPrecision(18, 2);
            entity.Property(e => e.OutstandingCollected).HasPrecision(18, 2);
            entity.HasIndex(e => e.DRSNo);
            entity.HasIndex(e => new { e.DeliveryEmployeeId, e.DRSDate });
            entity.HasIndex(e => new { e.Status, e.DRSDate });
        });

        modelBuilder.Entity<DRSDetail>(entity =>
        {
            entity.ToTable("DRSDetails");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DRS).WithMany(d => d.Details).HasForeignKey(e => e.DRSId);
            entity.HasOne(e => e.Inscan).WithMany().HasForeignKey(e => e.InscanId);
        });

        modelBuilder.Entity<CourierCashSubmission>(entity =>
        {
            entity.ToTable("CourierCashSubmissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CashSubmittedAmount).HasPrecision(18, 2);
            entity.Property(e => e.ReceivedAmount).HasPrecision(18, 2);
            entity.Property(e => e.ReceiptNo).HasMaxLength(50);
            entity.HasIndex(e => new { e.DRSId, e.SubmissionDate });
            entity.HasIndex(e => new { e.CourierId, e.SubmissionDate });
            entity.HasOne(e => e.DRS).WithMany(d => d.CashSubmissions).HasForeignKey(e => e.DRSId);
        });

        modelBuilder.Entity<CourierExpense>(entity =>
        {
            entity.ToTable("CourierExpenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BillImagePath).HasMaxLength(500);
            entity.HasIndex(e => new { e.DRSId, e.Status });
            entity.HasIndex(e => new { e.CourierId, e.ExpenseDate });
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.DRS).WithMany(d => d.Expenses).HasForeignKey(e => e.DRSId);
        });

        modelBuilder.Entity<CourierLedger>(entity =>
        {
            entity.ToTable("CourierLedgers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DebitAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
            entity.Property(e => e.RunningBalance).HasPrecision(18, 2);
            entity.Property(e => e.DRSNo).HasMaxLength(50);
            entity.Property(e => e.Narration).HasMaxLength(500);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.HasIndex(e => new { e.CourierId, e.TransactionDate });
            entity.HasIndex(e => new { e.CourierId, e.IsSettled });
            entity.HasOne(e => e.DRS).WithMany().HasForeignKey(e => e.DRSId);
        });

        modelBuilder.Entity<OtherChargeType>(entity =>
        {
            entity.ToTable("OtherChargeTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<AWBOtherCharge>(entity =>
        {
            entity.ToTable("AWBOtherCharges");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InscanId, e.OtherChargeTypeId }).IsUnique();
            entity.HasOne(e => e.Inscan).WithMany(i => i.OtherCharges).HasForeignKey(e => e.InscanId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OtherChargeType).WithMany().HasForeignKey(e => e.OtherChargeTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PickupRequest>(entity =>
        {
            entity.ToTable("PickupRequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PickupNo).HasMaxLength(50);
            entity.HasIndex(e => e.PickupNo);
            entity.HasIndex(e => new { e.BranchId, e.RequestDate });
            entity.HasIndex(e => new { e.CustomerId, e.Status });
            entity.HasIndex(e => new { e.CourierId, e.Status });
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<PickupRequestShipment>(entity =>
        {
            entity.ToTable("PickupRequestShipments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PickupRequestId, e.LineNo });
            entity.HasIndex(e => e.AWBId);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.PickupRequest)
                  .WithMany(p => p.Shipments)
                  .HasForeignKey(e => e.PickupRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehicleNo).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.VehicleNo);
        });

        modelBuilder.Entity<MasterAirwaybill>(entity =>
        {
            entity.ToTable("MasterAirwaybills");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MAWBNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OriginCityName).HasMaxLength(100);
            entity.Property(e => e.OriginCountryName).HasMaxLength(100);
            entity.Property(e => e.OriginAirportCode).HasMaxLength(10);
            entity.Property(e => e.DestinationCityName).HasMaxLength(100);
            entity.Property(e => e.DestinationCountryName).HasMaxLength(100);
            entity.Property(e => e.DestinationAirportCode).HasMaxLength(10);
            entity.Property(e => e.CarrierCode).HasMaxLength(20);
            entity.Property(e => e.CarrierName).HasMaxLength(100);
            entity.Property(e => e.FlightNo).HasMaxLength(20);
            entity.Property(e => e.CoLoaderName).HasMaxLength(200);
            entity.Property(e => e.CoLoaderMAWBNo).HasMaxLength(50);
            entity.Property(e => e.TotalGrossWeight).HasPrecision(18, 3);
            entity.Property(e => e.TotalChargeableWeight).HasPrecision(18, 3);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.CustomsDeclarationNo).HasMaxLength(50);
            entity.Property(e => e.ExportPermitNo).HasMaxLength(50);
            entity.Property(e => e.FinalizedByUserName).HasMaxLength(100);
            entity.Property(e => e.DispatchedByUserName).HasMaxLength(100);
            entity.HasIndex(e => e.MAWBNo).IsUnique();
            entity.HasIndex(e => new { e.BranchId, e.TransactionDate });
            entity.HasIndex(e => new { e.Status, e.TransactionDate });
            entity.HasIndex(e => new { e.OriginCityId, e.DestinationCityId });
        });

        modelBuilder.Entity<MAWBBag>(entity =>
        {
            entity.ToTable("MAWBBags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BagNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SealNo).HasMaxLength(50);
            entity.Property(e => e.BagType).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.GrossWeight).HasPrecision(18, 3);
            entity.Property(e => e.ChargeableWeight).HasPrecision(18, 3);
            entity.Property(e => e.SealedByUserName).HasMaxLength(100);
            entity.HasIndex(e => new { e.MAWBId, e.BagNo }).IsUnique();
            entity.HasIndex(e => e.BagNo);
            entity.HasOne(e => e.MAWB).WithMany(m => m.Bags).HasForeignKey(e => e.MAWBId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentNote>(entity =>
        {
            entity.ToTable("ShipmentNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Body).HasMaxLength(2000).IsRequired();
            entity.HasIndex(e => e.ShipmentId);
            entity.HasIndex(e => e.AuthorUserId);
            entity.HasOne(e => e.Shipment).WithMany(s => s.Notes).HasForeignKey(e => e.ShipmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentNoteMention>(entity =>
        {
            entity.ToTable("ShipmentNoteMentions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MentionedUserName).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.MentionedUserId);
            entity.HasOne(e => e.Note).WithMany(n => n.Mentions).HasForeignKey(e => e.NoteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentStatusGroup>(entity =>
        {
            entity.ToTable("ShipmentStatusGroups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.SequenceNo);
        });

        modelBuilder.Entity<ShipmentStatus>(entity =>
        {
            entity.ToTable("ShipmentStatuses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TimelineDescription).HasMaxLength(200);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.StatusGroupId, e.SequenceNo });
            entity.HasOne(e => e.StatusGroup).WithMany(g => g.Statuses).HasForeignKey(e => e.StatusGroupId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShipmentStatusHistory>(entity =>
        {
            entity.ToTable("ShipmentStatusHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50);
            entity.Property(e => e.EventRefType).HasMaxLength(50);
            entity.Property(e => e.LocationName).HasMaxLength(200);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasPrecision(10, 6);
            entity.Property(e => e.Longitude).HasPrecision(10, 6);
            entity.HasIndex(e => new { e.InscanMasterId, e.ChangedAt });
            entity.HasIndex(e => new { e.StatusId, e.ChangedAt });
            entity.HasIndex(e => e.ChangedAt);
            entity.HasOne(e => e.InscanMaster).WithMany().HasForeignKey(e => e.InscanMasterId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Status).WithMany(s => s.History).HasForeignKey(e => e.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusGroup).WithMany().HasForeignKey(e => e.StatusGroupId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportMaster>(entity =>
        {
            entity.ToTable("ImportMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImportRefNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MasterReferenceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OriginCountryName).HasMaxLength(100);
            entity.Property(e => e.OriginCityName).HasMaxLength(100);
            entity.Property(e => e.OriginPortCode).HasMaxLength(20);
            entity.Property(e => e.DestinationCountryName).HasMaxLength(100);
            entity.Property(e => e.DestinationCityName).HasMaxLength(100);
            entity.Property(e => e.DestinationPortCode).HasMaxLength(20);
            entity.Property(e => e.CarrierName).HasMaxLength(100);
            entity.Property(e => e.CarrierCode).HasMaxLength(20);
            entity.Property(e => e.FlightNo).HasMaxLength(20);
            entity.Property(e => e.VesselName).HasMaxLength(100);
            entity.Property(e => e.VoyageNumber).HasMaxLength(50);
            entity.Property(e => e.TruckNumber).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasMaxLength(20);
            entity.Property(e => e.ManifestNumber).HasMaxLength(50);
            entity.Property(e => e.ImportWarehouseName).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.CoLoaderName).HasMaxLength(200);
            entity.Property(e => e.CoLoaderRefNo).HasMaxLength(50);
            entity.Property(e => e.CustomsDeclarationNo).HasMaxLength(50);
            entity.Property(e => e.ExportPermitNo).HasMaxLength(50);
            entity.Property(e => e.InscannedByUserName).HasMaxLength(100);
            entity.Property(e => e.ClosedByUserName).HasMaxLength(100);
            entity.Property(e => e.TotalGrossWeight).HasPrecision(18, 3);
            entity.Property(e => e.TotalChargeableWeight).HasPrecision(18, 3);
            entity.HasIndex(e => e.ImportRefNo).IsUnique();
            entity.HasIndex(e => e.MasterReferenceNumber);
            entity.HasIndex(e => new { e.BranchId, e.TransactionDate });
            entity.HasIndex(e => new { e.Status, e.TransactionDate });
            entity.HasIndex(e => e.ImportMode);
        });

        modelBuilder.Entity<ImportBag>(entity =>
        {
            entity.ToTable("ImportBags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BagNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SealNumber).HasMaxLength(50);
            entity.Property(e => e.HandlingCode).HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.InscannedByUserName).HasMaxLength(100);
            entity.Property(e => e.GrossWeight).HasPrecision(18, 3);
            entity.Property(e => e.ChargeableWeight).HasPrecision(18, 3);
            entity.Property(e => e.Length).HasPrecision(18, 2);
            entity.Property(e => e.Width).HasPrecision(18, 2);
            entity.Property(e => e.Height).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.ImportMasterId, e.BagNumber }).IsUnique();
            entity.HasIndex(e => e.BagNumber);
            entity.HasOne(e => e.ImportMaster).WithMany(m => m.Bags).HasForeignKey(e => e.ImportMasterId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImportShipment>(entity =>
        {
            entity.ToTable("ImportShipments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ReferenceNo).HasMaxLength(50);
            entity.Property(e => e.ShipperName).HasMaxLength(200);
            entity.Property(e => e.ShipperAddress).HasMaxLength(500);
            entity.Property(e => e.ShipperCity).HasMaxLength(100);
            entity.Property(e => e.ShipperCountry).HasMaxLength(100);
            entity.Property(e => e.ShipperPhone).HasMaxLength(20);
            entity.Property(e => e.ConsigneeName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ConsigneeAddress).HasMaxLength(500);
            entity.Property(e => e.ConsigneeCity).HasMaxLength(100);
            entity.Property(e => e.ConsigneeState).HasMaxLength(100);
            entity.Property(e => e.ConsigneeCountry).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConsigneePostalCode).HasMaxLength(20);
            entity.Property(e => e.ConsigneePhone).HasMaxLength(20);
            entity.Property(e => e.ConsigneeMobile).HasMaxLength(20);
            entity.Property(e => e.Weight).HasPrecision(18, 3);
            entity.Property(e => e.VolumetricWeight).HasPrecision(18, 3);
            entity.Property(e => e.ChargeableWeight).HasPrecision(18, 3);
            entity.Property(e => e.ContentsDescription).HasMaxLength(500);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(500);
            entity.Property(e => e.DeclaredValue).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.HSCode).HasMaxLength(20);
            entity.Property(e => e.DutyAmount).HasPrecision(18, 2);
            entity.Property(e => e.VATAmount).HasPrecision(18, 2);
            entity.Property(e => e.OtherCharges).HasPrecision(18, 2);
            entity.Property(e => e.TotalCustomsCharges).HasPrecision(18, 2);
            entity.Property(e => e.CODAmount).HasPrecision(18, 2);
            entity.Property(e => e.HoldReasonDetails).HasMaxLength(500);
            entity.Property(e => e.ImporterOfRecord).HasMaxLength(200);
            entity.Property(e => e.CustomsEntryNumber).HasMaxLength(50);
            entity.Property(e => e.ExaminationRemarks).HasMaxLength(500);
            entity.Property(e => e.InscannedByUserName).HasMaxLength(100);
            entity.Property(e => e.CustomsClearedByUserName).HasMaxLength(100);
            entity.Property(e => e.ReleasedByUserName).HasMaxLength(100);
            entity.Property(e => e.HandedOverToUserName).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.HasIndex(e => e.AWBNo);
            entity.HasIndex(e => new { e.ImportMasterId, e.AWBNo }).IsUnique();
            entity.HasIndex(e => e.ImportBagId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomsStatus);
            entity.HasIndex(e => e.HoldReason);
            entity.HasOne(e => e.ImportMaster).WithMany().HasForeignKey(e => e.ImportMasterId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ImportBag).WithMany(b => b.Shipments).HasForeignKey(e => e.ImportBagId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportShipmentNote>(entity =>
        {
            entity.ToTable("ImportShipmentNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NoteText).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.AddedByUserName).HasMaxLength(100);
            entity.HasIndex(e => e.ImportShipmentId);
            entity.HasIndex(e => e.AddedAt);
            entity.HasOne(e => e.ImportShipment).WithMany(s => s.Notes).HasForeignKey(e => e.ImportShipmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFinanceModule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.HasIndex(e => e.InvoiceNo);
            entity.HasIndex(e => new { e.CustomerId, e.InvoiceDate });
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.ToTable("InvoiceDetails");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Invoice).WithMany(i => i.Details).HasForeignKey(e => e.InvoiceId);
        });

        modelBuilder.Entity<SpecialCharge>(entity =>
        {
            entity.ToTable("SpecialCharges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChargeName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ChargeCode).HasMaxLength(20);
            entity.HasIndex(e => new { e.CompanyId, e.CustomerId, e.FromDate, e.ToDate });
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<InvoiceSpecialCharge>(entity =>
        {
            entity.ToTable("InvoiceSpecialCharges");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Invoice).WithMany(i => i.SpecialCharges).HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SpecialCharge).WithMany().HasForeignKey(e => e.SpecialChargeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.ToTable("Receipts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNo).HasMaxLength(50);
            entity.HasIndex(e => e.ReceiptNo);
        });

        modelBuilder.Entity<ReceiptAllocation>(entity =>
        {
            entity.ToTable("ReceiptAllocations");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Receipt).WithMany(r => r.Allocations).HasForeignKey(e => e.ReceiptId);
        });

        modelBuilder.Entity<Journal>(entity =>
        {
            entity.ToTable("Journals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VoucherNo).HasMaxLength(50);
            entity.HasIndex(e => e.VoucherNo);
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntries");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Journal).WithMany(j => j.Entries).HasForeignKey(e => e.JournalId);
        });

        modelBuilder.Entity<AccountHead>(entity =>
        {
            entity.ToTable("AccountHeads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Parent).WithMany(a => a.Children).HasForeignKey(e => e.ParentId);
        });

        modelBuilder.Entity<ControlAccountSetting>(entity =>
        {
            entity.ToTable("ControlAccountSettings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.AccountType }).IsUnique();
            entity.HasOne(e => e.AccountHead).WithMany().HasForeignKey(e => e.AccountHeadId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.ToTable("CreditNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreditNoteNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.ReasonDetails).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.CreditNoteNo).IsUnique();
            entity.HasIndex(e => new { e.CustomerId, e.CreditNoteDate });
            entity.HasIndex(e => new { e.BranchId, e.CreditNoteDate });
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<DebitNote>(entity =>
        {
            entity.ToTable("DebitNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DebitNoteNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SupplierName).HasMaxLength(200);
            entity.Property(e => e.BillNo).HasMaxLength(50);
            entity.Property(e => e.ReasonDetails).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.DebitNoteNo).IsUnique();
            entity.HasIndex(e => new { e.SupplierId, e.DebitNoteDate });
            entity.HasIndex(e => new { e.BranchId, e.DebitNoteDate });
            entity.HasIndex(e => e.Status);
        });
    }
}
