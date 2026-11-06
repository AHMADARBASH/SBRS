# SBRS Roles and Permissions
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Two sources are reconciled here: the intended matrix (project decisions) and the real authorization code (`SBRS/Authorization/Policies.cs` + `[Authorize]` attributes + repository scoping). Mismatches are listed in §6 and tracked in [OPEN-QUESTIONS](../OPEN-QUESTIONS.md).

## 1. Role descriptions

| Role | What they do | Scope basis | Verified scope today |
|---|---|---|---|
| **SystemAdmin** | Owns the whole system: organizations, branches, facilities, partners, lookups, users; participates in search/list/review | none (global); may pass `facilityId` on beneficiary list | `[BUILT]` |
| **OrganizationAdmin** | Creates/updates branches and facilities of **their own organization**, manages addresses, updates own organization | `organization_id` claim derived from their facility/branch assignment | `[BUILT]` (org-scoped queries); OA is bound via facility → branch → org |
| **BranchAdmin** | Branch-level oversight of facilities and beneficiaries | `branch_id` claim `[PLANNED]` | Not built — role exists, gets empty scoped results (OQ-28) |
| **ProjectCoordinator** | Program oversight across assigned facilities | assigned-facilities table `[PLANNED]` | Not built — role exists, gets empty scoped results (OQ-28) |
| **FacilityAdmin** | Runs one facility: views its roster, reviews match cases | `facility_id` claim | `[BUILT]` |
| **DataEntry** | Registers and searches beneficiaries, views own facility roster, also on mobile (`is_mobile`) | `facility_id` claim | `[BUILT]` |
| **Pharmacy staff** | Dispensing only; must never register beneficiaries | facility-based `[PLANNED]` | Role does not exist — name/design open (OQ-01, OQ-02, ADR-015) |

Rules `[CONFIRMED]`: exactly **one role per user**; a user is bound to scope fields that the system derives (never hand-picked OrganizationId); deactivation revokes refresh tokens; deactivation of self is forbidden.

## 2. Authorization policies (`Policies.cs`) `[BUILT]`

| Policy | Roles included |
|---|---|
| *(fallback policy)* | any authenticated user — endpoints without an attribute still require login |
| `SystemAdminOnly` | SystemAdmin |
| `OrganizationLevel` | SystemAdmin, OrganizationAdmin |
| `BranchLevel` | SystemAdmin, OrganizationAdmin, BranchAdmin |
| `DataEntry` | all six roles (a "everything-logged-in" alias; **not** the DataEntry role only) |
| `RegisterBeneficiaries` | DataEntry only |
| `SearchBeneficiaries` | DataEntry, FacilityAdmin, SystemAdmin |
| `ReviewBeneficiaryMatches` | FacilityAdmin, SystemAdmin |
| `ViewFacilityBeneficiaries` | DataEntry, FacilityAdmin, SystemAdmin |
| `DeactivateBeneficiaries` | DataEntry, FacilityAdmin, SystemAdmin |

Controller application `[BUILT]`:

| Controller | Policy |
|---|---|
| Users (whole controller) | `SystemAdminOnly` |
| Accommodation/Disability/Education types, Facility types, Identity types, Nationalities, Partners — Add/Update | `SystemAdminOnly` |
| Facility partners (whole controller) | `SystemAdminOnly` |
| Organizations Add | `SystemAdminOnly`; Update `OrganizationLevel`; GetAll/GetById fallback |
| Branches Add/Update | `OrganizationLevel` |
| Facilities Add/Update/Deactivate | `OrganizationLevel` (scope: FA/DE not permitted at policy level — see OQ on BR-FAC-06) |
| Addresses Add/Update | `OrganizationLevel` |
| Beneficiaries Search/Register/List/Review/Deactivate | the four beneficiary policies above |

## 3. Role × action matrix per module

Legend: ✅ allowed · 📖 read-only · — no access · 🔜 `[PLANNED]`. Scope is in parentheses.

| Module / action | SA | OA | BA | PC | FA | DE |
|---|---|---|---|---|---|---|
| **Organizations** add | ✅ | — | — | — | — | — |
| **Organizations** update | ✅ | ✅ (own) | — | — | — | — |
| **Organizations** view | 📖 all | 📖 own | 📖 | 📖 | 📖 | 📖 |
| **Branches** add/update | ✅ (all) | ✅ (own org) | 🔜 | — | — | — |
| **Branches** view | 📖 all | 📖 own | 📖 own branch 🔜 | 📖 | 📖 own | 📖 own |
| **Facilities** add/update/deactivate | ✅ (all) | ✅ (own org) | 🔜 | — | — | — |
| **Facilities** view | 📖 all | 📖 own | 📖 own 🔜 | 📖 own 🔜 | 📖 own | 📖 own |
| **Facility partner history** view/assign | ✅ | — | — | — | — | — |
| **Facility types / identity types / nationalities / partners** add/update | ✅ | — | — | — | — | — |
| **Accommodation/disability/education types** add/update | ✅ | — | — | — | — | — |
| **Geography** (countries/cities/regions/districts/areas) | 📖 | 📖 | 📖 | 📖 | 📖 | 📖 |
| **Addresses** add/update | ✅ | ✅ (own org's facilities' areas) | 🔜 | — | — | — |
| **Addresses** view | 📖 | 📖 | 📖 | 📖 | 📖 | 📖 |
| **Users** create/update/deactivate/reset | ✅ | — | — | — | — | — |
| **Users** view | 🔜 (no endpoints, OQ-10) | — | — | — | — | — |
| **Beneficiary search** (cross-facility) | ✅ | — | — | — | ✅ (own facility identity only) | ✅ (own facility) |
| **Beneficiary register** | — † | — | — | — | — | ✅ (own facility) |
| **Beneficiary list** (facility roster) | ✅ (any facility via param) | — | — | — | ✅ (own) | ✅ (own) |
| **Beneficiary identities** view | ✅ | — | — | — | ✅ (own) | ✅ (own) |
| **Match review** (ConfirmSame / ConfirmNotAMatch) | ✅ (all facilities) | — | — | — | ✅ (own) | — |
| **Beneficiary soft delete** | ✅ | — | — | — | ✅ (own) | ✅ (own) |
| **Clinics/doctors** manage 🔜 | ✅ | ✅ (own org) | 📖 | 📖 | 📖 | — |
| **Medicines** propose 🔜 | ✅ | ✅ (own org) | — | — | — | — |
| **Pharmacy transactions** 🔜 | ✅ (all) | ✅ (own) | 📖 | 📖 | 📖 | — (pharmacist role TBF) |
| **Reports** 🔜 | ✅ (all) | ✅ (own org) | 📖 (own branch) | 📖 (assigned) | 📖 (own) | — |

† Register policy is DataEntry-only in the code, while the intended rule says SystemAdmin "can do both" (search + register) — tracked as [OQ-12](../OPEN-QUESTIONS.md#oq-12). The register handler also hard-requires a `facility_id` claim, which SystemAdmin does not have.

## 4. Scope rules per role

- **Scope is computed server-side** from the JWT claims (`sub`, `role`, `organization_id`, `facility_id`; `branch_id` `[PLANNED]`) via `ICurrentUser`, and applied by a private `Scoped()` query in each repository `[BUILT]`.
- OrganizationAdmin's organization is **derived** from their bound facility → branch → organization chain, not chosen `[BUILT]`.
- FacilityAdmin and DataEntry: exactly one facility. BranchAdmin: one branch `[PLANNED]`. ProjectCoordinator: ≥ 1 assigned facility, expected within one organization `[CONFIRMED]/[ASSUMED]` (OQ-04) `[PLANNED]`.
- Code-uniqueness ("does this code already exist?") checks are deliberately **unscoped** because codes are unique system-wide `[BUILT]`.
- Out-of-scope records → `404` (ADR-006) `[BUILT]`.
- SystemAdmin has no facility; beneficiary *register* therefore fails for them today (needs `facility_id`) — part of OQ-12.

## 5. Where the matrix and the code disagree

| # | Difference | Tracking |
|---|---|---|
| 1 | Intended: SA can register beneficiaries. Code: `RegisterBeneficiaries` = DataEntry only | [OQ-12](../OPEN-QUESTIONS.md#oq-12) |
| 2 | Intended: FA/DE can add/update their own facility (BR-FAC-06). Code: facility mutation is `OrganizationLevel` only | [OQ-33](../OPEN-QUESTIONS.md#oq-33) (aFA/FA facility-edit policy) — see business-rules BR-FAC-06 |
| 3 | Intended: BranchAdmin has real powers. Code: role exists, no scope, no endpoints beyond read-with-empty-results | [OQ-28](../OPEN-QUESTIONS.md#oq-28) |
| 4 | Intended: ProjectCoordinator exists for oversight. Code: assigned facilities not built | [OQ-04](../OPEN-QUESTIONS.md#oq-04) + OQ-28 |
| 5 | Intended: pharmacy role separation (ADR-015). Code: no pharmacy role/facility handling yet | [OQ-01](../OPEN-QUESTIONS.md#oq-01), [OQ-02](../OPEN-QUESTIONS.md#oq-02) |
| 6 | Ambiguity: the `DataEntry` **policy** name includes all six roles — easy to misread as "DataEntry users only" | noted in code comments; keep as-is unless renamed |

If OQ-33 does not exist yet when you read this, it was just added: FA/DE being able to edit their own facility is intended by BR-FAC-06 but **not** permitted by the current `OrganizationLevel` policy.

## 6. Screens per role

Full screen-by-endpoint mapping lives in [../03-integration/screens-and-roles.md](../03-integration/screens-and-roles.md).
