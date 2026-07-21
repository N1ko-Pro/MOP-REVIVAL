# Changelog

All notable changes to **MOPR (Modern Optimization Plugin — Revival)** are documented here.
Version numbers follow the original MOP scheme; `b` marks a beta build.

## 4.0.3b (21.07.2026)

Stability release focused on two hard-to-diagnose gameplay bugs — the black screen on
sleep and the kilju bucket spilling on load — plus a new optional water-reflection
optimization for players who want a few more frames near the lake.

### Added

- **Water reflection optimization toggle** (Graphics, default **off**) — when enabled, the
  lake's reflection camera (`LakeNice`) is allowed to be culled indoors and in far sectors,
  saving frames at the cost of flat, static water. Left **off** by default, so vanilla lake
  reflections are preserved unless you opt in.

### Bug Fixes

- **Fixed the black screen / frozen state when going to sleep.** As soon as the character
  closed their eyes the screen went black, the HUD and sound disappeared, gestures stopped
  working and the framerate dropped — with no crash and no error log. The cause was in the
  `Fsm.UpdateDelayedEvents` engine patch: it iterated PlayMaker's live delayed-event list and
  removed entries in place, but `DelayedEvent.Update()` **fires** its event when the timer
  elapses, which can re-enter and mutate that same list. The sleep "wake up / restore view"
  event was dropped, leaving the screen black forever. The patch now faithfully replicates
  PlayMaker's original snapshot-based algorithm (tick a copy of the list, remove finished
  events afterwards), so re-entrant changes can no longer lose an event.
- **Fixed the kilju bucket spilling every time the save is loaded.** MOP disabled the bucket
  lid's `Removal` FSM on unload and re-enabled it (plus zeroed its local rotation) on reload;
  because a PlayMaker FSM restarts from its initial state when re-enabled, this replayed the
  "lid removed" step on every load and spilled the kilju even when the player had done nothing.
  The bucket lid is now left entirely to its vanilla FSM.

## 4.0.1b (13.07.2026)

Compatibility release: an escape hatch for full Satsuma-overhaul mods, tooling for authoring
compatibility rules, and a couple of mod-compatibility fixes.

### Added

- **`satsuma_ignore` rule flag** — makes MOP leave the Satsuma entirely to another mod (no fixes,
  node toggling, renderer culling or save-time part gluing). Fixes a hard crash on save when a mod
  fully overhauls the Satsuma (e.g. *Satsuma LX*). The rest of the world is still optimized.
- **`mopr dump <ObjectName>` console command** — writes an object's full hierarchy with per-node
  components (Rigidbody/Joint/Collider/Renderer/PlayMakerFSM) and MOP item-management markers,
  to help author precise compatibility rules.
- Rule-load diagnostics — the rules-loaded summary, `Custom.txt` state and active rule flags are
  now always logged and shown in `mopr status`.
- Bundled compatibility rules for **Driveable Svoboda** and **Satsuma LX**.

### Bug Fixes

- Fixed a `NullReferenceException` (`FRIDGE_DOORHANDLE_ERROR`) in the Yard when another mod adds a
  working fridge (e.g. *BetterMSC*) whose door handle lacks the vanilla FSM — one bad handle no
  longer aborts the whole door-handle pass.
- Hardened the save-time sauna toggle against a missing stove-heat FSM (alternative scene layouts).

## 4.0.0b (11.07.2026)

First release of the **Revival** line by ANICKON — a complete, from-scratch rewrite of
Modern Optimization Plugin (MOP) by Konrad Figura (Athlon). The optimization behaviour of the
original is preserved, while the whole codebase was rewritten clean and reorganised into clear
modules: `Core / Managers / Performance / Fixes / Saves / Rules / Utils / Configuration /
Commands / Debug / Interface / Localization`.

### Added

- Full English/Russian localization of the settings window (live language switch).
- Per-module optimization toggles: **Items**, **Places**, **Vehicles** — each can be turned
  off independently and re-enables its objects instantly.
- **Satsuma driving mode** — while seated in the Satsuma, hidden triggers/pivots/cosmetic meshes
  are suppressed and restored on exit. Engine simulation is never touched.
- **Engine patches** (Harmony) — safe game-code optimizations (transform-setter no-ops,
  multi-mask mouse raycast cache, `Fsm.UpdateDelayedEvents` fix). Automatically skipped if the
  *Reharmonization* mod is installed.
- **Distant rigidbody sleeper** — sleeps far, still, non-kinematic bodies other systems miss.
  Skipped if *MSWCOptimization* is installed; never touches jointed (hinged) parts.
- **Adaptive garbage collector** — runs `GC.Collect` only after significant managed-memory
  growth instead of on a fixed timer. Skipped if *MSWCOptimization* is installed.
- Diagnostic toggles (default **off**): **Show debug monitor** (on-screen overlay) and
  **Show log messages** (verbose console output).
- Emergency **Disable optimization** toggle that stops/starts the whole optimization live.

### Changes

- Rebuilt the settings window with headed sections, performance presets, and inline hints.
- Smoothed transition spikes: indoor decor culling is now applied one group per frame, and the
  Satsuma's on-approach node toggling is amortized across frames (state-identical, no stutter).
- Dead code removed (unused `#if PRO` branches, unused dependencies).
- Rules server communication uses managed TLS 1.2 (BouncyCastle) so rule sync works on the
  game's old Mono runtime.

### Bug Fixes

- **Fixed the Satsuma's doors, hood and boot not opening** (and the car appearing frozen in the
  air) after loading a save. Body panels are tracked as items; the re-enable path now restores
  their live physics (`isKinematic/useGravity/detectCollisions`) and no longer freezes active
  nearby parts, matching the original's intended behaviour.
- **Fixed a wheel hanging in the air after load** — the car's rigidbody is now woken when its
  physics is re-enabled, so the suspension settles immediately instead of only after driving.
- Fixed `ExceptionManager` error de-duplication that never populated its buffer.
- Fixed `FsmManager.ResetAll()` failing to reach private static fields.
- Fixed the emergency "Disable optimization" toggle being declared but never wired up.

### Credits

Based on **Modern Optimization Plugin** by Konrad Figura (Athlon), which originates from
**KruFPS** by Krutonium. Revival rewrite and maintenance by **ANICKON**. Distributed under GPLv3.
