# 🧪 Complete API Testing Guide - QLCSV Alumni Management System

> **Total: 42 API Endpoints** | Test locally at `http://localhost:5083/swagger`

## 📋 Table of Contents

- [Setup & Prerequisites](#setup--prerequisites)
- [API Endpoints Summary](#api-endpoints-summary)
- [Testing Flow (Phase 1-8)](#testing-flow)
- [Quick Reference](#quick-reference)

---

## 🚀 Setup & Prerequisites

### 1. Database Setup (PostgreSQL)

```bash
# Open pgAdmin4
# Create new database: qlcsv
# Owner: postgres (or your username)
```

### 2. Environment Configuration

Update `.env` file:

```env
JWT_SECRET_KEY=7b2f0fcc45dbc5d970e4cdd6b873b676
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=qlcsv;Username=postgres;Password=YOUR_PASSWORD
AppUrl=http://localhost:5083

# Optional: For email testing
Resend__ApiKey=re_your_api_key
Resend__FromEmail=onboarding@resend.dev
Resend__FromName=QLCSV Alumni System
```

### 3. Start Application

```bash
cd /d/Workspace/SchoolProject/winform/bai3/QLCSV/QLCSV
dotnet run
```

✅ Open Swagger UI: **http://localhost:5083/swagger**

---

## 📊 API Endpoints Summary

### **AuthController** - `/api/auth` (8 endpoints)

| #   | Method | Endpoint               | Auth    | Description                |
| --- | ------ | ---------------------- | ------- | -------------------------- |
| 1   | POST   | `/register`            | Public  | Register new user account  |
| 2   | POST   | `/login`               | Public  | Login with email/password  |
| 3   | POST   | `/complete-profile`    | 🔒 User | Complete alumni profile    |
| 4   | GET    | `/me`                  | 🔒 User | Get current user's profile |
| 5   | POST   | `/verify-email`        | Public  | Verify email with token    |
| 6   | POST   | `/resend-verification` | Public  | Resend verification email  |
| 7   | POST   | `/forgot-password`     | Public  | Request password reset     |
| 8   | POST   | `/reset-password`      | Public  | Reset password with token  |

### **AlumniController** - `/api/alumni` (5 endpoints)

| #   | Method | Endpoint      | Auth               | Description               |
| --- | ------ | ------------- | ------------------ | ------------------------- |
| 9   | GET    | `/`           | Public             | Get paginated alumni list |
| 10  | GET    | `/{id}`       | Public/Conditional | Get alumni profile by ID  |
| 11  | GET    | `/me`         | 🔒 User            | Get my alumni profile     |
| 12  | PUT    | `/me`         | 🔒 User            | Update my alumni profile  |
| 13  | PUT    | `/me/privacy` | 🔒 User            | Toggle profile visibility |

### **EventsController** - `/api/events` (10 endpoints)

| #   | Method | Endpoint              | Auth     | Description                         |
| --- | ------ | --------------------- | -------- | ----------------------------------- |
| 14  | GET    | `/`                   | Public   | Get paginated events list           |
| 15  | GET    | `/{id}`               | Public   | Get event details by ID             |
| 16  | POST   | `/`                   | 🔐 Admin | Create new event                    |
| 17  | PUT    | `/{id}`               | 🔐 Admin | Update event                        |
| 18  | DELETE | `/{id}`               | 🔐 Admin | Delete event                        |
| 19  | POST   | `/{id}/register`      | 🔒 User  | Register for event                  |
| 20  | POST   | `/{id}/cancel`        | 🔒 User  | Cancel event registration           |
| 21  | GET    | `/my-registrations`   | 🔒 User  | Get my event registrations          |
| 22  | GET    | `/{id}/registrations` | 🔐 Admin | Get event registrations (paginated) |

### **UsersController** - `/api/users` (5 endpoints)

| #   | Method | Endpoint       | Auth     | Description              |
| --- | ------ | -------------- | -------- | ------------------------ |
| 23  | GET    | `/`            | 🔐 Admin | Get paginated users list |
| 24  | GET    | `/{id}`        | 🔐 Admin | Get user details by ID   |
| 25  | PUT    | `/{id}/role`   | 🔐 Admin | Update user's role       |
| 26  | PUT    | `/{id}/status` | 🔐 Admin | Update user's status     |
| 27  | DELETE | `/{id}`        | 🔐 Admin | Delete user              |

### **BatchesController** - `/api/batches` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description             |
| --- | ------ | -------- | -------- | ----------------------- |
| 28  | GET    | `/`      | Public   | Get batches list        |
| 29  | GET    | `/{id}`  | Public   | Get batch details by ID |
| 30  | POST   | `/`      | 🔐 Admin | Create new batch        |
| 31  | PUT    | `/{id}`  | 🔐 Admin | Update batch            |
| 32  | DELETE | `/{id}`  | 🔐 Admin | Delete batch            |

### **FacultiesController** - `/api/faculties` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description               |
| --- | ------ | -------- | -------- | ------------------------- |
| 33  | GET    | `/`      | Public   | Get faculties list        |
| 34  | GET    | `/{id}`  | Public   | Get faculty details by ID |
| 35  | POST   | `/`      | 🔐 Admin | Create new faculty        |
| 36  | PUT    | `/{id}`  | 🔐 Admin | Update faculty            |
| 37  | DELETE | `/{id}`  | 🔐 Admin | Delete faculty            |

### **MajorsController** - `/api/majors` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description                         |
| --- | ------ | -------- | -------- | ----------------------------------- |
| 38  | GET    | `/`      | Public   | Get majors list (filter by faculty) |
| 39  | GET    | `/{id}`  | Public   | Get major details by ID             |
| 40  | POST   | `/`      | 🔐 Admin | Create new major                    |
| 41  | PUT    | `/{id}`  | 🔐 Admin | Update major                        |
| 42  | DELETE | `/{id}`  | 🔐 Admin | Delete major                        |

**Legend:**

- Public = No authentication required
- 🔒 User = Requires authentication (any logged-in user)
- 🔐 Admin = Requires admin role

---

## 🧪 Testing Flow

### **PHASE 1: Authentication & Setup** (APIs 1-8)

#### **Step 1: Register New User** (API #1)

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "test@gmail.com",
  "fullName": "Test User",
  "password": "Test@123456"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
  "token": null,
  "userId": 2
}
```

✅ **Save:** `userId` for later use

---

#### **Step 2: Get Verification Token from Database**

```sql
-- In pgAdmin4, run this query:
SELECT "EmailVerificationToken"
FROM users
WHERE "Email" = 'test@gmail.com';
```

✅ **Copy:** The verification token

---

#### **Step 3: Verify Email** (API #5)

```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "token": "paste-your-token-here"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Xác thực email thành công!"
}
```

---

#### **Step 4: Login** (API #2)

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@gmail.com",
  "password": "Test@123456"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "pending",
  "profileCompleted": false
}
```

✅ **Save:** Copy the entire `token` value

---

#### **Step 5: Authorize in Swagger**

1. Click the **🔓 Authorize** button at the top
2. Paste your token (without "Bearer " prefix)
3. Click **Authorize**
4. Close the dialog

All subsequent requests will include your token automatically! 🎉

---

#### **Step 6: Get My Info** (API #4)

```http
GET /api/auth/me
Authorization: Bearer {your-token}
```

**Expected Response:**

```json
{
  "id": 2,
  "fullName": "Test User",
  "email": "test@gmail.com",
  "role": "pending",
  "emailVerified": true,
  "profile": null
}
```

---

### **PHASE 2: Setup Master Data (Admin Required)** (APIs 28, 30, 33, 35, 38, 40)

#### **Step 7: Login as Admin**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@qlcsv.com",
  "password": "Admin@123456"
}
```

✅ **Save:** Admin token, then click **Authorize** again with admin token

---

#### **Step 8: Create Faculty** (API #35)

```http
POST /api/faculties
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Công nghệ thông tin",
  "description": "Khoa CNTT"
}
```

**Expected Response:**

```json
{
  "id": 1,
  "name": "Công nghệ thông tin",
  "description": "Khoa CNTT",
  "createdAt": "2025-12-04T..."
}
```

✅ **Save:** `facultyId` = 1

---

#### **Step 9: Get Faculties List** (API #33)

```http
GET /api/faculties
```

**Expected Response:**

```json
[
  {
    "id": 1,
    "name": "Công nghệ thông tin",
    "description": "Khoa CNTT",
    "createdAt": "2025-12-04T..."
  }
]
```

---

#### **Step 10: Create Major** (API #40)

```http
POST /api/majors
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Khoa học máy tính",
  "facultyId": 1,
  "description": "Ngành KHMT"
}
```

**Expected Response:**

```json
{
  "id": 1,
  "name": "Khoa học máy tính",
  "facultyId": 1,
  "facultyName": "Công nghệ thông tin",
  "description": "Ngành KHMT"
}
```

✅ **Save:** `majorId` = 1

---

#### **Step 11: Get Majors by Faculty** (API #38)

```http
GET /api/majors?facultyId=1
```

---

#### **Step 12: Create Batch** (API #30)

```http
POST /api/batches
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "K18",
  "graduationYear": 2023,
  "description": "Khóa 18"
}
```

✅ **Save:** `batchId`

---

#### **Step 13: Get Batches** (API #28)

```http
GET /api/batches?graduationYear=2023
```

---

### **PHASE 3: Complete Alumni Profile** (APIs 3, 9-13)

#### **Step 14: Re-login as Test User**

```http
POST /api/auth/login

{
  "email": "test@gmail.com",
  "password": "Test@123456"
}
```

Click **Authorize** with user token

---

#### **Step 15: Complete Profile** (API #3)

```http
POST /api/auth/complete-profile
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "studentId": "2050001",
  "graduationYear": 2023,
  "facultyId": 1,
  "majorId": 1
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Hoàn thiện hồ sơ thành công! Role vẫn là 'pending' cho đến khi admin duyệt."
}
```

---

#### **Step 16: Get My Alumni Profile** (API #11)

```http
GET /api/alumni/me
Authorization: Bearer {user-token}
```

**Expected Response:**

```json
{
  "id": 1,
  "userId": 2,
  "studentId": "2050001",
  "graduationYear": 2023,
  "facultyName": "Công nghệ thông tin",
  "majorName": "Khoa học máy tính",
  "currentPosition": null,
  "company": null,
  "city": null,
  "country": "Việt Nam",
  "isPublic": false
}
```

---

#### **Step 17: Update Alumni Profile** (API #12)

```http
PUT /api/alumni/me
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "currentPosition": "Software Engineer",
  "company": "Google",
  "city": "Hà Nội",
  "country": "Việt Nam",
  "phoneNumber": "0123456789",
  "linkedin": "https://linkedin.com/in/test",
  "github": "https://github.com/test"
}
```

---

#### **Step 18: Toggle Privacy** (API #13)

```http
PUT /api/alumni/me/privacy
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "isPublic": true
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Cập nhật quyền riêng tư thành công",
  "isPublic": true
}
```

---

#### **Step 19: Get Alumni List** (API #9)

```http
GET /api/alumni?pageNumber=1&pageSize=10&city=Hà Nội
```

✅ Verify your profile appears in the list

---

#### **Step 20: Get Alumni by ID** (API #10)

```http
GET /api/alumni/2
```

✅ Can see full details because profile is public

---

### **PHASE 4: User Management (Admin Only)** (APIs 23-27)

#### **Step 21: Re-login as Admin**

Click **Authorize** with admin token

---

#### **Step 22: Get Users List** (API #23)

```http
GET /api/users?pageNumber=1&pageSize=10&search=test
Authorization: Bearer {admin-token}
```

**Expected Response:**

```json
{
  "items": [
    {
      "id": 2,
      "fullName": "Test User",
      "email": "test@gmail.com",
      "role": "pending",
      "isActive": true,
      "emailVerified": true
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

✅ **Save:** User ID = 2

---

#### **Step 23: Get User Details** (API #24)

```http
GET /api/users/2
Authorization: Bearer {admin-token}
```

---

#### **Step 24: Update User Role** (API #25)

```http
PUT /api/users/2/role
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "role": "alumni"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Cập nhật role thành công"
}
```

---

#### **Step 25: Update User Status** (API #26)

```http
PUT /api/users/2/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isActive": false
}
```

---

#### **Step 26: Test Login Blocked**

```http
POST /api/auth/login

{
  "email": "test@gmail.com",
  "password": "Test@123456"
}
```

**Expected Response:**

```json
{
  "success": false,
  "message": "Tài khoản đã bị khóa"
}
```

---

#### **Step 27: Re-activate User** (API #26)

```http
PUT /api/users/2/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isActive": true
}
```

---

### **PHASE 5: Events Management** (APIs 14-22)

#### **Step 28: Create Offline Event** (API #16)

```http
POST /api/events
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "title": "Alumni Meetup 2025",
  "description": "Gặp gỡ cựu sinh viên",
  "eventDate": "2025-12-25T18:00:00Z",
  "location": "Hà Nội",
  "isOnline": false,
  "maxParticipants": 100
}
```

✅ **Save:** `eventId`

---

#### **Step 29: Create Online Event** (API #16)

```http
POST /api/events
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "title": "Webinar: AI trong giáo dục",
  "description": "Hội thảo trực tuyến",
  "eventDate": "2025-12-20T14:00:00Z",
  "isOnline": true,
  "meetLink": "https://zoom.us/j/123456789",
  "maxParticipants": 500
}
```

✅ Note: `meetLink` is **required** for online events

---

#### **Step 30: Get Events List** (API #14)

```http
GET /api/events?pageNumber=1&pageSize=10&keyword=alumni
```

**Expected Response:**

```json
{
  "items": [
    {
      "id": 1,
      "title": "Alumni Meetup 2025",
      "description": "Gặp gỡ cựu sinh viên",
      "eventDate": "2025-12-25T18:00:00Z",
      "location": "Hà Nội",
      "isOnline": false,
      "maxParticipants": 100
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

#### **Step 31: Get Event Details** (API #15)

```http
GET /api/events/1
```

---

#### **Step 32: Update Event** (API #17)

```http
PUT /api/events/1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "title": "Alumni Meetup 2025 - UPDATED",
  "description": "Updated description",
  "eventDate": "2025-12-26T18:00:00Z",
  "location": "TP.HCM",
  "isOnline": false,
  "maxParticipants": 150
}
```

---

#### **Step 33: Re-login as Test User**

Authorize with user token

---

#### **Step 34: Register for Event** (API #19)

```http
POST /api/events/1/register
Authorization: Bearer {user-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng ký sự kiện thành công",
  "status": "registered"
}
```

---

#### **Step 35: Get My Registrations** (API #21)

```http
GET /api/events/my-registrations
Authorization: Bearer {user-token}
```

**Expected Response:**

```json
[
  {
    "eventId": 1,
    "eventTitle": "Alumni Meetup 2025 - UPDATED",
    "eventDate": "2025-12-26T18:00:00Z",
    "location": "TP.HCM",
    "status": "registered",
    "registeredAt": "2025-12-04T..."
  }
]
```

---

#### **Step 36: Cancel Registration** (API #20)

```http
POST /api/events/1/cancel
Authorization: Bearer {user-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Hủy đăng ký thành công"
}
```

---

#### **Step 37: Re-register** (API #19)

```http
POST /api/events/1/register
Authorization: Bearer {user-token}
```

---

#### **Step 38: Re-login as Admin**

Authorize with admin token

---

#### **Step 39: Get Event Registrations (Paginated)** (API #22)

```http
GET /api/events/1/registrations?pageNumber=1&pageSize=50
Authorization: Bearer {admin-token}
```

**Expected Response:**

```json
{
  "items": [
    {
      "userId": 2,
      "fullName": "Test User",
      "email": "test@gmail.com",
      "registeredAt": "2025-12-04T...",
      "status": "registered"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 50
}
```

---

### **PHASE 6: Additional CRUD Operations** (APIs 29, 31-32, 34, 36-37, 39, 41-42)

#### **Step 40: Get Batch Details** (API #29)

```http
GET /api/batches/1
```

---

#### **Step 41: Update Batch** (API #31)

```http
PUT /api/batches/1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "K18 - Updated",
  "graduationYear": 2023,
  "description": "Khóa 18 cập nhật"
}
```

---

#### **Step 42: Get Faculty Details** (API #34)

```http
GET /api/faculties/1
```

---

#### **Step 43: Update Faculty** (API #36)

```http
PUT /api/faculties/1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Công nghệ thông tin - Updated",
  "description": "Updated description"
}
```

---

#### **Step 44: Get Major Details** (API #39)

```http
GET /api/majors/1
```

---

#### **Step 45: Update Major** (API #41)

```http
PUT /api/majors/1
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Khoa học máy tính - Updated",
  "facultyId": 1,
  "description": "Updated description"
}
```

---

### **PHASE 7: Password Recovery** (APIs 6-8)

#### **Step 46: Logout & Test Resend Verification** (API #6)

```http
POST /api/auth/resend-verification
Content-Type: application/json

{
  "email": "test@gmail.com"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Email đã được xác thực rồi"
}
```

---

#### **Step 47: Request Password Reset** (API #7)

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@gmail.com"
}
```

---

#### **Step 48: Get Reset Token from Database**

```sql
-- In pgAdmin4:
SELECT "PasswordResetToken"
FROM users
WHERE "Email" = 'test@gmail.com';
```

✅ **Copy:** Reset token

---

#### **Step 49: Reset Password** (API #8)

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "paste-reset-token-here",
  "newPassword": "NewPassword@123"
}
```

---

#### **Step 50: Login with New Password** (API #2)

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@gmail.com",
  "password": "NewPassword@123"
}
```

✅ Should succeed!

---

### **PHASE 8: Cleanup & Delete Operations** (APIs 18, 27, 32, 37, 42)

#### **Step 51: Re-login as Admin**

Authorize with admin token

---

#### **Step 52: Delete Event** (API #18)

```http
DELETE /api/events/1
Authorization: Bearer {admin-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Xóa sự kiện thành công"
}
```

---

#### **Step 53: Create Second User for Deletion Test**

1. Register: `test2@gmail.com`
2. Verify email
3. Login
4. Complete profile
5. Get user ID from admin panel

---

#### **Step 54: Delete User** (API #27)

```http
DELETE /api/users/{test2-user-id}
Authorization: Bearer {admin-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Xóa user thành công"
}
```

---

#### **Step 55: Delete Batch** (API #32)

```http
DELETE /api/batches/1
Authorization: Bearer {admin-token}
```

---

#### **Step 56: Delete Major** (API #42)

```http
DELETE /api/majors/1
Authorization: Bearer {admin-token}
```

---

#### **Step 57: Delete Faculty** (API #37)

```http
DELETE /api/faculties/1
Authorization: Bearer {admin-token}
```

---

## ✅ Testing Checklist

### Phase 1: Authentication & Setup ☐

- [ ] API #1: Register user
- [ ] API #5: Verify email
- [ ] API #2: Login
- [ ] API #4: Get my info
- [ ] API #6: Resend verification
- [ ] API #7: Forgot password
- [ ] API #8: Reset password

### Phase 2: Master Data (Admin) ☐

- [ ] API #35: Create faculty
- [ ] API #33: Get faculties
- [ ] API #40: Create major
- [ ] API #38: Get majors
- [ ] API #30: Create batch
- [ ] API #28: Get batches

### Phase 3: Alumni Profile ☐

- [ ] API #3: Complete profile
- [ ] API #11: Get my alumni profile
- [ ] API #12: Update alumni profile
- [ ] API #13: Toggle privacy
- [ ] API #9: Get alumni list
- [ ] API #10: Get alumni by ID

### Phase 4: User Management (Admin) ☐

- [ ] API #23: Get users list
- [ ] API #24: Get user details
- [ ] API #25: Update user role
- [ ] API #26: Update user status
- [ ] API #27: Delete user

### Phase 5: Events ☐

- [ ] API #16: Create event (offline)
- [ ] API #16: Create event (online)
- [ ] API #14: Get events list
- [ ] API #15: Get event details
- [ ] API #17: Update event
- [ ] API #19: Register for event
- [ ] API #21: Get my registrations
- [ ] API #20: Cancel registration
- [ ] API #22: Get event registrations (admin)
- [ ] API #18: Delete event

### Phase 6: Additional CRUD ☐

- [ ] API #29: Get batch details
- [ ] API #31: Update batch
- [ ] API #34: Get faculty details
- [ ] API #36: Update faculty
- [ ] API #39: Get major details
- [ ] API #41: Update major

### Phase 7: Cleanup ☐

- [ ] API #32: Delete batch
- [ ] API #42: Delete major
- [ ] API #37: Delete faculty

---

## 🎯 Quick Reference

### Default Credentials

```
Admin Account:
Email: admin@qlcsv.com
Password: Admin@123456
```

### Common Query Parameters

```
Pagination:
?pageNumber=1&pageSize=10

Search/Filter:
?search=keyword
?city=Hà Nội
?facultyId=1
?graduationYear=2023
?keyword=alumni
```

### Password Requirements

- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character

### URL Validation

- Must start with `http://` or `https://`
- Valid for: LinkedIn, GitHub, MeetLink, ThumbnailUrl

### User Roles

- `pending` - Default after registration, waiting for admin approval
- `alumni` - Approved alumni with full access
- `admin` - Administrator with all permissions

---

## 📝 Notes

1. **Email Verification**: Tokens are valid for 24 hours
2. **Password Reset**: Tokens are valid for 1 hour
3. **JWT Tokens**: Expire after 7 days (configurable)
4. **Rate Limiting**: Auth endpoints limited to 10 requests per 10 seconds per IP
5. **Online Events**: Must have `meetLink` when `isOnline = true`
6. **Profile Privacy**: Default `isPublic = false`, must be toggled by user
7. **Pagination**: Max `pageSize = 200` for event registrations

---

## 🐛 Troubleshooting

### Database Connection Issues

```bash
# Check PostgreSQL is running
# Verify database 'qlcsv' exists in pgAdmin4
# Check .env ConnectionStrings__DefaultConnection
```

### JWT Token Issues

```bash
# Ensure JWT_SECRET_KEY is at least 32 characters
# Re-login to get fresh token
# Click Authorize button in Swagger with new token
```

### Migration Issues

```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

---

**Happy Testing! 🚀**

Generated on: December 4, 2025
