# SBRS Glossary
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Terms | Arabic | Definition, and where the concept lives (table / endpoint).

## 1. Domain terms

| Term | Arabic | Definition | Where used |
|---|---|---|---|
| Beneficiary | مستفيد | A real person served by SARC; registered **once** system-wide, regardless of how many facilities serve them. Table `beneficiaries` | `api/Beneficiaries/*` |
| Beneficiary code (`Code`) | — | The person's sequence number **inside the registering facility** (per-facility counter, no padding) | `beneficiaries.code` |
| Full code (`FullCode`) | الرقم الكامل | `{facility code}-{code}`, unique system-wide, the public handle for a beneficiary (e.g. `DAM-PHC-001-57`) | `beneficiaries.full_code` |
| MatchKey | — | SHA-256 hash of the five normalized fields (first, father, last, mother names + birth date) that detects exact duplicates | `beneficiaries.match_key` |
| Disambiguator | — | 1, 2, … appended logically when two records share the exact same MatchKey (kept under human review) | `beneficiaries.disambiguator` |
| Match review | مراجعة التطابق | The FacilityAdmin decision over a PendingReview record: merge into an existing person or ConfirmDistinct | `beneficiaries.match_review_status` |
| Identity document | وثيقة هوية | One document per person per type (national id, passport, …) with a validated value | `beneficiary_identities`, `api/Beneficiaries/{id}/Identities` |
| Registration facility | مركز التسجيل | The facility whose DataEntry registered the person; defines their scope/code basis (`RegistrationFacilityId`) until `FacilityBeneficiary` exists | `beneficiaries.registration_facility_id` |
| Facility | مركز / منشأة | A service point (hospital, PHC, SARC branch HQ…) of one branch; has a generated code, one address, one type, one current partner | `facilities` |
| Branch | فرع | A SARC branch of one organization; the HQ is a branch like any other | `branches` |
| Organization | منظمة / جمعية | A top-level entity (e.g. SARC itself or a partner org owning branches) | `organizations` |
| Partner | شريك / جهة داعمة | A supporting organization assigned to facilities, with history | `partners`, `facilities_partners` |
| Project code | رمز المشروع | Free ≤50-char tag of a facility-partner assignment; optional in API, stored as empty string | `facilities_partners.project_code` |
| Address | عنوان | A shared, human-readable location inside one area (street/landmark); unique per (area, name) | `addresses` |
| Geography hierarchy | التقسيم الإداري | Country → City → Region → District → Area, read-only, OCHA pcodes | `countries, cities, regions, districts, areas`, `api/Geography/*` |
| Area code (`pcode`) | — | OCHA place code (the `Code` column of each geography row); cities expose the short `cCode` (DAM, ALP…) | `cities.c_code` |
| Scope | نطاق الصلاحية | The slice of data a user may see, derived from their role + org/facility; out-of-scope is 404 | `ICurrentUser.Scoped()` in repositories |
| Soft delete | تعطيل | `IsActive = false` with audit stamps; nothing is ever hard-deleted | every table `is_active` |
| Match status | حالة التطابق | Machine conclusion at registration NotFound/Probable/Exact | `BeneficiaryMatchStatus` |
| Review status | حالة المراجعة | NoMatchFound / PendingReview / ConfirmedDistinct / MergedIntoExisting | `BeneficiaryMatchReviewStatus` |
| Merged (beneficiary) | مدمج | A record whose data lives on in the survivor after ConfirmSame; hidden from lists, its FullCode resolves to the survivor | match review |
| Twin pattern | توأم محتمل | Same mother+last name+birth date, different first name — always queued for human review (`SameMotherLastNameBirthDate` flag) | `BeneficiaryMatchReasons` |
| Normalized name | الاسم الموحّد | Name after trim, diacritics/tatweel removal, alef unification, ى→ي, ة→ه, Arabic→ASCII digits | `ArabicTextNormalizer` |
| Estimated birth date | تاريخ ميلاد تقريبي | Known only the year → stored as 1 Jan with `IsBirthDateEstimated = true` | `beneficiaries.is_birth_date_estimated` |
| Refresh token family | عائلة الرموز | Tokens issued from one login chain; reuse of a burned token revokes the whole family | `refresh_tokens.family_id` |

## 2. Actors

| Term | Arabic | Definition |
|---|---|---|
| SystemAdmin | مدير النظام | Global administrator; the only role that manages users, all lookups, and can act system-wide |
| OrganizationAdmin | مدير المنظمة | Manages one organization's branches/facilities/addresses |
| BranchAdmin | مدير الفرع | Branch-level oversight `[PLANNED]` (scope not wired) |
| ProjectCoordinator | منسق المشروع | Oversight across their assigned facilities `[PLANNED]` |
| FacilityAdmin | مدير المركز | Runs one facility; reviews match cases; views the facility roster |
| DataEntry | مدخل بيانات | Registers and searches beneficiaries at one facility (web or mobile) |
| Pharmacy staff | صيدلي | `[PLANNED]` separate role that must never register beneficiaries (ADR-015, OQ-01/02) |

## 3. Entities inventory (`SBRS.Domain/Entities`, all `[BUILT]` tables)

| Entity | Table | Note |
|---|---|---|
| `CountryEntity` / `CityEntity` / `RegionEntity` / `DistrictEntity` / `AreaEntity` | `countries`, `cities`, `regions`, `districts`, `areas` | read-only geography |
| `AddressEntity` | `addresses` | shared, mutable names |
| `OrganizationEntity` | `organizations` | |
| `BranchEntity` | `branches` | |
| `FacilityEntity` | `facilities` | generated code, current partner |
| `FacilityPartnerEntity` | `facilities_partners` | assignment history |
| `FacilityTypeEntity` | `facility_types` | insert-only, no IsActive |
| `PartnerEntity` | `partners` | |
| `BeneficiaryEntity` | `beneficiaries` | match fields, review status |
| `BeneficiaryIdentityEntity` | `beneficiary_identities` | one per (beneficiary, type) |
| `NationalityEntity` | `nationalities` | |
| `IdentityTypeEntity` | `identity_types` | code + value regex |
| `AccommodationTypeEntity` / `DisabilityTypeEntity` / `EducationTypeEntity` | `accommodation_types`, `disability_types`, `education_types` | reserved for BeneficiaryProfile (OQ-32) |
| *(planned)* `FacilityBeneficiary`, `BeneficiaryProfile`, beneficiary disabilities, `WorkingAddresses`, clinics/doctors/medicines/pharmacy | — | see [OPEN-QUESTIONS](../OPEN-QUESTIONS.md) and UC-11…14 |

## 4. API/tech terms

| Term | Definition |
|---|---|
| Access token | JWT, 240 minutes; carries sub/role/organization_id/facility_id claims |
| Refresh token | 30-day single-use rotating opaque token, stored SHA-256 hashed |
| `X-Client-Type: web` | Header that switches the auth endpoints to the HttpOnly cookie flow |
| `PagedResult<T>` | `{ items, page, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage }` |
| 404-scoping rule | Out-of-scope records return 404, never 403 (ADR-006) |
| `DomainException` | Domain rule violation; mapped to HTTP 400 |
| `citext` | PostgreSQL case-insensitive text type, applied to `facilities.code` and `beneficiaries.full_code` `[BUILT]` |
