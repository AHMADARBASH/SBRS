# ADR-010 Search by FullCode or first + last name across facilities
> Status: Accepted | Date: 2026-10-25

## Context
The original five-field search shared too much personal data and was clumsy for an operator who only knows how the person is called or their old card number.

## Decision
Cross-facility search accepts **either** a FullCode **or** first + last name. Name search is starts-with over normalized names, minimum 2 normalized characters, capped (≤ 20 results + `hasMore`). Results show limited demographics (fullCode, first/father/last, birth date, gender, identity-type code) — never the document value or mother name `[ASSUMED: display limits, BUILT in the DTO]`. Merged records are hidden; a merged FullCode resolves to the survivor.

## Consequences
- Fast, index-backed search (`normalized_last_name, normalized_first_name`).
- Privacy preserved even across facilities.
- Operators may need a state registry later — out of scope.

## Alternatives considered
- Five-field search as the only entry point — replaced by this simpler rule.
- Search by mother name — excluded for privacy.
