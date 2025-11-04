# BiteForm Backend Plan and Next Steps

Last updated: 2025-11-04

## Backend Status

- Architecture: .NET 9 minimal APIs with Clean-ish layering: `Domain/`, `Application/`, `Infrastructure/`, `Api/`, plus `Tests/`. DI extensions wire Application and Infrastructure.
- Endpoints: Feature-grouped minimal APIs for forms/fields and submissions/responses with paging, validation, and OpenAPI example filters.
- Validation: FluentValidation with an endpoint filter generating RFC 7807 `ValidationProblem` responses.
- OpenAPI: Swagger in Development; custom filters (pagination hints, examples, field-type enums) enrich schema and examples.
- Auth: Optional Supabase JWT (bearer) configured; authorization enforced when `Supabase:JwtSecret` is set. Anonymous submissions allowed even when auth is on.
- Persistence: EF Core with Npgsql; InMemory option for tests via `UseInMemoryDb=true`. DbContext and entity configurations with initial migration present.
- AI: Vendor-agnostic interface with NoOp, OpenAI, and Gemini providers selected via `AI:Provider`.
- Config/Docs: `appsettings.json` + `appsettings.Development.json`; README with run/testing instructions.
- Tests: NUnit integration tests (WebApplicationFactory), persistence tests (EF InMemory), and validator tests; smoke tests for AI provider selection.

## Alignment With Guidelines

- Project structure adheres to the monorepo/API layout and Clean-ish layering.
- Auth/DB approach matches guidance (Supabase JWT, Postgres via EF migrations; tenancy checks in queries).
- AI abstraction and env-driven provider selection align with vendor-agnostic goals.
- Testing stack (NUnit + coverlet) present and covers core endpoints.
- Developer experience: Swagger, ProblemDetails, and validation integrations are in place.

## Gaps / Risks

- Tenant trust: `tenantId` is provided as a query parameter; when authenticated, tenant should be derived from JWT to prevent spoofing.
- Logging/observability: Serilog packages referenced but not configured; no request logging or enrichment hooks.
- CORS: No policy configured; the web app will need cross-origin access during local dev and prod.
- Health/readiness: Health endpoint is static; no DB connectivity/readiness checks.
- Migrations: No startup migration/apply strategy documented/enforced (dev vs prod).
- AI cost controls: No caching, retries with jitter, or usage/cost tracking despite interface fields.
- Analytics: PostHog config exists but no instrumentation in code; ensure privacy/opt-out.
- Security hardening: No rate limiting or request size limits; validators cap lengths but broader protections are missing.
- Supabase RLS: If using RLS, ensure EF connections use appropriate roles and policies enforce tenant isolation beyond app-level checks.

## Prioritized Next Steps

1) Auth + Tenancy
- Enforce auth by default outside Development; derive tenant/org from JWT claims and stop accepting `tenantId` from the client when authenticated (dev-only fallback if needed).
- Validate issuer/audience when Supabase details are configured; add claim mapping helpers.

2) CORS + Security
- Add CORS policy for web app origins (dev + prod) and preflight caching.
- Add rate limiting and request size limits for write endpoints; consider payload size guards.

3) Observability
- Wire Serilog (`UseSerilog()`), JSON console in dev, compact JSON in prod; enrich with request IDs, user/tenant (when available).
- Plan for OpenTelemetry traces/metrics later; start with structured logs.

4) Health/Readiness
- Add DB health check (EF ping) and a readiness endpoint suitable for container orchestration.

5) DB & Migrations
- Document and/or automate migrations in Development; keep explicit migrations for Production with CI/CD step.
- Validate schema against Supabase and plan RLS policies for forms/fields/submissions.

6) AI Provider Maturity
- Add simple in-memory cache for deterministic prompts; add retry with jitter and basic usage/cost tracking plumbed through `AiResponse`.
- Prepare for streaming output support in provider surface area where applicable.

7) Analytics
- Integrate PostHog with an opt-out flag; capture minimal, non-PII event metadata on key actions (e.g., submission created), honoring privacy.

8) Tests & Quality
- Add tests for auth-required paths and tenant-claim mapping; verify ProblemDetails (400/404/405/409) consistently across routes.
- Add CORS header smoke test when policy is enabled.

9) Production Hardening
- Add HTTPS redirection parity, `Strict-Transport-Security`, and standard security headers.

## Run & Test Notes

- Dev API: `dotnet run --project services/api/Api` (Swagger at `/swagger` in Development).
- Tests: `dotnet test services/api/Tests` (uses EF InMemory).
- Config: Set `UseInMemoryDb=true` for integration tests; set `AI:Provider` to select AI backend; supply Supabase JWT details to enable auth enforcement.

