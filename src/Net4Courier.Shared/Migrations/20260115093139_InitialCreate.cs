using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Net4Courier.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address1 = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    Address3 = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    KeyPerson = table.Column<string>(type: "text", nullable: true),
                    MobileNo1 = table.Column<string>(type: "text", nullable: true),
                    MobileNo2 = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    CompanyPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AWBFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InvoicePrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InvoiceFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompanyLogo = table.Column<byte[]>(type: "bytea", nullable: true),
                    LogoFileName = table.Column<string>(type: "text", nullable: true),
                    EnableAPI = table.Column<bool>(type: "boolean", nullable: false),
                    EnableCashCustomerInvoice = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptSystem = table.Column<bool>(type: "boolean", nullable: false),
                    AWBAlphaNumeric = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menus_menus_ParentId",
                        column: x => x.ParentId,
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address1 = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    Address3 = table.Column<string>(type: "text", nullable: true),
                    KeyPerson = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    MobileNo1 = table.Column<string>(type: "text", nullable: true),
                    MobileNo2 = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    BranchPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AWBFormat = table.Column<string>(type: "text", nullable: true),
                    InvoicePrefix = table.Column<string>(type: "text", nullable: true),
                    InvoiceFormat = table.Column<string>(type: "text", nullable: true),
                    VATRegistrationNo = table.Column<string>(type: "text", nullable: true),
                    VATPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CODReceiptPrefix = table.Column<string>(type: "text", nullable: true),
                    CODReceiptFormat = table.Column<string>(type: "text", nullable: true),
                    TaxEnable = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentFinancialYearId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_branches_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financial_years",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_years", x => x.Id);
                    table.ForeignKey(
                        name: "FK_financial_years_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_financial_years_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    DefaultBranchId = table.Column<int>(type: "integer", nullable: true),
                    IsLoggedIn = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_branches_DefaultBranchId",
                        column: x => x.DefaultBranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_branches_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_branches_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "menus",
                columns: new[] { "Id", "CreatedAt", "Icon", "IsActive", "ModuleName", "Name", "ParentId", "SortOrder", "Url" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3793), "Dashboard", true, "Dashboard", "Dashboard", null, 1, "/" },
                    { 2, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3804), "Settings", true, "MasterData", "Master Data", null, 2, null },
                    { 8, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3820), "LocalShipping", true, "Operations", "Operations", null, 3, null },
                    { 11, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3827), "AccountBalance", true, "Accounts", "Accounts", null, 4, null },
                    { 14, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3833), "Assessment", true, "Reports", "Reports", null, 5, "/reports" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3535), "Full system access", true, "Administrator", null },
                    { 2, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3541), "Branch management access", true, "Manager", null },
                    { 3, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3544), "Standard user access", true, "User", null }
                });

            migrationBuilder.InsertData(
                table: "menus",
                columns: new[] { "Id", "CreatedAt", "Icon", "IsActive", "ModuleName", "Name", "ParentId", "SortOrder", "Url" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3807), "Business", true, "Companies", "Companies", 2, 1, "/companies" },
                    { 4, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3810), "AccountTree", true, "Branches", "Branches", 2, 2, "/branches" },
                    { 5, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3813), "People", true, "Users", "Users", 2, 3, "/users" },
                    { 6, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3815), "AdminPanelSettings", true, "Roles", "Roles", 2, 4, "/roles" },
                    { 7, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3818), "CalendarMonth", true, "FinancialYears", "Financial Years", 2, 5, "/financial-years" },
                    { 9, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3822), "Receipt", true, "AWB", "AWB Entry", 8, 1, "/awb" },
                    { 10, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3824), "Inventory", true, "Shipments", "Shipments", 8, 2, "/shipments" },
                    { 12, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3828), "Description", true, "Invoices", "Invoices", 11, 1, "/invoices" },
                    { 13, new DateTime(2026, 1, 15, 9, 31, 38, 819, DateTimeKind.Utc).AddTicks(3831), "ReceiptLong", true, "Receipts", "Receipts", 11, 2, "/receipts" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_branches_CompanyId",
                table: "branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_branches_CurrentFinancialYearId",
                table: "branches",
                column: "CurrentFinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_financial_years_BranchId",
                table: "financial_years",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_financial_years_CompanyId",
                table: "financial_years",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_menus_ParentId",
                table: "menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_RoleId_ModuleName",
                table: "role_permissions",
                columns: new[] { "RoleId", "ModuleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_branches_BranchId",
                table: "user_branches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_user_branches_UserId_BranchId",
                table: "user_branches",
                columns: new[] { "UserId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_DefaultBranchId",
                table: "users",
                column: "DefaultBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_UserName",
                table: "users",
                column: "UserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_branches_financial_years_CurrentFinancialYearId",
                table: "branches",
                column: "CurrentFinancialYearId",
                principalTable: "financial_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_branches_companies_CompanyId",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "FK_financial_years_companies_CompanyId",
                table: "financial_years");

            migrationBuilder.DropForeignKey(
                name: "FK_branches_financial_years_CurrentFinancialYearId",
                table: "branches");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_branches");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "financial_years");

            migrationBuilder.DropTable(
                name: "branches");
        }
    }
}
