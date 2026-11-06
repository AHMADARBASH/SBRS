# SBRS Screens and Roles
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

A screen-level map for the frontend team: what each screen does, which roles see it, and which API endpoints it calls. Roles abbreviations: **SA** SystemAdmin, **OA** OrganizationAdmin, **BA** BranchAdmin, **PC** ProjectCoordinator, **FA** FacilityAdmin, **DE** DataEntry.

Screen visibility is derived from the authorization policies in `SBRS/Authorization/Policies.cs` `[BUILT]` — the UI should show/hide screens by checking `GET api/Auth/me` (role) rather than guessing.

## 1. General screens

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| Login | username + password sign-in | all (anonymous) | `POST api/Auth/login` | `[BUILT]` |
| Session management | silent refresh, logout, logout everywhere | all | `POST api/Auth/refresh`, `POST api/Auth/logout`, `POST api/Auth/logout-all` | `[BUILT]` |
| Current user info | read role/org/facility to build the menu | all | `GET api/Auth/me` | `[BUILT]` |
| English UI toggle | switch interface language | all | — (no API support) | `[OPEN]` (OQ-09) |

## 2. Structure management (admin area)

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| Organizations list | list/search organizations | SA (sees all), OA (own only); others read | `GET api/organizations/GetAll`, `GetById/{id}` | `[BUILT]` |
| Organization add / update | create org, edit names | SA (add), SA+OA (update own) | `POST api/organizations/Add`, `PUT api/organizations/Update/{id}` | `[BUILT]` |
| Organization deactivate | bitwise disable | SA | — (endpoint commented out, OQ-15) | `[PLANNED]` |
| Branches list (by organization) | browse the org structure | SA, OA (others scoped read) | `GET api/Branches/GetAll`, `GetByOrganizationId/{organizationId}` | `[BUILT]` |
| Branch add / update | add branch (incl. HQ), rename | SA, OA | `POST api/Branches/Add`, `PUT api/Branches/Update/{id}` | `[BUILT]` |
| Branch deactivate | disable branch (guard: active facilities) | SA, OA | — (no endpoint, OQ-16) | `[PLANNED]` |
| Facility list (by branch) | browse facilities | SA, OA, BA, PC (scope), FA/DE (own) | `GET api/Facilities/GetByBranchId/{branchId}` | `[BUILT]` |
| Facility add / update | create facility (auto code, address, type, partner) | SA, OA | `POST api/Facilities/AddFacility`, `PUT …/UpdateFacility/{id}` | `[BUILT]` |
| Facility deactivate | disable facility | SA, OA | `POST api/Facilities/deactivateFacility/{id}` | `[BUILT]` |
| Facility partner history | view partner assignments, assign a new partner | SA | `GET api/facility-partners/GetByFacility/{facilityId}`, `POST …/Assign` | `[BUILT]` |

## 3. Lookup administration (SystemAdmin only)

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| Nationalities list/manage | maintain nationality list | SA manage; all read | `GET/POST/PUT api/nationalities/…` | `[BUILT]` |
| Identity types list/manage | maintain id document types + regex | SA manage; all read | `GET/POST/PUT api/identity-types/…` | `[BUILT]` |
| Facility types list/manage | maintain facility type codes | SA manage; all read | `GET/POST/PUT api/facility-types/…` | `[BUILT]` |
| Partners list/manage | maintain partner orgs | SA manage; all read | `GET/POST/PUT api/partners/…` | `[BUILT]` |
| Accommodation / Disability / Education types | maintain profile lookups | SA manage; all read | `GET/POST/PUT api/{accommodation,disability,education}-types/…` | `[BUILT]` (API only — not consumed yet, OQ-32) |

## 4. Geography and addresses

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| Geography browser (country → city → region/district → areas) | read-only cascading selects used in all address pickers | all logged-in | `GET api/Geography/Countries`, `countries/{id}/cities`, `cities/{id}/regions`, `cities/{id}/areas`, `regions/{id}/districts`, `districts/{id}/areas` | `[BUILT]` |
| Address list (per area) | find an existing address, incl. inactive filter | all logged-in | `GET api/Addresses/GetByAreaId/{areaId}?includeInactive=` | `[BUILT]` |
| Address add / update | add a shared address, fix names | SA, OA | `POST api/Addresses/Add`, `PUT api/Addresses/Update/{id}` | `[BUILT]` |
| Address deactivate | disable (guard: active facilities) | SA, OA | — (no endpoint, OQ-17) | `[PLANNED]` |
| Mobile working-location picker | choose the facility's working address for the day | FA, DE (mobile users) | — (WorkingAddresses table `[PLANNED]`) | `[PLANNED]` |

## 5. User administration

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| User list | list users by org/facility | SA | — (no GET endpoints, OQ-10) | `[OPEN]` |
| User create | pick role → dynamic scope fields (org/facility) | SA | `GET api/Users/Roles`, `POST api/Users/CreateUser` | `[BUILT]` |
| User edit | change name, role, scope, mobile flag | SA | `PUT api/Users/UpdateUser` | `[BUILT]` |
| User deactivate | disable an account (revokes refresh tokens) | SA | `POST api/Users/deactivateUser/{id}` (currently fails for every target — OQ-14) | `[BUILT]` ⚠ |
| Password reset | give a new password | SA | `POST api/Users/reset-password` | `[BUILT]` |

## 6. Beneficiary module (the core)

| Screen | Purpose | Roles | Endpoints | Status |
|---|---|---|---|---|
| Beneficiary search | find by FullCode **or** first+last name across all facilities; stop if the person exists | SA, FA, DE | `POST api/Beneficiaries/Search` | `[BUILT]` |
| Beneficiary register | search-first wizard: names, mother, birth date/year, gender, area, identity, enrollment date; shows match result | DE (only) — OQ-12 | `POST api/Beneficiaries/Register` | `[BUILT]` |
| Beneficiary list (my facility) | paginated roster, newest first | SA (this list), FA | `GET api/Beneficiaries/GetAll?page=&facilityId=` | `[BUILT]` |
| Beneficiary identities | documents of one beneficiary | SA, FA, DE (own facility) | `GET api/Beneficiaries/{id}/Identities` | `[BUILT]` |
| Match review queue | list records awaiting a match decision | SA, FA | `GET api/Beneficiaries/PendingReview` | `[BUILT]` |
| Match review detail | compare with candidates (scores + overlays), decide | SA, FA | `GET api/Beneficiaries/{id}/MatchCandidates`, `POST …/ConfirmSame`, `POST …/ConfirmNotAMatch` | `[BUILT]` |
| Beneficiary profile (demographics, disabilities, education) | extended 1:1 profile with disability types, accommodation, education | SA, FA, DE | — (BeneficiaryProfile `[PLANNED]`, OQ-23) | `[PLANNED]` |
| Beneficiary soft delete | hide a wrongly created record | SA, FA, DE | `DELETE api/Beneficiaries/{id}` | `[BUILT]` |

## 7. Planned modules (v1 backlog, endpoints do not exist)

| Screen | Purpose | Roles | Status |
|---|---|---|---|
| Clinics management | clinic entities per facility with types | SA, OA | `[PLANNED]` (UC-11) |
| Doctors management | doctors per clinic | SA, OA; read for FA | `[PLANNED]` (UC-11) |
| Medicines catalogue | add/propose/accept medicines | SA + OA propose/accept | `[PLANNED]` (UC-12) |
| Pharmacy transactions | dispensing records linked to beneficiary + medicine | pharmacist role (name open, OQ-01/02) | `[PLANNED]` (UC-13) — pharmacists must not register beneficiaries (ADR-015) |
| Reports | aggregated numbers within scope; aggregated-only vs detail `[OPEN]` (OQ-03) | SA, OA, BA, PC, FA | `[PLANNED]` (UC-14) |
| Facilities CRUD for OA screen completion | facility deactivate is SA/OA shared already | — | `[BUILT]` |

## 8. Role → module visibility summary

| Module | SA | OA | BA | PC | FA | DE |
|---|---|---|---|---|---|---|
| Structure (orgs, branches, facilities) | full | own org | read (empty today) | read (empty today) | own facility | own facility |
| Lookup admin | yes | no | no | no | no | no |
| Users | yes | no | no | no | no | no |
| Addresses/geography | add/update | add/update | read | read | read | read |
| Beneficiary register | no† | no | no | no | no | **yes** |
| Beneficiary search + list | yes | no | no | no | yes | yes |
| Match review | yes | no | no | no | yes | no |
| Pharmacy / clinics / reports | yes | yes | read | read | read | no |

† registers policy is DataEntry-only today — see [OQ-12](../OPEN-QUESTIONS.md#oq-12). BA/PC get empty scoped results until branch scoping is built ([OQ-28](../OPEN-QUESTIONS.md#oq-28)).

See also [roles-and-permissions.md](../01-analysis/roles-and-permissions.md) for the full matrix and policy mapping.
