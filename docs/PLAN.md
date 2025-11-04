# BiteForm — Comprehensive Plan

## Vision & Scope
- Build an AI‑assisted form platform: builder (authoring), runtime (embeds/links), submissions/results, team/tenancy, analytics, and billing.
- Monorepo (suggested): `apps/web` (Next.js), `services/api` (.NET 9), `packages/*` (ui/config), `assets/`.

## Workstreams

### API (services/api)
- Authentication & Tenancy
  - Supabase JWT, optional in dev; enforce tenant on all reads/writes via filters/joins.
  - Row‑level security modeled in Supabase; double‑enforce tenancy in API.
- Domain & CRUD
  - Forms, Fields, Submissions, Responses complete with pagination, sort, and filters.
  - Add: Folders/Collections, Organizations, Memberships, API keys, Webhooks, Tags.
- Validation & Errors
  - FluentValidation for all request DTOs; 400 ValidationProblem, RFC7807 for conflicts and generic errors.
  - Add idempotency keys for POST (submissions) and safe retries.
- OpenAPI & SDKs
  - Enrich with examples, enums (field type), parameter docs; keep sample GUIDs valid.
  - Generate typed clients (TS/C#) via `nswag`/`openapi-typescript` and publish to `packages`.
- Performance & Limits
  - Rate limiting per tenant/IP; output caching for public GETs (forms/fields schema).
  - N+1 audits, indexes and query hints; cap `pageSize` and request payloads.
- Migrations & Data
  - EF Core migrations source in `Infrastructure`; seed minimal demo data in dev only.
  - Data retention policies; soft delete for forms/submissions (with purge job).
- Webhooks & Integrations
  - Outbound webhooks (submission.created), signed payloads; retry w/ exponential backoff.
  - Import/export JSON schema for forms; CSV export for results.
- Observability
  - Structured logs (Serilog), request logging, correlation IDs; OpenTelemetry traces.

### Web App (apps/web)
- Foundations
  - Next.js (App Router, TS), shadcn/ui, Tailwind; state via Zustand slices.
  - Auth via Supabase; multi‑tenant org switcher; role‑based UI.
- Builder (authoring)
  - Form canvas with drag/drop ordering, field palette, validations, conditionals, sections/pages.
  - Theme editor (tokens), preview modes, autosave with optimistic updates.
- Runtime (player)
  - Public pages and embeddable script; client‑side validation; progressive enhancement.
  - Spam protection (honeypot / rate‑limit / optional hCaptcha/Turnstile).
- Results
  - Submissions list with filters, CSV export, response detail view; labels for fields.
- Admin & Billing
  - Plans/usage, seat management; invoices/portal via Stripe; org invites.
- Analytics
  - PostHog client, event taxonomy (builder.used, submission.started/completed, dropoff).
- DX & A11y
  - Component library in `packages/ui`; ARIA‑correct controls; i18n scaffolding.

### AI Services
- Provider Abstraction
  - Interface‑driven providers (OpenAI/Gemini/…); env‑selected; streaming support.
- Features
  - Prompt‑to‑form (generate schema), rewrite labels/help text, validation hints.
  - Insights on results: summaries, top fields, anomalies.
- Cost Controls
  - Cache deterministic prompts, batch requests, token/cost metering per tenant.

### Testing & Quality
- API
  - Unit tests for validators/mappers; integration tests (WebApplicationFactory) for all endpoints and edge cases (pagination limits, tenant isolation, 404 vs unknown route, 409 conflicts, idempotency).
  - Contract tests: verify OpenAPI invariants (enums/defaults, examples present).
- Web
  - Unit tests with Vitest/RTL; Playwright e2e (auth, builder flows, public submission happy‑path and validation errors).
- Tooling
  - Coverage targets ~80%; pre‑commit hooks (lint/test); secret scanning; SAST.

### Security & Compliance
- Secrets in `.env.local` / User Secrets; periodic key rotation guidance.
- RLS policies for Supabase; server‑side validation for all inputs.
- Audit trail for sensitive actions (delete, export, webhook keys); GDPR data export/delete.

### Analytics & Monitoring
- PostHog events and dashboards; capture funnel metrics and latency percentiles.
- SLOs: API availability, p95 latency budgets; alerting.

### CI/CD & Infra
- GitHub Actions
  - Matrix builds (web/api), lint, unit, integration, e2e (tagged), Swagger publish artifact.
  - On release: build/push Docker image for API; deploy web to Vercel/Netlify.
- Environments
  - Dev/Preview/Prod config parity; feature flags for risky features.

## Milestones
- M1 — Core Foundations
  - Repo, CI, Supabase auth, Forms/Fields/Submissions/Responses CRUD with pagination, validation, OpenAPI examples; web skeleton with auth and simple builder; e2e happy‑path.
- M2 — Share & Monetize
  - Embed runtime, public form pages, Stripe subscriptions, usage limits, exports, API keys.
- M3 — Depth & Analytics
  - Logic/conditionals, themes, analytics dashboard, webhooks, SDKs.
- M4 — AI & Launch
  - Prompt‑to‑form MVP, insights, onboarding, docs; beta hardening.

## Backlog (Detailed Tasks)
- API: add API keys (scoped to tenant) + middleware; add idempotency for POST submissions; webhook signing + retries; CSV export endpoint; indexes (submissions(formId, submittedAtUtc)).
- API: OpenAPI lint step; generate TS/C# clients; add 422 for semantic validation failures where applicable.
- Web: embed script package; builder conditional UI; theme tokens; CSV export UI; org/seat management.
- AI: schema generation prompt library; deterministic prompt cache; budget guardrails per tenant.
- Ops: rate limiting (per IP/tenant); request size limits; log redaction; health and readiness endpoints.

## Risks & Mitigations
- Mixed provider (EF) in tests — mitigated by `UseInMemoryDb=true` flag and single registration.
- Vendor lock‑in — abstract AI providers, portable prompt schemas, env‑driven selection.
- Cost overruns — metering, budgets, cache, batch; dogfood with capped models in non‑prod.

## Acceptance Criteria (per Milestone)
- M1: All CRUD endpoints green with integration tests; OpenAPI has examples/enums/defaults; web can auth and create basic form; CI green on PR.
- M2: Public embed works; Stripe payments enable plan gating; exports available; API keys usable; post‑deploy smoke tests.
- M3: Conditional logic and themes configurable; analytics charts accurate; webhooks retried and signed; SDKs published.
- M4: Prompt‑to‑form generates valid, editable schema; insights compute within SLO; docs complete.

