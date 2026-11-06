# SBRS Testing Pack — Start Here
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| File | Purpose |
|---|---|
| [test-strategy.md](test-strategy.md) | current state (no automated tests), risk-ranked must-test list, suggested test layout |
| [uat-checklist.md](uat-checklist.md) | step-by-step manual pass per use case per release |

Guiding thoughts:

1. The highest-risk area is **scoping** — a broken scope filter silently leaks data between facilities (out-of-scope = 404, ADR-006).
2. Second highest: **refresh-token rotation/reuse** — a family-revoke bug locks out real staff or lets stolen tokens run.
3. Arabic matching thresholds live in code and were never tested against real name data — regression-test variants (ال، أ/ا، ة/ه، ى/ي) with fixtures, not vibes.
4. Pharmacy role separation (ADR-015) is untestable until the pharmacy design lands (OQ-01/OQ-02).
