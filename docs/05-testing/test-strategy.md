# SBRS Test Strategy
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

What exists, and what must be tested first. **No test projects exist in the repo** — no unit, integration, or E2E suites today `[OPEN]` (NFR-MNT-03). Everything so far is manual testing through Swagger and `docs/api-access.md`. This document is the starting contract for as soon as a test project is added.

## 1. Current state

| Layer | Exists? |
|---|---|
| Unit tests (domain: normalizer, scorer, code builders, identity regexes) | ✘ |
| Integration tests (in-memory/EF + real PostgreSQL containers) | ✘ |
| Policy/authorization tests (per role) | ✘ |
| Manual scripts (`Swagger`, `docs/api-access.md`) | ✔ implicit |

## 2. Test-first list (highest risk first)

### 2.1 Scoping (security-critical)
| Case | Expected |
|---|---|
| FA reads other facility's beneficiary by id | **404** (not 403) |
| OA updates another org's organization | **404** |
| DE reads foreign facility roster | scoped/404 |
| SA passes `facilityId` explicitly | sees any facility |
| Code-exists probes across facilities | still return true (unscoped by design) |
| BranchAdmin/ProjectCoordinator reads | empty results today (OQ-28) — test documents reality |

### 2.2 Authorization policies
| Case | Expected |
|---|---|
| Anonymous access to protected endpoint | 401 |
| Wrong role (e.g. DE creating a user) | 403 |
| DE register / SA register / FA register | 201-equivalent / **403** / 403 (OQ-12 reality) |
| Each policy vs each controller action matrix | matches [roles-and-permissions](../01-analysis/roles-and-permissions.md) |

### 2.3 Refresh-token rotation and reuse
| Case | Expected |
|---|---|
| me-refresh twice sequentially | success, new token each time, old burned |
| Same refresh token twice (reuse) | second → **401**, whole family `revoked_at` set |
| Refresh after user deactivation | 401 |
| Refresh of inactive/locked/role-less user | 401 |
| Login of locked-out user | resolved by lockout investigation (OQ-36) — currently never locks |

### 2.4 Code generation
| Case | Expected |
|---|---|
| First facility in city X type PHC | code `X-PHC-001` |
| Third facility same city/type | `X-PHC-003` |
| Facility code collision retry | retries; on exhaustion 409 |
| Beneficiary sequential registrations same facility | codes 1,2,3 gaps-free under `FOR UPDATE` |
| Same beneficiary code painter in two facilities | distinct FullCodes (`F1-x`, `F2-x`) |
| Merged full code search | resolves to survivor |

### 2.5 Match detection with Arabic name variants
| Case | Expected |
|---|---|
| Same person, alef variants (أ/إ/آ) | Exact key match |
| ة vs ه, ى vs ي | Exact |
| With/without diacritics + tatweel | Exact |
| Different person, one letter edit distance | Probable (SimilarSpelling) then review |
| Twin pattern | 2 candidate rows, always PendingReview |
| Estimated birth year only | half weight on birth date |
| Same NATIONAL_ID digits in Arabic-Indic | normalized to ASCII → Exact (SameIdentity) |
| UNREGISTERED with a value | 400 |

### 2.6 Validation behaviors (400 contract, RFC 7807 shape)
| Case | Expected |
|---|---|
| Missing field | 400 + `errors` dictionary entry |
| Enrollment/default `0001-01-01` | currently accepted ⚠ (OQ-18); red test once fixed |
| Names > 100 chars | 400 (validator), not DB 500 |
| FacilityTypes name > 100 | **currently 500** ⚠ (odds-and-ends) — test pins to a fix |

## 3. Suggested test layout `[PLANNED]`

```
SBRS.Domain.Tests    — pure unit tests (no mocks): normalizer, scorer, builders, regexes
SBRS.Application.IntegrationTests — handlers + EF (Testcontainers PostgreSQL or in-memory + citext caveat)
SBRS.Api.IntegrationTests — WebApplicationFactory + policies + token flows
```

⚠ In-memory EF providers do **not** emulate `citext`, `FOR UPDATE` locks or raw SQL — these tests need a real PostgreSQL (containers).

## 4. Manual regression pack (until suites exist)

Run after every release, per [uat-checklist.md](uat-checklist.md): login/refresh matrix for all six roles, register+match review round trip, policy 401/403 matrix, deployment smoke test.
