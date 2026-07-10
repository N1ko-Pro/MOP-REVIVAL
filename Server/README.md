# MOP - REVIVAL HQ

Info site **and** rule-file server for the [MOP - REVIVAL](../README.md) mod. Built with Vite + React and
served as static files (Vercel). A small Node script generates the manifest the mod consumes.

## What it serves

- `/` — the site: **Home**, a no-code **Rule builder** for authors, the full **Rule format** reference, a
  searchable **Database**, and player **Mod docs** (install / settings / FAQ / console). English + Russian,
  switchable in the header.
- `/manifest.json` — `{ generated, count, rules: [{ id, path, sha256, size, minVer, directiveCount }] }`.
- `/rules/<ModID>.mopconfig` — the individual rule files.

The mod's `RemoteRuleSync` downloads the manifest in the background, then fetches only the rule files matching
the player's installed mods (sha256-checked, with an offline fallback).

## Navigation

A tiny hash-routed SPA (no router dependency): `#/`, `#/builder`, `#/format`, `#/database`, `#/docs`. Hash
routing keeps `/manifest.json` and `/rules/*` resolving to the real static files (no rewrites needed).

## Internationalization

`src/lib/i18n.jsx` provides `I18nProvider` + `useI18n()` with English (default) and Russian dictionaries; the
choice is persisted in `localStorage`. Long-form docs content is colocated bilingually in `Docs.jsx`; directive
descriptions are translated in `i18n.jsx` (`directiveText`) and fall back to the English canon in `ruleSpec.js`.

## Rule format: one source of truth

`src/lib/ruleSpec.js` is the single, dependency-free definition of the `.mopconfig` format — the directive
catalogue, the builder model (`BUILDER_ACTIONS`, `buildRuleText`) **and** the validator (`parseRuleFile`). It
mirrors the mod's parser exactly, so what the site generates is what the mod accepts. It is imported by:

- the website (the **Rule builder**, the **Rule format** tables and the paste/drop **Validator**), and
- `scripts/generate-manifest.mjs`, which validates **every** rule file at build time.

**Keep it in sync with the mod's parser: `../MOPR/src/Rules/RuleParser.cs`.** Supported directives: `min_ver`,
`ignore` (optional place + `fullignore`), `toggle` (modes: object/renderer/multitoggle/item/vehicle/
vehicle_physics), `change_parent`, `no_lod`, `sector`, and flags (`ignore_mod_vehicles`,
`toggle_all_vehicles_physics_only`, `no_lods`, `satsuma_ignore_renderer`, `dont_destroy_empty_bottles`,
`skip_fury_collider_fix`).

## How a rule reaches players (review pipeline)

1. **Submit** — the builder opens a prefilled GitHub "new file" PR adding `<ModID>.mopconfig` to `Server/rules/`.
2. **Auto-check** — `generate-manifest.mjs` validates every file on build; a file with errors can't be merged.
3. **Review** — a maintainer confirms the rule is sensible and merges it.
4. **Sync** — the deploy regenerates `manifest.json`; the mod downloads the rule automatically for players who
   have that mod. No mod update needed.

Configure the submit target at the top of `ruleSpec.js` (`REPO`, `RULES_PATH`).

## Project layout

```
Server/
  rules/                          # the rule database (source of truth) - *.mopconfig
  scripts/generate-manifest.mjs   # scans rules/, validates, writes public/manifest.json + copies files
  src/
    lib/ruleSpec.js               # format spec + builder model + validator (shared with the manifest script)
    lib/i18n.jsx                  # EN/RU provider, hook and dictionaries
    lib/router.js                 # tiny hash router
    components/                   # Home, Hero, Features, RuleBuilder, Format, Validator, ReviewSteps,
                                  #   Database, Docs, Footer
    styles.css
  public/                         # GENERATED (manifest.json, rules/) - git-ignored
  index.html, vite.config.js, vercel.json, package.json
```

## Develop

```bash
cd Server
npm install
npm run dev        # regenerates the manifest, then starts Vite (http://localhost:5173)
```

`npm run manifest` regenerates `public/manifest.json` on its own.

## Build / deploy (Vercel)

```bash
npm run build      # prebuild regenerates the manifest, then Vite builds into dist/
```

On Vercel, create a project from this repo and set the **Root Directory** to `Server`. Framework (Vite), build
command and output directory come from `vercel.json`. The mod points at `https://mop-revival.vercel.app`
(`SiteUrl` in `../MOPR/src/Rules/RemoteRuleSync.cs`) — change both if you deploy elsewhere.

## Add a rule file

1. **Authors:** use the **Rule builder** (or the paste/drop **Validator**) and click **Submit via GitHub** — it
   opens a prefilled PR.
2. **Maintainers:** drop `<ModID>.mopconfig` into `rules/` and commit. The manifest regenerates and validates on
   the next build/dev run.
