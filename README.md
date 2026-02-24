# Constitution

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **R.E.P.O.** that adds slow, passive health regeneration based on your Health upgrade level.

## What This Mod Does

Your **Health** upgrade level now grants a subtle passive health regeneration. The more you invest in Health, the faster you recover — designed for long-run sustain, not instant healing.

### How It Works

| Health Level | Regen Rate (default) | Time per 1 HP |
|-------------|----------------------|---------------|
| 1           | 0.05/sec             | 20 seconds    |
| 3           | 0.15/sec             | ~7 seconds    |
| 5           | 0.25/sec             | 4 seconds     |
| 10          | 0.50/sec             | 2 seconds     |

> **Health level 0 = no regeneration.** You must invest in Health upgrades to benefit.

### Regen Delay

After taking damage, regeneration pauses for a configurable delay (default: 10 seconds). This prevents the mod from being too powerful during combat.

## Configuration

Settings are in `BepInEx/config/headclef.Constitution.cfg` or in the **in-game mod config menu**:

| Key | Default | Range | Description |
|-----|---------|-------|-------------|
| Enable | `true` | — | Toggle health regen on/off |
| Regen Per Health Level | `0.05` | 0.01–1.0 | HP/sec per Health upgrade level |
| Max Regen Per Second | `0` | 0–5 | Cap the regen rate (0 = no cap) |
| Regen Delay | `10` | 0–60 | Seconds to wait after damage before regen starts |

## Requirements

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx) installed for R.E.P.O.
- [Character Stats](https://github.com/headclef/Repo-CharacterStats) — required dependency

## Installation

1. Install via **Thunderstore** (recommended) — Character Stats will be installed automatically.
2. Or manually: place both `Character Stats.dll` and `Constitution.dll` into your `BepInEx/plugins` folder.
3. Launch the game — config file is generated on first run.

## Multiplayer

- Health regeneration runs **per-client** — only players with the mod installed get the regen.
- Does not affect other players or enemies.

## Development

### Project Structure
```
├── Constitution.cs                  # Plugin entry point & config
├── Patches/
│   └── HealthRegenPatch.cs          # Harmony postfix — health regen
└── README.md
```

### Building
```bash
dotnet build "../Character Stats/Character Stats.csproj"
dotnet build
```

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
