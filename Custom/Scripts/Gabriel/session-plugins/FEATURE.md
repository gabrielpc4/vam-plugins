# Gabriel Session Plugins

## Purpose
Session bundle loaded from `GabrielSessionPlugins.cslist`. The host
`GabrielSessionOrchestrator` script drives scene loads: same-folder load
pulses, full scene-settle workflow, late feature ticks, HUD binding, and the
scene-settle release action for the shared hotkey dispatcher.

The same compile unit includes **DildoOnHands**, **`ClothingClassifier`** +
**`TriggerClothingRemover`**, **`GabrielSessionOrchestrator`**, and
**`../util/SameFolderLoadCheck.cs`** (path normalization plus idle vs load-folder
matching for **`SameFolderCameraRetain`**, **`SceneSettleRuntime`**, and
clothing touch-fall same-folder suppression). Bootstrap injects only
`VaMLogClipboardHud.cslist` plus **`GabrielSessionPlugins.cslist`** (VaM does not
nest `.cslist` files; multiple scripts share one compile by listing their `.cs`
paths in this list).

## Live Files
- `GabrielSessionPlugins.cslist`
- `src/GabrielSessionOrchestrator.cs` *(session runtime; scene settle + toggles)*
- `src/SceneSettle.cs`
- `src/PlaybackHold.cs`
- `src/InitialExposureChange.cs`
- `../util/SameFolderLoadCheck.cs` *(path normalize + same-folder checker;
  shared with `GabrielHud.cslist`)*
- `../features/dildo-on-hands/DildoOnHands.cs`
- `../features/clothing-interactions/ClothingClassifier.cs`
- `../features/clothing-interactions/TriggerClothingRemover.cs`
- `../features/clothing-interactions/ClothingTouchFallOffDeferredMerge.cs`
  *(optional standalone; touch-fall merge is implemented inline on **`GabrielSessionOrchestrator`**)*

## Load Path
- Loaded as a session plugin by `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`.
- `GabrielSessionOrchestrator` runs in `Update` / `LateUpdate` and detects
  same-folder load pulses before delegating scene-settle control to the
  `SceneSettleRuntime` partial set in `src/SceneSettle.cs`,
  `src/PlaybackHold.cs`, and `src/InitialExposureChange.cs`.

## Responsibilities
- Track scene load edges and same-folder load pulses.
- Hold simulation/audio/exposure during scene settle through
  `SceneSettleRuntime` and same-folder load guards.
- Prime the shared per-frame Person possession snapshot before late feature
  ticks fan out to hand, palm-HUD, and toy helpers.
- Release transient head-hide state after scene settle completes.
- Register the emergency `Space` release action consumed by the HUD hotkey
  dispatcher.

## Dependencies And Coupling
- Depends on `src/SceneSettle.cs`, `src/PlaybackHold.cs`,
  `src/InitialExposureChange.cs`, and `../util/SameFolderLoadCheck.cs` (Hud
  shares the latter).
- Calls `Custom/Scripts/Gabriel/features/head-hide/HeadProximityHide.cs` after
  the settle hold ends.
- Exposes a `JSONStorableAction` on `CoreControl` so the HUD plugin can
  trigger the emergency release without duplicating settle logic.

## References
- `Custom/Scripts/Gabriel/FEATURE.md` (tree-wide bootstrap / session bundle)
- `Reference/VaM-Scene-Startup-And-Settle.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Scene-settle timing, same-folder guards, or startup keybindings change.
- `SceneSettleRuntime` responsibility boundaries or the session-plugins file split
  changes.
- Action wiring or the `Space` shortcut behavior changes.
