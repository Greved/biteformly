# Next Steps Plan

## 1) Foundation & Repo
- Initialize monorepo: `pnpm init -y && pnpm init -w`.
- Workspaces: `apps/web`, `services/api`, `packages/{ui,config}`.
- Add CI (GitHub Actions): build, lint, test for web/api.

## 2) Web App (Next.js + shadcn/ui + Zustand)
- Scaffold Next.js (TS): `pnpm create next-app apps/web --ts`.
- Add shadcn/ui, Tailwind, Radix; set design tokens.
- State: Zustand slices per feature (builder, player, account).

## 3) API (.NET 9, Clean-ish)
- Create solution: `dotnet new sln`.
- Projects: `Domain`, `Application`, `Infrastructure`, `Api` (.NET 9 minimal APIs + MediatR).
- NUnit test project: `dotnet new nunit -o services/api/Tests`.

## 4) Auth, DB, Analytics
- Supabase: set project, enable RLS; create schema (forms, blocks, responses, orgs, memberships).
- Web: Supabase Auth client; server helpers for session/tenant.
- API: JWT verification via Supabase; enforce tenant on queries.
- Analytics: PostHog SDK (web/api), opt-out and capture key events.

## 5) AI Service (Vendor-Agnostic)
- Define provider interfaces + DI; env-driven selection.
- Cost controls: cache deterministic prompts, choose model by prompt size/risk, stream outputs, batch, token/cost metrics.
- Features: prompt-to-form, rewrite/tone, insights summaries.

## 6) Payments & Plans
- Stripe: products/tiers (Free, Pro, Team), metered usage (responses/seat).
- Webhooks to sync entitlements; gating middleware on web/api.

## 7) Quality, DX, Security
- Web tests: Vitest + Testing Library; E2E: Playwright.
- API tests: NUnit + coverlet; contract tests for critical endpoints.
- Lint/format: ESLint+Prettier, `dotnet format`; pre-commit hooks; secret scanning.
- Observability: Serilog, OpenTelemetry, structured logs; error tracking.

## 8) Milestones
- M1: Monorepo, Supabase auth, basic builder/player, save responses.
- M2: Prompt-to-form MVP, Stripe subscriptions, share/embed.
- M3: Logic/themes, analytics dashboard, webhooks/exports.
- M4: AI insights, onboarding, billing polish; public beta.
