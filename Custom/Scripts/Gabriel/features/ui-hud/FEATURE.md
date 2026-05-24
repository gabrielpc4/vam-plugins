# Gabriel UI HUD

## Purpose
Main session HUD and hotkey hub. World-space menu, log copy HUD,
scene-advance UIButton resolution (`NextSceneUiButton`), keyboard routing in
`GabrielHotkeys`. Heavier `LateUpdate` / scene-settle orchestration lives in
`GabrielSessionOrchestrator` in the same session bundle.

## Live Files
- `GabrielHud.cs`
- `GabrielHud.cslist`
- `NextSceneUiButton.cs`
- `GabrielHotkeys.cs`
- `VaMLogClipboardHud.cs`
- `VaMLogClipboardHud.cslist`

## Load Path
- `GabrielBootstrap` merges `VaMLogClipboardHud.cslist` and
  `GabrielSessionPlugins.cslist` only. That bundle compiles **`DildoOnHands`**
  with orchestrator + HUD + **ClothingClassifier** / trigger / deferred-merge
  sources.
- `GabrielHud.cslist` mirrors **the same `.cs` list** as `GabrielSessionPlugins.cslist`
  (paths relative to `features/ui-hud/`) for offline HUD bundle parity.
- `VaMLogClipboardHud.cslist` stays isolated so log copy buttons can still load
  if the main session compile fails.

## Responsibilities
- Build and refresh the world-space Gabriel menu on `mainHUD`.
- World-space E-Motion / Spankings buttons start **hidden** after load;
  `ResetVROrientation` exposes **Show Easy Buttons** / **Hide Easy Buttons**
  (same wording as before) which calls this plugin's **Show UI** / **Hide UI**
  actions only — Easy Mate `MainUIButtons` are no longer tied to that toggle.
- Own the keyboard hotkeys in `GabrielHotkeys` (`Space`, `Ctrl+Shift+S`, `K`,
  `O`, `F`), plugin toggles, scene-change routing, and palm HUD next-scene via
  `NextSceneUiButton`, plus thin glue into `AnimationNoLoopDetection`, path-rule
  E-Motion merges, Spankings/Clothing grip deferrals, and fluid/camera helpers.
- Cache the resolved next-scene `UIButtonTrigger` until scene/load or atom UID
  changes invalidate it.
- `GabrielHud` owns external plugin family paths for E-Motion and Spankings.
  Clothing touch-fall path and person merges live on
  **`GabrielSessionOrchestrator`**.
- The hotkey dispatcher also invokes the session-plugins `Space` release action
  through `CoreControl`.
- Expose user toggles on **`GabrielSessionOrchestrator`** (and HUD actions) for
  remote grip link blocking, head hide, long non-loop animation-end default loads,
  monitor lasers, and fluid-cum visibility.

## Dependencies And Coupling
- Calls into other feature folders compiled in **`GabrielSessionPlugins.cslist`**
  (same sources as `GabrielHud.cslist`, paths relative to `features/ui-hud/`).
  See adjacent `FEATURE.md` under `palm-hud`, `animation-no-loop-detection`,
  `passenger-possession`,
  `scene-camera`, `clothing-interactions`, `hands`, `head-hide`,
  `session-plugins`,
  `e-motion`, `spankings`, plus `GabrielHud`-owned plugin toggles.

## References
- `Custom/Scripts/Gabriel/FEATURE.md` (Gabriel overview)
- `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
- `Custom/Scripts/Gabriel/features/animation-no-loop-detection/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`
- `Custom/Scripts/Gabriel/features/hands/FEATURE.md`
- `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md`
- `Reference/EasyMate-next-scene-hand-hud-handoff.md`
- `Reference/VaM-Scripting-Notes.md`

## Update Checklist
Update this file in the same turn whenever any of these change:

- HUD rows, hotkeys, or default toggles change.
- Plugin family paths or next-scene resolution behavior changes.
- Cross-feature callback wiring changes.
