# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Build and Run
- **Build:** `dotnet build`
- **Run:** `dotnet run --project SBRS`
- **Migrations:** Uses Entity Framework Core with Npgsql (PostgreSQL). Run migrations via `dotnet ef` inside the `SBRS.Infrastructure` project or from the root pointing to `SBRS` as the startup project.

## Architecture
This project follows a **Clean Architecture** (Onion Architecture) pattern:

- **SBRS.Domain**: Core entities and abstractions. Contains no dependencies on other projects.
- **SBRS.Application**: Contains business logic, MediatR handlers, and interfaces. Depends only on `SBRS.Domain`.
- **SBRS.Infrastructure**: Contains implementation of data access (EF Core), authentication (JWT), and external services. Depends on `SBRS.Application`.
- **SBRS** (API): Web API entry point. Depends on `SBRS.Application` and `SBRS.Infrastructure`.

## Development Conventions
- **C# / .NET 10.0:** The project uses modern C# features and .NET 10.
- **DI:** Dependency Injection is handled via `Microsoft.Extensions.DependencyInjection`. Check `SBRS` or `SBRS.Infrastructure` for dependency registration (usually via extension methods).
- **MediatR:** Used for command/query pattern within the `Application` layer.
- **CORS:** Configured in `Program.cs`. Ensure any new origins are added to the `Cors:AllowedOrigins` configuration section.
