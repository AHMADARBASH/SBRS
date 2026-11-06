# ADR-002 MediatR pinned to the free 12.x line
> Status: Accepted | Date: unknown

## Context
MediatR announced licensing changes on its 13.x line; the project uses MediatR as the backbone of every request.

## Decision
Stay on the free 12.x line (pinned package version); do not upgrade to 13.x+ while its licensing is not accepted by the project owner.

## Consequences
- Behavior/feature set is stable; upgrading later is a deliberate re-licensed decision.
- The codebase must not depend on newer MediatR-only APIs.

## Alternatives considered
- Moving to MediatR-like alternatives (e.g. Wolverine) — not planned; the 12.x line serves.
- Rolling a homegrown dispatcher — rejected (restores boilerplate).
