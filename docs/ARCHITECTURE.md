# Architecture (Mermaid)

A high-level view of the web, API, data, analytics, billing, and AI layers.

```mermaid
flowchart TD
  Client["Next.js - apps/web (shadcn/ui + Zustand)"] -->|JWT| API[".NET 9 Web API services/api"]
  Client -->|Events| PostHog[PostHog Analytics]

  API -->|Verify JWT| SupabaseAuth[Supabase Auth]
  API -->|CRUD| DB[(Supabase PostgreSQL)]
  API -->|Billing| Stripe[Stripe]
  Stripe -->|Webhooks| API

  API -->|Capture| PostHog

  subgraph AI_Layer_VendorAgnostic
    Adapter[AI Adapter Interfaces]
    OpenAI[(OpenAI)]
    Anthropic[(Anthropic)]
    Local[(Local/Ollama)]
    Adapter --> OpenAI
    Adapter --> Anthropic
    Adapter --> Local
  end

  API -->|Prompts/Tools| Adapter
  API --> Cache[(Cache/CQRS)]
  Cache -. optional .- API
```

Notes
- API abstracts AI providers via DI; selection by env config.
- Supabase provides Auth (JWT) and PostgreSQL with RLS for tenancy.
- PostHog captures client and server events.
- Stripe manages subscriptions; webhooks sync entitlements.
