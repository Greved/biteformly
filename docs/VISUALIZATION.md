# Visualizing the Diagrams

These diagrams use Mermaid. You can view and export them in multiple ways.

## Quick Preview
- GitHub/GitLab: Markdown Mermaid renders natively. Open `docs/*.md`.
- VS Code: install extension "Markdown Preview Mermaid Support" and open preview.

## Export to PNG/SVG (CLI)
- Add Mermaid CLI: `pnpm add -D @mermaid-js/mermaid-cli`
- Export examples:
  - `npx mmdc -i docs/diagrams/roadmap.mmd -o docs/diagrams/roadmap.svg`
  - `npx mmdc -i docs/diagrams/architecture.mmd -o docs/diagrams/architecture.svg`
  - `npx mmdc -i docs/diagrams/data_model.mmd -o docs/diagrams/data_model.svg`

## Miro/diagrams.net
- diagrams.net (draw.io): File → Import → Mermaid text, then edit visually.
- Miro: paste exported SVG/PNG, or embed via link.

## Files
- Roadmap: `docs/ROADMAP.md` (Gantt) and `docs/diagrams/roadmap.mmd`.
- Architecture: `docs/ARCHITECTURE.md` and `docs/diagrams/architecture.mmd`.
- Data Model: `docs/DATA_MODEL.md` and `docs/diagrams/data_model.mmd`.
