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
   - Create the Platform Admin user
3. Watch the console for: `Platform admin user created`

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
- Check console logs for the generated password (if SETUP_KEY wasn't set)
- Verify the password is your SETUP_KEY value
- To reset password via SQL:
  ```sql
  UPDATE "Users" 
  SET "PasswordHash" = '$2a$11$hmDFlxl2RNiTrNgrKrtYruTvNpolPXKc/VxjIgpV.fkbuoclnS2VK'
  WHERE "Username" = 'platformadmin';
  ```
  This sets the password to `T6u3b00ks`
- Or delete and recreate: Delete `platformadmin` user from database and restart app (will use SETUP_KEY)
- See **SETUP-GUIDE.md** for detailed password reset methods

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
| Initial Setup Location | Platform Admin > Manage Demo Data |
| Tracking URL | `/tracking/{awbNumber}` |

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
