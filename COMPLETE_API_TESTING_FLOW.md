# 🧪 Complete API Testing Flow - QLCSV Alumni Management System

> **Total: 43 API Endpoints** | Test locally at `http://localhost:5083/swagger`

## 📋 Quick Navigation

- [Setup & Prerequisites](#setup--prerequisites)
- [API Endpoints Summary](#api-endpoints-summary)
- [Complete Testing Flow](#complete-testing-flow)
- [Testing Checklist](#testing-checklist)

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
cd d:/Workspace/SchoolProject/winform/bai3/QLCSV/QLCSV
dotnet run
```

✅ Open Swagger UI: **http://localhost:5083/swagger**

---

## 📊 API Endpoints Summary

### **AuthController** - `/api/auth` (9 endpoints)

| #   | Method | Endpoint               | Auth    | Description                                |
| --- | ------ | ---------------------- | ------- | ------------------------------------------ |
| 1   | POST   | `/register`            | Public  | Register new user account                  |
| 2   | POST   | `/login`               | Public  | Login with email/password                  |
| 3   | POST   | `/complete-profile`    | 🔒 User | Complete alumni profile after registration |
| 4   | GET    | `/me`                  | 🔒 User | Get current user's profile information     |
| 5   | POST   | `/verify-email`        | Public  | Verify email with token                    |
| 6   | POST   | `/resend-verification` | Public  | Resend verification email                  |
| 7   | POST   | `/forgot-password`     | Public  | Request password reset link                |
| 8   | POST   | `/reset-password`      | Public  | Reset password with token                  |
| 9   | POST   | `/logout`              | 🔒 User | Logout (client-side token deletion)        |

### **AlumniController** - `/api/alumni` (5 endpoints)

| #   | Method | Endpoint      | Auth               | Description                                 |
| --- | ------ | ------------- | ------------------ | ------------------------------------------- |
| 10  | GET    | `/`           | Public             | Get paginated alumni list (public profiles) |
| 11  | GET    | `/{id}`       | Public/Conditional | Get alumni profile by ID                    |
| 12  | GET    | `/me`         | 🎓 Alumni/Admin    | Get my alumni profile                       |
| 13  | PUT    | `/me`         | 🎓 Alumni/Admin    | Update my alumni profile                    |
| 14  | PUT    | `/me/privacy` | 🎓 Alumni/Admin    | Toggle profile visibility                   |

### **EventsController** - `/api/events` (10 endpoints)

| #   | Method | Endpoint              | Auth            | Description                         |
| --- | ------ | --------------------- | --------------- | ----------------------------------- |
| 15  | GET    | `/`                   | Public          | Get paginated events list           |
| 16  | GET    | `/{id}`               | Public          | Get event details by ID             |
| 17  | POST   | `/`                   | 🔐 Admin        | Create new event                    |
| 18  | PUT    | `/{id}`               | 🔐 Admin        | Update event                        |
| 19  | DELETE | `/{id}`               | 🔐 Admin        | Delete event                        |
| 20  | POST   | `/{id}/register`      | 🎓 Alumni/Admin | Register for event                  |
| 21  | POST   | `/{id}/cancel`        | 🎓 Alumni/Admin | Cancel event registration           |
| 22  | GET    | `/my-registrations`   | 🎓 Alumni/Admin | Get my event registrations          |
| 23  | GET    | `/{id}/registrations` | 🔐 Admin        | Get event registrations (paginated) |

### **UsersController** - `/api/users` (5 endpoints)

| #   | Method | Endpoint       | Auth     | Description                 |
| --- | ------ | -------------- | -------- | --------------------------- |
| 24  | GET    | `/`            | 🔐 Admin | Get paginated users list    |
| 25  | GET    | `/{id}`        | 🔐 Admin | Get user details by ID      |
| 26  | PUT    | `/{id}/role`   | 🔐 Admin | Update user's role          |
| 27  | PUT    | `/{id}/status` | 🔐 Admin | Update user's active status |
| 28  | DELETE | `/{id}`        | 🔐 Admin | Delete user                 |

### **BatchesController** - `/api/batches` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description             |
| --- | ------ | -------- | -------- | ----------------------- |
| 29  | GET    | `/`      | Public   | Get batches list        |
| 30  | GET    | `/{id}`  | Public   | Get batch details by ID |
| 31  | POST   | `/`      | 🔐 Admin | Create new batch        |
| 32  | PUT    | `/{id}`  | 🔐 Admin | Update batch            |
| 33  | DELETE | `/{id}`  | 🔐 Admin | Delete batch            |

### **FacultiesController** - `/api/faculties` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description               |
| --- | ------ | -------- | -------- | ------------------------- |
| 34  | GET    | `/`      | Public   | Get faculties list        |
| 35  | GET    | `/{id}`  | Public   | Get faculty details by ID |
| 36  | POST   | `/`      | 🔐 Admin | Create new faculty        |
| 37  | PUT    | `/{id}`  | 🔐 Admin | Update faculty            |
| 38  | DELETE | `/{id}`  | 🔐 Admin | Delete faculty            |

### **MajorsController** - `/api/majors` (5 endpoints)

| #   | Method | Endpoint | Auth     | Description                         |
| --- | ------ | -------- | -------- | ----------------------------------- |
| 39  | GET    | `/`      | Public   | Get majors list (filter by faculty) |
| 40  | GET    | `/{id}`  | Public   | Get major details by ID             |
| 41  | POST   | `/`      | 🔐 Admin | Create new major                    |
| 42  | PUT    | `/{id}`  | 🔐 Admin | Update major                        |
| 43  | DELETE | `/{id}`  | 🔐 Admin | Delete major                        |

**Legend:**

- Public = No authentication required
- 🔒 User = Requires authentication (any logged-in user)
- 🎓 Alumni/Admin = Requires `alumni` or `admin` role (pending users blocked)
- 🔐 Admin = Requires `admin` role only

---

## 🧪 Complete Testing Flow

### **PHASE 1: Registration & Email Verification** (APIs #1, #5, #2, #4)

#### **Test 1: Register New User** ✅

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

✅ **Save:** `userId = 2`  
🔍 **Verify:** User created with `role = "pending"`, `emailVerified = false`

---

#### **Test 2: Get Verification Token** 📧

```sql
-- In pgAdmin4, execute:
SELECT "EmailVerificationToken"
FROM users
WHERE "Email" = 'test@gmail.com';
```

✅ **Copy:** The token (e.g., `a1b2c3d4e5f6...`)

---

#### **Test 3: Verify Email** ✅

```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "token": "a1b2c3d4e5f6..."
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Xác thực email thành công!"
}
```

🔍 **Verify:** `emailVerified = true` in database

---

#### **Test 4: Login** 🔐

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

**🔓 Authorize in Swagger:**

1. Click **Authorize** button (top right)
2. Paste token (without "Bearer " prefix)
3. Click **Authorize**

---

#### **Test 5: Get My Info** ✅

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

🔍 **Verify:** User is authenticated but profile not completed yet

---

### **PHASE 2: Admin Setup - Master Data** (APIs #36, #34, #41, #39, #31, #29)

#### **Test 6: Login as Admin** 🔐

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@qlcsv.com",
  "password": "Admin@123456"
}
```

✅ **Save:** Admin token  
🔓 Click **Authorize** again with admin token

---

#### **Test 7: Create Faculty** ✅

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

✅ **Save:** `facultyId = 1`

---

#### **Test 8: Get Faculties List** ✅

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

#### **Test 9: Create Major** ✅

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

✅ **Save:** `majorId = 1`

---

#### **Test 10: Get Majors by Faculty** ✅

```http
GET /api/majors?facultyId=1
```

**Expected Response:**

```json
[
  {
    "id": 1,
    "name": "Khoa học máy tính",
    "facultyId": 1,
    "facultyName": "Công nghệ thông tin",
    "description": "Ngành KHMT"
  }
]
```

---

#### **Test 11: Create Batch** ✅

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

**Expected Response:**

```json
{
  "id": 1,
  "name": "K18",
  "graduationYear": 2023,
  "description": "Khóa 18"
}
```

✅ **Save:** `batchId = 1`

---

#### **Test 12: Get Batches** ✅

```http
GET /api/batches?graduationYear=2023
```

**Expected Response:**

```json
[
  {
    "id": 1,
    "name": "K18",
    "graduationYear": 2023,
    "description": "Khóa 18"
  }
]
```

---

### **PHASE 3: Complete Profile (Pending User)** (API #3)

#### **Test 13: Re-login as Test User** 🔐

```http
POST /api/auth/login

{
  "email": "test@gmail.com",
  "password": "Test@123456"
}
```

🔓 Authorize with user token

---

#### **Test 14: Complete Profile** ✅

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

🔍 **Verify:** Profile created but user still has `role = "pending"`

---

### **PHASE 4: Test Pending User Restrictions** ⛔

#### **Test 15: Try to Access Alumni Profile (Should Fail)** ❌

```http
GET /api/alumni/me
Authorization: Bearer {user-token}
```

**Expected Response:**

```
403 Forbidden
```

**Reason:** User has `role = "pending"`, needs `alumni` or `admin` role

---

#### **Test 16: Try to Register for Event (Should Fail)** ❌

```http
POST /api/events/1/register
Authorization: Bearer {user-token}
```

**Expected Response:**

```
403 Forbidden
```

**Reason:** Pending users cannot register for events

---

#### **Test 17: Try to View My Registrations (Should Fail)** ❌

```http
GET /api/events/my-registrations
Authorization: Bearer {user-token}
```

**Expected Response:**

```
403 Forbidden
```

---

### **PHASE 5: Admin Approves User** (APIs #24, #25, #26)

#### **Test 18: Re-login as Admin** 🔐

🔓 Authorize with admin token

---

#### **Test 19: Get Users List** ✅

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
      "emailVerified": true,
      "createdAt": "2025-12-04T..."
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

#### **Test 20: Get User Details** ✅

```http
GET /api/users/2
Authorization: Bearer {admin-token}
```

**Expected Response:**

```json
{
  "id": 2,
  "fullName": "Test User",
  "email": "test@gmail.com",
  "role": "pending",
  "isActive": true,
  "emailVerified": true,
  "createdAt": "2025-12-04T...",
  "updatedAt": "2025-12-04T...",
  "profile": {
    "studentId": "2050001",
    "graduationYear": 2023,
    "facultyName": "Công nghệ thông tin",
    "majorName": "Khoa học máy tính"
  }
}
```

---

#### **Test 21: Approve User (Change Role to Alumni)** ✅

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

🎉 **User is now approved!** Role changed from `pending` → `alumni`

---

### **PHASE 6: Alumni Features Access** (APIs #12-14, #10)

#### **Test 22: Re-login as Approved User** 🔐

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
  "success": true,
  "message": "Đăng nhập thành công",
  "token": "eyJ...",
  "role": "alumni",
  "profileCompleted": true
}
```

✅ Notice `role = "alumni"` now!  
🔓 Authorize with new token

---

#### **Test 23: Get My Alumni Profile (Now Works!)** ✅

```http
GET /api/alumni/me
Authorization: Bearer {alumni-token}
```

**Expected Response:**

```json
{
  "id": 1,
  "userId": 2,
  "studentId": "2050001",
  "graduationYear": 2023,
  "facultyId": 1,
  "facultyName": "Công nghệ thông tin",
  "majorId": 1,
  "majorName": "Khoa học máy tính",
  "currentPosition": null,
  "company": null,
  "city": null,
  "country": "Việt Nam",
  "isPublic": false
}
```

🎉 **Success!** Alumni can now access their profile

---

#### **Test 24: Update Alumni Profile** ✅

```http
PUT /api/alumni/me
Authorization: Bearer {alumni-token}
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

**Expected Response:**

```json
{
  "id": 1,
  "userId": 2,
  "studentId": "2050001",
  "graduationYear": 2023,
  "facultyName": "Công nghệ thông tin",
  "majorName": "Khoa học máy tính",
  "currentPosition": "Software Engineer",
  "company": "Google",
  "city": "Hà Nội",
  "country": "Việt Nam",
  "phoneNumber": "0123456789",
  "linkedin": "https://linkedin.com/in/test",
  "github": "https://github.com/test",
  "isPublic": false
}
```

---

#### **Test 25: Toggle Privacy to Public** ✅

```http
PUT /api/alumni/me/privacy
Authorization: Bearer {alumni-token}
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

#### **Test 26: Get Alumni List (Public View)** ✅

```http
GET /api/alumni?pageNumber=1&pageSize=10&city=Hà Nội
```

**Expected Response:**

```json
{
  "items": [
    {
      "id": 1,
      "fullName": "Test User",
      "avatarUrl": null,
      "studentId": "2050001",
      "graduationYear": 2023,
      "facultyId": 1,
      "facultyName": "Công nghệ thông tin",
      "majorId": 1,
      "majorName": "Khoa học máy tính",
      "currentPosition": "Software Engineer",
      "company": "Google",
      "city": "Hà Nội",
      "country": "Việt Nam",
      "isPublic": true
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

✅ Profile now visible in public list

---

#### **Test 27: Get Alumni by ID** ✅

```http
GET /api/alumni/1
```

**Expected Response:**

```json
{
  "id": 1,
  "fullName": "Test User",
  "studentId": "2050001",
  "graduationYear": 2023,
  "facultyName": "Công nghệ thông tin",
  "majorName": "Khoa học máy tính",
  "currentPosition": "Software Engineer",
  "company": "Google",
  "city": "Hà Nội",
  "phoneNumber": "0123456789",
  "linkedin": "https://linkedin.com/in/test"
}
```

✅ Anyone can view because `isPublic = true`

---

### **PHASE 7: Event Management** (APIs #17, #15, #18)

#### **Test 28: Re-login as Admin** 🔐

🔓 Authorize with admin token

---

#### **Test 29: Create Offline Event** ✅

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

**Expected Response:**

```json
{
  "id": 1,
  "title": "Alumni Meetup 2025",
  "description": "Gặp gỡ cựu sinh viên",
  "eventDate": "2025-12-25T18:00:00Z",
  "location": "Hà Nội",
  "isOnline": false,
  "meetLink": null,
  "thumbnailUrl": null,
  "createdBy": 1,
  "createdByName": "System Administrator",
  "createdAt": "2025-12-04T...",
  "maxParticipants": 100,
  "registeredCount": 0,
  "myRegistrationStatus": null
}
```

✅ **Save:** `eventId = 1`

---

#### **Test 30: Create Online Event** ✅

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

✅ **Note:** `meetLink` is **required** for online events  
✅ **Save:** `onlineEventId = 2`

---

#### **Test 31: Get Events List** ✅

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
      "maxParticipants": 100,
      "registeredCount": 0
    },
    {
      "id": 2,
      "title": "Webinar: AI trong giáo dục",
      "eventDate": "2025-12-20T14:00:00Z",
      "isOnline": true,
      "meetLink": "https://zoom.us/j/123456789",
      "maxParticipants": 500,
      "registeredCount": 0
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

#### **Test 32: Get Event Details** ✅

```http
GET /api/events/1
```

**Expected Response:**

```json
{
  "id": 1,
  "title": "Alumni Meetup 2025",
  "description": "Gặp gỡ cựu sinh viên",
  "eventDate": "2025-12-25T18:00:00Z",
  "location": "Hà Nội",
  "isOnline": false,
  "meetLink": null,
  "createdBy": 1,
  "createdByName": "System Administrator",
  "maxParticipants": 100,
  "registeredCount": 0,
  "myRegistrationStatus": null
}
```

---

#### **Test 33: Update Event** ✅

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

**Expected Response:**

```json
{
  "id": 1,
  "title": "Alumni Meetup 2025 - UPDATED",
  "description": "Updated description",
  "eventDate": "2025-12-26T18:00:00Z",
  "location": "TP.HCM",
  "isOnline": false,
  "maxParticipants": 150,
  "registeredCount": 0
}
```

---

### **PHASE 8: Event Registration (Alumni Only)** (APIs #20-22)

#### **Test 34: Re-login as Alumni User** 🔐

🔓 Authorize with alumni token

---

#### **Test 35: Register for Event** ✅

```http
POST /api/events/1/register
Authorization: Bearer {alumni-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng ký sự kiện thành công",
  "status": "registered"
}
```

🎉 **Success!** Alumni can now register for events

---

#### **Test 36: Get My Event Registrations** ✅

```http
GET /api/events/my-registrations
Authorization: Bearer {alumni-token}
```

**Expected Response:**

```json
[
  {
    "eventId": 1,
    "eventTitle": "Alumni Meetup 2025 - UPDATED",
    "eventDate": "2025-12-26T18:00:00Z",
    "location": "TP.HCM",
    "isOnline": false,
    "meetLink": null,
    "status": "registered",
    "registeredAt": "2025-12-04T..."
  }
]
```

---

#### **Test 37: Cancel Event Registration** ✅

```http
POST /api/events/1/cancel
Authorization: Bearer {alumni-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Hủy đăng ký thành công"
}
```

---

#### **Test 38: Re-register for Event** ✅

```http
POST /api/events/1/register
Authorization: Bearer {alumni-token}
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

### **PHASE 9: Admin Views Event Registrations** (API #23)

#### **Test 39: Re-login as Admin** 🔐

🔓 Authorize with admin token

---

#### **Test 40: Get Event Registrations (Paginated)** ✅

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

✅ Admin can see all registrations with pagination

---

### **PHASE 10: User Status Management** (API #27)

#### **Test 41: Deactivate User** ⛔

```http
PUT /api/users/2/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isActive": false
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Cập nhật trạng thái thành công"
}
```

---

#### **Test 42: Try to Login as Deactivated User** ❌

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

🔍 Deactivated users cannot login

---

#### **Test 43: Re-activate User** ✅

```http
PUT /api/users/2/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isActive": true
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Cập nhật trạng thái thành công"
}
```

✅ User can login again

---

### **PHASE 11: Password Recovery** (APIs #6-8)

#### **Test 44: Test Resend Verification** ✅

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

#### **Test 45: Request Password Reset** ✅

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@gmail.com"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi"
}
```

---

#### **Test 46: Get Reset Token from Database** 📧

```sql
-- In pgAdmin4:
SELECT "PasswordResetToken"
FROM users
WHERE "Email" = 'test@gmail.com';
```

✅ **Copy:** Reset token

---

#### **Test 47: Reset Password** ✅

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "paste-reset-token-here",
  "newPassword": "NewPassword@123"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đặt lại mật khẩu thành công!"
}
```

---

#### **Test 48: Login with New Password** ✅

```http
POST /api/auth/login

{
  "email": "test@gmail.com",
  "password": "NewPassword@123"
}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "token": "eyJ...",
  "role": "alumni",
  "profileCompleted": true
}
```

✅ Password successfully changed!

---

### **PHASE 12: Logout** (API #9)

#### **Test 49: Logout** 🔓

```http
POST /api/auth/logout
Authorization: Bearer {your-token}
```

**Expected Response:**

```json
{
  "success": true,
  "message": "Đăng xuất thành công. Vui lòng xóa token ở phía client."
}
```

**Note:** JWT is stateless, so client must delete the token to complete logout.

---

### **PHASE 13: Additional CRUD Operations** (APIs #30, #32, #35, #37, #40, #42)

#### **Test 50: Get Batch Details** ✅

```http
GET /api/batches/1
```

---

#### **Test 51: Update Batch** ✅

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

#### **Test 52: Get Faculty Details** ✅

```http
GET /api/faculties/1
```

---

#### **Test 53: Update Faculty** ✅

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

#### **Test 54: Get Major Details** ✅

```http
GET /api/majors/1
```

---

#### **Test 55: Update Major** ✅

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

### **PHASE 14: Cleanup & Delete Operations** (APIs #19, #28, #33, #38, #43)

#### **Test 56: Create Second User for Deletion Test** ✅

1. Register: `test2@gmail.com`
2. Verify email
3. Login
4. Complete profile
5. Admin approves (change role to alumni)
6. Get user ID from admin panel

---

#### **Test 57: Delete Event** ✅

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

#### **Test 58: Delete User** ✅

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

#### **Test 59: Delete Batch** ✅

```http
DELETE /api/batches/1
Authorization: Bearer {admin-token}
```

---

#### **Test 60: Delete Major** ✅

```http
DELETE /api/majors/1
Authorization: Bearer {admin-token}
```

---

#### **Test 61: Delete Faculty** ✅

```http
DELETE /api/faculties/1
Authorization: Bearer {admin-token}
```

---

## ✅ Testing Checklist

### Phase 1: Registration & Verification ☐

- [ ] Test 1: Register user
- [ ] Test 2: Get verification token
- [ ] Test 3: Verify email
- [ ] Test 4: Login
- [ ] Test 5: Get my info

### Phase 2: Admin Setup ☐

- [ ] Test 6: Login as admin
- [ ] Test 7: Create faculty
- [ ] Test 8: Get faculties
- [ ] Test 9: Create major
- [ ] Test 10: Get majors
- [ ] Test 11: Create batch
- [ ] Test 12: Get batches

### Phase 3: Complete Profile (Pending) ☐

- [ ] Test 13: Re-login as user
- [ ] Test 14: Complete profile

### Phase 4: Pending Restrictions ☐

- [ ] Test 15: Try alumni profile (should fail)
- [ ] Test 16: Try event registration (should fail)
- [ ] Test 17: Try view registrations (should fail)

### Phase 5: Admin Approval ☐

- [ ] Test 18: Re-login as admin
- [ ] Test 19: Get users list
- [ ] Test 20: Get user details
- [ ] Test 21: Approve user (change to alumni)

### Phase 6: Alumni Features ☐

- [ ] Test 22: Re-login as alumni
- [ ] Test 23: Get my alumni profile
- [ ] Test 24: Update alumni profile
- [ ] Test 25: Toggle privacy to public
- [ ] Test 26: Get alumni list (public)
- [ ] Test 27: Get alumni by ID

### Phase 7: Event Management ☐

- [ ] Test 28: Re-login as admin
- [ ] Test 29: Create offline event
- [ ] Test 30: Create online event
- [ ] Test 31: Get events list
- [ ] Test 32: Get event details
- [ ] Test 33: Update event

### Phase 8: Event Registration ☐

- [ ] Test 34: Re-login as alumni
- [ ] Test 35: Register for event
- [ ] Test 36: Get my registrations
- [ ] Test 37: Cancel registration
- [ ] Test 38: Re-register

### Phase 9: Admin Event Management ☐

- [ ] Test 39: Re-login as admin
- [ ] Test 40: Get event registrations

### Phase 10: User Status ☐

- [ ] Test 41: Deactivate user
- [ ] Test 42: Try login (should fail)
- [ ] Test 43: Re-activate user

### Phase 11: Password Recovery ☐

- [ ] Test 44: Resend verification
- [ ] Test 45: Request password reset
- [ ] Test 46: Get reset token
- [ ] Test 47: Reset password
- [ ] Test 48: Login with new password

### Phase 12: Logout ☐

- [ ] Test 49: Logout

### Phase 13: Additional CRUD ☐

- [ ] Test 50: Get batch details
- [ ] Test 51: Update batch
- [ ] Test 52: Get faculty details
- [ ] Test 53: Update faculty
- [ ] Test 54: Get major details
- [ ] Test 55: Update major

### Phase 14: Cleanup ☐

- [ ] Test 56: Create second user
- [ ] Test 57: Delete event
- [ ] Test 58: Delete user
- [ ] Test 59: Delete batch
- [ ] Test 60: Delete major
- [ ] Test 61: Delete faculty

---

## 🎯 Quick Reference

### Default Credentials

```
Admin Account:
Email: admin@qlcsv.com
Password: Admin@123456
```

### User Roles & Access

| Role      | Can Access                               |
| --------- | ---------------------------------------- |
| `pending` | Only complete profile, view public data  |
| `alumni`  | All alumni features + event registration |
| `admin`   | Everything                               |

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

---

## 📝 Important Notes

1. **Pending vs Alumni:**

   - New users get `role = "pending"` after registration
   - Admin must manually approve by changing role to `"alumni"`
   - **Pending users CANNOT access alumni features or register for events**

2. **Email Verification:**

   - Tokens valid for 24 hours
   - Must verify email before login
   - Can resend verification if needed

3. **Password Reset:**

   - Tokens valid for 1 hour
   - Security: doesn't reveal if email exists

4. **JWT Tokens:**

   - Expire after 7 days (configurable)
   - Client-side logout (delete token)

5. **Rate Limiting:**

   - Auth endpoints: 10 requests per 10 seconds per IP

6. **Online Events:**

   - Must have `meetLink` when `isOnline = true`
   - Validation enforced at API level

7. **Profile Privacy:**

   - Default `isPublic = false`
   - Alumni can toggle visibility
   - Private profiles only visible to owner/admin

8. **Pagination:**
   - Default `pageSize = 10`
   - Max `pageSize = 200` for event registrations
   - Max `pageSize = 100` for alumni list

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

### 403 Forbidden Errors

```bash
# Check user role in database
# Pending users cannot access alumni features
# Admin must approve user first (change role to "alumni")
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
Total APIs: 43 | Total Tests: 61
