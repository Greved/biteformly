# BiteForm Web (apps/web)

## Scripts
- Dev: `pnpm --filter web dev`
- Build: `pnpm --filter web build`
- Start: `pnpm --filter web start`
- Lint: `pnpm --filter web lint`
- Format: `pnpm --filter web format`

## Env
Copy `.env.example` to `.env.local` and set:
- `NEXT_PUBLIC_API_BASE_URL` (default `http://localhost:5000`)
- `NEXT_PUBLIC_SUPABASE_URL`
- `NEXT_PUBLIC_SUPABASE_ANON_KEY`

## Notes
- Tailwind tokens defined in `src/styles/globals.css` and consumed via CSS vars.
- Theme managed by `next-themes` via `ThemeProvider` and `ThemeToggle`.
- Shared configs extend `@biteform/config`.
- State management via Zustand:
  - UI store: `src/state/ui.ts`
  - Forms store with API actions: `src/state/forms.ts`
