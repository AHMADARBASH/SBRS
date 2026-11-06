# UC-14 View reports within scope
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Reporting `[PLANNED]` |
| Primary actor | SystemAdmin (all), OrganizationAdmin, BranchAdmin `[PLANNED]`, ProjectCoordinator `[PLANNED]`, FacilityAdmin (own) |
| Implementation status | `[PLANNED]` — no reporting endpoints |
| Related rules | OQ-03 aggregated-only vs individual records; distinct-beneficiary counting; privacy rules NFR-PRV-05 |
| Endpoints | none (planned e.g. `api/reports/*`) |

## Goal
Give each level of the hierarchy trustworthy numbers about registrations and services, strictly within the viewer's scope.

## Scope rules (proposed)
| Viewer | Sees |
|---|---|
| SystemAdmin | everything |
| OrganizationAdmin | own organization |
| BranchAdmin | own branch `[PLANNED]` (OQ-28) |
| ProjectCoordinator | their assigned facilities (OQ-04) |
| FacilityAdmin | own facility |
| DataEntry | none (or own-facility basics) |

## Main flow (proposed)
1. Viewer picks a date range and dimension (period, area, nationality, gender…).
2. API returns aggregated rows (counts by dimension) — **aggregated by default**.
3. Every beneficiary-derived count uses **distinct BeneficiaryId** — a person registered once but enrolled in many facilities must count once (Section 4 intent).
4. The decision whether drill-down to individual records is permitted at all is `[OPEN]` (OQ-03).

## Alternative flows
- A1 Data quality report: PendingReview backlog per facility (helps UC-09 adoption).

## Errors and edge cases
| Situation | Expected behaviour (proposed) |
|---|---|
| Out-of-scope filter values | sanitized to the viewer's scope (never trust client-side scope) |
| Merged duplicates | counted once as the survivor person |

## Postconditions
- Numbers reconcile with the registry (no double counting of merged records).

## Screens (for frontend)
- Dashboard with dimension filters; export of CSV/PDF `[ASSUMED]`.

> Open dependency: OQ-03 (aggregated vs individual) must be answered before the report API is designed.
