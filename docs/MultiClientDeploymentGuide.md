# Net4Courier Multi-Client Deployment Guide

This guide documents the step-by-step process for deploying Net4Courier to multiple clients using a single source codebase with separate databases for each client.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Net4Courier Source Code                       │
│                    (Main Repl - net4courier)                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Fork/Deploy
                              ▼
        ┌─────────────────────┴─────────────────────┐
        │                                           │
        ▼                                           ▼
┌───────────────────┐                   ┌───────────────────┐
│   GateEx Repl     │                   │   Rainbow Repl    │
│                   │                   │                   │
│ gateex.truebooks  │                   │ rainbow.truebooks │
│   erp.com         │                   │   erp.com         │
│                   │                   │                   │
│ ┌───────────────┐ │                   │ ┌───────────────┐ │
│ │ PostgreSQL DB │ │                   │ │ PostgreSQL DB │ │
│ │ (GateEx Data) │ │                   │ │(Rainbow Data) │ │
│ └───────────────┘ │                   │ └───────────────┘ │
└───────────────────┘                   └───────────────────┘
```

### Key Principles
- **Single Source Code**: All clients run the same Net4Courier application
- **Separate Databases**: Each client has isolated data in their own PostgreSQL database
- **Independent Deployments**: Each client runs as a separate Replit deployment
- **Custom Domains**: Each deployment has its own subdomain under truebookserp.com

---

## Current Deployments

| Client | Domain | Repl Name | Status |
|--------|--------|-----------|--------|
| GateEx | gateex.truebookserp.com | gateex-net4courier | Active |
| Rainbow | rainbow.truebookserp.com | rainbow-net4courier | Active |

---

## Step-by-Step: Deploying a New Client

### Prerequisites
- Access to the main Net4Courier Repl (source code)
- Replit account with deployment capabilities
- Access to your domain DNS settings (truebookserp.com)
- Client company details (name, logo, admin user info)

---

### Step 1: Fork the Source Repl

1. Open the main **Net4Courier** Repl (your source code)
2. Click the three-dot menu (⋮) in the top right
3. Select **"Fork"**
4. Name the new Repl: `[clientname]-net4courier` (e.g., `acme-net4courier`)
5. Click **"Fork Repl"**

> **Note**: Forking creates an independent copy of the code. The new Repl will not receive automatic updates from the source.

---

### Step 2: Create the Client's Database

1. In the new forked Repl, open the **Database** tool (left sidebar)
2. Click **"Create Database"** or **"PostgreSQL"**
3. Replit will automatically:
   - Provision a new PostgreSQL database
   - Set the `DATABASE_URL` environment variable
   - Configure connection details

4. Verify the database connection by checking the **Secrets** tab for:
   - `DATABASE_URL`
   - `PGHOST`
   - `PGDATABASE`
   - `PGUSER`
   - `PGPASSWORD`
   - `PGPORT`

---

### Step 3: Configure Environment Variables

In the **Secrets** tab, verify/add the following:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `DATABASE_URL` | PostgreSQL connection string | (Auto-created) |
| `PGHOST` | Database host | (Auto-created) |
| `PGDATABASE` | Database name | (Auto-created) |
| `PGUSER` | Database user | (Auto-created) |
| `PGPASSWORD` | Database password | (Auto-created) |

Optional secrets (if using integrations):
| Secret Name | Description |
|-------------|-------------|
| `GOOGLE_CLIENT_ID` | For Gmail integration |
| `GOOGLE_CLIENT_SECRET` | For Gmail integration |

---

### Step 4: Run Database Migrations

1. Start the application by clicking **Run**
2. The application will automatically:
   - Detect the new database
   - Run Entity Framework migrations
   - Create all required tables

3. Watch the console for migration messages:
   ```
   Applying migration 'InitialCreate'...
   Migration applied successfully.
   ```

4. If migrations don't run automatically, use the console:
   ```bash
   cd src/Net4Courier.Web && dotnet ef database update
   ```

---

### Step 5: Deploy the Application

1. Click the **"Deploy"** button (rocket icon) in the top right
2. Select deployment type: **"Autoscale"** or **"Reserved VM"**
   - **Autoscale**: Cost-effective, scales to zero when idle
   - **Reserved VM**: Always-on, better for production clients
3. Configure deployment settings:
   - Build command: `dotnet publish -c Release -o out`
   - Run command: `dotnet out/Net4Courier.Web.dll --urls http://0.0.0.0:5000`
4. Click **"Deploy"**

---

### Step 6: Configure Custom Domain

#### In Replit:
1. Go to **Deployments** → **Settings** → **Custom Domains**
2. Click **"Add Custom Domain"**
3. Enter: `[clientname].truebookserp.com` (e.g., `acme.truebookserp.com`)
4. Copy the provided CNAME target

#### In DNS (your domain registrar):
1. Go to DNS settings for `truebookserp.com`
2. Add a CNAME record:
   - **Host/Name**: `[clientname]` (e.g., `acme`)
   - **Type**: CNAME
   - **Value/Target**: (paste the Replit CNAME target)
   - **TTL**: 3600 (or default)
3. Save and wait for DNS propagation (5-30 minutes)

#### Verify:
1. Return to Replit Custom Domains
2. Click **"Verify"** next to the domain
3. Wait for SSL certificate provisioning
4. Test the URL: `https://[clientname].truebookserp.com`

---

### Step 7: Initial Client Setup

After deployment, perform initial configuration:

#### 7.1 Create Admin User
1. Access the login page
2. Use the default admin credentials (if seeded) or:
   - Access the database directly
   - Create an admin user with hashed password

#### 7.2 Configure Company Details
1. Login as admin
2. Navigate to **Masters & Settings** → **Organization** → **Company**
3. Enter client company details:
   - Company Name
   - Address
   - Phone, Email
   - Upload company logo
   - Tax registration numbers

#### 7.3 Setup Financial Year
1. Go to **Finance & Accounting** → **GL Masters** → **Financial Years**
2. Create the current financial year:
   - Start Date
   - End Date
   - Set as Active

#### 7.4 Create Branches
1. Go to **Masters & Settings** → **Organization** → **Branches**
2. Add client's branch locations

#### 7.5 Configure Users
1. Go to **Masters & Settings** → **User & Security** → **Users**
2. Create user accounts for client staff
3. Assign appropriate roles

---

## Updating Deployments

When you update the source code, you need to update each client deployment:

### Option 1: Manual Update (Recommended for Major Changes)

1. **In each client Repl**:
   - Compare changes needed from source
   - Manually apply critical updates
   - Test thoroughly before redeploying

2. **Redeploy**:
   - Click **Deploy** → **Redeploy**

### Option 2: Re-fork with Data Migration (Major Upgrades)

1. Export client data from current database
2. Fork the updated source Repl
3. Create new database
4. Import client data
5. Configure domain on new deployment
6. Update DNS to point to new deployment

### Best Practices for Updates
- Keep a changelog of source code changes
- Test updates on a staging environment first
- Schedule updates during low-usage hours
- Notify clients before major updates
- Backup databases before updating

---

## Troubleshooting

### Database Connection Issues
```bash
# Check database status
cd src/Net4Courier.Web && dotnet ef database update --verbose
```

### Domain Not Working
1. Verify DNS CNAME record is correct
2. Check Replit deployment is running
3. Wait for DNS propagation (can take up to 48 hours)
4. Verify SSL certificate is provisioned

### Application Errors
1. Check deployment logs in Replit
2. Review console output for exceptions
3. Verify all environment variables are set

### Migration Failures
```bash
# Reset and reapply migrations (WARNING: Data loss)
cd src/Net4Courier.Web
dotnet ef database drop --force
dotnet ef database update
```

---

## Security Considerations

1. **Database Isolation**: Each client has completely separate data
2. **SSL/TLS**: All deployments use HTTPS via Replit
3. **Secrets Management**: Use Replit Secrets for sensitive data
4. **Password Hashing**: BCrypt is used for all passwords
5. **Access Control**: Role-based permissions per deployment

---

## Cost Considerations

| Component | Cost Model |
|-----------|------------|
| Replit Deployment | Per deployment based on plan |
| PostgreSQL Database | Included with Replit database |
| Custom Domain | Free (you own the domain) |
| SSL Certificate | Free (auto-provisioned) |

---

## Client Onboarding Checklist

- [ ] Fork source Repl with client name
- [ ] Create PostgreSQL database
- [ ] Run database migrations
- [ ] Deploy application
- [ ] Configure custom domain
- [ ] Setup DNS CNAME record
- [ ] Verify SSL certificate
- [ ] Create admin user account
- [ ] Configure company details
- [ ] Setup financial year
- [ ] Create branches
- [ ] Add user accounts
- [ ] Upload company logo
- [ ] Configure email integration (if needed)
- [ ] Conduct client training
- [ ] Provide support documentation

---

## Support Contacts

For technical issues with Net4Courier deployments:
- Review this guide first
- Check Replit documentation: https://docs.replit.com
- Contact development team for source code issues

---

*Document Version: 1.0*
*Last Updated: January 2026*
