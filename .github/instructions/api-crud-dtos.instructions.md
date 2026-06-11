---
description: "Use when implementing or updating ASP.NET Core API CRUD endpoints, DTOs, or query/search behavior for KitchenAidAI entities. Enforces DTO exposure and nested DTO shaping."
applyTo: "KitchenAidAI/Controllers/**/*.cs"
---
# API CRUD + DTO Rules (KitchenAidAI)

- API controllers must implement CRUD where business rules allow: list (with optional search/query filters), get by id, create, update, delete.
- Do not expose internal entity fields directly; always return DTOs to the API client.
- Use separate DTOs for request/response when needed (e.g., create/update vs read).
- Represent related data using nested DTOs where it adds clarity.
- Keep responses consistent across entities (status codes, error shapes, and validation handling).
