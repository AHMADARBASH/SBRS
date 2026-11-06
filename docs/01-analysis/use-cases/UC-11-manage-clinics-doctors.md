# UC-11 Manage clinics and doctors
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Clinics `[PLANNED]` |
| Primary actor | SystemAdmin, OrganizationAdmin |
| Implementation status | `[PLANNED]` — no entities, endpoints or tables |
| Related rules | (planned) extend BR-FAC: facility → 1..n clinics → doctors; scoping as per facilities |
| Endpoints | none (planned e.g. `api/clinics`, `api/doctors`) |

## Goal
Represent each clinical unit inside a facility (e.g. ER, vaccination room) and the doctors working there, so pharmacy transactions and reports can attach to a clinic.

## Preconditions
Facilities exist (UC-04).

## Main flow (proposed)
1. OA/SA creates a clinic under a facility: names, type, active flag.
2. Doctors are added per clinic (personal identity, specialty, active flag) by SA/OA.
3. FA reads the list of their facility's clinics/doctors for daily work.
4. Pharmacy transactions (UC-13) reference the clinic and the dispensing doctor.

## Alternative flows
- A1 Clinic deactivation follows BR-GEN-02 (soft delete), guarded when attached transactions exist.

## Errors and edge cases
| Situation | Expected behaviour (proposed) |
|---|---|
| Out-of-scope clinic access | `404` (BR-GEN-05) |
| Parent immutability | clinic's facility cannot change after creation (BR-GEN-03) |

## Postconditions
- Clinics/doctors usable by downstream modules `[PLANNED]`.

## Screens (for frontend)
- Facility detail tab "Clinics", clinic form, doctors table.

> Open dependency: none of this exists in the code; the shape above follows the module conventions and awaits owner confirmation when pharmacy design lands (OQ-01).
