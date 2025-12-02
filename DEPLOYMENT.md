# QLCSV API Deployment Guide

## ⚠️ BEFORE DEPLOYING

### 1. Set Environment Variables

Your deployment platform (Render, Azure, AWS, etc.) must have these environment variables:

```bash
# REQUIRED: JWT Secret Key (minimum 32 characters)
JWT_SECRET_KEY=<your-strong-random-secret-key>

# REQUIRED: Database URL (if using Render.com PostgreSQL)
DATABASE_URL=postgres://username:password@host:port/database
```

### 2. Generate Strong JWT Secret Key

```bash
# Linux/Mac/Git Bash:
openssl rand -base64 48

# PowerShell:
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Online: https://generate-secret.vercel.app/32
```

## 📋 Deployment Steps

### Option 1: Render.com (Recommended for beginners)

1. **Create PostgreSQL Database**

   - Go to Render Dashboard → New → PostgreSQL
   - Copy the "Internal Database URL"

2. **Create Web Service**

   - Go to New → Web Service
   - Connect your GitHub repository
   - Settings:
     - **Environment**: `.NET`
     - **Build Command**: `dotnet publish -c Release -o out`
     - **Start Command**: `dotnet out/QLCSV.dll`
     - **Environment Variables**:
       ```
       JWT_SECRET_KEY=<your-generated-secret>
       DATABASE_URL=<your-postgres-internal-url>
       ASPNETCORE_ENVIRONMENT=Production
       ```

3. **Run Migrations**
   - After first deployment, use Render Shell:
     ```bash
     dotnet ef database update
     ```

### Option 2: Azure App Service

1. **Create Azure App Service** (ASP.NET Core 8.0)
2. **Create Azure Database for PostgreSQL**
3. **Set Application Settings**:

   - `JWT_SECRET_KEY`: Your secret key
   - `DATABASE_URL`: Your PostgreSQL connection string
   - `ASPNETCORE_ENVIRONMENT`: Production

4. **Deploy**:
   ```bash
   dotnet publish -c Release
   # Upload to Azure via VS Code or Azure CLI
   ```

### Option 3: Docker

1. **Create Dockerfile** (in project root):

   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 80

   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY ["QLCSV.csproj", "./"]
   RUN dotnet restore
   COPY . .
   RUN dotnet publish -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=build /app/publish .
   ENTRYPOINT ["dotnet", "QLCSV.dll"]
   ```

2. **Build & Run**:
   ```bash
   docker build -t qlcsv-api .
   docker run -p 5000:80 \
     -e JWT_SECRET_KEY=your-secret \
     -e DATABASE_URL=your-db-url \
     qlcsv-api
   ```

## 🔒 Security Checklist

- [ ] JWT_SECRET_KEY is at least 32 characters
- [ ] Database password is strong (not "12345")
- [ ] appsettings.json does NOT contain real secrets
- [ ] HTTPS is enabled (automatic on Render/Azure)
- [ ] Environment is set to "Production"
- [ ] Database migrations are applied

## 🧪 Testing After Deployment

1. **Health Check**:

   ```bash
   curl https://your-api-url.com/api/auth/me
   # Should return 401 Unauthorized
   ```

2. **Register Test User**:

   ```bash
   curl -X POST https://your-api-url.com/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"test@test.com","password":"Test123!","fullName":"Test User"}'
   ```

3. **Login**:
   ```bash
   curl -X POST https://your-api-url.com/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"test@test.com","password":"Test123!"}'
   ```

## 📱 WinForms Integration

In your WinForms app, use the deployed API URL:

```csharp
// appsettings.json in WinForms project
{
  "ApiBaseUrl": "https://your-api-url.onrender.com/api"
}

// HttpClient setup
var client = new HttpClient();
client.BaseAddress = new Uri("https://your-api-url.onrender.com/api/");
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

## 🐛 Troubleshooting

**Issue**: "Connection string not found"

- **Fix**: Ensure DATABASE_URL environment variable is set

**Issue**: "JWT_SECRET_KEY is not configured"

- **Fix**: Set JWT_SECRET_KEY environment variable

**Issue**: 500 errors

- **Fix**: Check application logs in Render/Azure dashboard

**Issue**: Database connection fails

- **Fix**: Ensure DATABASE_URL format is correct and database is accessible

## 📞 Support

For issues, check logs in your deployment platform's dashboard.
