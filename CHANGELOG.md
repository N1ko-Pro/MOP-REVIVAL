# Changelog

All notable changes to **MOPR (Modern Optimization Plugin — Revival)** are documented here.
Version numbers follow the original MOP scheme; `b` marks a beta build.

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
