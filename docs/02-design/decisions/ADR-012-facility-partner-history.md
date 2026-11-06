# ADR-012 Facility partner history table (facilities_partners)
> Status: Accepted | Date: 2026-10-19

## Context
A facility's supporting partner changes over time (project ends, new partner) but past reporting must stay attributable to the partner who paid.

## Decision
`facilities` keeps a **current** `partner_id`; the full story lives in `facilities_partners` (`facility_id`, `partner_id`, `start_date`, `end_date` NULL = open, `project_code` NOT NULL default `''`). Assigning a new partner closes the open assignment with `end_date = start_date − 1 day` and switches the current pointer; starting a new assignment later than the open one's start is enforced (with date-window validation).

## Consequences
- Time-accurate partner attribution for reports.
- Read paths must prefer the current pointer; joins filter `end_date IS NULL` for "today's partner".
- `project_code` is optional for users (empty default) but always present in data `[BUILT]`.

## Alternatives considered
- Only current partner on facility — rejected: loses history.
- Soft-deleting partner links — rejected: wrong temporal semantics.
