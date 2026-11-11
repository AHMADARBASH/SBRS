# ADR-014 Refresh token delivery: HttpOnly cookie for web, body for mobile
> Status: Accepted — built | Date: 2026-10-26 ([BUILT]: `RefreshCookieService` + `X-Client-Type` flow)

## Context
Web SPAs cannot safely store refresh tokens in JS-readable storage (XSS); mobile apps cannot rely on cookies and have secure OS storage instead. One auth API must serve both safely.

## Decision
The auth endpoints branch on the `X-Client-Type: web` request header: web clients receive the refresh token as an **HttpOnly cookie** (`sbrs_rt`, path `/api/Auth` — the casing must match the `api/[controller]` route because browser cookie-path matching is case-sensitive; `Secure`, `SameSite=Lax` — configurable via `RefreshCookie:*` settings) and the response body's `refreshToken` is null; every other client gets the token in the JSON body. Sending the body value is ignored for web; logout/refetch without a cookie is a no-op `204`. CORS must send credentials only to explicit origins.

## Consequences
- XSS on web cannot steal the refresh token (never readable by JS).
- The custom header doubles as CSRF hardening for the cookie flow (simple cross-site form posts cannot attach it) `[BUILT]`.
- Multi-tab web apps must serialize refresh calls — parallel refreshes trigger reuse-detection family revocation (see auth guide).
- Mobile devs: never implement the web header; use secure storage.

## Alternatives considered
- Cookies for all clients — rejected: mobile cookie management is fragile.
- Tokens in body for web too — rejected: XSS exposure.
