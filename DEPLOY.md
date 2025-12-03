# 🚀 Deployment Guide - Fresh Database

## ✅ Project Status

- ✅ **0 Errors, 0 Warnings** - Clean build
- ✅ **Fresh migration created** - `InitialCreate` with complete schema
- ✅ **All redundant code removed**
- ✅ **Ready for production deployment**

## 🎯 Steps to Deploy on Render

### 1. Delete Old Database

```
Render Dashboard → PostgreSQL Database → Delete Database
```

### 2. Create New Database

```
Render Dashboard → New → PostgreSQL
Name: qlcsv-db (or your choice)
Plan: Free
```

### 3. Update Environment Variables

Go to your Web Service → Environment tab:

```bash
DATABASE_URL=<copy-from-new-postgres-database>
JWT_SECRET_KEY=<generate-a-secure-32+-character-key>
```

Optional (for email features):

```bash
AppUrl=https://your-app.onrender.com
Smtp__Host=smtp.gmail.com
Smtp__Port=587
Smtp__Username=your-email@gmail.com
Smtp__Password=your-app-password
```

### 4. Deploy

```bash
git add .
git commit -m "Fresh database migration - production ready"
git push origin main
```

### 5. Verify Deployment

Check Render logs for:

```
✅ Database migrations applied successfully!
Now listening on: http://0.0.0.0:XXXX
```

## 📋 What's Included in Migration

The `InitialCreate` migration creates:

**Tables:**

- ✅ users (with email verification & password reset)
- ✅ alumni_profiles
- ✅ faculties
- ✅ majors
- ✅ batches
- ✅ alumni_batches (many-to-many)
- ✅ events
- ✅ event_registrations

**Features:**

- ✅ Email verification system
- ✅ Password reset functionality
- ✅ Pagination on all list endpoints
- ✅ Race condition protection for event registration
- ✅ Request size limits (10MB)

## 🔧 Local Testing (Optional)

```bash
# Drop local database
dotnet ef database drop --force

# Apply migration
dotnet ef database update

# Run app
dotnet run
```

Visit: http://localhost:5083/swagger

## 📝 API Quick Reference

**Authentication:**

- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - Login
- POST `/api/auth/verify-email` - Verify email
- POST `/api/auth/forgot-password` - Request password reset
- POST `/api/auth/reset-password` - Reset password
- GET `/api/auth/me` - Get current user

**All list endpoints support pagination:**

```
GET /api/users?pageNumber=1&pageSize=20
GET /api/alumni?pageNumber=1&pageSize=20
GET /api/events?pageNumber=1&pageSize=20
```

## ⚠️ Important Notes

1. **First Deployment:** App will automatically create all tables
2. **Subsequent Deployments:** Migrations apply automatically
3. **If Migration Fails:** App continues with warning (check logs)
4. **Email Without SMTP:** Features work but emails logged only

## 🎉 Ready!

Your project is clean, migration is ready, and you're good to deploy!

```bash
# Quick deploy
git add . && git commit -m "Production ready" && git push origin main
```
