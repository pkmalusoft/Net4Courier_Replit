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
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyAddress> PartyAddresses => Set<PartyAddress>();
    
    public DbSet<InscanMaster> InscanMasters => Set<InscanMaster>();
    public DbSet<InscanMasterItem> InscanMasterItems => Set<InscanMasterItem>();
    public DbSet<AWBTracking> AWBTrackings => Set<AWBTracking>();
    public DbSet<QuickInscanMaster> QuickInscanMasters => Set<QuickInscanMaster>();
    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<DRS> DRSs => Set<DRS>();
    public DbSet<DRSDetail> DRSDetails => Set<DRSDetail>();
    
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptAllocation> ReceiptAllocations => Set<ReceiptAllocation>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<AccountHead> AccountHeads => Set<AccountHead>();

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
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Branches)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
            entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
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

        modelBuilder.Entity<FinancialYear>(entity =>
        {
            entity.ToTable("FinancialYears");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
            entity.HasOne(e => e.Company).WithMany(c => c.FinancialYears).HasForeignKey(e => e.CompanyId);
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
    }

    private void ConfigureOperationsModule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InscanMaster>(entity =>
        {
            entity.ToTable("InscanMasters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AWBNo).HasMaxLength(50).IsRequired();
            
            entity.HasIndex(e => e.AWBNo);
            entity.HasIndex(e => new { e.BranchId, e.TransactionDate });
            entity.HasIndex(e => new { e.CustomerId, e.TransactionDate });
            entity.HasIndex(e => new { e.AWBNo, e.CourierStatusId });
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.FinancialYearId);
            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.DRSId);
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
            entity.HasIndex(e => e.DRSNo);
        });

        modelBuilder.Entity<DRSDetail>(entity =>
        {
            entity.ToTable("DRSDetails");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DRS).WithMany().HasForeignKey(e => e.DRSId);
            entity.HasOne(e => e.Inscan).WithMany().HasForeignKey(e => e.InscanId);
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
    }
}
