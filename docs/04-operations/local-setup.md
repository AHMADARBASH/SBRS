# SBRS Local Setup
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

How to get a development environment running from a clean clone. Placeholders only — never store real secrets.

## 1. Prerequisites

| Tool | Version | Where the project fixes it |
|---|---|---|
| .NET SDK | 10.0 (10.0.403 known-working) | `net10.0` in every csproj; no `global.json` present |
| PostgreSQL | 14+ (citext built in) | connection string; citext extension is part of the model |
| EF Core tooling | 10.x | `dotnet tool install --global dotnet-ef` |

## 2. Clone and build

1. `git clone <REPO_URL>` and `cd SBRS`
2. `dotnet restore` then `dotnet build` (4 projects: Domain → Application → Infrastructure → Web).

## 3. Create the database

PostgreSQL database name is capitalised (`SBRS`-style, not lowercase) `[CONFIRMED]`:

```sql
CREATE DATABASE "SBRS";
-- application role with least privilege for runtime (dev shortcut: full owner while developing)
```

## 4. User secrets (project: `SBRS` web project — UserSecretsId is in SBRS.csproj)

```bash
dotnet user-secrets init --project SBRS
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<CONNECTION_STRING>" --project SBRS
dotnet user-secrets set "Jwt:Key" "<JWT_KEY_32_PLUS_CHARS>" --project SBRS
dotnet user-secrets set "SeedAdmin:UserName" "<ADMIN_USERNAME>" --project SBRS
dotnet user-secrets set "SeedAdmin:Password" "<ADMIN_PASSWORD>" --project SBRS
```

- `Jwt:Key` must be ≥ 32 characters (HS256). The settings class binds property **`Key`**, so the secret name must be exactly `Jwt:Key`.
- `SeedAdmin:*` is optional — without it startup logs a warning and skips admin creation (you get roles only).
- Connection string goes to secrets, not to `appsettings.json` (the json ships empty on purpose) `[BUILT]`.

## 5. Apply migrations

```bash
dotnet ef database update -p SBRS.Infrastructure -s SBRS
```

(Add a migration with the command in [database-and-migrations.md](database-and-migrations.md).)

## 6. Run and verify

```bash
dotnet run --project SBRS
```

1. On startup the `IdentitySeeder` runs: 6 roles created, first SystemAdmin created (if `SeedAdmin:*` set).
2. Open `http://localhost:5261/` (or `https://localhost:7105`) — Swagger UI is the site root.
3. Log in via `POST api/Auth/login` using the seed admin, paste the token into Swagger's Authorize (without `Bearer `).
4. Structure data does not exist until you: create lookups via the SystemAdmin APIs and/or run the (still missing) seed scripts (OQ-21).

## 7. Common startup failures

| Symptom | Cause |
|---|---|
| Swagger/CLI export crashes at startup (`GetServiceProvider`) | no reachable database — the seeder runs before serving |
| `The 'Jwt' settings section is missing` | missing user secrets |
| Swagger UI shows but login 401s | seed admin credentials not configured, or password policy violated at seed time |
| CORS errors on the web client | add the dev origin to `Cors:AllowedOrigins` (or `*` while developing — see OQ-22) |
