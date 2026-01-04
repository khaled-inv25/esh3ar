# Technology Stack

## Framework & Runtime
- **.NET 9.0**: Primary runtime and SDK
- **ABP Framework 9.3.5**: Enterprise application framework providing DDD, multi-tenancy, permissions, and modular architecture
- **ASP.NET Core**: Web framework with MVC/Razor Pages
- **Entity Framework Core 9.0**: ORM with SQL Server provider

## Frontend
- **Razor Pages/MVC**: Server-side rendering
- **LeptonX Lite Theme**: ABP's UI theme
- **SignalR**: Real-time communication
- **Node.js v18/20**: Frontend tooling

## Infrastructure & Services
- **SQL Server**: Primary database
- **Redis**: Caching and distributed locking
- **RabbitMQ**: Message queuing and event bus
- **Hangfire**: Background job processing
- **Serilog**: Structured logging

## Key Libraries
- **OpenIddict**: Authentication and authorization
- **AutoMapper**: Object mapping
- **DistributedLock.Redis**: Distributed locking
- **AspNetCore.HealthChecks**: Health monitoring

## Common Commands

### Initial Setup
```bash
# Install client-side dependencies
abp install-libs

# Run database migrations
cd src/Esh3arTech.DbMigrator && dotnet run

# Generate development certificates
dotnet dev-certs https -v -ep openiddict.pfx -p 78711d2c-a333-42b6-90ba-a83863e4d241
```

### Development
```bash
# Build solution
dotnet build

# Run web application
cd src/Esh3arTech.Web && dotnet run

# Run database migrator
cd src/Esh3arTech.DbMigrator && dotnet run

# Restore packages
dotnet restore
```

### ABP CLI Commands
```bash
# Install ABP CLI
dotnet tool install -g Volo.Abp.Cli

# Install client libraries
abp install-libs

# Generate proxy classes
abp generate-proxy

# Add new module
abp add-module <module-name>
```

## Configuration Notes
- Connection strings in `appsettings.json`
- Redis configuration for caching and distributed locking
- RabbitMQ for event bus messaging
- WhatsApp API integration settings
- Blob storage configuration for file attachments