# UC-09 Review beneficiary match cases
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Beneficiaries |
| Primary actor | FacilityAdmin (`ReviewBeneficiaryMatches`), SystemAdmin (all facilities) |
| Implementation status | `[BUILT]` |
| Related rules | BR-BEN-12, BR-BEN-18, BR-BEN-19; statuses `PendingReview` / `ConfirmedDistinct` / `MergedIntoExisting` |
| Endpoints | `GET api/Beneficiaries/PendingReview`, `GET api/Beneficiaries/{id}/MatchCandidates`, `POST api/Beneficiaries/{id}/ConfirmSame`, `POST api/Beneficiaries/{id}/ConfirmNotAMatch` |

## Goal
Resolve every duplicate-suspect record with a human decision: merge it into the pre-existing person, or certify it as a distinct person.

## Preconditions
Record has `match_review_status = PendingReview` and belongs to the actor's facility (or actor is SA).

## Main flow
1. FacilityAdmin opens the review queue (`PendingReview`) — oldest first for their facility; SA sees all facilities.
2. Opens a record, fetches `MatchCandidates`: candidates with `score`, `status` (Probable/Exact) and `reasons` flags (BR-BEN-02 weights), including the identity block for comparison.
3. Decision A — same person: `POST {id}/ConfirmSame` with `existingBeneficiaryId` (must be one of the candidates). The suspect record is merged into the existing one: review status becomes `MergedIntoExisting`, the merged record disappears from lists/search, and its FullCode routes to the survivor `[BUILT]`.
4. Decision B — different person: `POST {id}/ConfirmNotAMatch` with optional `reason` (≤ 500). Status becomes `ConfirmedDistinct`; the record is fully usable.

## Alternative flows
- A1 Twin pattern (same mother/last/birth, different first name): the suspect record itself is also a candidate for the twin sibling — both stay separate persons.
- A2 Same MatchKey twice already exists (Disambiguator > 0): each has its own review; decisions are per record.

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Record has no pending review (already decided) | `400` "This beneficiary has no pending match review." |
| `existingBeneficiaryId` equals the record itself | `400` "Invalid beneficiary selected." |
| Selected id is not among the candidates | `400` "The selected beneficiary is not one of the match candidates." |
| Record out of scope (other facility, non-SA) | `404` |
| DataEntry attempts review | `403` |

## Postconditions
- Queue shrinks; either the roster gains a distinct person or a duplicate is merged (data of the merged record folds into the survivor `[ASSUMED]` what exactly migrates — `disambiguator` reset happens in code).

## Screens (for frontend)
- Review-worklist view with aging, record detail with side-by-side candidate cards (score chips + reason badges + identity comparison), merge confirmation dialog, "not a match" dialog with reason.

Details: [../../02-design/workflows/beneficiary-match-review.md](../../02-design/workflows/beneficiary-match-review.md).
