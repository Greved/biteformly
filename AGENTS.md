# Repository Guidelines

Initial contributor guide for BiteForm (AI form builder). Update as conventions evolve.

## Project Structure & Module Organization
- Monorepo suggestion:
  - `apps/web` — Next.js (React + shadcn/ui + Zustand)
  - `services/api` — .NET 9 Web API (Clean-ish: `Domain/`, `Application/`, `Infrastructure/`, `Api/`)
  - `packages/ui` — shared React primitives/tokens (optional)
  - `packages/config` — shared ESLint/tsconfig/editorconfig
  - `assets/` — logos, marketing, exported icons
  - Keep feature folders cohesive (builder, runtime, results, billing).

## Build, Test, and Development Commands
- Install: `pnpm install` (or `npm ci`).
- Web dev: `pnpm --filter web dev` (Next.js local server).
- API dev: `dotnet run --project services/api/Api`.
- Build: `pnpm -r build` (web) and `dotnet build` (api).
- Tests: `pnpm test` (vite-test/Vitest + RTL), `pnpm e2e` (Playwright), `dotnet test` (NUnit + coverlet).
- Lint/Format: `pnpm lint`, `pnpm format`, `dotnet format`.

## Coding Style & Naming Conventions
- Indent: 2 spaces (web), 4 spaces (C#).
- JS/TS: camelCase vars, PascalCase components/types; component files PascalCase; route folders kebab-case.
- C#: PascalCase types/methods, `_camelCase` private fields, `SCREAMING_SNAKE_CASE` consts.
- Tools: ESLint + Prettier; StyleCop.Analyzers + `.editorconfig` for C#.

## Testing Guidelines
- Web unit: `apps/web/src/**/*.{test,spec}.{ts,tsx}` with vite-test (Vitest) + Testing Library.
- E2E: `apps/web-e2e` Playwright; run against local dev server.
- API: `services/api/Tests/*.Tests.csproj` using NUnit; mock I/O at boundaries.
- Aim for ~80% coverage; prioritize critical paths (auth, submissions, payments).

## Commit & Pull Request Guidelines
- Conventional Commits (`feat:`, `fix:`, `chore:`, `refactor:`, `docs:`, `test:`).
- Keep PRs focused; include rationale, screenshots (UI), and verification steps.
- Link issues; ensure CI green (build, lint, unit, e2e) before review.

## Auth, DB & Analytics
- Auth: Supabase Auth for web and API; use RLS for row-level tenancy.
- DB: Supabase (PostgreSQL) as the primary datastore; migrations via Prisma or EF Core migrations targeting Postgres.
- Analytics: PostHog (self-hosted or cloud). Respect privacy; allow user opt-out.

## Security & Configuration Tips
- Secrets: never commit keys. Provide `.env.example`; use Next.js `.env.local` and .NET User Secrets.
- Config: Next.js via env vars; API via `appsettings.{Environment}.json` (per env).
- Validate inputs server-side; enforce per-tenant isolation with Supabase RLS policies.

## AI Service Guidelines
- .NET 9 API is vendor-agnostic: abstract AI providers behind interfaces; inject at runtime.
- Cost controls: cache deterministic prompts, choose model by prompt size/risk, stream where possible, batch requests, retry with jitter, track token usage/cost per request.
- Avoid vendor lock-in: keep prompts/tool schemas portable; use environment-driven provider selection.

## Agent-Specific Notes
- Scope: applies to entire repo. Prefer minimal, well-scoped diffs.
- Update this file when stack or workflows change.
