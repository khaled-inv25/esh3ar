# Project Structure

This solution follows ABP Framework's layered DDD architecture.

## Solution Layout

```
src/
├── Esh3arTech.Domain.Shared/     # Shared constants, enums, localization
├── Esh3arTech.Domain/            # Domain entities, managers, repositories
├── Esh3arTech.Application.Contracts/  # DTOs, interfaces, permissions
├── Esh3arTech.Application/       # Application services implementation
├── Esh3arTech.EntityFrameworkCore/    # EF Core DbContext, migrations, repos
├── Esh3arTech.HttpApi/           # API controllers
├── Esh3arTech.HttpApi.Client/    # HTTP client proxies
├── Esh3arTech.Web/               # Web host, Razor Pages, SignalR hubs
└── Esh3arTech.DbMigrator/        # Database migration console app
```

## Layer Responsibilities

### Domain.Shared
- Enums: `MessageStatus`, `MessageContentType`, `SubscriptionStatus`, `BillingInterval`
- Constants: `MessageConsts`, `MobileUserConsts`, `SubscriptionConsts`
- Localization resources: `ar.json`, `en.json`

### Domain
- **Entities**: `Message`, `MobileUser`, `UserPlan`, `Subscription`, `RegistretionRequest`
- **Domain Services**: `MessageManager`, `MobileUserManager`, `OtpManager`, `SubscriptionManager`
- **Repository Interfaces**: `IUserPlanRepository`, `ISubscriptionRepository`
- **Features**: Custom feature value providers for plan-based access

### Application.Contracts
- **DTOs**: Input/output data transfer objects per feature
- **Interfaces**: `IMessageAppService`, `IPlanAppService`, `ISubscriptionAppService`
- **Permissions**: `Esh3arTechPermissions`, `Esh3arTechPermissionDefinitionProvider`

### Application
- **AppServices**: Implement contracts, orchestrate domain logic
- **Event Handlers**: Local and distributed event handlers
- **AutoMapper Profiles**: Entity-to-DTO mappings

### EntityFrameworkCore
- **DbContext**: `Esh3arTechDbContext` with all DbSets
- **Configurations**: Fluent API entity configurations per feature folder
- **Repositories**: Custom repository implementations
- **Migrations**: EF Core migration files
- **Background Workers**: `IdentifySubscriptionsDueForRenewalWorker`

### Web
- **Pages/**: Razor Pages organized by feature (Messages, Plans)
- **Hubs/**: SignalR hubs (`OnlineMobileUserHub`)
- **EventBus/**: Distributed and local event handlers
- **Menus/**: Navigation menu contributors

## Naming Conventions

- Entities: PascalCase, singular (`Message`, `UserPlan`)
- DTOs: `{Entity}{Action}Dto` (e.g., `CreatePlanDto`, `MessageInListDto`)
- AppServices: `{Feature}AppService`
- Interfaces: `I{Feature}AppService`
- Repositories: `I{Entity}Repository`
- Configurations: `{Entity}Configuration`

## Feature Organization

Each feature follows this folder pattern:
```
Domain/
  {Feature}/
    {Entity}.cs
    {Entity}Manager.cs
    I{Entity}Repository.cs (if custom)

Application.Contracts/
  {Feature}/
    I{Feature}AppService.cs
    {Various}Dto.cs

Application/
  {Feature}/
    {Feature}AppService.cs

EntityFrameworkCore/
  EntityFrameworkCore/
    {Feature}/
      {Entity}Configuration.cs
      {Entity}Repository.cs (if custom)
```
