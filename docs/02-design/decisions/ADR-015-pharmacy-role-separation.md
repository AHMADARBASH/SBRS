# ADR-015 Pharmacy staff must never register beneficiaries (separate pharmacy role)
> Status: Proposed `[OPEN]` | Date: unknown

## Context
Pharmacy workers dispense to existing beneficiaries. If they could register, the dedup model erodes and unverified demographic data enters the registry (see the original module plan, OQ-01/OQ-02).

## Decision (projected, not yet implemented)
Pharmacy staff get a role of their own — the **name and exact design remain open** (OQ-01: `Pharmacy` role vs pharmacy facility type; OQ-02: dedicated role vs DataEntry reuse). Whatever lands, the `RegisterBeneficiaries` and beneficiary-mutation policies must be kept free of pharmacy staff, and pharmacy APIs must not accept registration scopes. ADR status stays Proposed until the pharmacy design is confirmed.

## Consequences
- If a dedicated role is chosen: new policy entries + role constants + UX gating.
- If DataEntry is reused: an explicit deny-list or facility-type check is required instead.
- Separation must be tested explicitly (test-strategy "pharmacy cannot register").

## Alternatives considered
- Letting pharmacists register — rejected (data quality + separation of duties).
- Facility-type-based permissions only — insufficient without a role gate.

Related: UC-11–UC-14, screens-and-roles, OQ-01, OQ-02.
