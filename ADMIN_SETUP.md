# Admin User Setup

## 🎉 Automatic Admin Creation

Your app now automatically creates an admin user on first deployment!

### Default Admin Credentials

**Email:** `admin@qlcsv.com`  
**Password:** `Admin@123456`  
**Role:** `admin`

⚠️ **IMPORTANT:** Change the password after first login!

---

## 🔧 Customizing Admin Credentials

### For Railway (Production):

Add these environment variables in Railway:

```
ADMIN_EMAIL=your-admin@example.com
ADMIN_PASSWORD=YourSecurePassword123!
ADMIN_NAME=Your Name
```

### For Local Development:

Edit `appsettings.Development.json`:

```json
"AdminUser": {
  "Email": "admin@qlcsv.com",
  "Password": "Admin@123456",
  "FullName": "System Administrator"
}
```

---

## 📝 How It Works

1. On app startup, after migrations run
2. Checks if any admin user exists
3. If not, creates one with the configured credentials
4. Admin is created with:
   - ✅ `EmailVerified = true` (no email verification needed)
   - ✅ `IsActive = true`
   - ✅ `Role = "admin"`

---

## 🚀 First Login

After deployment:

1. Go to your API: `https://your-app.railway.app/swagger`
2. Use `POST /api/auth/login`
3. Login with:
   ```json
   {
     "email": "admin@qlcsv.com",
     "password": "Admin@123456"
   }
   ```
4. Get your JWT token
5. **Change the password immediately!**

---

## 🔐 Security Notes

- The seeder only runs if NO admin exists
- Admin doesn't require email verification
- Default password is logged to console (only visible in deployment logs)
- **Always change default password in production!**

---

## 📊 Check Admin in Database

Railway → PostgreSQL → Data → Run:

```sql
SELECT id, email, "FullName", role, "EmailVerified"
FROM users
WHERE role = 'admin';
```
