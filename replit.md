# Net4Courier - Blazor Server Migration

## Overview
Net4Courier is a comprehensive courier/logistics management system being migrated from ASP.NET MVC 5 (.NET Framework 4.7.2) to .NET 8 Blazor Server. The application manages courier operations including shipments (AWB), customers, branches, financial transactions, and reporting.

## Current State
- **Framework**: .NET 8 Blazor Server with MudBlazor UI components
- **Database**: PostgreSQL (Replit hosted)
- **Status**: Core infrastructure complete, master data modules in progress

## Project Structure
```
src/
├── Net4Courier.Web/           # Blazor Server frontend
│   ├── Components/
│   │   ├── Layout/            # MainLayout, NavMenu
│   │   └── Pages/             # Dashboard, Companies, Branches, etc.
│   ├── Services/              # AuthService, MenuService
│   └── Program.cs             # Application entry point
├── Net4Courier.Shared/        # Shared class library
│   ├── Data/                  # ApplicationDbContext
│   ├── Entities/              # EF Core entities
│   └── Migrations/            # EF Core migrations
```

## Key Entities
- **Company**: Multi-tenant company management
- **Branch**: Company branches with fiscal year support
- **User**: System users with role-based access
- **Role**: User roles and permissions
- **FinancialYear**: Fiscal year definitions for data partitioning
- **Menu**: Navigation menu structure

## Database Configuration
- Connection string is parsed from `DATABASE_URL` environment variable
- PostgreSQL connection without SSL for Replit internal network
- EF Core migrations run automatically on startup

## Authentication
- Admin user seeded on first run: `admin` / `Admin@123`
- Password hashing using BCrypt

## Running the Application
```bash
cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
```

## Migration Progress
1. [x] Solution structure setup
2. [x] EF Core DbContext and entities
3. [x] MudBlazor dashboard layout
4. [x] Company management CRUD
5. [x] Branch management CRUD
6. [ ] User management
7. [ ] Role management
8. [ ] Financial Year management
9. [ ] Authentication/Login UI
10. [ ] Database partitioning by fiscal year
11. [ ] Operations modules (AWB, Shipments)
12. [ ] Finance modules (Invoices, Receipts)
13. [ ] Reporting (QuestPDF replacement for Crystal Reports)

## User Preferences
- Use MudBlazor for all UI components
- PostgreSQL for database (Replit compatible)
- Database partitioning by fiscal year for performance

## Recent Changes (Jan 15, 2026)
- Fixed DATABASE_URL connection string parsing
- Fixed MudPopoverProvider duplicate warning
- Enabled detailed error logging for development
