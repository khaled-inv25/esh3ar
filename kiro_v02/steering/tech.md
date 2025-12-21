# Technology Stack

## Framework & Runtime
- **.NET 9.0** - Target framework
- **ABP Framework 9.3.x** - Application framework (DDD-based)
- **ASP.NET Core MVC / Razor Pages** - Web UI

## Database & ORM
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Primary database
- **EF Core Migrations** - Database versioning

## Authentication & Authorization
- **OpenIddict** - OAuth/OpenID Connect server
- **ABP Identity** - User and role management
- **JWT Bearer** - API authentication

## Messaging & Background Jobs
- **RabbitMQ** - Distributed event bus
- **Hangfire** - Background job processing (SQL Server storage)
- **SignalR** - Real-time communication

## UI & Frontend
- **LeptonX Lite Theme** - ABP UI theme
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library
- **Node.js v18/20** - Client-side package management

## Logging & Monitoring
- **Serilog** - Structured logging
- **Health Checks UI** - Application health monitoring

## Key NuGet Packages
- `Volo.Abp.*` - ABP Framework modules
- `Otp.NET` - OTP generation
- `System.IdentityModel.Tokens.Jwt` - JWT handling

## Common Commands

```powershell
# Build solution
dotnet build Esh3arTech.sln

# Run database migrations
dotnet run --project src/Esh3arTech.DbMigrator

# Run web application
dotnet run --project src/Esh3arTech.Web

# Install client-side libraries
abp install-libs

# Add new EF migration
dotnet ef migrations add <MigrationName> --project src/Esh3arTech.EntityFrameworkCore --startup-project src/Esh3arTech.Web

# Update database
dotnet ef database update --project src/Esh3arTech.EntityFrameworkCore --startup-project src/Esh3arTech.Web
```

## Configuration Files
- `appsettings.json` - Application settings (connection strings, auth config)
- `appsettings.secrets.json` - Sensitive configuration (not in source control)
- `common.props` - Shared MSBuild properties
