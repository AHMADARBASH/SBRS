# SBRS Enums and Lookup Data
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Every enum value and every lookup list a client needs. Values are read from the code (`SBRS.Domain/Enums`, `SBRS.Domain/Constants`) `[BUILT]`.

> ⚠ **Lookups are not seeded in the repository.** No SQL or EF seeder exists for geography, nationalities, identity types, facility types, partners, accommodation/disability/education types (see [OQ-21](../OPEN-QUESTIONS.md#oq-21) and `IdentityTypeCodes`/regexes above). These DTO/API tables provide the data — until seed data is added, item counts are unknown `[OPEN]`.

## 1. Gender

Stored/transferred as a **boolean** (`bool` in JSON) `[CONFIRMED][BUILT]`:

| Value | Meaning | Arabic |
|---|---|---|
| `true` | **Male** (bit `1`) | ذكر |
| `false` | **Female** (bit `0`) | أنثى |

There is no "unspecified" value; the register request requires gender.

## 2. Beneficiary match status (`BeneficiaryMatchStatus`) — what the *system* concluded [BUILT]

| Value | Name | Meaning |
|---|---|---|
| `0` | `NotFound` | no similar beneficiary found |
| `1` | `Probable` | scored probable match (≥ 70 points), needs human review |
| `2` | `Exact` | identical MatchKey (all five fields) **or** same identity document type + value |

## 3. Beneficiary match review status (`BeneficiaryMatchReviewStatus`) — what the *human* decided [BUILT]

| Value | Name | Meaning |
|---|---|---|
| `0` | `NoMatchFound` | normal registration, nothing to review |
| `1` | `PendingReview` | match detected at registration; a FacilityAdmin must decide |
| `2` | `ConfirmedDistinct` | facility admin confirmed: this is a different person (with an optional reason) |
| `3` | `MergedIntoExisting` | facility admin merged this record into an existing beneficiary (record no longer appears in lists/search) |

## 4. Match reason flags (`BeneficiaryMatchReasons`) [BUILT]

Bitmask field (`number` in JSON) on `BeneficiaryMatchDto.reasons`. Sum the bits:

| Bit | Name | Meaning |
|---|---|---|
| 0 | `None` | no reason flag set |
| 1 | `SameFirstName` | first name equal after normalization |
| 2 | `SameFatherName` | father name equal |
| 4 | `SameLastName` | last name equal |
| 8 | `SameMotherName` | mother name equal |
| 16 | `SameBirthDate` | birth date equal |
| 32 | `SameBirthYear` | year equal and at least one date is estimated (half of the birth-date weight) |
| 64 | `SimilarSpelling` | name matched by edit distance, not equality |
| 128 | `SameMotherLastNameBirthDate` | possible twins: first name differs, everything else matches — always shown to a human |
| 256 | `SameIdentity` | same identity type + document number → 100% certain even if names differ |

Scoring detail (for context; the client only shows `score` and `reasons`): first name 25, father 15, last 20, mother 25, birth date 15; probable threshold 70 `[ASSUMED]` — see [business-rules.md](../01-analysis/business-rules.md).

## 5. Roles (`Roles.All`) [BUILT]

| Role | Scope requirement | Warehouse/mobile notes |
|---|---|---|
| `SystemAdmin` | (no facility) | never bound; can pass facilityId on beneficiary list |
| `OrganizationAdmin` | organizationId (from branch/facility) | created by SystemAdmin |
| `BranchAdmin` | branchId `[PLANNED]` not built yet | scoped empty today |
| `ProjectCoordinator` | ≥ 1 facility `[PLANNED]` (assigned-facilities table not built) | scoped empty today |
| `FacilityAdmin` | exactly one facilityId | reviews matches |
| `DataEntry` | exactly one facilityId | registers beneficiaries, searches, lists |

The `GET api/Users/Roles` endpoint returns these as `[{ role, requiresOrganizationId, requiresFacilityId }]` so the frontend can build the role picker dynamically.

## 6. Identity types (`IdentityTypeCodes`) and value regexes [BUILT]

The list of identity types in the DB comes from seed data `[OPEN]` — but these are the fixed codes the system recognizes and validates (`IdentityValuePatterns.cs`):

| Code | Value regex | Notes |
|---|---|---|
| `FAMILY_BOOK` | `^[\p{L}\p{N}/-]{1,30}$` | flexible letters/digits/slash/dash |
| `VACCINATION_CARD` | same flexible | |
| `PASSPORT` | `^[A-Za-z0-9/-]{1,20}$` | Latin/digits only, ≤ 20 |
| `NATIONAL_ID` | `^[0-9]{10}$` | exactly 10 ASCII digits (`\d` would match Arabic-Indic digits — deliberately avoided) |
| `CIVIL_REGISTRY` | flexible | |
| `DRIVING_LICENSE` | flexible | |
| `BIRTH_CERTIFICATE` | flexible | |
| `DISABILITY_CERTIFICATE` | flexible | |
| `FAMILY_STATEMENT` | flexible | |
| `MARRIAGE_CERTIFICATE` | flexible | |
| `UNREGISTERED` | `^$` | value **must be empty** (no identity document) |

- Before the regex runs, the value is trimmed and Arabic/Persian digits are converted to ASCII (`IdentityValueNormalizer`) `[BUILT]`. The regexes are case-sensitive for `PASSPORT`.
- Planned Arabic names of seeded types (from the original seed plan; not verified in repo because no seeder exists): بيان عائلي, دفتر عائلة, جواز سفر, وثيقة زواج, بطاقة لقاحات, هوية وطنية, شهادة ميلاد, شهادة إعاقة, شهادة قيادة, قيد مدني, غير مسجل `[ASSUMED]`.
- `GET api/identity-types/GetAll` returns the seeded rows with `id, arName, enName, code, valueRegex` — **read the actual list from this endpoint**, not from this table.

## 7. Lookup lists (DB tables seeded outside the repo)

These are plain admin-managed lookup tables. In each case fetch them through their endpoints at runtime; do not hardcode ids.

| Lookup | Endpoint | Fields | Managed by | Data status |
|---|---|---|---|---|
| Countries / Cities / Regions / Districts / Areas | `GET api/Geography/…` (hierarchical, see [endpoint-reference](endpoint-reference.md)) | `id, arName, enName, code` (+`cCode` for cities, +`cityId` on areas) | read-only; seeded from OCHA Syria admin boundaries `[PLANNED]` [ASSUMED seed source] | `[OPEN]` no seeder (OQ-21) |
| Nationalities | `GET api/nationalities/GetAll` | `id, arName, enName` | SystemAdmin | `[OPEN]` |
| Identity types | `GET api/identity-types/GetAll` | `id, arName, enName, code, valueRegex` | SystemAdmin | `[OPEN]` |
| Facility types | `GET api/facility-types/GetAll?includeInactive=` | `id, arName, enName, code` | never deleted, only inserted; SystemAdmin can add/update | `[OPEN]` |
| Partners | `GET api/partners/GetAll` | `id, arName, enName, code` | SystemAdmin | `[OPEN]` |
| Accommodation types | `GET api/accommodation-types/GetAll` | `id, arName, enName` | SystemAdmin | `[OPEN]` — not consumed yet (OQ-32) |
| Disability types | `GET api/disability-types/GetAll` | `id, arName, enName` | SystemAdmin | `[OPEN]` — not consumed yet (OQ-32) |
| Education types | `GET api/education-types/GetAll` | `id, arName, enName` | SystemAdmin | `[OPEN]` |
| Education issues | — | — | — | `[PLANNED]` lookup for "reason why no education" `[OPEN]` (OQ-07) — nothing built |

City `cCode` values used in facility codes (DAM, ALP, RUD, … for SY01-SY14) live in the `cities` seed `[OPEN]` — read them from `GET api/Geography/countries/{id}/cities`.

## 8. Reserved codes

- Beneficiary `FullCode` = `{facility code}-{per-facility sequence}` with **no zero padding** (e.g. `DAM-PHC-001-57`) `[BUILT]` — see [business-rules.md](../01-analysis/business-rules.md).
- Facility `Code` = `{city cCode}-{facility type code}-{3-digit sequence}` (e.g. `DAM-HC-001`) `[BUILT]`.
