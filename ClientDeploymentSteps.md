# Net4Courier - Client Deployment Steps

Complete guide for deploying Net4Courier to a new client via Replit.

---

## Step 1: Create New Replit Project via Git Import

1. Go to `https://replit.com/import`
2. Select **GitHub**
3. Paste your repository URL
4. Click **Import**
5. Name the project (e.g., `Net4Courier-ClientName`)

---

## Step 2: Create PostgreSQL Database

1. In the Replit sidebar, click **Tools**
2. Select **Database** (PostgreSQL)
3. Click **Create Database**
4. Wait for provisioning to complete

The following environment variables are automatically set:
- `DATABASE_URL`
- `PGHOST`, `PGPORT`, `PGUSER`, `PGPASSWORD`, `PGDATABASE`

---

## Step 3: Configure Secrets

1. In the Replit sidebar, click **Secrets**
2. Add the following secret:

| Key | Value | Purpose |
|-----|-------|---------|
| `SETUP_KEY` | Your chosen secure password | Platform Admin login password |

**Important**: If you don't set `SETUP_KEY`, a random 12-character password is generated and shown in the console logs on first run.

---

## Step 4: Set Up Environment

Use the Agent prompt to set up the .NET environment:

```
Set up this .NET 8.0 Blazor Server project:

1. Install .NET 8.0 SDK (use dotnet-8.0 module)
2. The project uses local NuGet packages in NuGet/packages folder - do NOT delete this folder
3. Restore packages: dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj
4. Configure workflow command: cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000
5. Build and run the application
```

---

## Step 5: Sync with Main Branch (if needed)

If importing an older version, sync to the latest:

```bash
git fetch origin
git reset --hard origin/main
```

---

## Updating Client Projects from Main Repository

When you've made changes in the main project and need to sync them to client projects:

### Complete Sync Process (Required Steps)

**Important**: Just running `git reset` is NOT enough! You must also clean build artifacts.

```bash
# Step 1: Sync the code
git fetch origin
git reset --hard origin/main

# Step 2: Clean ALL build artifacts (CRITICAL!)
rm -rf src/Net4Courier.Web/bin src/Net4Courier.Web/obj
rm -rf src/Net4Courier.Infrastructure/bin src/Net4Courier.Infrastructure/obj
rm -rf src/Net4Courier.Finance/bin src/Net4Courier.Finance/obj
rm -rf src/Net4Courier.Kernel/bin src/Net4Courier.Kernel/obj
rm -rf src/Net4Courier.Masters/bin src/Net4Courier.Masters/obj
rm -rf src/Net4Courier.Operations/bin src/Net4Courier.Operations/obj
rm -rf src/Net4Courier.Shared/bin src/Net4Courier.Shared/obj

# Step 3: Restore and rebuild
dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj
dotnet build src/Net4Courier.Web/Net4Courier.Web.csproj --configuration Release

# Step 4: Restart the workflow
```

### Quick One-Liner Command

Copy and paste this single command to do everything at once:

```bash
git fetch origin && git reset --hard origin/main && find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null; dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj && dotnet build src/Net4Courier.Web/Net4Courier.Web.csproj
```

### After Syncing

1. **Restart the workflow** in Replit
2. **Redeploy** if this is a production deployment
3. **Clear browser cache** (Ctrl+Shift+R or Cmd+Shift+R) to see UI changes

### Why This Is Needed

| What | Why |
|------|-----|
| `git reset --hard` | Updates source files (.cs, .razor, etc.) |
| `rm -rf bin obj` | Removes old compiled assemblies that git ignores |
| `dotnet restore` | Gets any new NuGet packages |
| `dotnet build` | Compiles fresh with the new code |

**Without cleaning bin/obj folders**, the app runs with old cached compiled code even though source files are updated.

---

## Step 6: Verify Required Packages

Ensure NuGet/packages/ folder contains:
- Truebooks.AccountsFinance.GL.UI.1.0.0.nupkg
- Truebooks.Platform.Contracts.1.0.0.nupkg
- Truebooks.Platform.Core.1.0.0.nupkg
- Truebooks.Platform.Finance.1.0.0.nupkg
- Truebooks.Reports.GL.UI.1.0.0.nupkg
- Truebooks.Shared.UI.1.0.0.nupkg

---

## Step 7: Run the Application

1. Click **Run** or start the workflow
2. The application will automatically:
   - Create all database tables (including GL module tables)
   - Seed currencies, countries, roles, and service types
   - Create the Platform Admin user (if not exists)
3. Watch the console for: `Database initialization completed successfully`

**Important for Existing Deployments**: If updating from an older version:
- The Platform Admin user may already exist with a different password hash
- Use the `/setup` page or CLI utility to reset the password (see Troubleshooting below)
- New database columns won't be added automatically - see "Schema Updates for Existing Deployments" section

---

## Step 8: Initial Client Setup

1. Open the application URL in your browser
2. Log in as Platform Admin:
   - **Username**: `platformadmin`
   - **Password**: Your `SETUP_KEY` value (or check console for generated password)
3. Navigate to **Platform Admin > Manage Demo Data**
4. Click **Create Demo Data**
5. Fill in the Initial Setup dialog:
   - **Company Name**: Client's company name
   - **Country**: Client's country
   - **Currency**: Client's default currency
   - **Admin Name**: Client admin's full name
   - **Admin Email**: Client admin's email
   - **Admin Username**: Login username for client admin
6. Click **Create**
7. **Save the temporary password** displayed - give this to the client admin

---

## Step 9: Publish to Production

1. Click the **Publish** button in Replit
2. Select deployment type:
   - **Reserved VM** (Recommended): Always-on, consistent performance
   - **Autoscale**: Scales based on traffic, cost-effective for low traffic
3. Configure build and run commands:
   - **Build**: `cd src/Net4Courier.Web && dotnet build --configuration Release`
   - **Run**: `cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000`
4. In the **Deployments** tab, verify database is connected
5. Click **Publish**

---

## Step 10: Custom Domain (Optional)

1. In Publish settings, click **Add Custom Domain**
2. Enter the client's domain (e.g., `courier.clientcompany.com`)
3. Add the provided DNS records to the client's domain registrar:
   - CNAME or A record as specified
4. Wait for verification (5-30 minutes)

---

## Schema Updates for Existing Deployments

When syncing code from the main repository to an existing client deployment, new database columns are **NOT automatically added**. EF Core's `EnsureCreated()` only creates tables that don't exist.

### Required SQL for Recent Updates (Jan 2026)

Run these SQL commands on existing deployments (Rainbow, Gateex, etc.) after syncing from main:

```sql
-- Step 1: Add CurrencyId columns
ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;
ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;
ALTER TABLE "RateCardZones" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;

-- Step 2: Set default currency (AED) for existing records
UPDATE "Companies" SET "CurrencyId" = (SELECT "Id" FROM "Currencies" WHERE "Code" = 'AED' LIMIT 1) WHERE "CurrencyId" IS NULL;
UPDATE "Branches" SET "CurrencyId" = (SELECT "Id" FROM "Currencies" WHERE "Code" = 'AED' LIMIT 1) WHERE "CurrencyId" IS NULL;

-- Step 3: Add foreign key constraints (run only once - will error if already exists)
-- Check first: SELECT * FROM pg_constraint WHERE conname LIKE '%Currency%';
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Companies_Currencies_CurrencyId') THEN
    ALTER TABLE "Companies" ADD CONSTRAINT "FK_Companies_Currencies_CurrencyId" 
      FOREIGN KEY ("CurrencyId") REFERENCES "Currencies"("Id");
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Branches_Currencies_CurrencyId') THEN
    ALTER TABLE "Branches" ADD CONSTRAINT "FK_Branches_Currencies_CurrencyId" 
      FOREIGN KEY ("CurrencyId") REFERENCES "Currencies"("Id");
  END IF;
END $$;
```

**Note**: The application's `DatabaseInitializationService` automatically seeds geographic data (countries, states, cities) on startup using `INSERT ... WHERE NOT EXISTS`, so geographic tables are updated automatically. The above commands are only needed for structural column additions.

### How to Run These Commands

**Option 1: Via Replit Database Tool**
1. Go to Tools > Database in Replit
2. Click on your PostgreSQL database
3. Use the SQL console to run the commands

**Option 2: Via psql command line**
```bash
# Add columns
psql $DATABASE_URL -c 'ALTER TABLE "Companies" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;'
psql $DATABASE_URL -c 'ALTER TABLE "Branches" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;'
psql $DATABASE_URL -c 'ALTER TABLE "RateCardZones" ADD COLUMN IF NOT EXISTS "CurrencyId" BIGINT;'

# Set defaults (AED currency)
psql $DATABASE_URL -c 'UPDATE "Companies" SET "CurrencyId" = (SELECT "Id" FROM "Currencies" WHERE "Code" = '\''AED'\'' LIMIT 1) WHERE "CurrencyId" IS NULL;'
psql $DATABASE_URL -c 'UPDATE "Branches" SET "CurrencyId" = (SELECT "Id" FROM "Currencies" WHERE "Code" = '\''AED'\'' LIMIT 1) WHERE "CurrencyId" IS NULL;'
```

### Verification

After running the ALTER TABLE commands:
1. Restart the application workflow
2. Check the console logs for any errors
3. Log in and verify Companies/Branches pages load correctly

---

## Security & Maintenance

### Avoid Security Scan Issues

1. **Never commit** `bin/`, `obj/`, or `publish/` folders
2. Keep `.gitignore` updated:
   ```
   bin/
   obj/
   publish/
   *.tar.gz
   *.zip
   attached_assets/
   ```
3. If scan shows old package versions, delete bin/obj folders and rebuild
4. Verify packages: `dotnet list package --vulnerable`

### Post-Deployment Checklist

- [ ] Client admin can log in with temporary password
- [ ] Client changed temporary password
- [ ] Company logo uploaded
- [ ] Financial year configured
- [ ] Branch details completed
- [ ] Rate cards configured (if applicable)

---

## Troubleshooting

### Platform Admin Login Fails

**Common cause after sync**: The Platform Admin already exists with an old password hash that doesn't match your `SETUP_KEY`.

**Option 1: Use Setup Page (Recommended)**
1. Set `SETUP_KEY` environment variable in Replit Secrets
2. Navigate to `/setup` in your browser
3. Enter your setup key to authenticate
4. Use the **Reset Password** tab to reset the `platformadmin` password

**Option 2: Use Command Line Utility**
```bash
cd src/Net4Courier.Web
export SETUP_KEY="your_setup_key"
dotnet run -- --reset-password platformadmin "YourNewPassword"
```
Note: Stop the workflow before running this command.

**Option 3: Direct SQL Reset**
```sql
UPDATE "Users" 
SET "PasswordHash" = '$2a$11$hmDFlxl2RNiTrNgrKrtYruTvNpolPXKc/VxjIgpV.fkbuoclnS2VK'
WHERE "Username" = 'platformadmin';
```
This sets the password to `T6u3b00ks`

**Option 4: Sync Hash from Working Deployment**
Copy the password hash from an environment where login works:
```sql
-- Run on working deployment (e.g., main project)
SELECT "PasswordHash" FROM "Users" WHERE "Username" = 'platformadmin';

-- Run on target deployment (e.g., Gateex)
UPDATE "Users" SET "PasswordHash" = '<copied_hash>' WHERE "Username" = 'platformadmin';
```

### Database Connection Error
- Verify PostgreSQL database was created in Tools
- Check DATABASE_URL is set in environment
- Restart the application

### Blank Page After Login
- Clear browser cache
- Check browser console for WebSocket errors
- Verify the workflow is running

### GL Module Errors
- Tables are created automatically on first run
- Check logs for "GLChartOfAccounts table ensured" message
- If missing, restart the application

---

## Quick Reference

| Item | Value |
|------|-------|
| Platform Admin Username | `platformadmin` |
| Platform Admin Password | Your `SETUP_KEY` secret |
| Default Port | 5000 |
| Setup & Maintenance Page | `/setup` (requires `SETUP_KEY` env var) |
| Initial Setup Location | Platform Admin > Manage Demo Data |
| Tracking URL | `/tracking/{awbNumber}` |
| Password Reset CLI | `dotnet run -- --reset-password <user> <pass>` |

---

## Console Messages to Watch For

Successful startup shows:
```
GLAccountClassifications table ensured
GLChartOfAccounts table ensured
GLTaxCodes table ensured
GLVoucherNumberings table ensured
Database initialization completed successfully
Platform admin user created (username: platformadmin, password: uses SETUP_KEY)
```
