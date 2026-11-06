# UC-01 Log in / refresh / log out
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Authentication |
| Primary actor | any user (mobile or web) |
| Implementation status | `[BUILT]` |
| Related rules | BR-USR-04, BR-USR-05, BR-USR-07, BR-AUTH-01…05 |
| Endpoints | `POST api/Auth/login`, `POST api/Auth/refresh`, `POST api/Auth/logout`, `POST api/Auth/logout-all`, `GET api/Auth/me` |

## Goal
Give the user a working authenticated session and let them end it deliberately on any device.

## Preconditions
- User exists in `AspNetUsers`, is active, has a role, is not locked out.
- Web clients send `X-Client-Type: web` on the auth endpoints.

## Main flow
1. User submits username + password.
2. API validates credentials (active check first), issues access JWT (240 min) and a 30-day rotating refresh token.
3. Web: refresh token is set as HttpOnly cookie `sbrs_rt` and the body's `refreshToken` is null; mobile: refresh token is in the body.
4. Client shows the role-based menu built from the response (`role`, `organizationId`, `facilityId`).
5. On access-token expiry: exactly **one** serialized `POST refresh` call; retry the failed request with the new token.
6. Logout: revoke the current family (`POST logout`, body or cookie); logout-all revokes every family plus the web cookie.

## Alternative flows
- A1 Web cookie absent on logout → still 204, no error.
- A2 `logout-all` on a user with several devices kills all sessions immediately (mobile included).

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Wrong password / unknown user / inactive / locked / no role | `401` with the same generic title (no reason leak) |
| Stored cookie missing on `refresh` (web) | `400` |
| Refresh token already used (burned) | `401`; whole family revoked — user must log in again |
| Refresh token expired / revoked | `401` |
| Access token used after logout/deactivation (≤ 4 h) | request still succeeds `[CONFIRMED]` caveat (OQ-05) |

## Postconditions
- Login: one active token family; audit columns stamped.
- Refresh: old token burned (`used_at`), new sibling in same family.
- Logout: family revoked (`revoked_at`), user redirected to login.

## Screens (for frontend)
- Login screen, role-based foyer/menu, session-expiry handler, "log out everywhere" button, account menu from `GET api/Auth/me`.

## Design details
See [../../03-integration/auth-guide.md](../../03-integration/auth-guide.md) and [../../02-design/workflows/authentication-and-tokens.md](../../02-design/workflows/authentication-and-tokens.md).
