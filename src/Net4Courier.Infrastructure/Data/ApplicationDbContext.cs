using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Net4Courier.Kernel.Entities;
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
    public DbSet<BranchAWBConfig> BranchAWBConfigs => Set<BranchAWBConfig>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<FeaturePermission> FeaturePermissions => Set<FeaturePermission>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyAddress> PartyAddresses => Set<PartyAddress>();
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<CustomerBranch> CustomerBranches => Set<CustomerBranch>();
    public DbSet<SLAAgreement> SLAAgreements => Set<SLAAgreement>();
    public DbSet<SLATransitRule> SLATransitRules => Set<SLATransitRule>();
    public DbSet<SLADocument> SLADocuments => Set<SLADocument>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<CustomerZone> CustomerZones => Set<CustomerZone>();
    public DbSet<CustomerZoneCity> CustomerZoneCities => Set<CustomerZoneCity>();
    public DbSet<CustomerZoneCourier> CustomerZoneCouriers => Set<CustomerZoneCourier>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<InscanMaster> InscanMasters => Set<InscanMaster>();
    public DbSet<InscanMasterItem> InscanMasterItems => Set<InscanMasterItem>();
    public DbSet<ShipmentDocument> ShipmentDocuments => Set<ShipmentDocument>();
    public DbSet<AWBTracking> AWBTrackings => Set<AWBTracking>();
    public DbSet<QuickInscanMaster> QuickInscanMasters => Set<QuickInscanMaster>();
    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<DRS> DRSs => Set<DRS>();
    public DbSet<DRSDetail> DRSDetails => Set<DRSDetail>();
    public DbSet<CourierCashSubmission> CourierCashSubmissions => Set<CourierCashSubmission>();
    public DbSet<CourierExpense> CourierExpenses => Set<CourierExpense>();
    public DbSet<CourierLedger> CourierLedgers => Set<CourierLedger>();
    public DbSet<OtherChargeType> OtherChargeTypes => Set<OtherChargeType>();
    public DbSet<FuelSurcharge> FuelSurcharges => Set<FuelSurcharge>();
    public DbSet<DiscountContract> DiscountContracts => Set<DiscountContract>();
    public DbSet<AWBOtherCharge> AWBOtherCharges => Set<AWBOtherCharge>();
    public DbSet<PickupRequest> PickupRequests => Set<PickupRequest>();
    public DbSet<PickupRequestShipment> PickupRequestShipments => Set<PickupRequestShipment>();
    public DbSet<PickupSchedule> PickupSchedules => Set<PickupSchedule>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<MasterAirwaybill> MasterAirwaybills => Set<MasterAirwaybill>();
    public DbSet<MAWBBag> MAWBBags => Set<MAWBBag>();
    public DbSet<ShipmentNote> ShipmentNotes => Set<ShipmentNote>();
    public DbSet<ShipmentNoteMention> ShipmentNoteMentions => Set<ShipmentNoteMention>();
    
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    
    public DbSet<AWBStock> AWBStocks => Set<AWBStock>();
    public DbSet<PrepaidDocument> PrepaidDocuments => Set<PrepaidDocument>();
    public DbSet<PrepaidAWB> PrepaidAWBs => Set<PrepaidAWB>();
    
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
    public DbSet<VendorBill> VendorBills => Set<VendorBill>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    
    // GL Module Native Entities (long IDs)
    public DbSet<GLChartOfAccount> GLChartOfAccounts => Set<GLChartOfAccount>();
    public DbSet<GLAccountClassification> GLAccountClassifications => Set<GLAccountClassification>();
    public DbSet<GLTaxCode> GLTaxCodes => Set<GLTaxCode>();
    public DbSet<GLVoucherNumbering> GLVoucherNumberings => Set<GLVoucherNumbering>();
    
    // Cash & Bank Module Entities
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<CashBankTransaction> CashBankTransactions => Set<CashBankTransaction>();
    public DbSet<CashBankTransactionLine> CashBankTransactionLines => Set<CashBankTransactionLine>();
    public DbSet<VoucherAttachment> VoucherAttachments => Set<VoucherAttachment>();
    
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<BankStatementImport> BankStatementImports => Set<BankStatementImport>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<ReconciliationMatch> ReconciliationMatches => Set<ReconciliationMatch>();
    public DbSet<ReconciliationAdjustment> ReconciliationAdjustments => Set<ReconciliationAdjustment>();
    
    public DbSet<EmpostLicense> EmpostLicenses => Set<EmpostLicense>();
    public DbSet<EmpostAdvancePayment> EmpostAdvancePayments => Set<EmpostAdvancePayment>();
    public DbSet<EmpostQuarter> EmpostQuarters => Set<EmpostQuarter>();
    public DbSet<EmpostShipmentFee> EmpostShipmentFees => Set<EmpostShipmentFee>();
    public DbSet<EmpostQuarterlySettlement> EmpostQuarterlySettlements => Set<EmpostQuarterlySettlement>();
    public DbSet<EmpostReturnAdjustment> EmpostReturnAdjustments => Set<EmpostReturnAdjustment>();
    public DbSet<EmpostAuditLog> EmpostAuditLogs => Set<EmpostAuditLog>();
    
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
    public DbSet<StatusEventMapping> StatusEventMappings => Set<StatusEventMapping>();
    public DbSet<PickupStatusHistory> PickupStatusHistories => Set<PickupStatusHistory>();
    
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Port> Ports => Set<Port>();
    public DbSet<ShipmentMode> ShipmentModes => Set<ShipmentMode>();
    public DbSet<IncoTerm> IncoTerms => Set<IncoTerm>();
    
    public DbSet<ImportMaster> ImportMasters => Set<ImportMaster>();
    public DbSet<ImportBag> ImportBags => Set<ImportBag>();
    public DbSet<ImportShipment> ImportShipments => Set<ImportShipment>();
    public DbSet<ImportShipmentNote> ImportShipmentNotes => Set<ImportShipmentNote>();
    public DbSet<ImportDocument> ImportDocuments => Set<ImportDocument>();
    
    public DbSet<ApiSetting> ApiSettings => Set<ApiSetting>();
    
    public DbSet<CODRemittance> CODRemittances => Set<CODRemittance>();
    public DbSet<CODRemittanceDetail> CODRemittanceDetails => Set<CODRemittanceDetail>();
    public DbSet<PickupCommitment> PickupCommitments => Set<PickupCommitment>();
    public DbSet<IncentiveSchedule> IncentiveSchedules => Set<IncentiveSchedule>();
    public DbSet<IncentiveAward> IncentiveAwards => Set<IncentiveAward>();
    public DbSet<TransferOrder> TransferOrders => Set<TransferOrder>();
    public DbSet<TransferOrderItem> TransferOrderItems => Set<TransferOrderItem>();
    public DbSet<TransferOrderEvent> TransferOrderEvents => Set<TransferOrderEvent>();
    
    public DbSet<CustomerAwbIssue> CustomerAwbIssues => Set<CustomerAwbIssue>();
    public DbSet<CustomerAwbIssueDetail> CustomerAwbIssueDetails => Set<CustomerAwbIssueDetail>();
    public DbSet<CustomerAwbBalance> CustomerAwbBalances => Set<CustomerAwbBalance>();
    
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    // Audit context - set these before SaveChanges for audit logging
    public long? CurrentUserId { get; set; }
    public string? CurrentUserName { get; set; }
    public long? CurrentBranchId { get; set; }
    public string? CurrentBranchName { get; set; }
    public string? CurrentIPAddress { get; set; }
    
    // Entities to exclude from audit logging
    private static readonly HashSet<string> ExcludedFromAudit = new()
    {
        nameof(AuditLog),
        nameof(AWBTracking),
        nameof(ShipmentStatusHistory),
        nameof(PickupStatusHistory)
    };

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries, cancellationToken);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var entityName = entry.Entity.GetType().Name;
            if (ExcludedFromAudit.Contains(entityName))
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entityName,
                UserId = CurrentUserId,
                UserName = CurrentUserName,
                BranchId = CurrentBranchId,
                BranchName = CurrentBranchName,
                IPAddress = CurrentIPAddress
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.Action = AuditAction.Create;
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                            auditEntry.HasTemporaryKey = property.IsTemporary;
                        }
                        auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    break;

                case EntityState.Modified:
                    auditEntry.Action = AuditAction.Update;
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                            auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                        }
                    }
                    break;

                case EntityState.Deleted:
                    auditEntry.Action = AuditAction.Delete;
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                    }
                    break;
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            // Get the generated primary key for new entities
            if (auditEntry.HasTemporaryKey)
            {
                foreach (var prop in auditEntry.Entry.Properties.Where(p => p.Metadata.IsPrimaryKey()))
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            var entityId = auditEntry.KeyValues.Values.FirstOrDefault();
            
            AuditLogs.Add(new AuditLog
            {
                EntityName = auditEntry.EntityName,
                EntityId = entityId is long id ? id : 0,
                Action = auditEntry.Action,
                OldValues = auditEntry.OldValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.OldValues) : null,
                NewValues = auditEntry.NewValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.NewValues) : null,
                UserId = auditEntry.UserId,
                UserName = auditEntry.UserName,
                BranchId = auditEntry.BranchId,
                BranchName = auditEntry.BranchName,
                IPAddress = auditEntry.IPAddress,
                Timestamp = DateTime.UtcNow
            });
        }

        await base.SaveChangesAsync(cancellationToken);
    }

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
            entity.HasOne(e => e.Currency).WithMany().HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Currency).WithMany().HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(e => e.ClientAddress).HasMaxLength(2000);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
            entity.HasOne(e => e.AccountType)
                  .WithMany(a => a.Parties)
                  .HasForeignKey(e => e.AccountTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Branches)
                  .WithOne(b => b.Party)
                  .HasForeignKey(b => b.PartyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CustomerZone)
                  .WithMany(z => z.Customers)
                  .HasForeignKey(e => e.CustomerZoneId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.ToTable("AccountTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<CustomerBranch>(entity =>
        {
            entity.ToTable("CustomerBranches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.ContactName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.HasOne(e => e.CityNavigation).WithMany().HasForeignKey(e => e.CityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.PartyId, e.Code }).IsUnique();
        });

        modelBuilder.Entity<PartyAddress>(entity =>
        {
            entity.ToTable("PartyAddresses");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Party).WithMany(p => p.Addresses).HasForeignKey(e => e.PartyId);
        });

        modelBuilder.Entity<SLAAgreement>(entity =>
        {
            entity.ToTable("SLAAgreements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AgreementNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.LiabilityLimitCurrency).HasMaxLength(10);
            entity.Property(e => e.SpecialTerms).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.TerminationReason).HasMaxLength(500);
            entity.Property(e => e.ApprovedByUserName).HasMaxLength(200);
            entity.Property(e => e.TerminatedByUserName).HasMaxLength(200);
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.HasIndex(e => new { e.CompanyId, e.AgreementNo }).IsUnique();
            entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SLATransitRule>(entity =>
        {
            entity.ToTable("SLATransitRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginZone).HasMaxLength(100);
            entity.Property(e => e.DestinationZone).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.SLAAgreement).WithMany(s => s.TransitRules).HasForeignKey(e => e.SLAAgreementId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ServiceType).WithMany().HasForeignKey(e => e.ServiceTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OriginCountry).WithMany().HasForeignKey(e => e.OriginCountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DestinationCountry).WithMany().HasForeignKey(e => e.DestinationCountryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OriginCity).WithMany().HasForeignKey(e => e.OriginCityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DestinationCity).WithMany().HasForeignKey(e => e.DestinationCityId)
                  .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<CustomerZone>(entity =>
        {
            entity.ToTable("CustomerZones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.BranchId, e.Code }).IsUnique();
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerZoneCity>(entity =>
        {
            entity.ToTable("CustomerZoneCities");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerZoneId, e.CityId }).IsUnique();
            entity.HasOne(e => e.CustomerZone).WithMany(z => z.Cities).HasForeignKey(e => e.CustomerZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerZoneCourier>(entity =>
        {
            entity.ToTable("CustomerZoneCouriers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerZoneId, e.UserId }).IsUnique();
            entity.HasOne(e => e.CustomerZone).WithMany(z => z.Couriers).HasForeignKey(e => e.CustomerZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("Currencies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(10);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.ToTable("Designations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DeactivationReason).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DeactivationReason).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ParentDepartment).WithMany(d => d.ChildDepartments).HasForeignKey(e => e.ParentDepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(200);
            entity.Property(e => e.LastName).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.TaxIdNumber).HasMaxLength(50);
            entity.Property(e => e.EmergencyContact).HasMaxLength(200);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.BankIFSC).HasMaxLength(20);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.DeactivationReason).HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.BranchId, e.DepartmentId });
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Designation).WithMany(d => d.Employees).HasForeignKey(e => e.DesignationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department).WithMany(d => d.Employees).HasForeignKey(e => e.DepartmentId)
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
            entity.HasOne(e => e.ServiceType).WithMany().HasForeignKey(e => e.ServiceTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ShipmentMode).WithMany().HasForeignKey(e => e.ShipmentModeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ZoneCategory>(entity =>
        {
            entity.ToTable("ZoneCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CategoryType).HasColumnName("CategoryType");
            entity.Property(e => e.DeliveryAgentId).HasColumnName("DeliveryAgentId");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Ignore(e => e.AgentId);
            entity.Ignore(e => e.Agent);
            entity.HasOne(e => e.ForwardingAgent).WithMany().HasForeignKey(e => e.ForwardingAgentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DeliveryAgent).WithMany().HasForeignKey(e => e.DeliveryAgentId)
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

        modelBuilder.Entity<ShipmentDocument>(entity =>
        {
            entity.ToTable("ShipmentDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.InscanMasterId);
            entity.HasOne(e => e.InscanMaster).WithMany().HasForeignKey(e => e.InscanMasterId);
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
            entity.HasOne(e => e.CurrentStatus)
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CurrentStatusGroup)
                  .WithMany()
                  .HasForeignKey(e => e.StatusGroupId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.StatusHistories)
                  .WithOne(h => h.PickupRequest)
                  .HasForeignKey(h => h.PickupRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PickupStatusHistory>(entity =>
        {
            entity.ToTable("PickupStatusHistories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PickupRequestId);
            entity.HasIndex(e => new { e.PickupRequestId, e.ChangedAt });
            entity.HasOne(e => e.Status)
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StatusGroup)
                  .WithMany()
                  .HasForeignKey(e => e.StatusGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<PickupSchedule>(entity =>
        {
            entity.ToTable("PickupSchedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.BranchId, e.IsActive });
            entity.HasIndex(e => e.SortOrder);
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

        modelBuilder.Entity<StatusEventMapping>(entity =>
        {
            entity.ToTable("StatusEventMappings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EventName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.EventCode).IsUnique();
            entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId)
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
            entity.Property(e => e.OriginPortCode).HasMaxLength(100);
            entity.Property(e => e.DestinationCountryName).HasMaxLength(100);
            entity.Property(e => e.DestinationCityName).HasMaxLength(100);
            entity.Property(e => e.DestinationPortCode).HasMaxLength(100);
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

        modelBuilder.Entity<ImportDocument>(entity =>
        {
            entity.ToTable("ImportDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentTypeName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UploadedByUserName).HasMaxLength(100);
            entity.HasIndex(e => e.ImportMasterId);
            entity.HasIndex(e => e.DocumentType);
            entity.HasOne(e => e.ImportMaster).WithMany().HasForeignKey(e => e.ImportMasterId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerAwbIssue>(entity =>
        {
            entity.ToTable("CustomerAwbIssues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IssueNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Origin).HasMaxLength(100);
            entity.Property(e => e.Destination).HasMaxLength(100);
            entity.Property(e => e.CashAccountName).HasMaxLength(200);
            entity.Property(e => e.BankAccountName).HasMaxLength(200);
            entity.Property(e => e.BankReferenceNo).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.RatePerAWB).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.IssueNo);
            entity.HasIndex(e => new { e.CustomerId, e.IssueDate });
            entity.HasIndex(e => new { e.BranchId, e.IssueDate });
            entity.HasIndex(e => e.IssueType);
        });

        modelBuilder.Entity<CustomerAwbIssueDetail>(entity =>
        {
            entity.ToTable("CustomerAwbIssueDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.AWBNo);
            entity.HasIndex(e => new { e.CustomerAwbIssueId, e.Status });
            entity.HasOne(e => e.CustomerAwbIssue)
                  .WithMany(c => c.Details)
                  .HasForeignKey(e => e.CustomerAwbIssueId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomerAwbBalance>(entity =>
        {
            entity.ToTable("CustomerAwbBalances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.TotalAdvanceAmount).HasPrecision(18, 2);
            entity.Property(e => e.UsedAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.CustomerId, e.BranchId }).IsUnique();
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

        modelBuilder.Entity<EmpostLicense>(entity =>
        {
            entity.ToTable("EmpostLicenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LicenseeName).HasMaxLength(200);
            entity.Property(e => e.MinimumAdvanceAmount).HasPrecision(18, 2);
            entity.Property(e => e.RoyaltyPercentage).HasPrecision(5, 2);
            entity.Property(e => e.WeightThresholdKg).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.HasIndex(e => new { e.CompanyId, e.IsActive });
        });

        modelBuilder.Entity<EmpostAdvancePayment>(entity =>
        {
            entity.ToTable("EmpostAdvancePayments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentReference).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AmountDue).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.BankReference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => new { e.EmpostLicenseId, e.ForLicenseYear });
            entity.HasOne(e => e.EmpostLicense).WithMany(l => l.AdvancePayments)
                  .HasForeignKey(e => e.EmpostLicenseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmpostQuarter>(entity =>
        {
            entity.ToTable("EmpostQuarters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuarterName).HasMaxLength(10);
            entity.Property(e => e.TotalGrossRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalTaxableRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalExemptRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalEmpostFee).HasPrecision(18, 2);
            entity.Property(e => e.TotalReturnAdjustments).HasPrecision(18, 2);
            entity.Property(e => e.NetEmpostFee).HasPrecision(18, 2);
            entity.Property(e => e.LockedByName).HasMaxLength(200);
            entity.Property(e => e.SubmittedByName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EmpostLicenseId, e.Year, e.Quarter }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.EmpostLicense).WithMany(l => l.Quarters)
                  .HasForeignKey(e => e.EmpostLicenseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmpostShipmentFee>(entity =>
        {
            entity.ToTable("EmpostShipmentFees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNumber).HasMaxLength(50);
            entity.Property(e => e.ActualWeight).HasPrecision(10, 3);
            entity.Property(e => e.ChargeableWeight).HasPrecision(10, 3);
            entity.Property(e => e.FreightCharge).HasPrecision(18, 2);
            entity.Property(e => e.FuelSurcharge).HasPrecision(18, 2);
            entity.Property(e => e.InsuranceCharge).HasPrecision(18, 2);
            entity.Property(e => e.CODCharge).HasPrecision(18, 2);
            entity.Property(e => e.OtherCharges).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.GrossAmount).HasPrecision(18, 2);
            entity.Property(e => e.RoyaltyPercentage).HasPrecision(5, 2);
            entity.Property(e => e.EmpostFeeAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdjustmentReason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EmpostQuarterId, e.InscanMasterId });
            entity.HasIndex(e => e.AWBNumber);
            entity.HasIndex(e => e.ShipmentDate);
            entity.HasOne(e => e.EmpostQuarter).WithMany(q => q.ShipmentFees)
                  .HasForeignKey(e => e.EmpostQuarterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmpostQuarterlySettlement>(entity =>
        {
            entity.ToTable("EmpostQuarterlySettlements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SettlementReference).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CumulativeFeeToDate).HasPrecision(18, 2);
            entity.Property(e => e.AdvancePaymentAmount).HasPrecision(18, 2);
            entity.Property(e => e.PreviousSettlements).HasPrecision(18, 2);
            entity.Property(e => e.QuarterFeeAmount).HasPrecision(18, 2);
            entity.Property(e => e.ReturnAdjustments).HasPrecision(18, 2);
            entity.Property(e => e.NetQuarterFee).HasPrecision(18, 2);
            entity.Property(e => e.ExcessOverAdvance).HasPrecision(18, 2);
            entity.Property(e => e.AmountPayable).HasPrecision(18, 2);
            entity.Property(e => e.VATOnFee).HasPrecision(18, 2);
            entity.Property(e => e.TotalPayable).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.BalanceDue).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EmpostLicenseId, e.Year, e.Quarter });
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.EmpostQuarter).WithMany(q => q.Settlements)
                  .HasForeignKey(e => e.EmpostQuarterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.EmpostLicense).WithMany()
                  .HasForeignKey(e => e.EmpostLicenseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmpostReturnAdjustment>(entity =>
        {
            entity.ToTable("EmpostReturnAdjustments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNumber).HasMaxLength(50);
            entity.Property(e => e.OriginalGrossAmount).HasPrecision(18, 2);
            entity.Property(e => e.OriginalFeeAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdjustmentAmount).HasPrecision(18, 2);
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.AppliedByName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EmpostQuarterId, e.Status });
            entity.HasIndex(e => e.AWBNumber);
            entity.HasOne(e => e.EmpostShipmentFee).WithMany()
                  .HasForeignKey(e => e.EmpostShipmentFeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.EmpostQuarter).WithMany(q => q.ReturnAdjustments)
                  .HasForeignKey(e => e.EmpostQuarterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmpostAuditLog>(entity =>
        {
            entity.ToTable("EmpostAuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionDescription).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.AWBNumber).HasMaxLength(50);
            entity.Property(e => e.OldValue).HasPrecision(18, 2);
            entity.Property(e => e.NewValue).HasPrecision(18, 2);
            entity.Property(e => e.OldData).HasMaxLength(500);
            entity.Property(e => e.NewData).HasMaxLength(500);
            entity.Property(e => e.PerformedByName).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EmpostLicenseId, e.PerformedAt });
            entity.HasIndex(e => new { e.EmpostQuarterId, e.Action });
        });

        // BankAccount entity configuration removed during Cash/Bank module migration

        modelBuilder.Entity<BankReconciliation>(entity =>
        {
            entity.ToTable("BankReconciliations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReconciliationNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.StatementOpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.StatementClosingBalance).HasPrecision(18, 2);
            entity.Property(e => e.BookOpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.BookClosingBalance).HasPrecision(18, 2);
            entity.Property(e => e.DifferenceAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.BranchId, e.StatementDate });
            entity.HasIndex(e => e.ReconciliationNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            // BankAccount relationship removed during Cash/Bank module migration
        });

        modelBuilder.Entity<BankStatementImport>(entity =>
        {
            entity.ToTable("BankStatementImports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(100);
            entity.Property(e => e.ColumnMapping).HasMaxLength(2000);
            entity.Property(e => e.ErrorLog).HasMaxLength(2000);
            entity.HasIndex(e => e.BankReconciliationId);
            entity.HasIndex(e => e.FileHash);
            entity.HasOne(e => e.BankReconciliation).WithMany(r => r.StatementImports)
                  .HasForeignKey(e => e.BankReconciliationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankStatementLine>(entity =>
        {
            entity.ToTable("BankStatementLines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ChequeNumber).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.MatchNotes).HasMaxLength(1000);
            entity.Property(e => e.LineHash).HasMaxLength(100);
            entity.Property(e => e.DebitAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Ignore(e => e.NetAmount);
            entity.HasIndex(e => e.BankStatementImportId);
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.IsMatched);
            entity.HasOne(e => e.BankStatementImport).WithMany(i => i.StatementLines)
                  .HasForeignKey(e => e.BankStatementImportId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReconciliationMatch>(entity =>
        {
            entity.ToTable("ReconciliationMatches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MatchGroup).HasMaxLength(50);
            entity.Property(e => e.MatchNotes).HasMaxLength(500);
            entity.Property(e => e.ReversalReason).HasMaxLength(500);
            entity.Property(e => e.MatchedAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.BankReconciliationId);
            entity.HasIndex(e => e.BankStatementLineId);
            entity.HasIndex(e => e.JournalId);
            entity.HasIndex(e => e.IsReversed);
            entity.HasOne(e => e.BankReconciliation).WithMany(r => r.Matches)
                  .HasForeignKey(e => e.BankReconciliationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.BankStatementLine).WithMany(l => l.Matches)
                  .HasForeignKey(e => e.BankStatementLineId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Journal).WithMany()
                  .HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReconciliationAdjustment>(entity =>
        {
            entity.ToTable("ReconciliationAdjustments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasIndex(e => e.BankReconciliationId);
            entity.HasIndex(e => e.IsPosted);
            entity.HasOne(e => e.BankReconciliation).WithMany(r => r.Adjustments)
                  .HasForeignKey(e => e.BankReconciliationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.BankStatementLine).WithMany()
                  .HasForeignKey(e => e.BankStatementLineId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Journal).WithMany()
                  .HasForeignKey(e => e.JournalId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.ToTable("TaxRates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Rate).HasPrecision(10, 4);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasOne(e => e.AccountHead).WithMany()
                  .HasForeignKey(e => e.AccountHeadId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CODRemittance>(entity =>
        {
            entity.ToTable("CODRemittances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RemittanceNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerCode).HasMaxLength(50);
            entity.Property(e => e.PaymentMode).HasMaxLength(50);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.ChequeNo).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.ApprovedByUserName).HasMaxLength(200);
            entity.Property(e => e.PaidByUserName).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.TotalCODAmount).HasPrecision(18, 2);
            entity.Property(e => e.ServiceCharge).HasPrecision(18, 2);
            entity.Property(e => e.ServiceChargePercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetPayable).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.RemittanceNo).IsUnique();
            entity.HasIndex(e => new { e.CustomerId, e.RemittanceDate });
            entity.HasIndex(e => new { e.BranchId, e.Status });
        });

        modelBuilder.Entity<CODRemittanceDetail>(entity =>
        {
            entity.ToTable("CODRemittanceDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConsigneeName).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.CODAmount).HasPrecision(18, 2);
            entity.Property(e => e.CollectedAmount).HasPrecision(18, 2);
            entity.Property(e => e.ServiceCharge).HasPrecision(18, 2);
            entity.Property(e => e.NetPayable).HasPrecision(18, 2);
            entity.HasIndex(e => e.CODRemittanceId);
            entity.HasIndex(e => e.InscanMasterId);
            entity.HasOne(e => e.CODRemittance).WithMany(r => r.Details)
                  .HasForeignKey(e => e.CODRemittanceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.InscanMaster).WithMany()
                  .HasForeignKey(e => e.InscanMasterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PickupCommitment>(entity =>
        {
            entity.ToTable("PickupCommitments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CourierName).HasMaxLength(200);
            entity.Property(e => e.ReleaseReason).HasMaxLength(500);
            entity.Property(e => e.ReleasedByUserName).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.HasIndex(e => new { e.PickupRequestId, e.Status });
            entity.HasIndex(e => new { e.CourierId, e.CommittedAt });
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasOne(e => e.PickupRequest).WithMany()
                  .HasForeignKey(e => e.PickupRequestId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IncentiveSchedule>(entity =>
        {
            entity.ToTable("IncentiveSchedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.ZoneName).HasMaxLength(100);
            entity.Property(e => e.IncentiveRate).HasPrecision(18, 4);
            entity.Property(e => e.MinWeight).HasPrecision(18, 3);
            entity.Property(e => e.MaxWeight).HasPrecision(18, 3);
            entity.Property(e => e.BonusAmount).HasPrecision(18, 2);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.HasIndex(e => new { e.CompanyId, e.IsActive });
            entity.HasIndex(e => e.EffectiveFrom);
        });

        modelBuilder.Entity<IncentiveAward>(entity =>
        {
            entity.ToTable("IncentiveAwards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CourierName).HasMaxLength(200);
            entity.Property(e => e.PickupNo).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.Weight).HasPrecision(18, 3);
            entity.Property(e => e.IncentiveAmount).HasPrecision(18, 2);
            entity.Property(e => e.BonusAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.CourierId, e.AwardDate });
            entity.HasIndex(e => new { e.IncentiveScheduleId, e.Status });
            entity.HasOne(e => e.IncentiveSchedule).WithMany()
                  .HasForeignKey(e => e.IncentiveScheduleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransferOrder>(entity =>
        {
            entity.ToTable("TransferOrders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransferNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceBranchName).HasMaxLength(200);
            entity.Property(e => e.DestinationBranchName).HasMaxLength(200);
            entity.Property(e => e.SourceWarehouseName).HasMaxLength(200);
            entity.Property(e => e.DestinationWarehouseName).HasMaxLength(200);
            entity.Property(e => e.VehicleNo).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(200);
            entity.Property(e => e.DriverPhone).HasMaxLength(50);
            entity.Property(e => e.SealNo).HasMaxLength(50);
            entity.Property(e => e.DispatchedByUserName).HasMaxLength(200);
            entity.Property(e => e.ReceivedByUserName).HasMaxLength(200);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.DispatchRemarks).HasMaxLength(1000);
            entity.Property(e => e.ReceiptRemarks).HasMaxLength(1000);
            entity.Property(e => e.TotalWeight).HasPrecision(18, 3);
            entity.HasIndex(e => e.TransferNo).IsUnique();
            entity.HasIndex(e => new { e.SourceBranchId, e.TransferDate });
            entity.HasIndex(e => new { e.DestinationBranchId, e.Status });
        });

        modelBuilder.Entity<TransferOrderItem>(entity =>
        {
            entity.ToTable("TransferOrderItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Dimensions).HasMaxLength(100);
            entity.Property(e => e.ScannedByUserName).HasMaxLength(200);
            entity.Property(e => e.ReceivedRemarks).HasMaxLength(500);
            entity.Property(e => e.DamageDescription).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Weight).HasPrecision(18, 3);
            entity.HasIndex(e => e.TransferOrderId);
            entity.HasIndex(e => e.InscanMasterId);
            entity.HasOne(e => e.TransferOrder).WithMany(t => t.Items)
                  .HasForeignKey(e => e.TransferOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.InscanMaster).WithMany()
                  .HasForeignKey(e => e.InscanMasterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransferOrderEvent>(entity =>
        {
            entity.ToTable("TransferOrderEvents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.UserName).HasMaxLength(200);
            entity.Property(e => e.OldValue).HasMaxLength(500);
            entity.Property(e => e.NewValue).HasMaxLength(500);
            entity.HasIndex(e => new { e.TransferOrderId, e.EventTime });
            entity.HasOne(e => e.TransferOrder).WithMany(t => t.Events)
                  .HasForeignKey(e => e.TransferOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // GL Module Native Entities (long IDs)
        modelBuilder.Entity<GLAccountClassification>(entity =>
        {
            entity.ToTable("GLAccountClassifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<GLChartOfAccount>(entity =>
        {
            entity.ToTable("GLChartOfAccounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AccountName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AccountType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DeactivationReason).HasMaxLength(500);
            entity.Property(e => e.CreatedByUser).HasMaxLength(100);
            entity.Property(e => e.UpdatedByUser).HasMaxLength(100);
            entity.Property(e => e.DeactivatedByUserId).HasMaxLength(100);
            entity.HasIndex(e => new { e.CompanyId, e.AccountCode }).IsUnique();
            entity.HasOne(e => e.Parent).WithMany(a => a.Children).HasForeignKey(e => e.ParentId);
            entity.HasOne(e => e.AccountClassification).WithMany(a => a.ChartOfAccounts)
                  .HasForeignKey(e => e.AccountClassificationId);
        });

        modelBuilder.Entity<GLTaxCode>(entity =>
        {
            entity.ToTable("GLTaxCodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Rate).HasPrecision(18, 4);
            entity.Property(e => e.TaxType).HasMaxLength(20);
            entity.Property(e => e.CreatedByUser).HasMaxLength(100);
            entity.Property(e => e.UpdatedByUser).HasMaxLength(100);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        });

        modelBuilder.Entity<GLVoucherNumbering>(entity =>
        {
            entity.ToTable("GLVoucherNumberings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Prefix).HasMaxLength(20);
            entity.Property(e => e.Suffix).HasMaxLength(20);
            entity.Property(e => e.Separator).HasMaxLength(10);
            entity.HasIndex(e => new { e.CompanyId, e.TransactionType, e.FinancialYearId }).IsUnique();
        });
        
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.BranchName).HasMaxLength(100);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.AdditionalInfo).HasMaxLength(500);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.EntityName, e.EntityId });
        });
    }
    
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public string EntityName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public long? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? IPAddress { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public bool HasTemporaryKey { get; set; }

        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }
    }
}
