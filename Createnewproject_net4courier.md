# Net4Courier - Create New Project Guide

## Step 1: Create New Replit Project via Git Import

1. Go to `https://replit.com/import`
2. Select **GitHub**
3. Paste your repository URL
4. Click **Import**
5. Name the project (e.g., "Net4Courier-Highway")

## Step 2: Set Up Environment

Once the project is imported, use the Agent prompt with this message:

> Set up this .NET 8.0 Blazor Server project:
> 1. Install .NET 8.0 SDK (use dotnet-8.0 module)
> 2. The project uses local NuGet packages in NuGet/packages folder - do NOT delete this folder
> 3. Restore packages: `dotnet restore src/Net4Courier.Web/Net4Courier.Web.csproj`
> 4. Configure workflow: `cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000`
> 5. Build and run the application

**Important notes:**
- When Replit asks you to choose a language/framework, pick **blank/Nix** if prompted — the Agent will install .NET 8.0 for you
- The first build will take about 2 minutes because there are 335+ Razor components to compile — this is normal, don't cancel it
- If the workflow times out on first run, just restart it — the second run will be fast because the build is cached

## Step 3: Sync with Main Branch (when pulling updates)

```bash
git fetch origin
git reset --hard origin/main
```

## Step 4: Verify Truebooks Packages

These must exist in `NuGet/packages/` folder (they come with the repo):
- Truebooks.AccountsFinance.GL.UI.1.0.0.nupkg
- Truebooks.Platform.Contracts.1.0.0.nupkg
- Truebooks.Platform.Core.1.0.0.nupkg
- Truebooks.Platform.Finance.1.0.0.nupkg
- Truebooks.Reports.GL.UI.1.0.0.nupkg
- Truebooks.Shared.UI.1.0.0.nupkg

## Step 5: Security - .gitignore

Make sure `.gitignore` includes:

```
bin/
obj/
publish/
*.tar.gz
*.zip
attached_assets/
```

If security scans flag old package versions, delete `bin/` and `obj/` folders and rebuild.

## Step 6: Configure Client-Specific Settings

1. **Database**: Create a PostgreSQL database in Replit (this automatically sets the `DATABASE_URL` secret)
2. **Other secrets**: Add any client-specific secrets (like Gmail API credentials) through the Secrets tab
3. **Production mode**: Set `PRODUCTION_MODE=true` as an environment variable for client deployments

## Step 7: Deploy

1. Build the project first: `cd src/Net4Courier.Web && dotnet build`
2. Configure the deployment run command: `cd src/Net4Courier.Web && dotnet bin/Debug/net8.0/Net4Courier.Web.dll --urls http://0.0.0.0:5000`
3. Click **Publish** in Replit

**Note:** Running the compiled DLL directly is much faster than `dotnet run` because it skips the build step. For development, `dotnet run` is fine since it rebuilds automatically when you make changes. For deployment, running the pre-built DLL avoids timeout issues.
