# SBRS Documentation
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

This is the documentation hub of the SBRS (Syria Beneficiary Registration System) API project.

## Status tags used everywhere in these docs

| Tag | Meaning |
|---|---|
| `[CONFIRMED]` | decided by the project owner |
| `[ASSUMED]` | proposal the owner has not confirmed yet |
| `[OPEN]` | undecided question (tracked in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md)) |
| `[PLANNED]` | designed, not built yet |
| `[BUILT]` | verified in the running code |

## Index

| Area | Start file | Audience |
|---|---|---|
| Analysis & requirements | [01-analysis/vision-and-scope.md](01-analysis/vision-and-scope.md) · [use-cases/README.md](01-analysis/use-cases/README.md) | Owner, SARC management, new team members |
| Design & decisions | [02-design/architecture.md](02-design/architecture.md) | Backend, reviewers |
| Integration (build screens) | [03-integration/README.md](03-integration/README.md) | Web/Frontend, mobile developers |
| Operations | [04-operations/local-setup.md](04-operations/local-setup.md) | DevOps, backend |
| Testing | [05-testing/test-strategy.md](05-testing/test-strategy.md) | QA, backend |
| Open questions | [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) | Everyone |

## Maintenance rules

1. **A feature is not finished until the matching docs are updated in the same commit.**
2. **New decisions get an ADR** under `02-design/decisions/` (see the template there).
3. **Breaking API changes are recorded** in [03-integration/changelog.md](03-integration/changelog.md).
4. **Re-export `openapi.json`** after every API change; see [03-integration/README.md](03-integration/README.md).
5. IDs (`BR-*`, `UC-*`, `ADR-*`, `OQ-*`) are never reused or renumbered.
6. The code is the source of truth for facts; if a doc and the code disagree, fix the doc and raise an `OQ-*` if the disagreement looks like a code bug.
7. Never put secrets (keys, passwords, connection strings, host names) in any file.

## Known limitations of this documentation

- `docs/03-integration/openapi.json` could not be exported during writing (no database available in the writing environment); see the note in [03-integration/README.md](03-integration/README.md).
- No EF/SQL seeders exist for the lookup tables (geography, nationalities, etc.); only the `IdentitySeeder` is built, so lookup seed values could not be verified from the seed data (see [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md)).
