# SBRS — Working Pattern (Architecture & Conventions)

Reference for adding a new feature. Derived from the FacilityTypes feature (kept in sync with the codebase, Oct 2026 version of the solution layout).

## Solution layout (Clean Architecture)

```
SBRS.slnx
├── SBRS.Domain            (entities, domain interfaces, constants, exceptions) — no deps
├── SBRS.Application       (CQRS features: MediatR commands/queries, DTOs, mappings, validators, behaviors)
├── SBRS.Infrastructure    (EF Core: DbContext, configurations, repositories, Identity, migrations)
└── SBRS                   (ASP.NET Core API host: controllers, contracts, authorization, middleware)
```

Cross-cutting flow: Controller → `ISender.Send(command/query)` → FluentValidation (pipeline behavior) → Logging behavior → Handler (Domain repo + `IUnitOfWork`) → EF Core → DTO returned. Exceptions are translated to ProblemDetails by `Middleware/GlobalExceptionHandler.cs` (Validation→400, NotFound→404, Conflict→409, Domain→400, Unauthorized→401, other→500).

## Namespaces

| Layer | Namespace |
|---|---|
| Domain entities | `SBRS.Domain.Entities` (Geography sub-namespace: `SBRS.Domain.Entities.Geography`) |
| Domain common / interfaces / constants / exceptions | `SBRS.Domain.Common` / `SBRS.Domain.Interfaces` / `SBRS.Domain.Constants` / `SBRS.Domain.Exceptions` |
| Application features | `SBRS.Application.Features.<Feature>.{Commands,Queries,DTOs,Mappings}` |
| Application common | `SBRS.Application.Common.{Behaviors,Exceptions,Interfaces}` |
| Infrastructure | `SBRS.Infrastructure`, `.Persistence`, `.Persistence.Repositories`, `.Persistence.Configurations` |
| API host | `SBRS`, `SBRS.Controllers`, `SBRS.Contracts.<Feature>`, `SBRS.Authorization`, `SBRS.Middleware` |

## Entity pattern (Domain)

- Entity class `<Name>Entity : BaseEntity` in `SBRS.Domain\Entities`.
- `BaseEntity` provides: `Guid Id`, `DateTime CreatedAt`, `DateTime? UpdatedAt`, `Guid? CreatedBy`, `Guid? UpdatedBy` (audit timestamps set in `AppDbContext.SaveChangesAsync`; CreatedBy/UpdatedBy currently never populated).
- Private parameterless ctor marked `// EF`.
- Public ctor receives required fields and calls a private `Set(...)` method.
- Properties with `private set`.
- `Set(...)` trims values and throws `DomainException` (namespace `SBRS.Domain.Exceptions`) when required fields are blank.
- Public `Update(...)` method calls `Set(...)`.
- A reusable `Common\LookupEntity` base exists (ArName/EnName/Code), but some entities (e.g. FacilityType) declare their own properties instead.

## Repository pattern (Domain + Infrastructure)

- Per-feature interface in `SBRS.Domain\Interfaces\I<Name>Repository.cs` — no generic repository. Typical members: `GetByIdAsync(Guid, CancellationToken)`, `GetAllAsync(CancellationToken)`, uniqueness check `XExistsAsync(value, Guid? excludeId, CancellationToken)`, `AddAsync(entity, CancellationToken)`.
- Implementation in `SBRS.Infrastructure\Persistence\Repositories\<Name>Repository.cs`, ctor-injects `AppDbContext` (primary constructor syntax).
- `GetAllAsync` uses `AsNoTracking()` and `OrderBy(...)` (e.g. by `Code` or `EnName`).
- Registered in `SBRS.Infrastructure\DependencyInjection.cs`: `services.AddScoped<I<Name>Repository, <Name>Repository>();`
- `IUnitOfWork` (`Task<int> SaveChangesAsync(CancellationToken ct = default)`) is registered from the same scoped `AppDbContext` — handlers use it to persist.

## Persistence pattern (EF Core)

- `AppDbContext` (IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>) exposes one `public DbSet<...> <Plural> => Set<...>();` per entity; `OnModelCreating` calls `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);`
- Per-entity config class `IEntityTypeConfiguration<<Name>Entity>` in `Persistence\Configurations\<Name>Configuration.cs`: `ToTable("<Plural>")`, `HasKey`, property `IsRequired()/HasMaxLength(...)`, unique index where applicable.
- Migrations: run in `SBRS.Infrastructure` project — `dotnet ef migrations add <Name> --project ..\SBRS.Infrastructure` (files `yyyyMMddHHmmss_<Name>.cs` + `.Designer.cs` under `Persistence\Migrations`).

## Application feature pattern (CQRS)

Per feature folder `SBRS.Application\Features\<Feature>\`:

- `Commands\Add<Name>Command.cs` — one file containing:
  - `public sealed record Add<Name>Command(...) : IRequest<<Name>Dto>;`
  - `public sealed class Add<Name>CommandValidator : AbstractValidator<...>` (FluentValidation: NotEmpty / MaximumLength matched to EF config)
  - `public sealed class Add<Name>CommandHandler(...) : IRequestHandler<..., Dto>` — checks uniqueness → `ConflictException`; creates entity, `AddAsync`, `uow.SaveChangesAsync`, returns `entity.ToDto()`.
- `Commands\Update<Name>Command.cs` — record with `Guid Id` + fields; handler: `GetByIdAsync` → `NotFoundException(nameof(Entity), id)` when missing; uniqueness check with `excludeId`; `entity.Update(...)`; save; return DTO.
- `Queries\Get<Name>sQuery.cs` — `IRequest<IReadOnlyList<<Name>Dto>>`; no pagination exists in this codebase (plain lists). `IncludeInactive`-style flags are passed through the query.
- `DTOs\<Name>Dto.cs` — `public sealed record <Name>Dto(Guid Id, ...);`
- `Mappings\<Name>Mappings.cs` — static class with `public static <Name>Dto ToDto(this <Name>Entity e) => new(...);`
- Handler ctor uses C# 12 primary constructors; DI: `AddApplicationDI()` (SBRS.Application) registers MediatR + validators + `ValidationBehavior` + `LoggingBehavior` automatically from assemblies.

## API host pattern

- Request/response contracts: `SBRS\Contracts\<Feature>\<Add/Update><Name>Request.cs` — `public sealed record ... ({ ... });`
- Controller: `SBRS\Controllers\<Feature>Controller.cs` — `[ApiController]`, `[Route("api/<kebab-case>")]`, ctor-injects `ISender` (primary constructor). Endpoints:
  - `[HttpGet("GetAll")]` — anonymous (auth-only via FallbackPolicy)
  - `[HttpPost("Add")]` and `[HttpPut("Update")]` with `[Authorize(Policy = Policies.SystemAdminOnly)]`
  - Add returns `StatusCode(StatusCodes.Status201Created, dto)`; Update returns `Ok(dto)`.
- Auth: `Authorization\Policies.cs` (`FallbackPolicy = RequireAuthenticatedUser()`); roles from `SBRS.Domain\Constants\Roles.cs`.

## Checklist for a new feature

1. Domain: `Entities\<Name>Entity.cs`, `Interfaces\I<Name>Repository.cs`.
2. Application: `Features\<Name>s\{Commands (Add/Update files), Queries, DTOs, Mappings}`.
3. Infrastructure: `Persistence\Repositories\<Name>Repository.cs`, `Persistence\Configurations\<Name>Configuration.cs`, DbSet in `AppDbContext`, `AddScoped` in `AddInfrastructureDI`, EF migration.
4. Host: `Contracts\<Name>s\...Request.cs`, `Controllers\<Name>sController.cs` with policies. GlobalExceptionHandler already maps all custom exceptions.
5. Build: `dotnet build SBRS.slnx`.
