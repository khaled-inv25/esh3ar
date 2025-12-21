# Project Structure

## Solution Architecture

Esh3arTech follows ABP's layered architecture based on Domain-Driven Design (DDD) principles. The solution is organized into the following projects:

## Core Layers

### Domain Layer (`Esh3arTech.Domain`)
Contains business logic, domain entities, domain services, and repository interfaces.

**Key folders**:
- `Plans/`: UserPlan entity, UserPlanManager, IUserPlanRepository
- `Plans/Subscriptions/`: Subscription entity, SubscriptionManager, renewal history
- `Messages/`: Message entity, MessageManager
- `MobileUsers/`: MobileUser entity, MobileUserManager
- `Registretions/`: RegistrationRequest entity, RegistrationRequestManager
- `Otp/`: OTP management service
- `Features/`: Custom feature management providers
- `SmsProviders/`: SMS/Email sending abstractions and implementations
- `BackgroundJobs/`: Background job definitions (e.g., EmailSendingJob)
- `Settings/`: Application settings definitions
- `Data/`: Database migration service and schema migrator interface

### Domain.Shared Layer (`Esh3arTech.Domain.Shared`)
Contains constants, enums, and shared types used across all layers.

**Key folders**:
- `Localization/Esh3arTech/`: Localization resources (en.json, ar.json)
- `Messages/`: Message-related enums (MessageStatus, MessageType, Priority, MessageContentType)
- `MobileUsers/`: MobileUser constants and enums
- `Plans/Subscriptions/`: Subscription enums (BillingInterval, SubscriptionStatus, LastPaymentStatus)
- `Users/`: User constants
- `Utility/`: Helper utilities (e.g., MobileNumberPreparator)

### Application Layer (`Esh3arTech.Application`)
Contains application services implementing business use cases.

**Key folders**:
- `Plans/`: PlanAppService for plan CRUD and feature management
- `Plans/Subscriptions/`: SubscriptionAppService for subscription management
- `Messages/`: MessageAppService for message operations
- `Users/`: UserAppService for user operations
- `Registrations/`: RegistrationAppService for user registration and OTP verification
- `Tokens/`: TokenProvider for JWT token generation
- `Sandbox/`: Testing/sandbox services

**Conventions**:
- Application services inherit from `Esh3arTechAppService`
- Use AutoMapper for DTO mapping (profiles in `Esh3arTechApplicationAutoMapperProfile.cs`)
- Implement interfaces from Application.Contracts layer

### Application.Contracts Layer (`Esh3arTech.Application.Contracts`)
Contains DTOs, application service interfaces, and permissions.

**Key folders**:
- `Plans/`: Plan DTOs (CreatePlanDto, UpdatePlanDto, PlanDto, PlanInListDto, etc.)
- `Plans/Subscriptions/`: Subscription DTOs
- `Messages/`: Message DTOs and events (OneWayMessageCreatedEvent, SendOneWayMessageEto)
- `Registrations/`: Registration DTOs (RegisterRequestDto, VerifyOtpRequestDto)
- `Permissions/`: Permission definitions (Esh3arTechPermissions, Esh3arTechPermissionDefinitionProvider)
- `Feature/`: Feature definitions (Esh3arTechFeatureDefinitionProvider)

**Naming conventions**:
- DTOs: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`, `{Entity}InListDto`
- Interfaces: `I{Entity}AppService`
- Events: `{Event}Eto` (Event Transfer Object) or `{Event}Event`

## Infrastructure Layer

### EntityFrameworkCore Layer (`Esh3arTech.EntityFrameworkCore`)
Contains EF Core DbContext, configurations, and repository implementations.

**Key folders**:
- `EntityFrameworkCore/`: DbContext and module configuration
- `EntityFrameworkCore/Plans/`: UserPlanConfiguration, UserPlanRepository
- `EntityFrameworkCore/Subscriptions/`: SubscriptionConfiguration, SubscriptionRepository
- `EntityFrameworkCore/Messages/`: MessageConfiguration
- `EntityFrameworkCore/MobileUsers/`: MobileUserConfiguration
- `EntityFrameworkCore/Registrations/`: RegistrationRequestConfiguration
- `Migrations/`: EF Core migrations
- `BackgroundWorkers/`: Background workers (e.g., IdentifySubscriptionsDueForRenewalWorker)

**Conventions**:
- Entity configurations use Fluent API in separate `{Entity}Configuration` classes
- Custom repositories implement domain repository interfaces
- Table names use `Et` prefix (defined in `Esh3arTechConsts.DbTablePrefix`)

### DbMigrator (`Esh3arTech.DbMigrator`)
Console application for applying migrations and seeding initial data.

## Presentation Layer

### Web Layer (`Esh3arTech.Web`)
ASP.NET Core MVC/Razor Pages application.

**Key folders**:
- `Pages/`: Razor Pages organized by feature (Plans, Messages, etc.)
- `Pages/{Feature}/`: Each feature has Index.cshtml, Index.cshtml.cs, Index.js
- `Pages/{Feature}/`: Modals for create/edit operations (CreateModal.cshtml, EditModal.cshtml)
- `Menus/`: Menu and toolbar contributors
- `Hubs/`: SignalR hubs (OnlineMobileUserHub)
- `MessagesHandler/`: Message delivery handlers
- `MobileUsers/`: Online user tracking services and cache items
- `HealthChecks/`: Health check implementations
- `wwwroot/`: Static files (CSS, JS, images, client libraries)

**Conventions**:
- Page models inherit from `Esh3arTechPageModel`
- JavaScript files use ABP's AJAX helpers and DataTables
- Modals follow ABP modal conventions

### HttpApi Layer (`Esh3arTech.HttpApi`)
Contains API controllers exposing application services as REST endpoints.

**Key folders**:
- `Controllers/`: API controllers inheriting from `Esh3arTechController`

**Conventions**:
- Controllers inherit from `Esh3arTechController` which inherits from `AbpControllerBase`
- ABP auto-generates REST endpoints from application services

### HttpApi.Client Layer (`Esh3arTech.HttpApi.Client`)
Contains HTTP client proxies for consuming the API.

## Configuration & Scripts

- `etc/scripts/`: PowerShell scripts for initialization and migration
- `etc/abp-studio/run-profiles/`: ABP Studio run configurations
- `.editorconfig`: Code style configuration
- `common.props`: Shared MSBuild properties

## Naming Conventions

- **Namespaces**: Follow folder structure (e.g., `Esh3arTech.Plans.Subscriptions`)
- **Entities**: PascalCase, singular (e.g., `UserPlan`, `Subscription`)
- **Services**: `{Entity}Manager` (domain), `{Entity}AppService` (application)
- **Repositories**: `I{Entity}Repository` (interface), `{Entity}Repository` (implementation)
- **Constants**: Defined in `Esh3arTechConsts` class with `Tbl` prefix for table names
- **Localization keys**: Use dot notation (e.g., `Menu:Plans`, `Clm:DisplayName`, `Enum:MessageStatus.0`)

## Module System

Each layer has a module class (e.g., `Esh3arTechDomainModule`, `Esh3arTechApplicationModule`) that:
- Inherits from `AbpModule`
- Declares dependencies via `[DependsOn]` attribute
- Configures services in `ConfigureServices` method

## File Organization

- Group related files by feature/aggregate (Plans, Messages, Subscriptions)
- Keep DTOs close to their service interfaces in Application.Contracts
- Entity configurations separate from entities in EntityFrameworkCore layer
- JavaScript files alongside their Razor Pages in Web layer
