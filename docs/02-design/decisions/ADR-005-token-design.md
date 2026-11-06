# ADR-005 JWT access token 4 h + single-use rotating refresh token 30 d with reuse detection
> Status: Accepted | Date: 2026-10-13/14 (auth feature; refresh hardening 2026-10-26)

## Context
Field users live with unstable connectivity; frequent re-logins are unacceptable, but stolen tokens must not live forever.

## Decision
Access token: JWT, 240 minutes, HS256 with `Jwt:Key`, claims sub/role/jti/organization_id/facility_id/is_mobile, `ClockSkew = Zero`. Refresh token: 30 days, single-use, rotating within a family, stored SHA-256 hashed (`refresh_tokens.token_hash` unique); atomic claim via conditional UPDATE (`used_at`); reusing a burned token revokes the whole family. Refresh reloads the user (active/role/lockout check). Logout revokes the family; logout-all revokes every family.

## Consequences
- Good UX for long shifts; a stolen refresh token self-destructs on reuse.
- Accepted caveat: access tokens stay valid ≤ 4 h after logout/deactivation (OQ-05).
- Burned rows need a cleanup job (`[PLANNED]`, OQ-31).

## Alternatives considered
- Sliding sessions / opaque session ids checked per request — rejected for now: per-request DB hits; candidate if the 4 h gap proves unacceptable (with short cache).
- 15–30-minute access tokens — the direct OQ-05 option.
