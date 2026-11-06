# SBRS Deployment (Linux + systemd)
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Production is one Linux machine with the .NET 10 API, PostgreSQL and systemd. **Everything here that is not in `azure-pipelines.yml` is `[ASSUMED]`** — the current repo contains the pipeline and restart convention, nothing about nginx or TLS.

## 1. What the CI pipeline does today (`azure-pipelines.yml`, root + `SBRS/`) `[BUILT]`

1. Triggers on `master`; agent pool `SBRS-Pool` (self-hosted Linux agent).
2. `dotnet publish SBRS/SBRS.csproj -c Release -o <publish>` and verify `SBRS.dll` exists.
3. `rsync -av --delete <publish>/ /var/www/sbrs/SBRS/` — excluding `appsettings.Production.json`, `wwwroot/uploads`, `logs` (host-local files survive deploys).
4. `sudo systemctl restart sbrs` then check `systemctl is-active` (+ `journalctl -u sbrs -n 50` on failure).

⚠ The pipeline does **not** apply EF migrations and does **not** run seeds — apply migrations manually around deploys (see [database-and-migrations.md](database-and-migrations.md)).

A second, simpler checkout-based rsync pipeline exists at repo root (deploys sources to the same path instead of publish output) — presumably legacy; keep only one active to avoid double deploys `[ASSUMED]`.

## 2. Systemd unit (the file is host-local, not in the repo) `[ASSUMED]`

```ini
# /etc/systemd/system/sbrs.service
[Unit]
Description=SBRS API
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/sbrs/SBRS
ExecStart=/usr/bin/dotnet /var/www/sbrs/SBRS/SBRS.dll
Restart=on-failure
RestartSec=5
User=<SERVICE_USER>
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
# secrets via an EnvironmentFile readable ONLY by the service user:
EnvironmentFile=/etc/sbrs/sbrs.env

[Install]
WantedBy=multi-user.target
```

`/etc/sbrs/sbrs.env` (chmod 600, owner `<SERVICE_USER>`):

```ini
ConnectionStrings__DefaultConnection=Host=<DB_HOST>;Port=5432;Database=SBRS;Username=<DB_USER>;Password=<DB_PASSWORD>
Jwt__Key=<JWT_KEY_32_PLUS>
SeedAdmin__UserName=<ADMIN_USERNAME>
SeedAdmin__Password=<ADMIN_PASSWORD>
```

Nested keys use double underscores in environment files `[CONFIRMED]`.

## 3. Reverse proxy and HTTPS `[ASSUMED]`

- nginx (or similar) terminates TLS for `<PROD_HOST>` and proxies to `http://127.0.0.1:5000`.
- Forwarded headers are already honored by the app (`UseForwardedHeaders: X-Forwarded-For/Proto`) `[BUILT]`.
- Certificate renewal (certbot or ACME-companion) `[ASSUMED]`.
- Keep `Cors:AllowedOrigins` as an explicit origin list in production (OQ-22) and consider disabling Swagger UI in production (OQ-25).

## 4. Database user with least privilege `[ASSUMED]`

- Runtime user: `CONNECT` on DB, `USAGE` on `public`, CRUD on all app tables (`GRANT SELECT,INSERT,UPDATE,DELETE`), usage of the `citext` extension — **no** DDL rights; migrations run by a separate admin identity.
- Optional read-only reporting user later.

## 5. Update procedure (a release)

1. Merge to `master`; pipeline publishes and restarts.
2. Before merge (locally): add the migration; after deploy: `dotnet ef database update -p SBRS.Infrastructure -s SBRS` from a machine with production connectivity `[ASSUMED]` (or run it on the server against the publish output).
3. Watch logs: `journalctl -u sbrs -f`; verify startup seeder logs; smoke-test login.

## 6. Rollback

- `sudo systemctl stop sbrs`; restore the previous publish dir (keep a copy of the current one before rsync-based deploys — `--delete` makes it destructive) `[ASSUMED]`; restart.
- Migrations: prefer forward-fix migrations; only roll back with `dotnet ef migrations remove` on a copy — never hand-edit applied migrations.

## 7. Logs and monitoring `[ASSUMED]`

- Console logging → journald (`journalctl -u sbrs`).
- No structured sink (Serilog/seq) configured — propose one if log diving becomes a habit `[OPEN]`.
- `pg_dump` scheduling belongs to [backup-and-restore.md](backup-and-restore.md).
