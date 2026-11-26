---
name: Token Saver
description: Load FIRST at the start of every coding task in this repository. Blocks all documentation access and replaces repo exploration with a generated project map so only files that must change are opened.
---

# Token Saver

## 1. Never touch docs
- Never read, open, grep, create, edit, move or delete: `docs/`, any `*.md` except `.opencode/skills/**/SKILL.md` and `AGENTS.md`, OpenAPI/Swagger JSON exports.
- Existing docs stay exactly as they are. Never create docs, briefs, summaries or markdown reports.
- Never write Swagger/XML `<summary>` comments, examples or annotations; leave existing ones untouched.
- No plan or explanation before coding. Final reply max 5 lines: files changed, migration if any, assumptions.

## 2. Project map (read before exploring)
Map lives in `.opencode/project-map/`: `index.txt` (layers, feature index, pointers), `flows.txt`, `entities.txt`, `endpoints.txt`, `auth-di.txt`, `xref.txt`.
- If the map is missing or older than the newest source file, run first:
  `pwsh -File .opencode/skills/token-saver/scripts/generate-map.ps1 -Mode refresh`
  (fallback: `powershell -File ...`). `-Mode full` rebuilds everything.
- Read `index.txt` first, then only the section you need, and jump straight to the listed files.
- Open only the files that must change, reading only the needed parts.

## 3. Exploration rules
- Targeted `grep`, never directory-wide reads. Never read `bin`, `obj`, `Migrations`, `node_modules` or generated files unless the task is about a migration.
- Never read the whole EF model snapshot. Use the entity list in the map; grep the snapshot for one entity only when a table or column name is needed. Never assume table or column names.

## 4. Project conventions
- New entity classes use the `Entity` suffix.
- EF configuration files contain no `ToTable`; the snake_case naming convention names tables and columns.
- One file per operation (command/query + validator + handler) under `Features/<Name>/Commands` or `Features/<Name>/Queries`.
- Repositories only load and stage; handlers call `IUnitOfWork.SaveChangesAsync`.
- Do not re-send or rewrite earlier code unless asked.

## 5. After changing files
- Added, removed or moved files: run the map refresh command from section 2 before finishing.
