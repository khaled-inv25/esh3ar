# Technology Stack

## Framework & Runtime
- **.NET 9.0**: Primary runtime and SDK
- **ABP Framework 9.3.5**: Application framework providing DDD infrastructure
- **ASP.NET Core**: Web framework with MVC/Razor Pages
- **Entity Framework Core**: ORM with SQL Server provider

## Key Libraries & Dependencies
- **AutoMapper**: Object-to-object mapping
- **Hangfire**: Background job processing
- **SignalR**: Real-time communication
- **RabbitMQ**: Message queuing and event bus
- **Redis**: Caching and distributed locking
- **Serilog**: Structured logging
- **OpenIddict**: OAuth2/OpenID Connect server
- **Swashbuckle**: API documentation

## Frontend
- **LeptonX Lite Theme**: ABP's UI theme
- **Node.js v18/20**: Frontend tooling
- **Yarn**: Package management

## Infrastructure
- **SQL Server**: Primary database
- **Redis**: Caching layer
- **RabbitMQ**: Message broker
- **File System**: Blob storage

## Common Commands

### Initial Setup
```bash
# Install client-side dependencies
abp install-libs

# Run database migrations
cd src/Esh3arTech.DbMigrator && dotnet run

# Generate development certificate
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

# Add new migration
dotnet ef migrations add <MigrationName> -p src/Esh3arTech.EntityFrameworkCore
```

### Package Management
```bash
# Install ABP CLI
dotnet tool install -g Volo.Abp.Cli

# Update ABP packages
abp update

# Install frontend packages
cd src/Esh3arTech.Web && yarn install
```