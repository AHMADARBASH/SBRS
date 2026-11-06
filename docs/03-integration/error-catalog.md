# SBRS Error Catalog
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

All errors return RFC 7807 `application/problem+json`, produced by `SBRS/Middleware/GlobalExceptionHandler.cs` `[BUILT]`.

## 1. Status mapping

| Exception (source) | HTTP | Title | Detail |
|---|---|---|---|
| `ValidationException` (FluentValidation behavior) | 400 | `Validation failed` | status detail + `errors` dictionary |
| `DomainException` (domain rules e.g. `FacilityEntity.Update`) | 400 | `Business rule violated` | exact rule message |
| `NotFoundException` (Application handlers) | 404 | `Not found` | `"<Entity> with id '<key>' was not found."` |
| `ConflictException` (explicit uniqueness/race checks) | 409 | `Conflict` | exact message |
| Npgsql `23505` unique violation caught as `DbUpdateException` | 409 | `This record was just created by someone else. Search again and retry.` | yes |
| `UnauthorizedException` (auth flows) | 401 | `Unauthorized` | exact message |
| `ForbiddenException` (exists but currently rare — policies mostly reject with 403 by framework) | 403 | (framework standard `Forbidden`) | — |
| anything else | 500 | `An unexpected error occurred` | **detail omitted** (never leak internals) |

Note: throwing `ForbiddenException` is implemented but not wired in the HTTP mapping table above; the framework returns its own 403 when a policy fails.

## 2. Example bodies

### 400 — validation (`errors` dictionary)

`detail` repeats the (long) FluentValidation summary; clients should rely on `errors`.

```json
{
  "status": 400,
  "title": "Validation failed",
  "detail": "FluentValidation.ValidationException: Validation failed: ... (summary of all failures)",
  "errors": {
    "UserName": [ "User name may contain only letters, digits, '.', '_' and '-'." ],
    "Password": [ "The length of 'Pw1' must be at least 8 characters." ]
  }
}
```

### 400 — domain rule

```json
{
  "status": 400,
  "title": "Business rule violated",
  "detail": "Cannot add a facility to a deactivated branch."
}
```

### 404 — not found (also used for out-of-scope)

```json
{
  "status": 404,
  "title": "Not found",
  "detail": "FacilityEntity with id '0f2f7a9e-4fdd-4c1c-9b7a-1b2c3d4e5f60' was not found."
}
```

### 409 — conflict (explicit)

```json
{
  "status": 409,
  "title": "Conflict",
  "detail": "An organization with code 'SARC' already exists."
}
```

### 409 — PostgreSQL unique violation (`23505`)

```json
{
  "status": 409,
  "title": "This record was just created by someone else. Search again and retry.",
  "detail": "Nobody may have created this record concurrently — the message is generic by design."
}
```

### 401 — unauthorized

```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Invalid refresh token."
}
```

### 500 — unexpected

```json
{
  "status": 500,
  "title": "An unexpected error occurred"
}
```

## 3. Domain messages the frontend should present nicely

Collected from `SBRS.Domain/{Entities,Common}` and handlers `[BUILT]`; show them as-is (they are user-safe English on purpose).

| Area | Message | When |
|---|---|---|
| Organization | `A deactivated organization cannot be updated.` | update on inactive |
| Organization | `The organization is already deactivated.` | double deactivate |
| Branch | `A deactivated branch cannot be updated.` | update on inactive |
| Branch | `The branch is already deactivated.` | double deactivate |
| Facility | `A deactivated facility cannot be updated.` | update on inactive |
| Facility | `The facility is already deactivated.` | double deactivate |
| Facility | `Cannot add a facility to a deactivated branch.` | create |
| Facility | `A deactivated facility cannot change its partner.` | partner change |
| Facility code | `The city c_code is required.` / `The facility type code is required.` / `The facility sequence must be positive.` | code generation |
| Facility partner | `The new start date must be later than the current assignment start date ({date}).` | assign |
| Facility partner | `End date cannot be before the start date.` | close |
| Address | `A deactivated address cannot be updated.` / `The address is already deactivated.` | address update/deactivate |
| User | `You cannot deactivate your own account.` | self deactivate |
| User | `You cannot change your own role.` | self role change |
| Beneficiary | `First name is required.` / `Father name is required.` / `Last name is required.` / `Mother name is required.` | register |
| Beneficiary | `Birth date cannot be in the future.` / `Enrollment date cannot be in the future.` | register |
| Beneficiary | `Code must be positive.` / `Full code is required.` / `Registration facility is required.` / `Area is required.` / `Nationality is required.` / `Disambiguator cannot be negative.` | register (server-side checks) |
| Beneficiary | `A beneficiary with the same identity key as another one must start as PendingReview.` | register race |
| Beneficiary | `The beneficiary is already deactivated.` | double deactivate |
| Beneficiary | `This beneficiary has no pending match review.` | review actions |
| Beneficiary | `Invalid beneficiary selected.` | merge into itself |
| Beneficiary | `The selected beneficiary is not one of the match candidates.` | merge |
| Beneficiary | `Beneficiary {fullCode} has no identity record.` | list mapping (data problem) |
| Identity | `The identity value is not valid for the identity type {code}.` | register/tracking |
| Identity | `The identity value must be empty for an unregistered beneficiary.` | UNREGISTERED with a value |
| Identity | `Identity type '{code}' has no validation pattern.` | unknown code |
| Identity type | `Value regex is required.` / `Value regex is not a valid regular expression.` | admin edit |
| Auth/refresh | `Invalid refresh token.` | unknown/revoked/expired/reused |
| Search | `Provide the full code, or both first name and last name (at least 2 characters each).` | bad search input |
| Users scope | `Users with the role '{role}' cannot be managed yet.` | BranchAdmin/ProjectCoordinator creation |
| Fetch | `The selected area does not exist.` / `The selected nationality does not exist.` / `The selected identity type does not exist.` | register references |
