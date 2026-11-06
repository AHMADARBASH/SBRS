# UC-05 Manage users and assign role/scope
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Users |
| Primary actor | SystemAdmin |
| Implementation status | `[BUILT]` ⚠ deactivation currently fails for every target (OQ-14) |
| Related rules | BR-USR-01…10 |
| Endpoints | `GET api/Users/Roles`, `POST api/Users/CreateUser`, `PUT api/Users/UpdateUser`, `POST api/Users/deactivateUser/{id}`, `POST api/Users/reset-password` |

## Goal
SystemAdmin provisions every account, binds exactly one role and the matching scope, and manages the lifecycle.

## Preconditions
Actor is SystemAdmin; the referenced organization/facility exist.

## Main flow
1. SA loads the role metadata form `GET api/Users/Roles` (`requiresOrganizationId` / `requiresFacilityId` per role).
2. Create user: userName (3–50, `[A-Za-z0-9._-]`), password (≥ 8), firstName/lastName, role, and scope:
   - OrganizationAdmin → `organizationId` required
   - FacilityAdmin / DataEntry → `facilityId` required (org derived automatically)
   - BranchAdmin / ProjectCoordinator → rejected today ("cannot be managed yet") `[BUILT]` ⚠ OQ-28
   - `isMobile` flags mobile users.
3. `201` returns `UserDto`.
4. Editions: `UpdateUser` changes names, role, scope, `isMobile`; changing role/scope **revokes the user's refresh tokens** `[BUILT]`.
5. Deactivation: `POST deactivateUser/{id}` → 204 + refresh-token revocation `[BUILT]` (but see the bug below).
6. Password forgotten: `reset-password` sets a new password → 204.

## Alternative flows
- A1 SA re-issues their own password via reset (they cannot deactivate self or change own role — BR-USR-08).

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Duplicate username | `409` |
| Role without required scope field (or vice versa) | `400` validation |
| FA/DE without exactly one facility | `400` |
| SA deactivates anyone today | `400` "System Admin cannot be deactivated." — bug: handler checks caller role instead of target role (OQ-14) |
| BranchAdmin/ProjectCoordinator creation | `400` "Users with the role '…' cannot be managed yet." |
| No list/read endpoints | cannot build a user-admin table UI today (OQ-10) |

## Postconditions
- The user can (or can no longer) authenticate; a deactivated user's next refresh fails; their access token remains valid ≤ 4 h (OQ-05).

## Screens (for frontend)
- User admin console: create/edit forms with dynamic scope pickers following the selected role; password reset dialog. A list screen needs `GET users` (OQ-10).
