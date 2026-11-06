# SBRS UAT Checklist
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Manual acceptance pass before each release. Mark Pass/Fail per step; every step maps to a use case and business rule. Test accounts: see [../03-integration/environments-and-test-data.md](../03-integration/environments-and-test-data.md).

## UC-01 Login / refresh / logout (BR-USR-04…07, BR-AUTH-*)

| # | Step | Role | Expected | Pass/Fail |
|---|---|---|---|---|
| 1 | Login with valid credentials | DE | 200 + token, refreshToken body (mobile) / cookie (web) | |
| 2 | Login with wrong password ×N | DE | 401 every time (lockout behaviour per OQ-36) | |
| 3 | Login inactive user | any | 401 | |
| 4 | Expired access token → serialized refresh | DE | new tokens; retried request 200 | |
| 5 | Replay used refresh token | DE | 401 + family revoked; login required | |
| 6 | logout → replay same token | DE | 401, token family revoked | |
| 7 | logout-all after second device login | SA | both devices re-auth needed; inactive user also kicked on refresh | |

## UC-02/03/04 Structure management (BR-ORG, BR-BRN, BR-FAC)

| # | Step | Role | Expected | Pass/Fail |
|---|---|---|---|---|
| 8 | Add organization with unique code | SA | 201 | |
| 9 | Add organization duplicate code | SA | 409 | |
| 10 | OA updates own organization title; attempts foreign org update | OA | 200; 404 (foreign) | |
| 11 | SA adds branch to new org; OA adds branch to own org; OA adds to foreign | SA/OA | 201/201/404 | |
| 12 | Deactivated org update attempt | SA | 400 "cannot be updated" | |
| 13 | Create facility: pick branch, address, type, partner (+project code) | SA/OA | 201 with generated code `{c_code}-{type}-{NNN}` | |
| 14 | Facility code collides/repeats city count sanity | SA/OA | unique codes in list | |
| 15 | Assign new partner earlier than open start | SA | 400 (start date rule) | |
| 16 | Assign new partner (valid) → history shows old row closed `end_date = new_start − 1` | SA | GetByFacility ordering correct | |
| 17 | Deactivate facility then GetAll with `includeInactive=true` | OA | appears inactive | |
| 18 | Add facility to deactivated branch | SA | 400 "Cannot add … deactivated branch" | |

## UC-05 Users (BR-USR-01…10)

| # | Step | Role | Expected | Pass/Fail |
|---|---|---|---|---|
| 19 | `GET Roles` shape | SA | array with requirements flags | |
| 20 | Create FA without facility | SA | 400 | |
| 21 | Create FA with facility; org derived | SA | 201, `organizationId` set | |
| 22 | Create OA (org), DE (facility, isMobile) | SA | 201 | |
| 23 | Create BranchAdmin/ProjectCoordinator | SA | 400 "cannot be managed yet" (OQ-28) | |
| 24 | Duplicate username | SA | 409 | |
| 25 | SA deactivates DE account **(known bug OQ-14)** | SA | currently 400 "System Admin cannot be deactivated" — FAIL expected until fixed | |
| 26 | After deactivation, DE's refresh | DE | 401 | |
| 27 | reset password, old login | SA | 204; old password 401; new 200 | |

## UC-06 Geography/addresses (BR-GEO)

| # | Step | Role | Expected | Pass/Fail |
|---|---|---|---|---|
| 28 | cascade picks: countries → cities → districts → areas | any user | lists populated (seed data!) | |
| 29 | duplicate address name in area | OA | 409 | |
| 30 | list addresses includeInactive=true & false | any | both modes behave | |

## UC-07/08/09/10 Beneficiary core (BR-BEN-*)

| # | Step | Role | Expected | Pass/Fail |
|---|---|---|---|---|
| 31 | Search by FullCode of known person | DE | 1 hit, limited demographics | |
| 32 | Search first+last name of Arabic-variant twin spellings | DE | finds via normalization | |
| 33 | Search with 1-char name | DE | 400 message | |
| 34 | Register clean beneficiary | DE | result fullCode, status NoMatchFound | |
| 35 | Register same 5-field person again | DE (register) | saves as PendingReview (disambiguator) | |
| 36 | Register with existing NATIONAL_ID (same doc) | DE | PendingReview; candidates score 100 SameIdentity | |
| 37 | Register UNREGISTERED with text value | DE | 400 | |
| 38 | Register NATIONAL_ID wrong format (9 digits) | DE | 400 | |
| 39 | SA calls Register | SA | **403** (OQ-12 documented reality) | |
| 40 | PendingReview list shows the case in FA queue | FA | oldest first | |
| 41 | ConfirmSame → roster hides new record; its FullCode resolves survivor | FA | hidden in GetAll/Search | |
| 42 | ConfirmNotAMatch with reason | FA | stays listable, status ConfirmedDistinct | |
| 43 | Out-of-scope beneficiary GET identities | FA (other facility) | 404 | |
| 44 | GetAll paging (34 registrations) | FA | page 1 = 30, hasNext true | |
| 45 | SA GetAll without facilityId | SA | 400 | |
| 46 | DELETE (soft) wrongly created | DE | 204; not in roasters/search | |

## UC-11…14 Planned modules

Not testable against the API (nothing built) — verify the UI shows them as disabled/hidden until implemented `[PLANNED]`.

## Deployment smoke test (every release)

| # | Step | Expected | Pass/Fail |
|---|---|---|---|
| S1 | `systemctl is-active sbrs` | active | |
| S2 | Open `<PROD_BASE_URL>/swagger` (or root) | UI or API responds | |
| S3 | Login with a production test user | 200 | |
| S4 | Beneficiary search + list round trip | no errors in `journalctl` | |
| S5 | Check migrations applied (compare `__EFMigrationsHistory`) | matches release notes | |
