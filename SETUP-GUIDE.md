# .NET 8 Blazor Server Project Setup Guide for Replit

This guide helps you create a new Replit project for Net4Courier without environment and version issues.

## Step 1: Create New Replit Project via Git Import

1. Go to `https://replit.com/import`
2. Select **GitHub**
3. Paste your repository URL
4. Click **Import**

---

## Step 2: Use This Prompt for Agent Setup

After import, give the Agent this prompt:

```
Set up this .NET 8.0 Blazor Server project:

1. Install .NET 8.0 SDK (use dotnet-8.0 module)
2. The project uses local NuGet packages in NuGet/packages folder - do NOT delete this folder
3. Restore packages: dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj
4. Configure workflow command: cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
5. Build and run the application

Important:
- nuget.config references ./NuGet/packages for Truebooks custom packages
- Do not upgrade or modify package versions
- The app uses MudBlazor 7.x and PostgreSQL
```

---

## Step 3: Pre-Import Checklist

Before importing, ensure your Git repo has:

| File/Folder | Purpose |
|-------------|---------|
| `NuGet/packages/` | 6 Truebooks .nupkg files |
| `NuGet.config` | Points to local packages |
| `src/` | All project source code |
| `Net4Courier.sln` | Solution file |
| `.gitignore` | Excludes bin/obj/publish |

### Required Truebooks Packages in NuGet/packages/:
- Truebooks.AccountsFinance.GL.UI.1.0.0.nupkg
- Truebooks.Platform.Contracts.1.0.0.nupkg
- Truebooks.Platform.Core.1.0.0.nupkg
- Truebooks.Platform.Finance.1.0.0.nupkg
- Truebooks.Reports.GL.UI.1.0.0.nupkg
- Truebooks.Shared.UI.1.0.0.nupkg

---

## Step 4: After Import - Verify

Run these commands to verify setup:
```bash
dotnet --version          # Should show 8.x.x
dotnet restore            # Should complete without errors
dotnet build              # Should build successfully
```

---

## Common Issues & Fixes

| Issue | Fix |
|-------|-----|
| "Package not found" | Ensure NuGet/packages folder exists with .nupkg files |
| Wrong .NET version | Ask Agent to install dotnet-8.0 module |
| Port not accessible | Ensure workflow uses `--urls http://0.0.0.0:5000` |
| Security scan fails | Clean bin/obj/publish folders before import |
| Truebooks package missing | Copy .nupkg files from GL-Migration-Package to NuGet/packages |

---

## NuGet.config Content

Ensure your NuGet.config looks like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="Truebooks" value="./NuGet/packages" />
  </packageSources>
</configuration>
```

---

## Project Structure (Clean)

```
.
├── .gitignore
├── Net4Courier.sln
├── NuGet/
│   └── packages/          (6 Truebooks .nupkg files)
├── NuGet.config
├── replit.md
├── SETUP-GUIDE.md         (this file)
└── src/
    ├── Net4Courier.Finance/
    ├── Net4Courier.Infrastructure/
    ├── Net4Courier.Kernel/
    ├── Net4Courier.Masters/
    ├── Net4Courier.Operations/
    ├── Net4Courier.Shared/
    └── Net4Courier.Web/
```

---

## Security Scan Tips

To avoid security scan issues:

1. **Never commit** `bin/`, `obj/`, or `publish/` folders
2. Keep `.gitignore` updated with these entries:
   ```
   bin/
   obj/
   publish/
   *.tar.gz
   *.zip
   attached_assets/
   ```
3. If scan shows old package versions, delete bin/obj folders and rebuild
4. Verify packages with: `dotnet list package --vulnerable`

---

## Workflow Configuration

The app should run with this workflow command:
```
cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
```

Port 5000 is required for Replit's webview to work correctly.

---

## Deployment Workflow for Multiple Clients

### Recommended Git Strategy

Maintain a main development project and create client-specific deployments:

```
Net4Courier_Replit (Main Development)
    ├── Net4Courier_ClientA (Production)
    ├── Net4Courier_ClientB (Production)
    └── Net4Courier_ClientC (Production)
```

### Creating a New Client Deployment

1. **Create new Replit project** via Git import (see Step 1)
2. **Sync with main branch:**
   ```bash
   git fetch origin
   git reset --hard origin/main
   ```
3. **Set up environment** using the Agent prompt (see Step 2)
4. **Configure client-specific settings** (database, secrets)
5. **Deploy**

---

## Database & Schema Changes

When deploying to a new client or updating an existing deployment with schema changes:

### For New Client Deployments (Complete Step-by-Step)

#### Step 1: Create the Replit Project
1. Go to `https://replit.com/import`
2. Select **GitHub** and paste your repository URL
3. Click **Import**
4. Wait for Replit to set up the project

#### Step 2: Sync Code from Main Repository
```bash
git fetch origin
git reset --hard origin/main
```

#### Step 3: Set Up .NET Environment
Give the Agent this prompt:
```
Set up this .NET 8.0 Blazor Server project:
1. Install .NET 8.0 SDK
2. Restore packages: dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj
3. Configure workflow: cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
4. Build and run the application
```

#### Step 4: Create PostgreSQL Database
1. In Replit, click **Tools** in the left sidebar
2. Click **Database**
3. Click **Create Database** (select PostgreSQL)
4. This automatically creates the `DATABASE_URL` environment variable

#### Step 5: Initialize Database (Choose One Option)

**Option A: Use Initial Setup Wizard (Recommended)**
1. Start the application (run the workflow)
2. Navigate to `/setup` in your browser
3. The wizard will:
   - Create all database tables
   - Seed initial data (Company, Branch, Admin user)
   - Set up Chart of Accounts
   - Configure default settings

**Option B: Run EF Core Migrations Manually**
```bash
cd src/Net4Courier.Web
dotnet ef database update
```

#### Step 6: Configure Client-Specific Settings
After initial setup, log in as Platform Admin and configure:

| Setting | Location | Description |
|---------|----------|-------------|
| Company Details | Company Settings | Name, logo, address, contact |
| Branches | Branch Settings | Set up client branches |
| Financial Year | Finance > Financial Year | Create active financial year |
| Users | User Management | Create client users and assign roles |
| Gmail Integration | Secrets | Add GOOGLE_MAIL credentials for email |

#### Step 7: Deploy
1. Click **Publish** in the Replit interface
2. Run security scan (should pass with clean code)
3. Deploy to production

### For Existing Client Updates (Schema Changes)

When you push schema changes from main to a client project:

1. **Pull latest code:**
   ```bash
   git fetch origin
   git reset --hard origin/main
   ```

2. **Check for pending migrations:**
   ```bash
   cd src/Net4Courier.Web
   dotnet ef migrations list
   ```

3. **Apply migrations:**
   ```bash
   dotnet ef database update
   ```

4. **If using EF Core migrations in code:**
   - The app applies migrations automatically on startup if configured
   - Check `Program.cs` for `context.Database.Migrate()` call

### Migration Best Practices

| Practice | Description |
|----------|-------------|
| Always backup | Export database before major schema changes |
| Test first | Apply migrations to development before production |
| Version control | Keep migration files in Git |
| Rollback plan | Know how to revert: `dotnet ef database update <PreviousMigration>` |

### Manual Schema Updates (If Not Using EF Migrations)

If schema changes require manual SQL:

1. **Export SQL scripts** from main project's database folder
2. **Connect to client database** using Replit's database tools
3. **Execute SQL scripts** in order:
   ```sql
   -- Example: Adding new column
   ALTER TABLE "Parties" ADD COLUMN "NewField" VARCHAR(100);
   ```

### Environment Variables Per Client

Each client deployment needs its own:

| Variable | Description |
|----------|-------------|
| `DATABASE_URL` | PostgreSQL connection (auto-created by Replit) |
| `GOOGLE_MAIL_*` | Gmail API credentials (if using email) |
| Company-specific secrets | API keys, integrations |

### Keeping Client Data Safe

- **Never** run `git reset --hard` on a client project with uncommitted local changes
- Client databases are **separate** from code - Git operations don't affect them
- **Backup client databases** before major updates:
  ```bash
  pg_dump $DATABASE_URL > backup_$(date +%Y%m%d).sql
  ```
