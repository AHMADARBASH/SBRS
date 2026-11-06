# SBRS Backup and Restore
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

**Everything in this file is `[ASSUMED]`/`[PLANNED]`** — the repository contains no backup script, job, or doc today. The procedure below is the standard PostgreSQL approach tailored to SBRS.

## 1. What to back up `[PLANNED]`

| Content | Method | Note |
|---|---|---|
| Database `SBRS` | `pg_dump` (custom format) | nightly + before every deploy |
| `sbrs.env` secret file | host-secure copy | only where you keep secrets |
| Uploaded files (`wwwroot/uploads`) | rsync/backup tool | none exist yet `[OPEN]` |

The seeder is re-runnable (idempotent for roles/admin), so a restore recreates roles/SystemAdmin automatically on next startup; the rest is data.

## 2. Nightly dump procedure `[PLANNED]`

```bash
# as the postgres user (or a dedicated backup role)
pg_dump --host=<DB_HOST> --dbname=SBRS \
  --file=/var/backups/sbrs/SBRS_$(date +%F_%H%M).dump \
  --format=custom --compress=6
# keep 30 days
find /var/backups/sbrs -name 'SBRS_*.dump' -mtime +30 -delete
```

Recommended cadence `[ASSUMED]`: nightly full dump; full weekly backup retained longer; test restore monthly (below). Push dumps off-host (object storage / another machine) — a dump on the same disk protects from nothing.

## 3. Restore procedure `[PLANNED]`

```bash
# if recreating from scratch
createdb --host=<DB_HOST> --owner=<DB_USER> SBRS
pg_restore --host=<DB_HOST> --dbname=SBRS --no-owner --role=<DB_USER> \
  /var/backups/sbrs/SBRS_<STAMP>.dump

# or fully drop & restore (server-side)
dropdb --host=<DB_HOST> SBRS && createdb --host=<DB_HOST> --owner=<DB_USER> SBRS
pg_restore ...
```

Then restart the API (`sudo systemctl restart sbrs`) — the seeder re-creates missing roles/first admin (skips if users exist).

## 4. Restore test checklist (monthly) `[PLANNED]`

| # | Step | Expected |
|---|---|---|
| 1 | Restore latest dump to a scratch DB | command succeeds; errors list should only show role issues on scratch host |
| 2 | `SELECT count(*) FROM beneficiaries;` and a few key tables | counts match production's known values |
| 3 | Point a local API instance at the scratch DB | starts, seeder logs, login works with a test user |
| 4 | Log in as FA, view a facility roster (UC-10) | data legible, full codes intact (`citext` preserved) |
| 5 | Check last backup's size trend | no unexplained growth/zero-byte dumps |

## 5. Rollback-in-disaster playbook `[PLANNED]`

1. Stop API; note current schema version (EF `__EFMigrationsHistory`).
2. Restore the dump of the last good state.
3. If schema versions differ, re-run `dotnet ef database update` only if restoring *forward*; otherwise keep API build matching the restored schema era.
4. Rotate secrets if the compromise involved credentials (`Jwt:Key` rotation invalidates all access tokens; refresh tokens die on next rotation mismatch only if hash check fails — a DB restore already contains their hashes).

Migration tangent: `pg_restore` restores the database as it was; EF migrations history comes along with it.
