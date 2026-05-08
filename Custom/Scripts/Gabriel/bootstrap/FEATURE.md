# Gabriel Bootstrap

## Purpose
Single session entry plugin merged on menu/default scenes. It seeds the Gabriel runtime by injecting the canonical session plugin set onto CoreControl.

## Live Files
- `GabrielBootstrap.cs`
- `GabrielBootstrap.cslist`

## Load Path
- Scenes point plugin slot 0 at `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cslist`.
- `GabrielBootstrap.cs` injects `VaMLogClipboardHud.cslist`, `GabrielSessionPlugins.cslist`
  (includes `GabrielHud`, `GabrielSessionOrchestrator`, and related session helpers),
  `DildoOnHands.cslist`, and `VrProximityStripClothing.cslist`.
- Desktop mode appends `Custom/Scripts/prestigitis_DesktopClothGrab.cs`.

## Responsibilities
- Create the session plugin JSON payload and merge only missing entries into the CoreControl PluginManager.
- Own the bootstrap-level `Additional Button Text` and `Additional Button Scene` storables used by menu navigation.
- Set the initial navigation rig position differently for desktop and VR startup.

## Dependencies And Coupling
- Serialized scene ids use `plugin#0_geesp0t.GabrielBootstrap`.
- If plugin order or bootstrap storables change, `ui-hud` and menu scenes usually need matching doc updates.

## References
- `Custom/Scripts/Gabriel/docs/RETAIN-REMOVE-MATRIX.md` (tree ownership; no separate
  feature index)
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Session plugin list or order changes.
- Bootstrap storables or default menu target changes.
- Serialized bootstrap ids or startup positioning rules change.
