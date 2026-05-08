# Gabriel Session Stack

## Purpose
Runtime automation layer for scene loads and Person atoms. It decides which person plugins Gabriel owns, when they are merged, and how scene-settle playback holds behave.

## Live Files
- `GabrielSessionStack.cslist`
- `SETTINGS.json`
- `src/GabrielSessionStack.cs`
- `src/OnSceneStartup.cs`
- `src/SameFolderSceneLoadCheck.cs`
- `src/SessionKeyboardShortcuts.cs`

## Load Path
- Loaded as a session plugin by `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`.
- Reads configuration from `Custom/Scripts/Gabriel/session-stack/SETTINGS.json`.
- Owns Gabriel-managed person plugin paths for `ImprovedPoV` and `ClothingTouchFallOff` plus scene-settle control.

## Responsibilities
- Track scene load edges, Person atom changes, and pending reloads before merging managed person plugins.
- Keep person plugin state consistent across scene loads and Person atom changes.
- Hold simulation/audio/exposure during scene settle through `OnSceneStartup` and same-folder load guards.
- Expose emergency/session shortcuts through `SessionKeyboardShortcuts`.

## Dependencies And Coupling
- Depends on `Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV.cs` and `Custom/Scripts/Gabriel/features/clothing-interactions/ClothingTouchFallOff.cslist`.
- Shares same-folder load logic with the `scene-camera` feature and startup-state assumptions with `ui-hud`.

## References
- `Reference/VaM-Scene-Startup-And-Settle.md`
- `Reference/VaM-Scripting-Notes.md`
- `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- Managed person plugin paths or settings keys change.
- Scene-settle timing, same-folder guards, or startup keybindings change.
- Person plugin merge timing or atom-change reload behavior changes.
