# Data Model (Mermaid ER)

Supabase (PostgreSQL) entities and relationships for multi-tenant forms.

```mermaid
erDiagram
  USER {
    uuid id
    text email
    text name
  }
  ORG {
    uuid id
    text name
    timestamptz created_at
  }
  MEMBERSHIP {
    uuid id
    uuid org_id
    uuid user_id
    text role
  }
  FORM {
    uuid id
    uuid org_id
    text name
    text status
  }
  FORM_VERSION {
    uuid id
    uuid form_id
    int version
    jsonb schema
    timestamptz created_at
  }
  BLOCK {
    uuid id
    uuid form_version_id
    text type
    jsonb config
    int position
  }
  RESPONSE {
    uuid id
    uuid form_id
    uuid form_version_id
    uuid user_id
    timestamptz created_at
  }
  RESPONSE_ITEM {
    uuid id
    uuid response_id
    uuid block_id
    text value
  }

  USER ||--o{ MEMBERSHIP : has
  ORG ||--o{ MEMBERSHIP : has
  ORG ||--o{ FORM : owns
  FORM ||--o{ FORM_VERSION : versions
  FORM_VERSION ||--o{ BLOCK : contains
  FORM ||--o{ RESPONSE : receives
  RESPONSE ||--o{ RESPONSE_ITEM : includes
```

Notes
- Enforce tenant isolation with RLS: `org_id = auth.org_id()` where applicable.
- Consider partial indexes on `form_id`, `created_at` for response queries.
