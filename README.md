# 👗 LehengaVerse — Backend Developer Setup Guide

> 📱 **Mobile App Developers**: For complete API endpoints, request/response JSON payloads, and authentication guides for iOS & Android, please refer directly to [MOBILE_APP_API_DOCS.md](file:///d:/lehenga/backend/MOBILE_APP_API_DOCS.md).

This document contains step-by-step instructions for developers to clone, set up, configure, and run the **LehengaVerse Backend Web API** on their local development machine.

---

## 📌 Tech Stack Overview

- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 8 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **API Documentation**: Swagger / OpenAPI (Swashbuckle)
- **Architecture**: Controller-based REST API with DTO pattern

---

## 💻 Prerequisites

Ensure you have the following installed on your machine:

1. **.NET 8.0 SDK**: [Download .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify installation:
     ```bash
     dotnet --version
     # Expected output: 8.0.xxx
     ```
2. **PostgreSQL 16** (or **Docker Desktop** for 1-click database launch):
   - [Download PostgreSQL](https://www.postgresql.org/download/) OR [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)
3. **Git**: [Download Git](https://git-scm.com/)
4. **Code Editor**: Visual Studio 2022 (v17.8+), VS Code (with C# Dev Kit), or JetBrains Rider.

---

## 🚀 Step-by-Step Setup Guide

### 1️⃣ Clone the Repository

Clone the monorepo from GitHub and navigate directly to the `backend` folder:

```bash
git clone https://github.com/Jetly143/Rent_Lehenga_Application.git
cd Rent_Lehenga_Application/backend
```

---

### 2️⃣ Start Local PostgreSQL Database

You can run PostgreSQL either via **Docker** (Recommended) or **Native PostgreSQL**.

#### Option A: Using Docker (Quickest & Recommended)
Run this single command in your terminal to start a pre-configured PostgreSQL 16 container:

```bash
docker run -d \
  --name lehenga_postgres \
  -p 5432:5432 \
  -e POSTGRES_USER=lehenga_admin \
  -e POSTGRES_PASSWORD=StrongPassword@2026 \
  -e POSTGRES_DB=lehenga_db \
  postgres:16-alpine
```

#### Option B: Using Local Native PostgreSQL
If you have PostgreSQL installed directly on your machine:
1. Open **pgAdmin** or `psql`:
   ```bash
   psql -U postgres
   ```
2. Create the user and database:
   ```sql
   CREATE USER lehenga_admin WITH PASSWORD 'StrongPassword@2026' SUPERUSER;
   CREATE DATABASE lehenga_db OWNER lehenga_admin;
   GRANT ALL PRIVILEGES ON DATABASE lehenga_db TO lehenga_admin;
   ```

---

### 3️⃣ Configure Connection String

The API is pre-configured to look for the database connection in `appsettings.json`.

Check `appsettings.json` (or override in `appsettings.Development.json` if your local DB credentials differ):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=lehenga_db;Username=lehenga_admin;Password=StrongPassword@2026"
  }
}
```

> **Note**: If your local Postgres user or password is different, edit the `DefaultConnection` string accordingly.

---

### 4️⃣ Restore Dependencies & Build

Run the following commands in the `backend` directory to restore NuGet packages and build the project:

```bash
dotnet restore
dotnet build
```

---

### 5️⃣ Database Migrations

The application has automatic migration configured in `Program.cs` (`dbContext.Database.Migrate()`), which applies all pending migrations automatically whenever the API starts up.

However, if you want to run the migrations manually or manage migrations with EF CLI:

```bash
# 1. Install dotnet-ef tool globally (if not installed already)
dotnet tool install --global dotnet-ef

# 2. Apply all migrations to your database
dotnet ef database update
```

This creates all 10 core tables:
- `users`
- `listings`
- `bookings`
- `payments`
- `notifications`
- `payout_batches`
- `payout_items`
- `logistics_tasks`
- `logistics_task_photos`
- `tailor_tasks`

---

### 6️⃣ Run the Backend API

Start the Web API server:

```bash
dotnet run
```

Or run with a specific URL/port:
```bash
dotnet run --urls "http://localhost:5000"
```

Once running, you will see output similar to:
```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5016
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

### 7️⃣ Verify & Test

Open your browser or API client (Postman/Curl):

- **Health Check Endpoint**:
  ```text
  GET http://localhost:5016/
  ```
  **Response**:
  ```json
  {
    "status": "Healthy",
    "message": "Rent Lehenga API is running successfully!",
    "database": "PostgreSQL Connected",
    "timestamp": "2026-10-23T06:15:00Z"
  }
  ```

- **Interactive Swagger UI**:
  ```text
  http://localhost:5016/swagger
  ```
  Explore and execute test requests on all available endpoints directly from the interactive documentation.

---

## 📁 Project Architecture & Structure

```text
backend/
├── Controllers/              # REST API Controllers (Endpoints)
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   ├── ListingsController.cs
│   ├── PaymentsController.cs
│   ├── LogisticsController.cs
│   └── TailorsController.cs
├── Data/
│   └── AppDbContext.cs       # EF Core DbContext & Fluent API table mappings
├── DTOs/                     # Data Transfer Objects (Request/Response schemas)
├── Migrations/               # EF Core database migration snapshots
├── Models/                   # Domain entity models (matching 10 DB tables)
│   ├── User.cs
│   ├── Listing.cs
│   ├── Booking.cs
│   ├── Payment.cs
│   ├── Notification.cs
│   ├── PayoutBatch.cs
│   ├── PayoutItem.cs
│   ├── LogisticsTask.cs
│   ├── LogisticsTaskPhoto.cs
│   └── TailorTask.cs
├── Properties/
│   └── launchSettings.json   # Local debug ports and environments
├── appsettings.json          # Core configuration & DB connection string
├── appsettings.Development.json
├── Program.cs                # App bootstrap, DI container, Middleware & Swagger config
└── README.md                 # This developer setup guide
```

---

## 🛠 Useful EF Core Commands

When you modify any entity models in `Models/`:

```bash
# Add a new migration
dotnet ef migrations add <DescriptiveMigrationName>

# Apply migration to database
dotnet ef database update

# Rollback last migration (if not yet applied or after updating to previous)
dotnet ef migrations remove

# Generate SQL script without running
dotnet ef migrations script
```

---

## ❓ Troubleshooting & FAQs

### 1. `Npgsql.NpgsqlException: Failed to connect to 127.0.0.1:5432`
- **Cause**: PostgreSQL is not running or the port is blocked.
- **Fix**: Check if your Docker container is running (`docker ps`) or start your PostgreSQL service (`net start postgresql-x64-16` on Windows or `sudo systemctl start postgresql` on Linux/macOS).

### 2. `28P01: password authentication failed for user "lehenga_admin"`
- **Cause**: Database password in `appsettings.json` does not match your PostgreSQL user password.
- **Fix**: Update the `Password=` value in `appsettings.json` or reset the user's password in PostgreSQL:
  ```sql
  ALTER USER lehenga_admin WITH PASSWORD 'StrongPassword@2026';
  ```

### 3. Port already in use (`Address already in use`)
- **Fix**: Run the API on a different port:
  ```bash
  dotnet run --urls "http://localhost:5005"
  ```
  Then access Swagger at `http://localhost:5005/swagger`.

---

## 👥 Need Help?

For issues related to server deployment, cloud VM, or database access, contact the repository administrator or check the repository issue tracker.
