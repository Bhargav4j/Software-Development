# Tour Management System - .NET 8

A modern tour booking and management system built with .NET 8, Razor Pages, and Entity Framework Core.

## Features

- **Tour Management**: Create, view, edit, and delete tour packages
- **User Management**: Registration and authentication system
- **Booking System**: Book tours and manage bookings
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Web layers
- **Modern Security**: BCrypt password hashing, CSRF protection, parameterized queries

## Technology Stack

- **.NET 8**: Latest framework version
- **ASP.NET Core Razor Pages**: Modern UI framework
- **Entity Framework Core 8**: ORM for data access
- **SQL Server**: Database engine
- **Serilog**: Structured logging
- **Bootstrap 5**: UI styling
- **BCrypt.Net**: Password hashing

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or SQL Server LocalDB

### Installation

1. Clone the repository
2. Update connection string in appsettings.json
3. Apply database migrations:
   ```bash
   dotnet ef database update --project src/TourManagement.Infrastructure --startup-project src/TourManagement.Web
   ```
4. Run: `dotnet run --project src/TourManagement.Web`
5. Open: https://localhost:5001

## Build and Test

Build: `dotnet build TourManagement.sln`

## Migration from Web Forms

Migrated from ASP.NET Web Forms 4.7.2 to .NET 8. See docs/MIGRATION_NOTES.md for details.

---

**Migration Status**: ✅ Completed Successfully
**Build Status**: ✅ Passing (0 errors, 0 warnings)
**Framework**: .NET 8
**Generated**: 2025-12-31
