# Modern Optimization Plugin — Revival (MOPR)

<img align="right" src="Images/icon.png" alt="icon" width=128 />

**MOPR** is a revival and complete rewrite of the ultimate performance-enhancing project for
*My Summer Car*. Quickly and easily improve your FPS — install the mod and it's ready to go.

The **only** kind of mod that improves framerate seamlessly: no configuration required, no
buttons to press, no magic tricks. It just works out of the box, while still giving you full
control if you want it.

Based on [MOP](https://github.com/Athlon007/MOP) by Konrad Figura (Athlon), which originates
from [KruFPS](https://github.com/Krutonium/KruFPS) by Krutonium. Rewritten from scratch and
maintained by **ANICKON**.

> **Results may vary!** Performance gains depend on your hardware, installed mods and save.

## Features

- Automatic distance-based loading/unloading of items, vehicles, world objects and locations.
- Indoor decor culling and Dynamic Draw Distance for smoother framerate.
- Deep, behaviour-safe Satsuma optimizations (including a driving mode).
- Optional engine-level Harmony patches, distant-body sleeping and adaptive garbage collection.
- Full English/Russian UI with performance presets and per-module toggles.
- Save protection with automatic backups.
- Smoothed transitions — no stutter when entering buildings or approaching the Satsuma.

## Requirements

- A **legal copy** of *My Summer Car*.
- [MSCLoader](https://github.com/piotrulos/MSCModLoader).

## Installation

1. Build `MOPR.dll` (see **Building** below) or download a release.
2. Copy `MOPR.dll` into your MSC `Mods` folder
   (e.g. `...\steamapps\common\My Summer Car\Mods`).
3. Copy `Libs\BouncyCastle.Crypto.dll` into `Mods\References` (needed for rule syncing).
4. Launch the game — MOPR loads automatically.

## Building

The project targets **.NET 3.5** and restores its reference assemblies from NuGet, so no Visual
Studio installation is required — just the .NET SDK.

```powershell
# Release build
.\Scripts\build.ps1

# Debug build
.\Scripts\build.ps1 -Mode debug

# Build and deploy straight into the game's Mods folder
.\Scripts\build.ps1 -Mode deploy
```

Override the game path with `-ManagedDir "<path-to>\mysummercar_Data\Managed"` if it differs
from the default.

## Configuration

Open the mod's settings from the MSCLoader menu. Highlights:

- **Performance preset** — Ultra / Quality / Balanced / Performance, plus an active-distance slider.
- **Optimization modules** — toggle Items, Places and Vehicles independently.
- **Graphics** — shadows, dynamic draw distance, sector culling, framerate limit.
- **Diagnostics** — on-screen debug monitor and verbose logging (off by default).

## Compatibility

MOPR detects neighbouring optimization mods (*Reharmonization*, *MSWCOptimization*) and skips
overlapping work so nothing is applied twice. Mod-specific behaviour can be tuned with
`.mopconfig` rule files.

## License

Distributed under the **GNU General Public License v3**. You may use, modify, share and
distribute the source as long as you preserve attribution and state your changes. See
[LICENSE](LICENSE.md).

## Credits

- **ANICKON** — Revival rewrite and maintenance.
- **Konrad Figura (Athlon)** — original Modern Optimization Plugin.
- **Krutonium** and contributors (RedJohn260, EPS) — original KruFPS.
