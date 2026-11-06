# SBRS Configuration Reference
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Every setting the API reads. Values in this table are placeholders, never real ones.

## 1. In `SBRS/appsettings.json` (committed, empty values)

| Key | Purpose | Example placeholder | Required in prod | Read by |
|---|---|---|---|---|
| `Logging:LogLevel:Default` | log verbosity | `Information` | yes | default logging |
| `Logging:LogLevel:Microsoft.AspNetCore` | framework logs | `Warning` | yes | default logging |
| `AllowedHosts` | host filter | `*` (or your domain) | tighten to your host | host filtering middleware |
| `ConnectionStrings:DefaultConnection` | PostgreSQL DSN | `<CONNECTION_STRING>` (host, db `SBRS`, user, password) | **yes** (secret) — ships empty | `AddInfrastructureDI` → `AppDbContext` |
| `Jwt:Issuer` | JWT issuer claim | `SBRS` | yes | `AddAppDI` (ValidateIssuer) |
| `Jwt:Audience` | JWT audience claim | `SBRS.Clients` | yes | `AddAppDI` (ValidateAudience) |
| `Jwt:AccessTokenExpiryMinutes` | access token lifetime | `240` | yes | `TokenService` |
| `Jwt:RefreshTokenExpiryDays` | refresh token lifetime | `30` | yes | `RefreshTokenService` |
| `Cors:AllowedOrigins` | allowed browser origins array | `["*"]` in dev; `<PROD_ORIGIN>` list in prod | yes (see note) | `Program.cs` |

⚠ `Cors:AllowedOrigins: ["*"]` compiles into *allow all origins with credentials* — acceptable in dev, must be an explicit origin list in production (OQ-22).

## 2. Secrets (user secrets in dev / EnvironmentFile in prod) — values never in files

| Key | Purpose | Placeholder | Missing → behaviour |
|---|---|---|---|
| `Jwt:Key` | HS256 signing key, ≥ 32 chars | `<JWT_KEY>` | app fails: JWT section missing/invalid |
| `ConnectionStrings:DefaultConnection` | DB DSN | `<CONNECTION_STRING>` | app fails: seeder/connection |
| `SeedAdmin:UserName` | first SystemAdmin username | `<ADMIN_USERNAME>` | warning logged, seeder skips admin |
| `SeedAdmin:Password` | first SystemAdmin password | `<ADMIN_PASSWORD>` | as above |
| `SeedAdmin__UserName` / `SeedAdmin__Password` | EnvironmentFile naming (double underscore) | `<ADMIN_USERNAME>` / `<ADMIN_PASSWORD>` | as above |

`Jwt:Issuer`/`Jwt:Audience` may stay in appsettings; `Jwt:Key` must be secret.

## 3. Optional `RefreshCookie:*` section (all values optional)

Bound to `RefreshCookieSettings` (`SectionName = "RefreshCookie"`) `[BUILT]`:

| Key | Purpose | Default behaviour |
|---|---|---|
| `RefreshCookie:Name` | cookie name | `sbrs_rt` |
| `RefreshCookie:Path` | cookie path | `/api/auth` |
| `RefreshCookie:SameSite` | SameSite mode | `Lax` |
| `RefreshCookie:Secure` | Secure flag | true |
| `RefreshCookie:Domain` | cookie domain | host-only |

## 4. Environment-level (not appsettings)

| Variable | Purpose | Notes |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | gates Swagger UI (enabled in Dev **and** Production bucket code) and HTTPS redirect behaviour |
| `ASPNETCORE_URLS` | listen addresses | e.g. `http://127.0.0.1:5000` behind reverse proxy |
| `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `SeedAdmin__*` | secret override | double-underscore nested keys in env vars |
| `DOTNET_CLI_TELEMETRY_OPTOUT`, `DOTNET_NOLOGO` | CI/build conveniences | set in azure-pipelines.yml |

## 5. Notes

- No `appsettings.Development.json` exists today — Development shares `appsettings.json`.
- `appsettings.Production.json` is excluded from the deploy rsync (it is host-specific if you ever add it) `[BUILT]` (pipeline).
- Swagger/OpenAPI: enabled in both Development and Production buckets (OQ-25).
