# KitchenAidAI API CRUD Actions

This skill documents the API CRUD endpoints, query behavior, and response shape for KitchenAidAI. Use it when you need a quick reference for routes or payloads.

## Response Shape
All API endpoints return a consistent wrapper:

- `success`: boolean
- `data`: payload on success
- `alert`: UX-friendly error object when `success` is false

Example error payload:

```
{
  "success": false,
  "alert": {
    "type": "error",
    "title": "Neispravan zahtjev",
    "message": "Provjerite unesene podatke.",
    "code": "BAD_REQUEST"
  }
}
```

## How To Create Integration Tests (Detailed)

Use this workflow for every API controller so tests are consistent and complete.

### 1. Arrange test infrastructure
- Create or reuse `WebApplicationFactory<Program>` for test host.
- In factory, replace app DbContext registration with `UseInMemoryDatabase(...)`.
- Use one stable in-memory database name per factory instance, not a new name per request.
- Add test auth handler with request headers:
  - `X-Test-UserId`
  - `X-Test-Role`

### 2. Create reusable test helpers
- `CreateAuthorizedClient(userId, role)` for authenticated requests.
- Helper for creating valid payloads with unique fields (`Guid`) to avoid accidental duplicates.
- Helper like `CreateEntityAsAdminAsync()` for tests that need real existing IDs.

### 3. Build mandatory scenario matrix per CRUD action
- `GET collection`:
  - unauthenticated -> `401`
  - allowed role -> `200`
- `GET by id`:
  - unauthenticated -> `401`
  - forbidden role/owner mismatch -> `403` (where applicable)
  - existing id + allowed role -> `200`
  - non-existing id -> `404`
- `POST create`:
  - unauthenticated -> `401`
  - forbidden role -> `403` (admin-only endpoints)
  - invalid payload -> `400`
  - duplicate unique values -> `400`
  - valid payload + allowed role -> `201`
- `PUT update`:
  - unauthenticated -> `401`
  - forbidden role/owner mismatch -> `403`
  - invalid payload -> `400`
  - non-existing id -> `404`
  - valid payload + allowed role -> `200`
- `DELETE`:
  - unauthenticated -> `401`
  - forbidden role -> `403`
  - non-existing id -> `404`
  - allowed role -> `200`

### 4. Add KitchenAidAI-specific checks
- For soft-delete endpoints, verify delete behavior beyond status code:
  - resource becomes hidden for normal queries
  - delete marks soft-delete state (directly or through observable API behavior)
- For restore endpoints:
  - admin restore -> `200`
  - non-admin restore -> `403`
  - after restore, resource is available again in relevant `GET` endpoints
- For domain uniqueness rules (especially `Users` and `Countries`):
  - create first entity -> success
  - create duplicate -> `400` (and business error code if present)

### 5. Validate response body, not only status
- For success responses, assert:
  - `success == true`
  - `data` is present
  - key fields (`id`, changed properties) match expectation
- For error responses, assert:
  - `success == false`
  - `alert` is present
  - `alert.code` and/or `alert.message` match expected failure reason

### 6. Validate edge cases
- Required fields missing.
- Invalid formats (email, dates, etc.).
- Out-of-range or too-long values where validation attributes exist.
- Owner-vs-admin access boundaries.
- Repeated operations (e.g. restore already restored, delete already deleted) if endpoint supports it.

### 7. Completion criteria for a controller
- All exposed CRUD routes are covered.
- Happy path exists for each route where applicable.
- Non-existing ID scenarios are covered.
- Validation-error scenarios are covered.
- Authorization matrix (`401`, `403`, allowed role) is covered.
- Response wrapper (`success`, `data`, `alert`) is asserted in all key scenarios.

## Users (`/api/users`)
- `GET /api/users?search=`: admin list (non-admin gets self only)
- `GET /api/users/{id}`: admin or owner
- `POST /api/users`: admin create (uses `UserCreateDto`)
- `PUT /api/users/{id}`: admin or owner (uses `UserUpdateDto`)
- `DELETE /api/users/{id}`: admin soft-delete
- `POST /api/users/{id}/restore`: admin restore

## Recipes (`/api/recepti`)
- `GET /api/recepti?search=&includeDeleted=`: list with search; non-admin sees own cookbook
- `GET /api/recepti/{id}`: admin or owner
- `POST /api/recepti`: create (uses `ReceptFormDto`)
- `PUT /api/recepti/{id}`: update (uses `ReceptFormDto`)
- `DELETE /api/recepti/{id}`: soft-delete with cascade
- `POST /api/recepti/{id}/restore`: admin restore

## Ingredients (`/api/namirnice`)
- `GET /api/namirnice?friziderId=&search=&includeDeleted=`
- `GET /api/namirnice/{id}`
- `POST /api/namirnice`: create (uses `NamirnicaFormDto`)
- `PUT /api/namirnice/{id}`: update (uses `NamirnicaFormDto`)
- `DELETE /api/namirnice/{id}`: soft-delete

## Fridges (`/api/frizideri`)
- `GET /api/frizideri?userId=`
- `GET /api/frizideri/{id}`
- `POST /api/frizideri`: create (uses `FriziderCreateDto`)
- `PUT /api/frizideri/{id}`: update (uses `FriziderUpdateDto`)
- `DELETE /api/frizideri/{id}`: soft-delete with cascade to items

## Cookbooks (`/api/kuharice`)
- `GET /api/kuharice?search=&includeDeleted=`
- `GET /api/kuharice/{id}`
- `POST /api/kuharice`: create (uses `KuharicaCreateDto`)
- `PUT /api/kuharice/{id}`: update (uses `KuharicaUpdateDto`)
- `DELETE /api/kuharice/{id}`: soft-delete with cascade to joins

## Cookbook Recipes (`/api/recept-kuharice`)
- `GET /api/recept-kuharice?kuharicaId=&includeDeleted=`
- `GET /api/recept-kuharice/{id}`
- `POST /api/recept-kuharice`: create (uses `ReceptKuharicaCreateDto`)
- `PUT /api/recept-kuharice/{id}`: update (uses `ReceptKuharicaUpdateDto`)
- `DELETE /api/recept-kuharice/{id}`: soft-delete

## Steps (`/api/koraci`)
- `GET /api/koraci?receptId=&includeDeleted=`
- `GET /api/koraci/{id}`
- `POST /api/koraci`: create (uses `KorakReceptaFormDto`)
- `PUT /api/koraci/{id}`: update (uses `KorakReceptaFormDto`)
- `DELETE /api/koraci/{id}`: soft-delete

## Chat Messages (`/api/chat-messages`)
- `GET /api/chat-messages?userId=&includeDeleted=`
- `GET /api/chat-messages/{id}`
- `POST /api/chat-messages`: create (uses `ChatMessageFormDto`)
- `PUT /api/chat-messages/{id}`: update (uses `ChatMessageFormDto`)
- `DELETE /api/chat-messages/{id}`: soft-delete

## Countries (`/api/countries`)
- `GET /api/countries?search=`
- `GET /api/countries/{id}`
- `POST /api/countries`: admin create
- `PUT /api/countries/{id}`: admin update
- `DELETE /api/countries/{id}`: admin delete
