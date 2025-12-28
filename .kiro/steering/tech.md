# Technology Stack

## Framework & Runtime

- **.NET 9.0**: Target framework for all projects
- **ABP Framework**: Modular application framework with DDD architecture
- **ASP.NET Core MVC/Razor Pages**: Web application layer
- **Entity Framework Core**: ORM for database access
- **OpenIddict**: Authentication and authorization (OAuth 2.0/OpenID Connect)

## Key Libraries & Dependencies

- **AutoMapper**: Object-to-object mapping
- **Serilog**: Structured logging (file + console + ABP Studio)
- **SignalR**: Real-time communication for message delivery
- **RabbitMQ**: Distributed event bus for message processing
- **ABP Modules**: Identity, Tenant Management, Feature Management, Permission Management, Audit Logging, Background Jobs, Blob Storage

## Database

- **SQL Server**: Primary database (connection string in appsettings.json)
- **Entity Framework Core Migrations**: Database schema management

## Frontend

- **jQuery**: DOM manipulation and AJAX
- **Bootstrap 5**: UI framework
- **DataTables**: Table management
- **Select2**: Enhanced select controls
- **SweetAlert2**: Modal dialogs
- **SignalR Client**: Real-time communication

## Common Commands

### Initial Setup
```bash
# Install client-side dependencies
abp install-libs

# Run database migrations and seed data
dotnet run --project src/Esh3arTech.DbMigrator
```

### Build & Run
```bash
# Build solution
dotnet build

# Run web application
dotnet run --project src/Esh3arTech.Web

# Run media service (separate microservice)
dotnet run --project src/Esh3arTech.Abp.Media
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/Esh3arTech.EntityFrameworkCore

# Update database
dotnet ef database update --project src/Esh3arTech.EntityFrameworkCore
```

### Development
```bash
# Restore NuGet packages
dotnet restore

# Clean build artifacts
dotnet clean

# Watch for changes (auto-rebuild)
dotnet watch run --project src/Esh3arTech.Web
```

## Configuration Files

- **appsettings.json**: Connection strings, authentication, external services (WhatsApp, Email, RabbitMQ)
- **appsettings.secrets.json**: Sensitive configuration (not in source control)
- **NuGet.Config**: Package sources
- **.editorconfig**: Code style rules
