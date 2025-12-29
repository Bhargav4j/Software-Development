# Tour Management System - .NET 8 Migration

## Overview
This is a fully migrated ASP.NET Web Forms application to .NET 8 using Razor Pages and Clean Architecture principles.

## Architecture
The solution follows Clean Architecture with four main layers:

### Domain Layer (`TourManagement.Domain`)
- Core business entities (Tour, User, Booking)
- Repository and service interfaces
- Domain exceptions

### Application Layer (`TourManagement.Application`)
- Business logic implementation
- Service implementations
- DTOs and mappings
- Validators

### Infrastructure Layer (`TourManagement.Infrastructure`)
- EF Core database context
- Repository implementations
- Entity configurations
- Data access logic

### Web Layer (`TourManagement.Web`)
- ASP.NET Core Razor Pages
- User interface
- View models
- Dependency injection configuration

## Key Features
- **Clean Architecture**: Separation of concerns with proper layering
- **Entity Framework Core 8**: Modern ORM with async operations
- **BCrypt Password Hashing**: Secure password storage
- **Serilog**: Structured logging
- **Session Management**: User authentication state
- **File Upload**: Image upload for tour packages
- **CRUD Operations**: Complete CRUD for Tours, Users, and Bookings

## Database Schema
The application uses SQL Server LocalDB with three main tables:
- **Tour**: Tour packages with details and pricing
- **Userinfo**: User accounts with encrypted passwords
- **Booking**: Tour bookings linking users to tours

## Configuration
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TourManagement;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB or SQL Server

### Running the Application
1. Navigate to the solution directory:
   ```bash
   cd /modernize-data/studio-data/TNT1001/APP2612/transformed-code/757/studio-workspace/softwarecomp
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the web application:
   ```bash
   dotnet run --project src/TourManagement.Web
   ```

5. Navigate to `https://localhost:5001` or `http://localhost:5000`

### Database Migration
The application automatically runs EF Core migrations on startup. To manually create migrations:
```bash
dotnet ef migrations add InitialCreate --project src/TourManagement.Infrastructure --startup-project src/TourManagement.Web
dotnet ef database update --project src/TourManagement.Infrastructure --startup-project src/TourManagement.Web
```

## Migration from Web Forms
This application was migrated from ASP.NET Web Forms 4.7.2 to .NET 8. Key changes include:

### Replaced Components
- **System.Web → ASP.NET Core**: All System.Web dependencies removed
- **Web Forms Pages → Razor Pages**: .aspx files converted to .cshtml
- **ViewState → TempData/Session**: Modern state management
- **Server Controls → Tag Helpers**: HTML helpers and tag helpers
- **ADO.NET → EF Core**: Modern data access with LINQ
- **Web.config → appsettings.json**: Configuration management
- **Global.asax → Program.cs**: Application startup

### Security Improvements
- **SQL Injection Fixed**: Parameterized queries via EF Core
- **Password Hashing**: BCrypt instead of plain text storage
- **CSRF Protection**: Built-in with Razor Pages
- **Structured Logging**: Serilog for better observability

## Project Structure
```
/modernize-data/studio-data/TNT1001/APP2612/transformed-code/757/studio-workspace/softwarecomp/
├── src/
│   ├── TourManagement.Domain/
│   ├── TourManagement.Application/
│   ├── TourManagement.Infrastructure/
│   └── TourManagement.Web/
├── tests/
│   ├── TourManagement.UnitTests/
│   └── TourManagement.IntegrationTests/
├── docs/
└── TourManagement.sln
```

## Build Verification
✅ **Build Status**: SUCCESS
✅ **Target Framework**: .NET 8.0
✅ **All Projects**: Compiled successfully
⚠️ **Warnings**: 1 (resolved with 'new' keyword)

## Next Steps
1. Test all CRUD operations
2. Add user authentication middleware
3. Implement authorization policies
4. Add unit and integration tests
5. Configure production database
6. Set up CI/CD pipeline
7. Add API endpoints if needed

## Known Issues
- Database must be created before first run (handled by migrations)
- Images are stored in wwwroot/images (consider cloud storage for production)
- Session state requires distributed cache for multi-server deployment

## Support
For issues or questions, refer to the original analysis report ID: 2091
