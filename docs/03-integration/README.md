# SBRS Integration Pack — Start Here
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

You are building a web or mobile client for the SBRS API. Read the files in this order:

1. [api-conventions.md](api-conventions.md) — dates, casing, 404-vs-403 rule, pagination, soft delete.
2. [auth-guide.md](auth-guide.md) — login, tokens, the HttpOnly cookie for web, refresh rules.
3. [endpoint-reference.md](endpoint-reference.md) — every route, body and role.
4. [error-catalog.md](error-catalog.md) — how errors look, which messages to surface.
5. [enums-and-lookups.md](enums-and-lookups.md) — gender, match statuses, roles, identity regexes, lookup endpoints.
6. [screens-and-roles.md](screens-and-roles.md) — screen-by-screen map with the endpoints each screen calls.
7. [environments-and-test-data.md](environments-and-test-data.md) — base URLs, seed accounts, fixtures.

## Base conventions in 10 lines

- JSON over HTTPS; all routes under `api/…`; camelCase fields; GUID ids.
- Send `Authorization: Bearer <access_token>` on everything except the anonymous auth endpoints.
- Access token lives in memory only; the refresh token is an HttpOnly cookie (web, send `X-Client-Type: web`) or the JSON body (mobile).
- Serialize refresh calls; a reused burned refresh token revokes the whole token family.
- Out-of-scope data is **404**, not 403. 403 = your role cannot use the endpoint.
- All errors are RFC 7807 `application/problem+json`; validation adds an `errors` dictionary.
- Dates are ISO 8601 `yyyy-MM-dd`; timestamps UTC. Gender is `true`=male.
- Deletion is always soft (`isActive=false`); deactivated records are hidden by default.
- Beneficiary lists are paginated (30/page); all other lists return full arrays.
- Records are identified by generated codes (`DAM-PHC-001`, `DAM-PHC-001-57`).

## OpenAPI / typed clients

`openapi.json` **could not be exported** while these docs were written: starting the app needs a reachable PostgreSQL database (`IdentitySeeder` runs before serving), and the Swashbuckle CLI fails at startup for the same reason. Generate it yourself with one of:

```bash
# Option A — from the running API (needs a database):
curl http://<DEV_BASE_URL>/swagger/v1/swagger.json -o docs/03-integration/openapi.json

# Option B — Swashbuckle CLI against the built dll (still needs a database):
dotnet tool install --global Swashbuckle.AspNetCore.Cli
ConnectionStrings__DefaultConnection='<CONNECTION_STRING>' Jwt__Key='<JWT_KEY>' \
  swagger tofile --output docs/03-integration/openapi.json SBRS/bin/Debug/net10.0/SBRS.dll v1
```

Commit the file after generating, and re-run it after every API change (maintenance rule 4).

Then generate a typed client:

| Stack | Tool | Notes |
|---|---|---|
| Flutter (Dart) | `openapi-generator` with `dart-dio` | generates a dio client + models; handles nullable fields well [ASSUMED] |
| Web (TypeScript) | `openapi-typescript` (types) or NSwag (client) | pair with a small typed fetch wrapper adding the Bearer header |
