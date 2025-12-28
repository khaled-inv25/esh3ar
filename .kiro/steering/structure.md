# Project Structure

## Solution Organization

The solution follows **ABP Framework's layered architecture** based on Domain-Driven Design (DDD) principles.

## Core Projects

### Domain Layer

- **Esh3arTech.Domain.Shared**: Enums, constants, shared types (no dependencies)
  - Localization resources (ar.json, en.json)
  - Domain constants (MessageConsts, UserConsts, SubscriptionConsts)
  - Enums (MessageStatus, MessageType, Priority, BillingInterval, SubscriptionStatus)

- **Esh3arTech.Domain**: Domain entities, domain services, repositories
  - Entities: Message, MobileUser, UserPlan, Subscription, RegistrationRequest
  - Domain services: MessageManager, OneWayMessageManager, UserPlanManager, SubscriptionManager
  - Specifications: PendingMessageSpecification, MobileVerifiedSpecification
  - Background jobs: EmailSendingJob
  - Settings & feature providers

### Application Layer

- **Esh3arTech.Application.Contracts**: DTOs, interfaces, permissions
  - Application service interfaces (IMessageAppService, IPlanAppService, ISubscriptionAppService)
  - DTOs for input/output
  - Permission definitions (Esh3arTechPermissions)
  - Feature definitions (Esh3arTechFeatureDefinitionProvider)
  - Event Transfer Objects (ETOs) for distributed events

- **Esh3arTech.Application**: Application service implementations
  - Application services: MessageAppService, PlanAppService, SubscriptionAppService, UserAppService
  - AutoMapper profiles
  - Business logic orchestration

### Infrastructure Layer

- **Esh3arTech.EntityFrameworkCore**: EF Core implementation
  - DbContext configuration (Esh3arTechDbContext)
  - Entity configurations (MessageConfiguration, SubscriptionConfiguration)
  - Custom repositories (UserPlanRepository, SubscriptionRepository)
  - Migrations
  - Background workers (IdentifySubscriptionsDueForRenewalWorker)

### Presentation Layer

- **Esh3arTech.HttpApi**: HTTP API controllers
  - Base controller (Esh3arTechController)
  - API models

- **Esh3arTech.HttpApi.Client**: HTTP client proxies for remote API calls

- **Esh3arTech.Web**: ASP.NET Core MVC/Razor Pages application
  - Pages: Messages, Plans, Subscriptions
  - Components: Toolbar, ViewComponents
  - SignalR Hubs: OnlineMobileUserHub
  - Services: OnlineUserTrackerService, MessagesDeliveryHandler
  - Static files (wwwroot)

### Utility Projects

- **Esh3arTech.DbMigrator**: Console app for database migrations and seeding

- **Esh3arTech.Abp.Media**: Separate microservice for media/blob handling
  - MediaController for file upload/download
  - Runs independently from main web app

- **Esh3arTech.Abp.Blob**: Blob storage service library
  - BlobService, IBlobService

## Folder Conventions

### Domain Entities
- Located in `src/Esh3arTech.Domain/{EntityName}/`
- Entity classes use aggregate root pattern
- Specifications in `Specs/` subfolder

### Application Services
- Located in `src/Esh3arTech.Application/{EntityName}/`
- Corresponding contracts in `src/Esh3arTech.Application.Contracts/{EntityName}/`
- DTOs suffixed with `Dto` (e.g., MessageDto, CreatePlanDto)

### EF Core Configurations
- Entity configurations in `src/Esh3arTech.EntityFrameworkCore/EntityFrameworkCore/{EntityName}/`
- Configuration classes suffixed with `Configuration` (e.g., MessageConfiguration)

### Web Pages
- Razor pages in `src/Esh3arTech.Web/Pages/{EntityName}/`
- JavaScript files alongside .cshtml files
- Modals suffixed with `Modal.cshtml`

## Naming Conventions

- **Entities**: PascalCase, singular (Message, MobileUser, Subscription)
- **DTOs**: PascalCase with `Dto` suffix (MessageDto, SendOneWayMessageDto)
- **Interfaces**: PascalCase with `I` prefix (IMessageAppService, IBlobService)
- **App Services**: PascalCase with `AppService` suffix (MessageAppService)
- **Managers**: PascalCase with `Manager` suffix (MessageManager, UserPlanManager)
- **Repositories**: PascalCase with `Repository` suffix (UserPlanRepository)
- **Permissions**: Const strings in Esh3arTechPermissions class
- **Table names**: Defined in Esh3arTechConsts (e.g., TblMessage)

## Key Patterns

- **Repository Pattern**: Custom repositories for complex queries
- **Specification Pattern**: For reusable query logic (e.g., PendingMessageSpecification)
- **Factory Pattern**: MessageFactory for creating different message types
- **Event-Driven**: Distributed events (ETOs) via RabbitMQ for async processing
- **Unit of Work**: Automatic transaction management via ABP
- **Authorization**: Attribute-based with [Authorize(Esh3arTechPermissions.X)]
