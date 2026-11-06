# SBRS Operations Pack — Start Here
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| File | Purpose |
|---|---|
| [local-setup.md](local-setup.md) | from clone to running API with secrets + seeder |
| [configuration-reference.md](configuration-reference.md) | every setting: appsettings, secrets, RefreshCookie section, env vars |
| [database-and-migrations.md](database-and-migrations.md) | EF migration workflow, existing migrations, seed scripts status |
| [deployment-linux.md](deployment-linux.md) | azure-pipelines deploy, systemd unit, nginx/TLS, DB user, update + rollback |
| [backup-and-restore.md](backup-and-restore.md) | pg_dump/restore procedure and restore testing `[PLANNED]` |

Rules of thumb:

1. Secrets live in user secrets (dev) or the systemd `EnvironmentFile` (prod) — double underscores for nested keys.
2. The startup `IdentitySeeder` needs a reachable DB — anything that builds the host (incl. Swagger CLI export) fails without one.
3. Migrations are applied manually; the pipeline only publishes and restarts.
4. Lookup seed scripts do not exist yet (OQ-21) — factor in empty lookup tables after a restore.
