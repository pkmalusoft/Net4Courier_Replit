using Microsoft.EntityFrameworkCore;
using Net4Courier.Shared.Entities;

namespace Net4Courier.Shared.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Menu> Menus => Set<Menu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CompanyPrefix).HasMaxLength(20);
            entity.Property(e => e.AWBFormat).HasMaxLength(50);
            entity.Property(e => e.InvoicePrefix).HasMaxLength(20);
            entity.Property(e => e.InvoiceFormat).HasMaxLength(50);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BranchPrefix).HasMaxLength(20);
            entity.Property(e => e.VATPercent).HasPrecision(18, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CurrentFinancialYear)
                .WithMany()
                .HasForeignKey(e => e.CurrentFinancialYearId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DefaultBranch)
                .WithMany(b => b.Users)
                .HasForeignKey(e => e.DefaultBranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserBranch>(entity =>
        {
            entity.ToTable("user_branches");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.BranchId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserBranches)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialYear>(entity =>
        {
            entity.ToTable("financial_years");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.FinancialYears)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.RoleId, e.ModuleName }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.ToTable("menus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ModuleName).HasMaxLength(100);

            entity.HasOne(e => e.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Administrator", Description = "Full system access", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = 2, Name = "Manager", Description = "Branch management access", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Id = 3, Name = "User", Description = "Standard user access", IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Menu>().HasData(
            new Menu { Id = 1, Name = "Dashboard", Icon = "Dashboard", Url = "/", SortOrder = 1, ModuleName = "Dashboard" },
            new Menu { Id = 2, Name = "Master Data", Icon = "Settings", SortOrder = 2, ModuleName = "MasterData" },
            new Menu { Id = 3, Name = "Companies", Icon = "Business", Url = "/companies", ParentId = 2, SortOrder = 1, ModuleName = "Companies" },
            new Menu { Id = 4, Name = "Branches", Icon = "AccountTree", Url = "/branches", ParentId = 2, SortOrder = 2, ModuleName = "Branches" },
            new Menu { Id = 5, Name = "Users", Icon = "People", Url = "/users", ParentId = 2, SortOrder = 3, ModuleName = "Users" },
            new Menu { Id = 6, Name = "Roles", Icon = "AdminPanelSettings", Url = "/roles", ParentId = 2, SortOrder = 4, ModuleName = "Roles" },
            new Menu { Id = 7, Name = "Financial Years", Icon = "CalendarMonth", Url = "/financial-years", ParentId = 2, SortOrder = 5, ModuleName = "FinancialYears" },
            new Menu { Id = 8, Name = "Operations", Icon = "LocalShipping", SortOrder = 3, ModuleName = "Operations" },
            new Menu { Id = 9, Name = "AWB Entry", Icon = "Receipt", Url = "/awb", ParentId = 8, SortOrder = 1, ModuleName = "AWB" },
            new Menu { Id = 10, Name = "Shipments", Icon = "Inventory", Url = "/shipments", ParentId = 8, SortOrder = 2, ModuleName = "Shipments" },
            new Menu { Id = 11, Name = "Accounts", Icon = "AccountBalance", SortOrder = 4, ModuleName = "Accounts" },
            new Menu { Id = 12, Name = "Invoices", Icon = "Description", Url = "/invoices", ParentId = 11, SortOrder = 1, ModuleName = "Invoices" },
            new Menu { Id = 13, Name = "Receipts", Icon = "ReceiptLong", Url = "/receipts", ParentId = 11, SortOrder = 2, ModuleName = "Receipts" },
            new Menu { Id = 14, Name = "Reports", Icon = "Assessment", Url = "/reports", SortOrder = 5, ModuleName = "Reports" }
        );
    }
}
