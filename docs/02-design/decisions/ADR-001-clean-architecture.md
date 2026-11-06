# ADR-001 Clean architecture with MediatR, FluentValidation, no AutoMapper
> Status: Accepted | Date: 2026-10-08 (project start)

## Context
A small backend team maintains a domain with strong invariants (scoping, match rules, coding). Longevity and testability matter more than raw delivery speed, and mapping overhead should stay transparent.

## Decision
Four projects in strict dependency order: Domain → Application → Infrastructure → Web. Cross-cutting request processing uses MediatR with pipeline behaviors; input validation uses FluentValidation validators registered per feature; mapping is done by hand via `ToDto` extension methods (no AutoMapper). Repositories live in Infrastructure behind Domain interfaces; handlers own `IUnitOfWork.SaveChangesAsync`.

## Consequences
- One file per operation keeps review diffs small; conventions are discoverable (see architecture.md §3).
- Manual mapping is more code but explicit and refactor-friendly; nothing hidden.
- MediatR pipeline makes validation/logging uniform without base classes.

## Alternatives considered
- Layered N-tier with services — rejected: domain rules would leak into services.
- AutoMapper — rejected: runtime mapping bugs and debug friction.
- Minimal APIs with inline logic — rejected: no structure for 10+ modules.
