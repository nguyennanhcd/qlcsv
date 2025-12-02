# Deploy to Render.com - Step by Step Guide

## Prerequisites

- GitHub account
- Render.com account (free)
- Your project pushed to GitHub

## Step 1: Prepare Your Repository

### 1.1 Ensure .gitignore is working

Check that these files are NOT in your repo:

```
.env
bin/
obj/
*.user
```

### 1.2 Push to GitHub

```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/yourusername/your-repo.git
git push -u origin main
```

## Step 2: Create PostgreSQL Database on Render

1. Go to https://render.com and sign in
2. Click **"New +"** → **"PostgreSQL"**
3. Configure:
   - **Name**: `qlcsv-db` (or any name)
   - **Database**: `qlcsv`
   - **User**: `qlcsv`
   - **Region**: Choose closest to you
   - **Instance Type**: **Free**
4. Click **"Create Database"**
5. Wait ~2 minutes for provisioning
6. **IMPORTANT**: Copy the **"Internal Database URL"** (starts with `postgres://`)
   - Example: `postgres://qlcsv:password@dpg-xxxxx-a.oregon-postgres.render.com/qlcsv`

## Step 3: Create Web Service

1. Click **"New +"** → **"Web Service"**
2. Connect your GitHub repository
3. Configure:
   - **Name**: `qlcsv-api` (this will be your URL)
   - **Region**: Same as database
   - **Branch**: `main`
   - **Runtime**: **Docker** (Render auto-detects .NET)
   - **Build Command**: Leave empty (auto-detected)
   - **Start Command**: Leave empty (auto-detected)
   - **Instance Type**: **Free**

## Step 4: Add Environment Variables

In the **Environment** section, add these variables:

| Key                      | Value                                       |
| ------------------------ | ------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT` | `Production`                                |
| `DATABASE_URL`           | Paste the Internal Database URL from Step 2 |
| `JWT_SECRET_KEY`         | Generate a random 32+ character string¹     |

¹ Generate secure key: https://generate-random.org/api-key-generator (select 32+ characters)

Example:

```
JWT_SECRET_KEY=a8f5f167f44f4964e6c998dee827110c1234567890abcdef
```

## Step 5: Deploy

1. Click **"Create Web Service"**
2. Render will:
   - Clone your repo
   - Build the .NET app
   - Start the service
3. Wait 5-10 minutes for first deployment
4. Watch the logs for any errors

## Step 6: Run Database Migrations

### Option A: Using Render Shell (Recommended)

1. In your web service dashboard, click **"Shell"** tab
2. Run:

```bash
dotnet ef database update
```

### Option B: Let EF Auto-Create Tables

Your code already has connection string parsing for Render. On first startup, EF will connect using the `DATABASE_URL`.

If you want auto-creation, temporarily add this to `Program.cs` before deploying:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}
```

## Step 7: Test Your API

Your API will be available at:

```
https://qlcsv-api.onrender.com
```

Test endpoints:

- Swagger: `https://qlcsv-api.onrender.com/swagger`
- Register: `POST https://qlcsv-api.onrender.com/api/auth/register`
- Login: `POST https://qlcsv-api.onrender.com/api/auth/login`

### Test with Postman:

```json
POST https://qlcsv-api.onrender.com/api/auth/register
Content-Type: application/json

{
  "username": "admin",
  "email": "admin@example.com",
  "password": "Admin@123",
  "fullName": "System Admin"
}
```

## Step 8: Connect WinForms App

In your WinForms app, update the API base URL:

```csharp
private const string API_BASE_URL = "https://qlcsv-api.onrender.com";
```

## Important Notes

### Free Tier Limitations

- **Sleep after 15 minutes**: Service spins down when inactive
- **Cold start**: First request after sleep takes 30-60 seconds
- **750 hours/month**: Enough for development/testing
- **Database**: 1GB storage, 100 max connections

### Keep Service Awake (Optional)

To prevent sleep, use a free service like:

- **UptimeRobot**: Ping your API every 14 minutes
- **Cron-job.org**: Schedule health check requests

Setup:

1. Create health endpoint in your API (or use `/swagger`)
2. Set UptimeRobot to ping `https://qlcsv-api.onrender.com/swagger` every 14 minutes

### Security Checklist

- ✅ `.env` file is in `.gitignore`
- ✅ No secrets in `appsettings.json` or code
- ✅ Strong JWT_SECRET_KEY (32+ chars)
- ✅ Production environment variables set
- ✅ Database credentials from Render (never hardcoded)

### Troubleshooting

**Build Fails**

- Check Render logs for error details
- Ensure all NuGet packages are in `.csproj`
- Verify .NET 8.0 SDK is specified

**Database Connection Fails**

- Verify `DATABASE_URL` is correct (use Internal URL, not External)
- Check database is in same region as web service
- Ensure connection string parser in `Program.cs` is correct

**App Crashes on Startup**

- Check environment variables are set correctly
- Look for errors in Render logs
- Verify `JWT_SECRET_KEY` is at least 32 characters

**API Returns 500 Errors**

- Check Render logs for exceptions
- Verify database tables exist (run migrations)
- Test endpoints in Swagger UI first

### Updating Your App

After code changes:

```bash
git add .
git commit -m "Update feature X"
git push
```

Render auto-deploys on every push to `main` branch.

### Database Backup

Free tier doesn't include automatic backups. To backup manually:

1. Use Render Shell or local connection
2. Export data:

```bash
pg_dump -h dpg-xxxxx.oregon-postgres.render.com -U qlcsv qlcsv > backup.sql
```

## Next Steps

1. **Set up CI/CD**: Render auto-deploys from GitHub
2. **Monitor**: Check Render dashboard for logs and metrics
3. **Scale**: Upgrade to paid tier when needed ($7/month for always-on service)
4. **Custom domain**: Add your own domain in Render settings (paid tier)

## Support

- Render Docs: https://render.com/docs
- Community: https://community.render.com
- Your API logs: Render Dashboard → Your Service → Logs tab

---

**Estimated deployment time**: 15-20 minutes
**Cost**: $0/month (Free tier)
