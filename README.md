# Hospital Management System

A Hospital Management System built with ASP.NET Core: a REST API backend plus an MVC front end, for managing Doctors, Patients, and Appointments.

## Architecture

Multi-project Clean Architecture. Dependencies flow one way — outer layers reference inner ones, never the reverse:

```
HospitalManagement.Shared          (cross-cutting kernel, no dependencies)
  ^
HospitalManagement.Domain          (entities, enums)
  ^
HospitalManagement.Application     (services, DTOs, FluentValidation)
  ^
HospitalManagement.Infrastructure  (EF Core, repositories, JWT/security)
  ^
HospitalManagement.WebAPI          (API host, composition root)

HospitalManagement.MVC             (consumes the API over HTTP, no direct project references)
```

- **Domain** — `Doctor`, `Patient`, `Appointment`, `User`, `RefreshToken` entities; `AppointmentStatus` and `UserRole` enums; soft-delete via `BaseAuditEntity` (`CreatedAt`/`UpdatedAt`/`DeletedAt`).
- **Application** — service interfaces, request/response DTOs, FluentValidation validators, and the services (`DoctorService`, `PatientService`, `AppointmentService`, `AuthenticationService`) that enforce all business rules.
- **Infrastructure** — EF Core `ApplicationDbContext` + migrations, repository implementations, JWT/refresh-token generation, password hashing (BCrypt).
- **WebAPI** — controllers, global exception-handling middleware, JWT bearer auth, Swagger, Serilog logging.
- **MVC** — Razor views/controllers consuming the API via `HttpClient`, cookie-based auth wrapping the JWT, its own global exception-handling middleware, Serilog logging.

## Features

- Full CRUD for Doctors, Patients, and Appointments
- Search doctors/patients by name; filter appointments by doctor, patient, status, and date range
- Pagination and sorting on every list endpoint
- JWT authentication with `Administrator` and `Receptionist` roles — receptionists can create/update appointments but cannot delete doctors or patients
- Business rules enforced server-side:
  - No overlapping doctor appointments (30-minute slots)
  - A receptionist may not have more than 3 future appointments booked
  - No appointments may be booked in the past
  - Completed appointments are immutable; cancelled appointments cannot be restored
  - Doctors/patients with future appointments cannot be deleted
- Global exception handling with structured Serilog logging (console + rolling daily file) in both the API and the MVC app

## Tech Stack

- ASP.NET Core 8 (Web API + MVC)
- Entity Framework Core (Code First, migrations)
- SQL Server
- JWT bearer authentication + refresh tokens
- FluentValidation
- Serilog
- Swagger / Swashbuckle

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (local or remote)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Setup

1. Clone the repo and restore dependencies:
   ```bash
   dotnet restore
   ```
2. Create `HospitalManagement.WebAPI/.env` with your connection string:
   ```
   DB_CONNECTION_STRING=Server=...;Database=HospitalManagement;Trusted_Connection=True;TrustServerCertificate=True;
   ```
3. Apply migrations:
   ```bash
   dotnet ef database update --project HospitalManagement.Infrastructure --startup-project HospitalManagement.WebAPI
   ```
4. Run the API:
   ```bash
   dotnet run --project HospitalManagement.WebAPI
   ```
   Swagger UI opens automatically at `https://localhost:7240/swagger`.
5. Run the MVC app (in a separate terminal):
   ```bash
   dotnet run --project HospitalManagement.MVC
   ```
   Available at `https://localhost:7243`.

## Project Structure

```
HospitalManagement.sln
HospitalManagement.Shared/
HospitalManagement.Domain/
HospitalManagement.Application/
HospitalManagement.Infrastructure/
HospitalManagement.WebAPI/
HospitalManagement.MVC/
```
