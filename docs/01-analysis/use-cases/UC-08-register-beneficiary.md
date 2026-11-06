# UC-08 Register a beneficiary
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Beneficiaries |
| Primary actor | DataEntry (`RegisterBeneficiaries` policy — DataEntry only, OQ-12) |
| Implementation status | `[BUILT]` (FacilityBeneficiary enrollment `[PLANNED]`) |
| Related rules | BR-BEN-01, 02, 04, 05, 06, 07, 08, 09, 12, 18, 19 |
| Endpoints | `POST api/Beneficiaries/Register` |

## Goal
Create a beneficiary record at the operator's facility with a generated code, valid Arabic names, birth/enrollment dates and identity documents — running automatic duplicate detection.

## Preconditions
- Operator has a `facility_id` claim (policy: DataEntry).
- UC-07 search done: the person does not yet exist (or exists and enrollment into a new facility — `[PLANNED]`).
- Geography, nationality and identity-type lookups are loaded (OQ-21: seeds missing).

## Main flow
1. DataEntry submits: `firstName`, `fatherName`, `lastName`, `motherName`, `birthDate` (+ `isBirthDateEstimated`), `gender` (true=male), `enrollmentDate`, `areaId`, `nationalityId`, and one identity (`identityTypeId`, `identityValue?`).
2. If only the birth year is known, the client sends `YYYY-01-01` with `isBirthDateEstimated = true` (BR-BEN-05 — client-side rule).
3. Server normalizes all four names, builds the five-field MatchKey (SHA-256).
4. Server validates the identity value against the type regex after digit normalization; UNREGISTERED must carry an empty value (BR-BEN-09).
5. Server allocates the beneficiary `Code` under a `FOR UPDATE` lock on the facility and builds `FullCode` (BR-BEN-06) → unique index protects races.
6. The system evaluates matches:
   - exact key duplicate (or same identity document) → saves the record as **PendingReview** + assigns `Disambiguator` (`match_key, disambiguator` unique index) (BR-BEN-18).
   - probable (score ≥ 70) → saved with review status PendingReview.
   - nothing found → NoMatchFound; record is immediately settled.
7. Response `200`: `{ id, code, fullCode, matchReviewStatus }`.
8. If PendingReview → the FacilityAdmin review begins (UC-09); until decided, the record counts as new with a clear status.

## Alternative flows
- A1 Same person exists → registration stops at UC-07 with the existing FullCode; no new record is created `[CONFIRMED]` (enrollment to another facility is `[PLANNED]`).
- B1 Identity document matches another person → Exact match (score 100, `SameIdentity`) even with different names → PendingReview.

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Missing/empty name after normalization | `400` domain message (e.g. "First name is required.") |
| Birth/enrollment date in the future | `400` |
| EnrollmentDate default `0001-01-01` slipped in | accepted today ⚠ (OQ-18) |
| Identity value fails regex | `400` "The identity value is not valid for the identity type …" |
| UNREGISTERED with a value | `400` "The identity value must be empty for an unregistered beneficiary." |
| Unknown nationality/identity type/area | `400` "The selected … does not exist." |
| Concurrent duplicate FullCode (race) | unique-violation → `409` generic retry message |
| SA attempts register | `403` — policy is DataEntry-only (OQ-12) |
| Facility unknown in claim | `403 (no facility)` |

## Postconditions
- Beneficiary exists with FullCode, match status set, audit stamps; visible in the facility roster (UC-10) and in the review queue if PendingReview (UC-09).

## Screens (for frontend)
- Registration wizard with: search-first step (UC-07), form for names/mother/birth (year-only helper), identity document block, area/nationality pickers, and the post-save banner showing the match decision ("under review by your facility admin" vs "registered").

Details: [../../02-design/workflows/beneficiary-registration.md](../../02-design/workflows/beneficiary-registration.md).
