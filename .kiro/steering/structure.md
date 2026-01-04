# Project Structure

## Solution Organization

The solution follows ABP Framework's layered architecture with DDD principles:

```
src/
├── Esh3arTech.Domain.Shared/          # Shared constants, enums, DTOs
├── Esh3arTech.Domain/                 # Domain entities, services, repositories
├── Esh3arTech.Application.Contracts/  # Application service interfaces, DTOs
├── Esh3arTech.Application/            # Application services implementation
├── Esh3arTech.EntityFrameworkCore/    # EF Core configurations, repositories
├── Esh3arTech.HttpApi/                # HTTP API controllers
├── Esh3arTech.HttpApi.Client/         # HTTP client proxies
├── Esh3arTech.Web/                    # Web UI (Razor Pages/MVC)
├── Esh3arTech.DbMigrator/             # Database migration console app
└── Custom Modules/
    ├── Esh3arTech.Abp.Blob/           # Blob storage module
    ├── Esh3arTech.Abp.Media/          # Media handling module
    └── Esh3arTech.Abp.Worker/         # Background worker module
```

## Layer Responsibilities

### Domain Layer (`Esh3arTech.Domain`)
- **Entities**: Core business objects (Message, MobileUser, UserPlan, Subscription)
- **Domain Services**: Business logic that doesn't belong to entities
- **Repositories**: Data access interfaces
- **Specifications**: Query specifications for complex filtering
- **Domain Events**: Business events

### Application Layer (`Esh3arTech.Application`)
- **Application Services**: Use case implementations
- **DTOs**: Data transfer objects in contracts project
- **AutoMapper Profiles**: Object mapping configurations
- **Background Workers**: Long-running background services

### Infrastructure Layer (`Esh3arTech.EntityFrameworkCore`)
- **DbContext**: EF Core database context
- **Entity Configurations**: Fluent API configurations
- **Repository Implementations**: Data access implementations
- **Migrations**: Database schema changes

### Presentation Layer (`Esh3arTech.Web`)
- **Pages**: Razor Pages for UI
- **Controllers**: MVC controllers for APIs
- **Hubs**: SignalR hubs for real-time communication
- **Menus**: Navigation configuration

## Domain Organization

### Messages Domain
```
Messages/
├── Message.cs                    # Aggregate root
├── MessageAttachment.cs          # Value object
├── MessageManager.cs             # Domain service
├── IMessageRepository.cs         # Repository interface
├── Specs/                        # Query specifications
└── SendBehavior/                 # Message sending strategies
```

### Plans Domain
```
Plans/
├── UserPlan.cs                   # Aggregate root
├── UserPlanManager.cs            # Domain service
├── Subscriptions/
│   ├── Subscription.cs           # Aggregate root
│   ├── SubscriptionManager.cs    # Domain service
│   └── SubscriptionRenewalHistory.cs
```

## Naming Conventions

### Files & Classes
- **Entities**: PascalCase (e.g., `Message`, `MobileUser`)
- **Services**: End with `Service` or `Manager` (e.g., `MessageManager`)
- **DTOs**: End with `Dto` (e.g., `MessageDto`, `CreatePlanDto`)
- **Repositories**: Start with `I` for interfaces (e.g., `IMessageRepository`)
- **Specifications**: End with `Specification` (e.g., `PendingMessageSpecification`)

### Database
- **Tables**: Use `Esh3arTechConsts.TblMessage` pattern for table names
- **Columns**: PascalCase matching entity properties
- **Indexes**: Descriptive names with purpose

## Configuration Files

- **appsettings.json**: Main configuration (connection strings, external services)
- **common.props**: Shared MSBuild properties
- **package.json**: Frontend dependencies (in Web project)
- **.abppkg files**: ABP package configurations

## Key Patterns

### Repository Pattern
- Interfaces in Domain layer
- Implementations in EntityFrameworkCore layer
- Custom methods for complex queries

### Specification Pattern
- Used for complex query logic
- Located in `Specs/` folders within domain folders
- Composable and testable

### Factory Pattern
- `MessageFactory` for creating different message types
- Extensible for new message providers

### Event-Driven Architecture
- Domain events for business logic
- Distributed events for cross-service communication
- ETOs (Event Transfer Objects) for event data