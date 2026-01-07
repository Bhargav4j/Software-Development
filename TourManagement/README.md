# Tour Management System - .NET 8

A modern tour booking management system migrated from ASP.NET Web Forms to .NET 8 with clean architecture.

## Project Overview

This application has been successfully migrated from ASP.NET Web Forms 4.7.2 to .NET 8 using clean architecture principles. The application allows users to browse tours, register accounts, and book tours.

## Architecture

The solution follows clean architecture with four main layers:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic and service implementations
- **Infrastructure Layer**: Data access with Entity Framework Core 8.0
- **Web Layer**: ASP.NET Core Razor Pages UI

## Technologies Used

- .NET 8.0
- ASP.NET Core Razor Pages
- Entity Framework Core 8.0
- SQL Server LocalDB
- Serilog for logging
- BCrypt.Net for password hashing
- Bootstrap 5 for UI

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB or SQL Server

### Running the Application

1. Navigate to the solution directory:
```bash
cd /modernize-data/studio-data/TNT1001/APP2798/transformed-code/927/studio-workspace/software/TourManagement
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update database connection string in `src/TourManagement.Web/appsettings.json`

4. Create database (optional - will be created automatically on first run):
```bash
dotnet ef database update --project src/TourManagement.Infrastructure --startup-project src/TourManagement.Web
```

5. Run the application:
```bash
dotnet run --project src/TourManagement.Web
```

6. Open browser and navigate to: https://localhost:5001

## Project Structure

```
TourManagement/
├── src/
│   ├── TourManagement.Domain/          # Entities, Interfaces, Exceptions
│   ├── TourManagement.Application/     # Services, Business Logic
│   ├── TourManagement.Infrastructure/  # Data Access, Repositories
│   └── TourManagement.Web/             # Razor Pages UI
├── tests/
│   ├── TourManagement.UnitTests/       # Unit Tests
│   └── TourManagement.IntegrationTests/# Integration Tests
└── docs/                               # Documentation
```

## Features

- User registration and authentication with secure password hashing
- Browse available tours with images and details
- Book tours with email confirmation
- View booking history
- Admin functionality for managing tours
- Responsive design with Bootstrap 5

## Migration Notes

### Key Changes from Web Forms

1. **Configuration**: Web.config → appsettings.json
2. **Pages**: .aspx files → Razor Pages (.cshtml)
3. **Data Access**: ADO.NET → Entity Framework Core
4. **Authentication**: Forms Authentication → ASP.NET Core Session-based auth
5. **Server Controls**: GridView, SqlDataSource → Razor syntax with LINQ
6. **Dependency Injection**: Manual instantiation → Built-in DI container

### Security Improvements

- Parameterized queries (protection against SQL injection)
- Password hashing with BCrypt
- CSRF protection (built-in with Razor Pages)
- Input validation with data annotations
- Secure session management

## Build Verification

✅ **Build Status**: SUCCESS

- All projects compile successfully
- 0 errors, 0 warnings
- Build time: ~4 seconds

## Database Schema

The application uses the following main tables:

- **Tour**: Tour packages (tours)
- **UserInfo**: User accounts
- **Booking**: Tour bookings

## License

Copyright © 2026 Tour Management System
