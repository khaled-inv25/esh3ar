# Project Structure

## Solution Organization

The solution follows ABP Framework's layered architecture with DDD principles:

### Core Layers
- **Domain.Shared**: Shared constants, enums, and DTOs
- **Domain**: Business logic, entities, domain services, and specifications
- **Application.Contracts**: Application service interfaces and DTOs
- **Application**: Application services and business workflows
- **EntityFrameworkCore**: Data access layer with repositories and DbContext
- **HttpApi**: Web API controllers
- **HttpApi.Client**: HTTP client proxies
- **Web**: MVC/Razor Pages presentation layer

### Additional Modules
- **Abp.Blob**: File storage and blob management
- **Abp.Media**: Media handling and processing
- **Abp.Worker**: Background job processing

### Support Projects
- **DbMigrator**: Database migration console application

## Key Directories

### `/src` - Source Code
- Each project follows the naming convention `Esh3arTech.{Layer}`
- Projects are organized by architectural layer
- Custom ABP modules are grouped under solution folders

### `/test` - Test Projects
- Unit and integration tests
- Follows same naming convention with `.Tests` suffix

### `/etc` - Configuration & Scripts
- `/etc/scripts/`: PowerShell scripts for setup and deployment
- `/etc/abp-studio/`: ABP Studio configuration

### `/files` - File Storage
- `/files/host/default/`: Default tenant file storage
- Organized by tenant structure

## Naming Conventions

### Projects
- **Format**: `Esh3arTech.{Layer}.{Module?}`
- **Examples**: `Esh3arTech.Domain`, `Esh3arTech.Abp.Blob`

### Namespaces
- Follow project structure: `Esh3arTech.{Domain}.{Feature}`
- **Examples**: `Esh3arTech.Messages`, `Esh3arTech.Plans.Subscriptions`

### Database Tables
- Defined in `Esh3arTechConsts` class
- **Format**: `TblMessage`, `TblMobileUser`, etc.

### Files & Folders
- Domain entities in `/Domain/{Feature}/`
- Application services in `/Application/{Feature}/`
- Controllers follow feature-based organization
- Specifications in `/Domain/{Feature}/Specs/`

## Configuration Files

### Application Settings
- `appsettings.json`: Main configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.secrets.json`: Sensitive configuration

### Build Configuration
- `common.props`: Shared MSBuild properties
- Individual `.csproj` files for project-specific settings
- `NuGet.Config`: Package source configuration

## Key Patterns

### Domain Layer
- Aggregate roots with business logic
- Domain services for complex operations
- Specifications for query logic
- Domain events for cross-boundary communication

### Application Layer
- Application services as use case orchestrators
- DTOs for data transfer
- AutoMapper profiles for object mapping
- Background workers for async processing

### Infrastructure
- Repository implementations in EntityFrameworkCore layer
- External service integrations (WhatsApp, Email)
- Caching and distributed locking implementations