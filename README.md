# QLCSV - Alumni Management System

## ✅ Project Cleaned & Migration Created

All redundant code has been removed and a fresh, complete migration has been created.

## What Was Done

1. ✅ Removed unused imports (`Microsoft.Extensions.Logging` from User model)
2. ✅ Deleted old migrations
3. ✅ Simplified database initialization code
4. ✅ Created fresh `InitialCreate` migration with complete schema:
   - Users (with email verification & password reset fields)
   - Alumni Profiles
   - Faculties & Majors
   - Batches & Alumni-Batch relationships
   - Events & Event Registrations
5. ✅ Fixed all nullable reference warnings
6. ✅ Removed temporary documentation files

## Fresh Database Setup (For Render)

### Step 1: Delete Old Database on Render

1. Go to Render Dashboard → Your PostgreSQL Database
2. Click "Delete Database"
3. Create a new PostgreSQL database

### Step 2: Update Environment Variable

Update your web service's `DATABASE_URL` with the new database connection string

### Step 3: Deploy

```bash
git add .
git commit -m "Clean migration - fresh database schema"
git push origin main
```

The app will automatically create all tables on first run!

## Local Development

To apply the migration locally:

```bash
# Drop your local database (if it exists)
dotnet ef database drop --force

# Apply the fresh migration
dotnet ef database update

# Run the app
dotnet run
```

## Migration Details

**Migration Name:** `InitialCreate`  
**Timestamp:** `20251203110446`

**Includes:**

- ✅ All 8 database tables
- ✅ Email verification fields (EmailVerified, EmailVerificationToken, EmailVerificationTokenExpiry)
- ✅ Password reset fields (PasswordResetToken, PasswordResetTokenExpiry)
- ✅ All relationships and constraints
- ✅ Proper indexes and defaults

## New Features Included

1. **Email Verification** - `POST /api/auth/verify-email`, `POST /api/auth/resend-verification`
2. **Password Reset** - `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`
3. **Pagination** - All list endpoints support `?pageNumber=1&pageSize=20`
4. **Race Condition Fix** - Event registration uses database transactions
5. **Request Size Limits** - 10MB maximum to prevent DoS

## SMTP Configuration (Optional)

Add to your environment variables or `appsettings.json`:

```json
{
  "AppUrl": "https://your-app.onrender.com",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "QLCSV Alumni System"
  }
}
```

If SMTP is not configured, email features will log warnings but won't crash the app.

## API Documentation

Swagger UI available at: `http://localhost:5083/swagger` (development)

## Production Checklist

- [ ] Delete old Render database
- [ ] Create new Render database
- [ ] Update `DATABASE_URL` environment variable
- [ ] Set `JWT_SECRET_KEY` environment variable (min 32 chars)
- [ ] (Optional) Configure SMTP environment variables
- [ ] Push to main branch
- [ ] Verify deployment logs show "✅ Database migrations applied successfully!"

## Support

The application will:

- ✅ Create database and all tables automatically on first run
- ✅ Continue running even if migrations fail (with warning)
- ✅ Apply pending migrations automatically on each deployment
- ✅ Work with both new and existing databases

Ready to deploy! 🚀
