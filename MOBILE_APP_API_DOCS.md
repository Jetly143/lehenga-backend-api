# 📱 LehengaVerse — Complete Mobile App Developer API Specification

> **Official Backend API Documentation for iOS & Android App Developers**  
> Covers all mobile user journeys: **Bride / Outfit Renter (Customer)**, **P2P Lady Closet Partner**, and **B2B Rental Boutique**.  
> Backend: **.NET 8 Web API**, **PostgreSQL 16**, **JWT Bearer Authentication**, **BCrypt Security**.

---

## 🌐 1. Server Endpoints & Network Setup

| Environment | Base URL | Swagger OpenAPI Reference |
| :--- | :--- | :--- |
| **Android Emulator** | `http://10.0.2.2:5000` | `http://10.0.2.2:5000/swagger` |
| **iOS Simulator / Local** | `http://localhost:5000` | `http://localhost:5000/swagger` |
| **Live Staging Server** | `http://137.23.39.72` | `http://137.23.39.72/swagger` |

### Required Headers
```http
Content-Type: application/json
Accept: application/json
```

### Protected Endpoints Authorization Header
```http
Authorization: Bearer <YOUR_JWT_TOKEN>
```

---

## 👥 2. User Roles & Personas Supported in Mobile App

| Role Key | App Persona | Description |
| :--- | :--- | :--- |
| `"CUSTOMER"` | 👰 **Bride / Renter** | Browse & rent designer lehengas, custom fitting, doorstep delivery. |
| `"INDIVIDUAL_OWNER"` | 👗 **P2P Lady Closet** | Monetize personal bridal wardrobe, earn 70% rental revenue. |
| `"SHOP_OWNER"` | 🏪 **Rental Boutique** | Commercial rental store with multi-stock & GST registration. |

> ⚠️ **Security Policy**: Public self-registration for `"ADMIN"` is **strictly disabled** and rejected with `400 Bad Request`. Admin accounts are internal only.

---

## 📑 3. Master API Quick Reference Table

### 🔐 A. Authentication & Account Management
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 1 | `POST` | `/api/auth/register` | ❌ No | All Users | Register Customer, P2P Lady, or Boutique. Returns JWT. |
| 2 | `POST` | `/api/auth/login` | ❌ No | All Users | Sign in with email & password. Returns JWT + Profile. |
| 3 | `POST` | `/api/auth/google` | ❌ No | All Users | Google Social Sign-In with selected role. |
| 4 | `GET` | `/api/auth/me` | 🔒 Yes | All Users | Get current user profile & KYC verification status. |
| 5 | `PUT` | `/api/auth/profile/{id}` | 🔒 Yes | All Users | Update Name, Phone, City, Address. |
| 6 | `POST` | `/api/auth/forgot-password` | ❌ No | All Users | Request password reset token. |
| 7 | `POST` | `/api/auth/reset-password` | ❌ No | All Users | Set new password using token. |
| 8 | `POST` | `/api/auth/change-password` | 🔒 Yes | All Users | Change password with current password verification. |
| 9 | `POST` | `/api/auth/logout` | ❌ No | All Users | Logout (client clears stored token). |

### 🪪 B. Partner KYC & Identity Verification
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 10 | `POST` | `/api/auth/submit-kyc` | 🔒 Yes | P2P & B2B | Submit Aadhar / 5s Video Selfie (Lady) or GSTIN (Boutique). |
| 11 | `GET` | `/api/auth/me` | 🔒 Yes | P2P & B2B | Inspect `kycStatus` (`NOT_SUBMITTED`, `PENDING`, `VERIFIED`). |

### 👗 C. Lehenga Catalog & Search (Bride Experience)
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 12 | `GET` | `/api/listings` | ❌ No | Bride / Customer | Feed with filters (`category`, `supplyType`, `status=ACTIVE`, `search`). |
| 13 | `GET` | `/api/listings/{id}` | ❌ No | Bride / Customer | Detailed view with 4-angle photos, video, measurements, pricing. |

### 💎 D. Partner Wardrobe Management (P2P & Boutique)
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 14 | `GET` | `/api/listings/owner/{ownerId}` | 🔒 Yes | P2P & B2B | Fetch all outfits listed by this owner with live status. |
| 15 | `POST` | `/api/listings` | 🔒 Yes | P2P & B2B | List new lehenga (locked until `kycStatus == "VERIFIED"`). |
| 16 | `DELETE` | `/api/listings/{id}` | 🔒 Yes | P2P & B2B | Remove/deactivate outfit from rental catalog. |

### 📦 E. Rental Bookings & Orders
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 17 | `POST` | `/api/bookings` | 🔒 Yes | Bride / Customer | Place rental booking with dates, measurements & deposit. |
| 18 | `GET` | `/api/bookings/{id}` | 🔒 Yes | All | Get order details with tailor & logistics tracking. |
| 19 | `GET` | `/api/bookings?userId={id}` | 🔒 Yes | Bride / Customer | My rental orders history and active reservations. |
| 20 | `PATCH` | `/api/bookings/{id}/status` | 🔒 Yes | Operations | Order status updates (`CONFIRMED`, `DELIVERED`, etc.). |

### 💳 F. Payments & Security Deposit Hold
| # | Method | Endpoint Path | Auth? | Persona | Description |
| :-: | :---: | :--- | :---: | :--- | :--- |
| 21 | `POST` | `/api/payments` | 🔒 Yes | Bride / Customer | Record payment (Rental + Deposit). Sets booking to `CONFIRMED`. |
| 22 | `GET` | `/api/payments?bookingId={id}` | 🔒 Yes | All | Payment receipts, deposit hold amount & transaction ID. |

---

## 🔍 4. Detailed Payload Specifications

---

### 1️⃣ Register User (`POST /api/auth/register`)

#### 📥 Bride / Customer Signup:
```json
{
  "name": "Simran Kaur",
  "email": "simran.test@gmail.com",
  "phone": "+919876543210",
  "password": "SecurePassword@123",
  "role": "CUSTOMER",
  "city": "Chandigarh"
}
```

#### 📥 P2P Lady Closet Partner Signup:
```json
{
  "name": "Pooja Verma",
  "email": "pooja.verma@gmail.com",
  "phone": "+919812345678",
  "password": "SecurePassword@123",
  "role": "INDIVIDUAL_OWNER",
  "city": "Jaipur"
}
```

#### 📥 Rental Boutique Store Signup:
```json
{
  "name": "Rajesh Singhania",
  "email": "rajesh@royalbanaras.com",
  "phone": "+919811223344",
  "password": "SecurePassword@123",
  "role": "SHOP_OWNER",
  "shopName": "Royal Banaras Studio",
  "city": "Varanasi"
}
```

#### 📤 Success Response (`200 OK`):
```json
{
  "message": "Registration successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "user": {
    "id": 2,
    "name": "Pooja Verma",
    "email": "pooja.verma@gmail.com",
    "phone": "+919812345678",
    "role": "INDIVIDUAL_OWNER",
    "kycStatus": "NOT_SUBMITTED",
    "isVerified": false,
    "city": "Jaipur",
    "createdAt": "2026-11-24T14:30:00Z"
  }
}
```

---

### 2️⃣ Partner KYC Verification (`POST /api/auth/submit-kyc`)
> **Requirement**: Partners cannot list outfits until identity is verified by Admin.

#### 📥 P2P Lady Submission (Aadhar + 5s Selfie Video):
```json
{
  "userId": 2,
  "aadharNumber": "5432-8765-1234",
  "aadharDocUrl": "https://storage.lehengaverse.com/kyc/aadhar_pooja.jpg",
  "selfieVideoUrl": "https://storage.lehengaverse.com/kyc/selfie_pooja.mp4",
  "city": "Jaipur"
}
```

#### 📥 Boutique Store Submission (GSTIN + Address):
```json
{
  "userId": 3,
  "shopName": "Royal Banaras Studio",
  "gstNumber": "09AAECR1234F1Z8",
  "address": "K-44/12 Vishwanath Gali, Central Market",
  "city": "Varanasi"
}
```

#### 📤 Success Response (`200 OK`):
```json
{
  "message": "KYC documents submitted successfully. Verification pending admin review.",
  "user": {
    "id": 2,
    "name": "Pooja Verma",
    "role": "INDIVIDUAL_OWNER",
    "kycStatus": "PENDING",
    "isVerified": false
  }
}
```

---

### 3️⃣ Browse Lehenga Catalog (`GET /api/listings`)

#### 📥 Query Parameters:
- `status`: Pass `"ACTIVE"` to fetch outfits available for rent.
- `category`: Filter by `"BRIDAL"`, `"SANGEET"`, `"MEHENDI"`, `"HALDI"`, `"RECEPTION"`.
- `supplyType`: Filter by `"ADMIN_OWN"`, `"B2B_SHOP"`, `"P2P_INDIVIDUAL"`.
- `search`: Keyword search matching title, fabric, or category.

**Request Example**:
`GET /api/listings?status=ACTIVE&category=BRIDAL`

#### 📤 Response (`200 OK`):
```json
[
  {
    "id": 1,
    "title": "Gold Floral Zardozi Bridal Ensemble",
    "description": "Hand-embroidered heavy bridal lehenga with zari border and dual dupattas.",
    "supplyType": "P2P_INDIVIDUAL",
    "category": "BRIDAL",
    "fabric": "Tissue Silk",
    "size": "M",
    "bustSize": 36,
    "waistSize": 30,
    "skirtLength": 42,
    "pricePerDay": 3500,
    "securityDeposit": 8750,
    "originalPrice": 52500,
    "status": "ACTIVE",
    "owner": {
      "id": 2,
      "name": "Simran Kaur",
      "city": "Chandigarh",
      "isVerified": true
    },
    "photos": [
      {
        "id": 101,
        "photoUrl": "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?auto=format&fit=crop&q=80&w=800",
        "photoType": "FRONT",
        "isCover": true
      },
      {
        "id": 102,
        "photoUrl": "https://images.unsplash.com/photo-1610030469983-98e550d6193c?auto=format&fit=crop&q=80&w=800",
        "photoType": "DETAIL",
        "isCover": false
      }
    ]
  }
]
```

---

### 4️⃣ Partner Lists Outfit for Rent (`POST /api/listings`)
> **Listing Gate**: Backend rejects listing if owner's `isVerified == false`.

#### 📥 Request Body (Multi-angle photos & measurements):
```json
{
  "ownerId": 2,
  "title": "Crimson Raw Silk Bridal Lehenga with Gotapatti",
  "description": "Worn once for 4 hours. Dry-cleaned and in mint showroom condition.",
  "supplyType": "P2P_INDIVIDUAL",
  "category": "BRIDAL",
  "fabric": "Raw Silk",
  "size": "M",
  "bustSize": 36,
  "waistSize": 32,
  "skirtLength": 42,
  "pricePerDay": 4200,
  "securityDeposit": 10500,
  "originalPrice": 65000,
  "stockQty": 1,
  "photos": [
    {
      "photoUrl": "https://storage.lehengaverse.com/outfits/front_cover.jpg",
      "photoType": "FRONT",
      "isCover": true
    },
    {
      "photoUrl": "https://storage.lehengaverse.com/outfits/back_view.jpg",
      "photoType": "BACK",
      "isCover": false
    },
    {
      "photoUrl": "https://storage.lehengaverse.com/outfits/zari_closeup.jpg",
      "photoType": "DETAIL",
      "isCover": false
    },
    {
      "photoUrl": "https://storage.lehengaverse.com/outfits/dupatta.jpg",
      "photoType": "DUPATTA",
      "isCover": false
    }
  ]
}
```

#### 📤 Response (`201 Created`):
```json
{
  "id": 14,
  "title": "Crimson Raw Silk Bridal Lehenga with Gotapatti",
  "status": "PENDING_VERIFICATION",
  "message": "Outfit submitted for quality review. ETA for catalog approval: 4-12 hours."
}
```

---

### 5️⃣ Create Rental Booking (`POST /api/bookings`)

#### 📥 Request Body:
```json
{
  "userId": 4,
  "listingId": 1,
  "startDate": "2026-12-10T00:00:00Z",
  "endDate": "2026-12-14T00:00:00Z",
  "securityDepositAmount": 8750,
  "totalAmount": 22750
}
```

#### 📤 Response (`201 Created`):
```json
{
  "id": 105,
  "userId": 4,
  "listingId": 1,
  "startDate": "2026-12-10T00:00:00Z",
  "endDate": "2026-12-14T00:00:00Z",
  "securityDepositAmount": 8750,
  "totalAmount": 22750,
  "status": "PENDING",
  "createdAt": "2026-11-24T14:40:00Z"
}
```

---

### 6️⃣ Record Payment & Confirm Booking (`POST /api/payments`)

#### 📥 Request Body:
```json
{
  "bookingId": 105,
  "amount": 22750,
  "paymentStatus": "SUCCESS",
  "transactionId": "PAY_UPI_9874561230"
}
```

#### 📤 Response (`200 OK`):
```json
{
  "id": 42,
  "bookingId": 105,
  "amount": 22750,
  "paymentStatus": "SUCCESS",
  "transactionId": "PAY_UPI_9874561230",
  "createdAt": "2026-11-24T14:42:00Z"
}
```
*(Booking status automatically transitions from `PENDING` to `CONFIRMED`)*

---

## 🎯 5. Mobile App Flow Best Practices

1. **Token Persistence**: Store `token` in **EncryptedSharedPreferences** (Android) or **Keychain** (iOS). Include in header: `Authorization: Bearer <token>`.
2. **KYC Gate on Listing**: Before opening the "Add Outfit" screen in the Partner tab, verify `user.isVerified == true`. If `false`, navigate the partner to the KYC submission screen.
3. **Security Deposit Handling**: The `totalAmount` in booking includes the rental fee and the refundable security deposit. When order status is updated to `RETURNED`, the deposit is refunded.
4. **Multi-Angle Photos**: The app should enforce taking 4 required angles:
   - 📸 Slot 1: Front Full Length
   - 📸 Slot 2: Back Latkan / Blouse Cut
   - 📸 Slot 3: Zari & Embroidery Closeup
   - 📸 Slot 4: Dupatta Border
