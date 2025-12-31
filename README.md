# Tour Management System - .NET 8

This is a modern Tour Management System built with .NET 8, following clean architecture principles.

## Migration from ASP.NET Web Forms

This application was migrated from ASP.NET Web Forms 4.7.2 to .NET 8 with Razor Pages.

### Key Changes

- **System.Web** replaced with ASP.NET Core
- **Web Forms pages** migrated to Razor Pages
- **ADO.NET** replaced with Entity Framework Core 8
- **Plain text passwords** replaced with BCrypt hashing
- **SQL injection vulnerabilities** fixed with parameterized queries
- **Configuration** migrated from Web.config to appsettings.json
- **Authentication** upgraded to Cookie Authentication

## Architecture

The solution follows clean architecture with the following layers:

- **TourManagement.Domain**: Entity definitions and interfaces
- **TourManagement.Application**: Business logic and services
- **TourManagement.Infrastructure**: Data access with EF Core
- **TourManagement.Web**: Razor Pages UI

## Prerequisites

- .NET 8 SDK
- SQL Server or LocalDB

## Setup

1. Update the connection string in `src/TourManagement.Web/appsettings.json`
2. Run database migrations:
   ```bash
   cd src/TourManagement.Web
   dotnet ef migrations add InitialCreate --project ../TourManagement.Infrastructure
   dotnet ef database update --project ../TourManagement.Infrastructure
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## Features

- Tour management (CRUD operations)
- User registration and authentication
- Secure password hashing
- File upload for tour images
- Responsive UI with Bootstrap 5

## Security Improvements

- SQL injection protection through EF Core
- Password hashing with BCrypt
- CSRF protection enabled by default
- Secure authentication with cookies
- Input validation on all forms

## Database Schema

The application uses Entity Framework Core with the following entities:

- **Tour**: Tour packages with details and pricing
- **User**: User accounts with secure password storage
- **Booking**: Tour bookings linked to users and tours

