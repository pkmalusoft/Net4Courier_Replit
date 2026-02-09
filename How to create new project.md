# How to Create a New Net4Courier Project on Replit

## Step 1: Import from Git

1. Go to Replit and click **"Create Repl"**
2. Select **"Import from GitHub"**
3. Paste the Net4Courier repository URL
4. Click **"Import from GitHub"** to start the import
5. Wait for the import to complete

## Step 2: Add Database

1. In the left sidebar, click the **"Database"** (or Tools) tab
2. Select **PostgreSQL** to provision a new database
3. The `DATABASE_URL` environment variable will be automatically set

## Step 3: Set Up the Project with Agent

Once the import is complete and database is ready, give this prompt to the Replit Agent:

```
Set up this .NET 8.0 Blazor Server project with the following requirements:

1. Install .NET 8.0 SDK using the module installer
2. Restore all NuGet packages from both nuget.org and the local NuGet/packages folder (contains Truebooks custom packages). The nuget.config file references ./NuGet/packages for local Truebooks packages. Make sure to restore from both sources.
3. The project uses MudBlazor 7.x (already in .csproj files)
4. Configure the workflow named "Net4Courier Web" to run:
   cd src/Net4Courier.Web && dotnet build --no-incremental && dotnet bin/Debug/net8.0/Net4Courier.Web.dll --urls http://0.0.0.0:5000
5. Build and verify the application starts successfully
6. Check the workflow logs to confirm "Database initialization completed successfully" appears
7. Confirm the login page loads in the webview

Important notes:
- The nuget.config file references ./NuGet/packages for local Truebooks packages
- If the build runs out of memory, use DevMinimalMode=true flag:
  cd src/Net4Courier.Web && DOTNET_gcServer=0 DOTNET_GCConserveMemory=9 dotnet build --no-incremental -p:DevMinimalMode=true && DOTNET_gcServer=0 DOTNET_GCConserveMemory=9 dotnet bin/Debug/net8.0/Net4Courier.Web.dll --urls http://0.0.0.0:5000
- DevMinimalMode compiles only ~35 essential pages to stay within memory limits
```

## Step 4: Log In as Platform Admin

The system **automatically** creates a `platformadmin` account on first startup.

- **Username:** `platformadmin`
- **Password:** `Admin@123` (default)

The credentials are displayed in the workflow/console logs at startup:

```
=====================================================
  PLATFORM ADMIN LOGIN CREDENTIALS
  Username: platformadmin
  Password: Admin@123 (default - change via Secrets tab)
=====================================================
```

### Optional: Use a Custom Password

If you want a different password for platformadmin:

1. Before the first run, go to the **Secrets** tab (lock icon in the sidebar)
2. Add a new secret: Key = `PLATFORMADMIN_PASSWORD`, Value = your desired password
3. Start or restart the workflow
4. The platformadmin account will use your custom password instead of the default

**Password priority order:**
1. `PLATFORMADMIN_PASSWORD` secret (highest priority)
2. `SETUP_KEY` secret (legacy fallback)
3. `Admin@123` (default if no secrets are set)

## Step 5: Initial Company Setup

1. Log in as `platformadmin`
2. The **Initial Setup Wizard** will appear automatically (if no company exists)
3. Fill in your company details:
   - Company Name
   - Company Code
   - Address, City, Country
   - Tax Number, Registration Number
   - Contact details
4. Click **Save** to create the company

## Step 6: Seed Demo Data (Optional)

1. After company setup, go to **Platform Admin** in the navigation menu
2. Click **"Create Demo Data"**
3. This will populate sample data including:
   - Sample customers and parties
   - Sample AWBs and shipments
   - Sample invoices and financial entries
   - This helps with testing and learning the system

## Step 7: Create Admin Users

1. Go to **Settings > User Management**
2. Click **"Add User"**
3. Fill in user details:
   - Username
   - Full Name
   - Email
   - Phone
   - Role (select "Administrator" for full access)
   - Branch assignment
   - Set a password
4. Click **Save**

## Step 8: Start Using the System

1. Log out from platformadmin
2. Log in as the new admin user you created
3. Begin configuring and using the system:
   - Set up branches
   - Configure rate cards
   - Manage customers
   - Process AWBs and shipments
   - Handle invoicing and payments

## Troubleshooting

### Cannot log in as platformadmin
- Check the workflow logs for the credentials banner
- If no banner appears, the database initialization may have failed - check logs for errors
- Try restarting the workflow
- If all else fails, add `PLATFORMADMIN_PASSWORD` secret and restart

### Build fails with out of memory
- Use DevMinimalMode=true in the workflow command (see Step 3)
- This compiles only ~35 essential pages instead of the full application

### Login page not loading
- Make sure the workflow is running (check workflow status)
- Verify the server started on port 5000 (check logs for "Now listening on: http://0.0.0.0:5000")
- Wait for "Database initialization completed successfully" in the logs before trying to access

### Database initialization errors
- Check logs for specific error messages
- Common issue: missing columns in seed data SQL - the schema auto-sync should handle this
- If errors persist, you may need to drop and recreate the database
