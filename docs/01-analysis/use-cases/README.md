# SBRS Use Case Index
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| ID | Title | Primary actor | Module | Status |
|---|---|---|---|---|
| [UC-01](UC-01-login-and-session.md) | Log in / refresh / log out | any user | Authentication | `[BUILT]` |
| [UC-02](UC-02-manage-organizations.md) | Manage organizations | SystemAdmin (+OA update) | Organizations | `[BUILT]` |
| [UC-03](UC-03-manage-branches.md) | Manage branches | SystemAdmin, OrganizationAdmin | Branches | `[BUILT]` (deactivate `[PLANNED]`) |
| [UC-04](UC-04-manage-facilities.md) | Manage facilities (type, address, partner, partner history) | SystemAdmin, OrganizationAdmin | Facilities | `[BUILT]` |
| [UC-05](UC-05-manage-users.md) | Manage users and assign role + scope | SystemAdmin | Users | `[BUILT]` ⚠ deactivation bug (OQ-14) |
| [UC-06](UC-06-geography-and-addresses.md) | Browse geography and manage addresses | all (manage: SA, OA) | Geography / Addresses | `[BUILT]` |
| [UC-07](UC-07-search-beneficiary.md) | Search for a beneficiary (FullCode or first+last name) | SA, FA, DE | Beneficiaries | `[BUILT]` |
| [UC-08](UC-08-register-beneficiary.md) | Register a beneficiary (search-first + match detection) | DataEntry | Beneficiaries | `[BUILT]` |
| [UC-09](UC-09-review-match-cases.md) | Review beneficiary match cases | FacilityAdmin (+SA) | Beneficiaries | `[BUILT]` |
| [UC-10](UC-10-facility-beneficiary-list.md) | List my facility's beneficiaries (paginated) | FA, DE (+SA) | Beneficiaries | `[BUILT]` |
| [UC-11](UC-11-manage-clinics-doctors.md) | Manage clinics and doctors | SA, OA | Clinics `[PLANNED]` | `[PLANNED]` |
| [UC-12](UC-12-manage-medicines.md) | Manage medicines (add, remove, accept) | SA, OA | Pharmacy `[PLANNED]` | `[PLANNED]` |
| [UC-13](UC-13-pharmacy-transactions.md) | Pharmacy transactions | Pharmacy role `[OPEN]` | Pharmacy `[PLANNED]` | `[PLANNED]` |
| [UC-14](UC-14-view-reports.md) | View reports within scope | SA, OA, BA, PC, FA | Reporting `[PLANNED]` | `[PLANNED]` |

Rules each use case relies on: [../business-rules.md](../business-rules.md). Endpoint details: [../../03-integration/endpoint-reference.md](../../03-integration/endpoint-reference.md).
