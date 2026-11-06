# ADR-009 Beneficiary is one global person; five-field match key with normalised Arabic names
> Status: Accepted | Date: 2026-10-25 (beneficiary feature)

## Context
People move between facilities and branches; per-facility silos caused duplicate registrations and inflated statistics. Arabic names introduce spelling variants that naive string comparison would call different persons.

## Decision
A beneficiary is **one global person** across all facilities/branches (`beneficiaries` table with unique identity documents + system-wide full code). Duplicate detection uses a five-field key — normalised first, father, last, mother names + birth date — hashed (SHA-256) as `match_key` after `ArabicTextNormalizer` (trim, diacritics/tatweel removal, alef unification, ى→ي, ة→ه, digit unification). Exact-key collisions are allowed under human review via `disambiguator`; probable matches use a weighted score (25/15/20/25/15, threshold 70) `[ASSUMED: weights need field testing]`; the twin pattern is always routed to review.

## Consequences
- One person, one identity; services count correctly per person.
- Normalization costs are paid at write time; matching becomes hash/index lookups.
- Wrong acceptance of the scoring needs testing; tune via real data (tracked BR-BEN-02).

## Alternatives considered
- Per-facility local registries — rejected: defeats the dedup purpose (the core business goal).
- Fuzzy search at read time only — rejected: unbounded latency, no durable status.
