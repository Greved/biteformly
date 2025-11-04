# BiteForm Frontend Plan (Next.js) — Phased

Last updated: 2025-11-04

## Current State

- No `apps/web` app in the repo yet. This plan assumes a Next.js (TypeScript) app with shadcn/ui + Tailwind + Radix and Zustand for state, aligned with AGENTS.md.

## Assumptions & Prereqs

- Node LTS and `pnpm` available.
- Monorepo workspaces: `apps/web`, optional `packages/ui` and `packages/config`.
- API runs at `http://localhost:5000` in Development (adjust via env).

## Phase 0 — Monorepo Foundation

- Initialize `pnpm` workspace with `apps/` and `packages/` folders.
- Create `packages/config` (shared ESLint, tsconfig, editorconfig) and wire to web.
- Set up GitHub Actions for Node installs, lint, unit tests, and Playwright (headed false) as a matrix job.

Deliverables:
- `pnpm-workspace.yaml`, root `package.json` with workspaces.
- `packages/config` with ESLint + Prettier, base tsconfig, editorconfig.
- CI workflow (build/lint/test for web).

## Phase 1 — Web App Bootstrap

- Scaffold `apps/web` with Next.js (app router, TS).
- Add Tailwind CSS, shadcn/ui, and Radix; establish design tokens (colors, radius, spacing, typography).
- Global layout: header/sidebar shell, theme provider (light/dark), fonts, and basic pages.
- Configure environment (`.env.local`) for API base URL and Supabase project keys (public anon key only on client).

Deliverables:
- Next.js app with base layout, theme toggle, and component library wired.
- Shared UI primitives imported from `packages/ui` if created.
- API client utility with fetch wrappers and error handling.

## Phase 2 — Auth & Session

- Integrate Supabase Auth (client and server helpers); persist session; expose user and tenant context.
- Protect pages/routes that require auth; redirect to sign-in.
- Derive tenant ID from session/claims; avoid user-supplied tenant IDs for privileged operations.

Deliverables:
- Auth pages (sign in/out), hooks for `useSession`, `useTenant`.
- Middleware/route guards for protected areas.

## Phase 3 — Form Builder (CRUD)

- Domain model mirrors API: Form, FormField.
- Screens: Forms list with search/sort/paging; create/edit form; manage fields (add/edit/delete/reorder) with client-side validation and accessible controls (drag/drop or up/down ordering).
- Wire to API endpoints; optimistic updates where safe; show ProblemDetails errors.

Deliverables:
- Forms list/table, form details, field manager UI.
- Zustand slices for builder state; API services for forms/fields.

## Phase 4 — Form Runtime (Player) & Embed

- Public render of a form with client-side validation consistent with server constraints.
- Submission flow: POST to API; success/thank-you page; basic spam/abuse protections (honeypot/time-to-complete heuristic).
- Embeddable script and share URL; light CSS isolation for embed.

Deliverables:
- Player route (`/f/{formId}`) and embeddable snippet.
- Submission service and feedback UI.

## Phase 5 — Results & Insights (MVP)

- Results dashboard: list submissions with filtering by submittedBy/date and paging; details view with responses.
- CSV export of current filtered set (client-side for MVP; server export later).
- Basic insights placeholders (counts over time) for future AI summaries.

Deliverables:
- Results pages, table, detail modal/page, CSV export.

## Phase 6 — Analytics & Telemetry

- Integrate PostHog (respect opt-out). Track key events: form created, field added, submission created, export triggered.
- Avoid PII in events; include anonymized or hashed identifiers when needed.

Deliverables:
- Analytics provider wrapper, config flag, and event calls in key flows.

## Phase 7 — Testing & QA

- Unit/component tests with Vitest + Testing Library for UI primitives, hooks, and pages.
- E2E tests with Playwright covering: auth, form CRUD, field CRUD, submission, and results listing.
- Mock API via MSW in unit tests; point E2E at local backend or MSW server mode.

Deliverables:
- Test setup files, representative test suites, Playwright config and key specs.

## Phase 8 — Plans, Billing UI, and Gating (Later)

- Integrate Stripe client-side for checkout links/portal (server webhooks later on API).
- Show current plan/usage; gate premium features (e.g., AI helpers, exports at scale) based on entitlements.

Deliverables:
- Billing screens and feature gates (hidden/disabled states with messaging).

## Phase 9 — Production Hardening & DX

- Error boundaries and toasts; robust empty/loading/error states.
- Strict CSP and security headers via Next config; sanitize/escape dynamic content.
- Performance passes: code-splitting, image optimization, caching, ISR where applicable.
- Preview deployments (Vercel) with env management.

Deliverables:
- Error boundary components, DX docs, and perf/security configs.

## Cross-Cutting Standards

- Coding conventions from AGENTS.md: 2-space indent; camelCase vars, PascalCase components/types; route folders kebab-case.
- Linting/formatting wired from `packages/config`; CI must block on lint/test.
- Accessibility: use Radix primitives and test with axe where feasible.

## Milestones Mapping

- M1: Monorepo + Web bootstrap + Auth + Basic builder (create/list forms). (Phases 0–3 subset)
- M2: Player + Submissions + Results MVP + Analytics instrumentation. (Phases 4–6)
- M3: Billing UI + Gating + Expanded tests/E2E coverage. (Phases 7–8)
- M4: Hardening, performance, and polish for public beta. (Phase 9)

## Run & Dev Notes (Target)

- Install: `pnpm install`
- Dev: `pnpm --filter web dev`
- Unit tests: `pnpm --filter web test`
- E2E: `pnpm --filter web e2e`
- Lint/format: `pnpm --filter web lint`, `pnpm --filter web format`

