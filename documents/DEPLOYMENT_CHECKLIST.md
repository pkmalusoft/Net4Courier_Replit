# Net4Courier Deployment Checklist

This document provides step-by-step instructions for deploying Net4Courier to new clients.

---

## Option 1: Deploy via Replit (Recommended)

### Prerequisites
- Replit account with sufficient Cycles
- Access to the Net4Courier source repository

### Step 1: Fork or Import the Project
1. Go to your Replit dashboard
2. Click **"Create Repl"** or **"Import from GitHub"**
3. Select the Net4Courier repository
4. Wait for the project to import

### Step 2: Set Up the Database
1. In Replit, go to the **Tools** panel
2. Click on **Database** (PostgreSQL)
3. Click **"Create Database"** - this automatically provisions a PostgreSQL instance
4. The following environment variables are automatically set:
   - `DATABASE_URL`
   - `PGHOST`, `PGPORT`, `PGUSER`, `PGPASSWORD`, `PGDATABASE`

### Step 3: Configure Environment Variables (Secrets)
1. Go to **Secrets** in the Replit sidebar
2. Add the following required secret:
   - **`SETUP_KEY`** = Your chosen secure password for the Platform Admin account
     - This becomes the initial password for the `platformadmin` user
     - If not set, a random 12-character password is generated (check console logs)
3. Optional secrets:
   - `ASPNETCORE_ENVIRONMENT` = `Production` (for production deployments)
   - API keys for third-party integrations (if used)

### Step 4: Run the Application
1. Click the **Run** button or start the workflow
2. The application will:
   - Automatically create all database tables (including GL tables)
   - Seed essential data (countries, currencies, roles)
   - Create the Platform Admin user
   - Start the web server on port 5000
3. **Check the console logs** for the Platform Admin password if you didn't set `SETUP_KEY`

### Step 5: Initial Setup (First-Time Configuration)
1. Access the application at the provided URL
2. Log in as Platform Admin:
   - **Username**: `platformadmin`
   - **Password**: Value of your `SETUP_KEY` secret (or check console logs for generated password)
3. Navigate to **Platform Admin > Manage Demo Data**
4. Click **"Create Demo Data"** to open the Initial Setup dialog
5. Fill in the required details:
   - Company Name
   - Country
   - Currency
   - Admin Name, Email, Username
6. The system will create:
   - Your company with the provided details
   - A branch for the company
   - An admin user with a secure temporary password (shown on screen)
7. **Save the temporary password** - you'll need it for the new admin user

### Step 6: Publish to Production
1. Click the **Publish** button in Replit
2. Choose deployment type:
   - **Reserved VM**: Recommended for business applications (always-on, consistent performance)
   - **Autoscale**: Good for variable traffic (scales down when idle to save costs)
3. Configure:
   - Build command: `cd src/Net4Courier.Web && dotnet build --configuration Release`
   - Run command: `cd src/Net4Courier.Web && dotnet run --urls http://0.0.0.0:5000`
4. **Important**: In the Deployments tab, ensure the production database is connected
   - The `DATABASE_URL` must be available in production environment
5. Click **Publish**

### Step 7: Custom Domain (Optional)
1. In Publish settings, click **"Add Custom Domain"**
2. Enter your domain (e.g., `courier.yourcompany.com`)
3. Add the provided DNS records to your domain registrar
4. Wait for verification (usually 5-30 minutes)

---

## Setup & Maintenance Utilities

Net4Courier provides two utilities for initial setup and password management:

### 1. Web-Based Setup Page (`/setup`)

The `/setup` page provides a secure web interface for:
- Creating administrator accounts
- Resetting existing user passwords
- Viewing all users in the system

**How to use:**
1. Set the `SETUP_KEY` environment variable to a secure password
2. Access `/setup` in your browser
3. Enter the setup key to authenticate
4. Use the tabs to:
   - **Create Admin**: Add new administrator users
   - **Reset Password**: Reset any user's password
   - **Users**: View all existing users

**Security:** The setup page is disabled by default. It only activates when `SETUP_KEY` is set. Remove `SETUP_KEY` after completing setup to disable access.

### 2. Command-Line Password Reset

For quick password resets without the web interface:

```bash
cd src/Net4Courier.Web
export SETUP_KEY="your_setup_key"  # Required for security
dotnet run -- --reset-password <username> <newpassword>
```

**Example:**
```bash
export SETUP_KEY="MySecureKey123"
dotnet run -- --reset-password platformadmin "T6u3b00ks"
```

**Requirements:**
- `SETUP_KEY` environment variable must be set (security requirement)
- The workflow must be stopped before running this command (it starts a temporary connection to the database)

### Help Command

View available command-line options:
```bash
dotnet run -- --help
```

---

## Option 2: Deploy to External Server

### Prerequisites
- Server with .NET 8.0 Runtime installed
- PostgreSQL 14+ database server
- Git access to the repository

### Step 1: Clone the Repository
```bash
git clone <repository-url> net4courier
cd net4courier
```

### Step 2: Set Up PostgreSQL Database
```sql
CREATE DATABASE net4courier;
CREATE USER net4courier_user WITH ENCRYPTED PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE net4courier TO net4courier_user;
\c net4courier
GRANT ALL ON SCHEMA public TO net4courier_user;
```

### Step 3: Configure Environment Variables
The application reads `DATABASE_URL` environment variable. Set it using the Npgsql connection string format:

```bash
export DATABASE_URL="Host=localhost;Port=5432;Database=net4courier;Username=net4courier_user;Password=your_secure_password"
export SETUP_KEY="your_secure_platform_admin_password"
export ASPNETCORE_ENVIRONMENT="Production"
```

Alternatively, create `src/Net4Courier.Web/appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=net4courier;Username=net4courier_user;Password=your_secure_password"
  }
}
```

**Note**: The application checks `DATABASE_URL` first, then falls back to `ConnectionStrings:DefaultConnection`.

### Step 4: Build and Run
```bash
cd src/Net4Courier.Web
dotnet restore
dotnet build --configuration Release
dotnet run --configuration Release --urls "http://0.0.0.0:5000"
```

### Step 5: Production Deployment with Reverse Proxy
For production, use a reverse proxy (nginx/Apache) with HTTPS:

**nginx configuration example:**
```nginx
server {
    listen 80;
    server_name courier.yourcompany.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name courier.yourcompany.com;
    
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # WebSocket timeout for Blazor SignalR
        proxy_read_timeout 86400;
    }
}
```

### Step 6: Run as a Service (systemd)
Create `/etc/systemd/system/net4courier.service`:
```ini
[Unit]
Description=Net4Courier Web Application
After=network.target postgresql.service

[Service]
WorkingDirectory=/path/to/net4courier/src/Net4Courier.Web
ExecStart=/usr/bin/dotnet run --configuration Release --urls http://localhost:5000
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DATABASE_URL=Host=localhost;Port=5432;Database=net4courier;Username=net4courier_user;Password=your_secure_password
Environment=SETUP_KEY=your_secure_platform_admin_password
User=www-data
Group=www-data

[Install]
WantedBy=multi-user.target
```

Then enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable net4courier
sudo systemctl start net4courier
```

---

## Automatic Database Initialization

The application automatically handles database setup on first run. **No migrations needed.**

### How It Works
1. `EnsureCreatedAsync()` creates all EF Core-managed tables
2. `DatabaseInitializationService` creates additional tables using `CREATE TABLE IF NOT EXISTS`:
   - GL Module tables (GLAccountClassifications, GLChartOfAccounts, GLTaxCodes, GLVoucherNumberings)
   - ShipmentStatusGroups and ShipmentStatuses
3. Seed data is inserted with `WHERE NOT EXISTS` checks (idempotent)

### Tables Created Automatically
- All application tables via EF Core `EnsureCreated`
- GL Module tables (with proper foreign keys and indexes)
- Shipment status tables
- Geographic data tables

### Seed Data Created Automatically
- Default currencies (AED, USD, EUR, GBP, INR, SAR, etc.)
- Geographic data (UAE Emirates, GCC countries, India states/cities, etc.)
- Default roles (Administrator, PlatformAdmin)
- Platform Admin user
- Service types, shipment modes, ports
- Other charge types, vehicles

---

## Security Best Practices

### Credentials
- **Never use default or hardcoded passwords**
- Always set `SETUP_KEY` environment variable before first run
- The Platform Admin password is either:
  - Your `SETUP_KEY` value, OR
  - A randomly generated 12-character password (logged to console on first run)
- Change all passwords after initial setup

### Environment Variables
| Variable | Required | Description |
|----------|----------|-------------|
| `DATABASE_URL` | Yes | PostgreSQL connection string |
| `SETUP_KEY` | Recommended | Initial Platform Admin password |
| `ASPNETCORE_ENVIRONMENT` | Recommended | Set to `Production` for production deployments |

### Production Security Checklist
- [ ] Set `SETUP_KEY` before first deployment
- [ ] Configure HTTPS/SSL certificate
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Restrict database access to application only
- [ ] Change Platform Admin password after setup
- [ ] Create individual admin accounts (don't share Platform Admin)
- [ ] Enable firewall rules (only expose ports 80/443)

---

## Post-Deployment Configuration

### Required Setup
1. [ ] Log in as Platform Admin
2. [ ] Run Initial Setup to create Company and Admin
3. [ ] Log out and log in as the new Admin user
4. [ ] Change the Admin password

### Business Configuration
- [ ] Upload company logo (Settings > Company)
- [ ] Configure branches (Settings > Branches)
- [ ] Set financial year (Finance > Financial Periods)
- [ ] Configure rate cards (Pricing > Rate Cards)
- [ ] Set up zones and routing (Masters > Zones)
- [ ] Configure AWB number series per branch (Settings > Branches > AWB Config)

### Optional Configuration
- [ ] Set up email integration (Gmail API)
- [ ] Configure API webhooks for third-party integrations
- [ ] Set up backup schedule for PostgreSQL database

---

## Troubleshooting

### Database Connection Issues
```
Error: Connection refused / Host not found
```
- Verify `DATABASE_URL` is correctly formatted
- Check PostgreSQL is running: `sudo systemctl status postgresql`
- Verify firewall allows connections on port 5432
- Test connection: `psql "$DATABASE_URL"`

### Application Won't Start
```
Error: Unable to find project / Build failed
```
- Verify .NET 8.0 SDK/Runtime is installed: `dotnet --version`
- Ensure NuGet packages restored: `dotnet restore`
- Check for missing dependencies in logs

### Platform Admin Login Fails
- If `SETUP_KEY` wasn't set, check console logs for generated password
- The password is only logged on first user creation
- **Option 1: Use Setup Page** (Recommended)
  1. Set `SETUP_KEY` environment variable
  2. Navigate to `/setup`
  3. Enter the setup key
  4. Use "Reset Password" tab to reset any user's password
- **Option 2: Use Command Line**
  ```bash
  cd src/Net4Courier.Web
  dotnet run -- --reset-password platformadmin "NewPassword123"
  ```
- **Option 3: Direct SQL**
  ```bash
  psql $DATABASE_URL -c "UPDATE \"Users\" SET \"PasswordHash\" = 'bcrypt_hash_here' WHERE \"Username\" = 'platformadmin';"
  ```

### GL Module Errors
- GL tables are created automatically on startup
- Check logs for "GLAccountClassifications table ensured" messages
- If tables missing, restart the application

### Blank Page After Login / WebSocket Errors
- Clear browser cache and cookies
- Check browser console for errors
- Verify WebSocket connections not blocked by firewall/proxy
- For nginx, ensure `proxy_read_timeout 86400;` is set

### Replit-Specific Issues
- If deployment fails, check that database is connected in Deployments tab
- For Reserved VM, ensure machine has enough memory (1GB+ recommended)
- Check `REPLIT_DEPLOYMENT` environment variable is set automatically in production

---

## Quick Reference

### Default URLs
| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5000` or Replit preview URL |
| Production | Your custom domain or Replit deployment URL |

### Key Application Routes
| Route | Description |
|-------|-------------|
| `/` | Login page |
| `/setup` | Setup & maintenance page (requires SETUP_KEY) |
| `/dashboard` | Main dashboard (after login) |
| `/platform-admin/demo-data` | Initial Setup / Demo Data management |
| `/how-to-guides` | Built-in documentation |
| `/tracking/{awbNo}` | Public shipment tracking |

### Console Log Messages to Watch For
```
GLAccountClassifications table ensured
GLChartOfAccounts table ensured
GLTaxCodes table ensured
GLVoucherNumberings table ensured
Database initialization completed successfully
Platform admin user created (username: platformadmin, password: ...)
```

---

## Support

For additional assistance:
- Check the Knowledge Base in the application (`/how-to-guides`)
- Review application logs for detailed error messages
- For Replit deployments, check the Logs tab in the deployment dashboard
