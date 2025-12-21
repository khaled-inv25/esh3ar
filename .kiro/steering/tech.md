# Technology Stack

## Framework & Runtime

- **.NET 9.0**: Target framework for all projects
- **ABP Framework 9.3+**: Application framework providing DDD infrastructure, modularity, and cross-cutting concerns
- **ASP.NET Core MVC/Razor Pages**: Web application framework
- **Entity Framework Core**: ORM for database access with SQL Server

## Key Libraries & Packages

- **ABP Modules**:
  - `Volo.Abp.Identity`: User and role management
  - `Volo.Abp.OpenIddict`: Authentication and authorization
  - `Volo.Abp.FeatureManagement`: Feature toggle system
  - `Volo.Abp.PermissionManagement`: Permission system
  - `Volo.Abp.SettingManagement`: Application settings
  - `Volo.Abp.BackgroundJobs`: Background job processing
  - `Volo.Abp.AuditLogging`: Audit trail
  - `Volo.Abp.TenantManagement`: Multi-tenancy support (currently disabled)

- **Frontend**:
  - `@abp/aspnetcore.mvc.ui.theme.leptonxlite`: UI theme
  - `@abp/signalr`: Real-time communication
  - jQuery, Bootstrap 5, DataTables, Select2, Moment.js

- **Infrastructure**:
  - **Serilog**: Logging to file and console
  - **RabbitMQ**: Message bus for distributed events
  - **SignalR**: Real-time messaging hub

## Database

- **SQL Server**: Primary database
- **Connection String**: Configured in `appsettings.json`
- **Table Prefix**: `Et` (e.g., `EtMobileUsers`, `EtUserPlans`)
- **Migrations**: Entity Framework Core migrations in `Esh3arTech.EntityFrameworkCore/Migrations`

## Common Commands

### Initial Setup
```bash
# Install client-side dependencies
abp install-libs

# Run database migrations and seed data
dotnet run --project src/Esh3arTech.DbMigrator
```

### Development
```bash
# Build solution
dotnet build

# Run web application
dotnet run --project src/Esh3arTech.Web

# Add new migration
dotnet ef migrations add MigrationName --project src/Esh3arTech.EntityFrameworkCore --startup-project src/Esh3arTech.DbMigrator
```

### Package Management
```bash
# Restore NuGet packages
dotnet restore

# Install/update ABP CLI
dotnet tool install -g Volo.Abp.Cli
dotnet tool update -g Volo.Abp.Cli
```

## Configuration Files

- `appsettings.json`: Application configuration (connection strings, auth server, external services)
- `appsettings.secrets.json`: Sensitive configuration (not in source control)
- `common.props`: Shared MSBuild properties across all projects
- `NuGet.Config`: NuGet package sources
- `package.json`: Frontend dependencies (in Web project)

## Development Environment

- **IDE**: Visual Studio 2022+ or Rider
- **Node.js**: v18 or v20 required for frontend tooling
- **SQL Server**: Local or remote instance
- **ABP Studio**: Optional ABP-specific IDE features
