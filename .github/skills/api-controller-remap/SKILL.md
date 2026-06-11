# API Controller + Remap Skill

Use this skill when you need to create or extend API controllers and remap MVC actions to API endpoints in KitchenAidAI.

## Goals
- Implement CRUD endpoints with query/search parameters.
- Enforce access rules (admin vs user scope).
- Use DTOs for all responses and nested DTOs for related data.
- Return a consistent error alert payload on non-success responses.

## Checklist
1. Identify the entity and its relationships.
2. Pick request/response DTOs (create/update/read).
3. Add access rules (admin can access all; user can access own data only).
4. Apply soft-delete instead of hard-delete where supported.
5. Ensure error responses use ApiResponse and ApiAlertDto.

## Output Expectations
- New API controller under KitchenAidAI/Controllers/Api.
- DTO updates under KitchenAidAI/Models/DTOs.
- Optional route remap notes if MVC actions should forward to API.
