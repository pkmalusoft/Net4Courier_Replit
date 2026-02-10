# Net4Courier — Azure Deployment Guide

Step-by-step instructions for deploying Net4Courier on Azure App Service with PostgreSQL database.

---

## Part 1: Set Up Azure PostgreSQL Database

### Step 1: Log into Azure Portal
Go to [portal.azure.com](https://portal.azure.com)

### Step 2: Create a PostgreSQL Flexible Server
- Search for **"Azure Database for PostgreSQL Flexible Server"**
- Click **Create**
- Fill in:
  - **Resource Group**: Create new (e.g., `net4courier-rg`)
  - **Server name**: e.g., `net4courier-db`
  - **Region**: Choose one close to your users (e.g., UAE North if serving UAE)
  - **PostgreSQL version**: 16
  - **Workload type**: Production (Small/Medium) or Development for testing
  - **Compute + Storage**: Start with **Burstable B1ms** (cheapest, ~$13/month) — you can scale up later
  - **Admin username**: e.g., `net4admin`
  - **Password**: Set a strong password
- Click **Next: Networking**

### Step 3: Configure Networking
- Select **Public access**
- Check **"Allow public access from any Azure service"**
- Add your current IP if you want to connect from your local machine
- Click **Review + Create**, then **Create**

### Step 4: Get the Connection String
- Once created, go to the PostgreSQL server resource
- Click **Connect** in the left menu
- Copy the connection string. It looks like:
  ```
  Host=net4courier-db.postgres.database.azure.com;Database=net4courier;Username=net4admin;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
  ```

### Step 5: Create the Database
- Go to **Databases** in the left menu
- Click **Add** and create a database named `net4courier`

---

## Part 2: Set Up Azure App Service

### Step 6: Create an App Service
- Search for **"App Service"** in Azure Portal
- Click **Create** > **Web App**
- Fill in:
  - **Resource Group**: Same as before (`net4courier-rg`)
  - **Name**: e.g., `gateex` (this becomes `gateex.azurewebsites.net`)
  - **Publish**: Code
  - **Runtime stack**: .NET 8 (LTS)
  - **Operating System**: Linux (recommended, cheaper)
  - **Region**: Same region as your database
  - **Pricing plan**: Start with **B1** (Basic, ~$13/month) — minimum for production
- Click **Review + Create**, then **Create**

### Step 7: Configure App Settings (Environment Variables)
- Go to your App Service > **Configuration** > **Application settings**
- Add these settings:

| Name | Value |
|------|-------|
| `ConnectionStrings__DefaultConnection` | Your PostgreSQL connection string from Step 4 |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `PRODUCTION_MODE` | `true` |
| `SCHEMA_AUTO_APPLY` | `true` (for first deployment, change to `false` later) |
| `PLATFORMADMIN_PASSWORD` | Your desired admin password (optional) |

- Click **Save**

### Step 8: Configure General Settings
- Go to **Configuration** > **General settings**
- **Startup Command**:
  ```
  dotnet Net4Courier.Web.dll --urls http://0.0.0.0:8080
  ```
- Azure Linux App Service uses port **8080** by default
- Click **Save**

---

## Part 3: Publish Your Code

### Option A: Publish from Local Machine (Recommended for first deploy)

#### Step 9: Publish the project locally
```bash
cd src/Net4Courier.Web
dotnet publish -c Release -o ./publish
```

#### Step 10: Zip the output
```bash
cd publish
zip -r ../deploy.zip .
```

#### Step 11: Deploy using Azure CLI
```bash
az login
az webapp deploy --resource-group net4courier-rg --name gateex --src-path deploy.zip --type zip
```

### Option B: Set Up GitHub/Git Deployment (For ongoing updates)

#### Step 9: Push your code to GitHub (if not already)

#### Step 10: Connect App Service to GitHub
- Go to App Service > **Deployment Center**
- Source: **GitHub**
- Authorize and select your repository and branch
- Azure will auto-create a GitHub Actions workflow file
- Every push to your selected branch will auto-deploy

---

## Part 4: Configure Custom Domain (for gateex.truebookserp.com)

### Step 12: Add Custom Domain
- Go to App Service > **Custom domains**
- Click **Add custom domain**
- Enter `gateex.truebookserp.com`
- Azure will show DNS records you need to add

### Step 13: Update DNS Records (at your domain registrar)
- Add a **CNAME** record:
  - **Name**: `gateex`
  - **Value**: `gateex.azurewebsites.net`
- Or if using root domain, add the **TXT** verification record shown by Azure

### Step 14: Enable Free SSL
- Go to **Certificates** > **Add certificate**
- Select **App Service Managed Certificate** (free)
- Bind it to your custom domain
- Enable **HTTPS Only** in the custom domains section

---

## Part 5: Post-Deployment Checklist

### Step 15: First Launch Verification
- Browse to your site URL
- Check the login page loads with proper styling
- Log in as `platformadmin` with your configured password
- Run the Initial Setup Wizard to create your company
- The database tables will be created automatically on first startup

### Step 16: Secure Production Settings
- Go back to Configuration and change `SCHEMA_AUTO_APPLY` to `false`
- This enables preview-only mode for future schema changes (safer for production)

### Step 17: Enable Logging (for troubleshooting)
- Go to App Service > **App Service logs**
- Turn on **Application logging (Filesystem)**
- Set level to **Information**
- Use **Log stream** to watch live logs

---

## Estimated Monthly Costs

| Service | Tier | Approx. Cost |
|---------|------|-------------|
| App Service | B1 (Basic) | ~$13/month |
| PostgreSQL Flexible | Burstable B1ms | ~$13/month |
| Custom Domain SSL | Managed Certificate | Free |
| **Total** | | **~$26/month** |

You can scale up as needed — the next tier (Standard S1) is ~$70/month for the App Service.

---

## Troubleshooting

### Styling not loading (404 for CSS/JS)
- Ensure you deployed the latest code with MudBlazor files in `wwwroot/lib/mudblazor/`
- Check App Service logs for static file resolution messages

### Database connection errors
- Verify the connection string in App Settings
- Ensure "Allow public access from any Azure service" is enabled on the PostgreSQL server
- Check that SSL Mode is set to `Require` in the connection string

### App not starting
- Check **Log stream** in App Service for startup errors
- Verify the startup command is correct
- Ensure the runtime stack is set to .NET 8

### Custom domain not working
- Allow up to 24 hours for DNS propagation
- Verify CNAME record is correctly configured
- Check that the SSL certificate is bound to the domain
