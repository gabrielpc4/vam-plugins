# Gabriel (`Custom/Scripts/Gabriel`)

## Purpose

Gabriel is the repo-maintained VaM session stack: bootstrap entry, the main
session compile (`GabrielSessionPlugins.cslist`), optional log-clipboard plugin,
and feature modules under `features/` plus offline `tools/`.

## Bootstrap (`bootstrap/GabrielBootstrap.cs`)

Injects plugins onto **CoreControl** when missing (see `bootstrap/FEATURE.md`).
Default session URLs (VR):

1. `features/ui-hud/VaMLogClipboardHud.cslist` — copy/clear logs (its **own**
   compile).
2. `session-plugins/GabrielSessionPlugins.cslist` — **main session assembly** (see
   below).

Desktop startup may also merge `prestigitis_DesktopClothGrab.cs`.

VaM **does not** nest `.cslist` files. Shared sources belong in **one** cslist as
multiple `.cs` lines.

## Session bundle (`GabrielSessionPlugins.cslist`)

Single plugin URL; one compile that typically includes:

- **`session-plugins/src/*`** — `GabrielSessionOrchestrator`,
  `SceneSettleRuntime` partials (`SceneSettle`, `PlaybackHold`,
  `InitialExposureChange`), `SameFolderLoadCheck` (`util/SameFolderLoadCheck.cs`)
- **`features/ui-hud/*`** — `GabrielHud`, `GabrielHotkeys`, `NextSceneUiButton`
- **`features/dildo-on-hands/DildoOnHands.cs`** — toy spawn `MVRScript`
- **`features/clothing-interactions/*`** — `ClothingClassifier`,
  `TriggerClothingRemover`, `ClothingTouchFallOffDeferredMerge`
  (`ClothingTouchFallOffPluginPath`; **not** the person `MVRScript` on CoreControl)
- **`util/*`**, **`features/passenger-possession/*`**, palm-hud, scene-camera,
  hands, head-hide, e-motion, spankings, animation-no-loop — helpers

**Offline parity:** `features/ui-hud/GabrielHud.cslist` lists the **same** `.cs`
set with paths relative to `features/ui-hud/`.

**Person-only:** `ClothingTouchFallOff.cs` is merged onto **Person** atoms via
orchestrator + `PluginManager` — it must **not** be listed beside session scripts
in `GabrielSessionPlugins.cslist` or VaM would also run it on CoreControl.

## Feature index (`FEATURE.md` per folder)

| Area | Documentation |
|------|---------------|
| `bootstrap/` | [bootstrap/FEATURE.md](bootstrap/FEATURE.md) |
| `session-plugins/` | [session-plugins/FEATURE.md](session-plugins/FEATURE.md) |
| `features/ui-hud/` | [features/ui-hud/FEATURE.md](features/ui-hud/FEATURE.md) |
| `features/palm-hud/` | [features/palm-hud/FEATURE.md](features/palm-hud/FEATURE.md) |
| `features/passenger-possession/` | [features/passenger-possession/FEATURE.md](features/passenger-possession/FEATURE.md) |
| `features/scene-camera/` | [features/scene-camera/FEATURE.md](features/scene-camera/FEATURE.md) |
| `features/clothing-interactions/` | [features/clothing-interactions/FEATURE.md](features/clothing-interactions/FEATURE.md) |
| `features/hands/` | [features/hands/FEATURE.md](features/hands/FEATURE.md) |
| `features/head-hide/` | [features/head-hide/FEATURE.md](features/head-hide/FEATURE.md) |
| `features/e-motion/` | [features/e-motion/FEATURE.md](features/e-motion/FEATURE.md) |
| `features/animation-no-loop-detection/` | [features/animation-no-loop-detection/FEATURE.md](features/animation-no-loop-detection/FEATURE.md) |
| `features/spankings/` | [features/spankings/FEATURE.md](features/spankings/FEATURE.md) |
| `features/dildo-on-hands/` | [features/dildo-on-hands/FEATURE.md](features/dildo-on-hands/FEATURE.md) |
| `features/improved-pov/` | [features/improved-pov/FEATURE.md](features/improved-pov/FEATURE.md) |
| `tools/scene-camera/` | [tools/scene-camera/FEATURE.md](tools/scene-camera/FEATURE.md) |
| `tools/scene-menu/` | [tools/scene-menu/FEATURE.md](tools/scene-menu/FEATURE.md) |

## Tree ownership

[docs/RETAIN-REMOVE-MATRIX.md](docs/RETAIN-REMOVE-MATRIX.md)

## Runtime optimization history

[docs/RUNTIME-OPTIMIZATION-HISTORY.md](docs/RUNTIME-OPTIMIZATION-HISTORY.md)

## References

- `Reference/VaM-Scripting-Notes.md`

## Update checklist

Update this file when bootstrap injection paths/order change, when
`GabrielSessionPlugins.cslist` membership changes in a way users must see at a
glance, or when new `features/*/` areas gain a `FEATURE.md`.
