# Tour Management System - .NET 8 Migration

This project has been successfully migrated from ASP.NET Web Forms 4.7.2 to .NET 8.

## Project Structure

```
TourManagement/
├── src/
│   ├── TourManagement.Domain/          # Domain entities and interfaces
│   ├── TourManagement.Application/     # Business logic and services
│   ├── TourManagement.Infrastructure/  # Data access and EF Core
│   └── TourManagement.Web/            # Razor Pages UI
└── tests/
    ├── TourManagement.UnitTests/      # Unit tests
    └── TourManagement.IntegrationTests/ # Integration tests
```

## Technologies Used

- .NET 8
- ASP.NET Core Razor Pages
- Entity Framework Core 8
- SQL Server
- Serilog for logging
- Bootstrap 5 for UI

## Build Status

✅ **BUILD SUCCESSFUL**

All projects compile successfully with no errors.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB

### Configuration

Update the connection string in `src/TourManagement.Web/appsettings.json`

### Running the Application

```bash
cd src/TourManagement.Web
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## Features

- Tour Management (CRUD operations)
- User Registration and Authentication
- Booking System
- Tour Search
- Admin Panel
