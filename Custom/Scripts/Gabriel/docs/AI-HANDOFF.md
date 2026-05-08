# Gabriel AI Handoff

This file is a self-contained handoff for another AI that needs to keep
working on the Gabriel script cutover without replaying the whole chat.

## Goal

The user wanted a final, non-transitional cutover of Gabriel-authored VaM
code out of piggy-backed EasyMate/AutoMate customizations and into a
canonical `Custom/Scripts/Gabriel` tree.

The intent was:

- no "good enough for now" stopgap work
- no backwards compatibility layers for old Gabriel-owned code
- delete dead or clearly obsolete Gabriel logic instead of hiding it
- keep only real upstream/third-party dependencies outside Gabriel
- organize each feature so a feature-specific AI can work from adjacent docs

## Non-Negotiable User Constraints

- VaM plugin `.cs` files use C# 6 only.
- Do not use `System.Reflection`; use SuperController/VaM APIs.
- When comments are added in code, keep them wrapped to 80 chars max.
- This repo uses a whitelist-style `.gitignore`; new files must be visible
  to git without relying on `git add -f`.
- Commit only files changed in the current task. Never stage repo-wide.
- The repo can have more than 10k unrelated dirty/untracked files.
- Do not revert unrelated user changes.
- Do not expand scene JSON rewrites unless the user explicitly asks.
- The user wants a one-line semantic commit message at the end of every
  response, and expects scoped commits after edits in this workspace.

## High-Level Current State

- Gabriel now has a canonical script tree under `Custom/Scripts/Gabriel`.
- Bootstrap/session loading lives in Gabriel-owned files, not in custom
  EasyMate/AutoMate piggy-backing chains.
- Feature-local `FEATURE.md` files, project-local Cursor skills, rules,
  and a reminder hook were created for feature-specific AI workflows.
- Old "OnSceneStartup" naming is gone from the session settle runtime.
- Keyboard polling was centralized and then trimmed down to only the
  current live hotkeys.
- The old snap-possession, overhead gesture, and dual-hand look/gesture
  paths were removed because the user said they are no longer used.

## Major User Decisions To Preserve

These are not tentative:

- Full Gabriel cutover was chosen.
- The agent/doc system chosen was project-local skills + rules + hooks.
- No MP4-related code is wanted anymore.
- `ImprovedPoV_TongueLicking.cs` was explicitly removed.
- Old `motion-hooks` was split into `e-motion` and `spankings`.
- `PossessionWalker` and `FemaleJointsModifier` were declared obsolete.
- Old male creation/template logic was removed from session-stack.
- `RemoveAllPersonPlugins`, `ResetMorphs`, `ShowUI`, and `HideUI` were
  removed from the session-stack side.
- `EXPLOSION_LIMITER`, `_IMPROVED_POV`, and `Spankings` should not be
  auto-loaded onto every male on scene load.
- `OnSceneStartup` naming had to disappear from both file names and code.
- The only desired possession flow now is passenger possession triggered
  from the hand menu or by pointing lasers at a target.
- `P` should do nothing.
- `HotkeySnapNearestHeadHideHandsThenSnap` is dead and should stay dead.
- `CycleFemaleThenMalePersonRootEditMenuOnHotkey` is dead and should stay
  dead.
- `LogYKeyHmdPoseDebug` is dead and should stay dead.
- Over-head gesture logic is dead and should stay dead.
- Detection based on "looking at my two hands" is dead and should stay
  dead.
- The back-of-hand angle logic for showing the palm HUD is still required.

## Canonical Areas

Start with `Custom/Scripts/Gabriel/docs/FEATURE-INDEX.md`. It points to
the current feature docs, skills, and rules.

Key roots:

- `Custom/Scripts/Gabriel/bootstrap`
- `Custom/Scripts/Gabriel/session-stack`
- `Custom/Scripts/Gabriel/features/ui-hud`
- `Custom/Scripts/Gabriel/features/palm-hud`
- `Custom/Scripts/Gabriel/features/passenger-possession`
- `Custom/Scripts/Gabriel/features/scene-camera`
- `Custom/Scripts/Gabriel/features/clothing-interactions`
- `Custom/Scripts/Gabriel/features/head-hide`
- `Custom/Scripts/Gabriel/features/e-motion`
- `Custom/Scripts/Gabriel/features/spankings`
- `Custom/Scripts/Gabriel/tools/scene-camera`
- `Custom/Scripts/Gabriel/tools/scene-menu`

## Files To Read First

If you need the current runtime shape quickly, read these in roughly this
order:

1. `Custom/Scripts/Gabriel/docs/FEATURE-INDEX.md`
2. `Custom/Scripts/Gabriel/bootstrap/GabrielBootstrap.cs`
3. `Custom/Scripts/Gabriel/bootstrap/FEATURE.md`
4. `Custom/Scripts/Gabriel/session-stack/FEATURE.md`
5. `Custom/Scripts/Gabriel/session-stack/src/GabrielSessionStack.cs`
6. `Custom/Scripts/Gabriel/session-stack/src/SceneSettle.cs`
7. `Custom/Scripts/Gabriel/session-stack/src/PlaybackHold.cs`
8. `Custom/Scripts/Gabriel/session-stack/src/InitialExposureChange.cs`
9. `Custom/Scripts/Gabriel/features/ui-hud/FEATURE.md`
10. `Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cs`
11. `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtons.cs`
12. `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtonsHotkeys.cs`
13. `Custom/Scripts/Gabriel/features/palm-hud/FEATURE.md`
14. `Custom/Scripts/Gabriel/features/palm-hud/VrEulerPossessHandHud.cs`
15. `Custom/Scripts/Gabriel/features/palm-hud/VrEulerPossessPoseCheck.cs`
16. `Custom/Scripts/Gabriel/features/palm-hud/VrInput.cs`
17. `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
18. `Custom/Scripts/Gabriel/features/passenger-possession/PassengerRuntime.cs`
19. `Custom/Scripts/Gabriel/features/scene-camera/FEATURE.md`
20. `Custom/Scripts/Gabriel/features/scene-camera/SceneCameraPatch.cs`
21. `Reference/VaM-Scripting-Notes.md`
22. `Reference/VaM-Scene-Startup-And-Settle.md`
23. `Reference/EasyMate-next-scene-hand-hud-handoff.md`

If you need the older palm-hud reasoning/history after the recent cleanup:

- `Custom/Scripts/Gabriel/features/palm-hud/LEGACY-NOTES.md`

## Session Stack State

Canonical session-stack files:

- `Custom/Scripts/Gabriel/session-stack/src/GabrielSessionStack.cs`
- `Custom/Scripts/Gabriel/session-stack/src/SceneSettle.cs`
- `Custom/Scripts/Gabriel/session-stack/src/PlaybackHold.cs`
- `Custom/Scripts/Gabriel/session-stack/src/InitialExposureChange.cs`

Important facts:

- The old `OnSceneStartup` naming was intentionally removed.
- The runtime type is `SceneSettleRuntime`.
- The old `SessionKeyboardShortcuts.cs` file is gone.
- `Space` hotkey release now crosses plugin boundaries through a
  `JSONStorableAction` registered on the session-stack side.

## HUD / Palm HUD / Possession State

Current live model:

- Desktop hotkeys are owned by
  `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtonsHotkeys.cs`.
- `GabrielHudButtons.cs` is no longer partial.
- Remaining first-party keyboard handling is only:
  - `Space`
  - `Ctrl+Shift+S`
  - `K`
  - `O`
  - `F`
- `GabrielHudButtons.Hotkeys.cs` was replaced by
  `GabrielHudButtonsHotkeys.cs`.
- `VrGestureRuntime.cs` was removed.
- `VrEulerPossessPoseCheck.cs` now only preserves the right-hand
  back-of-hand pose logic needed by the palm HUD.
- The old `I`, `P`, `C`, and `Y` hotkeys were removed.
- The old head snap helper cluster was removed.
- The old over-head gesture and dual-hand gesture/detection code was
  removed.
- `GripHandVisibility.cs` no longer carries the old temporary suppress
  window that was only used by the removed dual-hand VR-euler path.

Live possession expectation:

- passenger possession only
- hand-menu driven or laser-target driven
- no `P` key possession
- no old snap-to-head possession flow

## Other Important Cleanup Already Landed

- `Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV_TongueLicking.cs`
  was deleted.
- `Custom/Scripts/Gabriel/features/local-mp4-viewer/` was deleted.
- The old `motion-hooks` area was split into:
  - `Custom/Scripts/Gabriel/features/e-motion`
  - `Custom/Scripts/Gabriel/features/spankings`
- `Custom/Scripts/Gabriel/session-stack/SETTINGS.json` was deleted.
- `Custom/Scripts/Gabriel/session-stack/src/OnSceneStartup.cs` and its
  old split files were deleted in favor of the new settle runtime files.

## Feature-Agent System

The AI support structure already exists:

- per-feature `FEATURE.md`
- per-feature Cursor skills under `.cursor/skills`
- per-feature doc rules under `.cursor/rules`
- reminder hook wired through `.cursor/hooks.json`

Use the feature docs as the canonical first stop before editing a feature.
If you change a feature, keep its adjacent `FEATURE.md` aligned in the
same turn.

## Most Relevant Recent Commits

These are the commits most relevant to the current state of the code:

- `35b6ae7` `refactor(hotkeys): remove obsolete snap and gesture paths`
- `293ac26` `refactor(hotkeys): centralize keyboard polling`
- `6afb582` `refactor(session-stack): rename OnSceneStartup runtime`
- `60020f3` `refactor(session-stack): remove legacy possession hotkey flow`
- `f03fcac` `refactor(session-stack): rename settle partial files`
- `e90da88` `refactor(session-stack): split OnSceneStartup into partial components`

## Known Caveats

- Some log/error strings may still say "Easy Mate" even in Gabriel-owned
  code. Do not assume those strings mean the old architecture is still
  desired.
- The repo may still contain unrelated dirty or deleted files outside the
  Gabriel scope. Do not stage them accidentally.
- The last hotkey/gesture cleanup was lint-clean in the editor, but there
  was no in-game VaM runtime verification after the final removal pass.

## Safe Continuation Strategy

When picking up new work:

1. Read `Custom/Scripts/Gabriel/docs/FEATURE-INDEX.md`.
2. Read the feature-local `FEATURE.md` for the area you are touching.
3. Confirm whether the target behavior is still supposed to exist; many old
   Gabriel/EasyMate behaviors were intentionally deleted.
4. Keep edits C# 6 compatible.
5. Stage only the files you touched.
6. Commit scoped changes only.

## If The Next AI Touches Input Or Possession

Read these before changing anything:

- `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtonsHotkeys.cs`
- `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtons.cs`
- `Custom/Scripts/Gabriel/features/palm-hud/VrEulerPossessHandHud.cs`
- `Custom/Scripts/Gabriel/features/palm-hud/VrEulerPossessPoseCheck.cs`
- `Custom/Scripts/Gabriel/features/passenger-possession/PassengerRuntime.cs`
- `Custom/Scripts/Gabriel/features/clothing-interactions/GripHandVisibility.cs`

## If The Next AI Touches Scene Startup / Settle

Read these before changing anything:

- `Custom/Scripts/Gabriel/session-stack/FEATURE.md`
- `Custom/Scripts/Gabriel/session-stack/src/GabrielSessionStack.cs`
- `Custom/Scripts/Gabriel/session-stack/src/SceneSettle.cs`
- `Custom/Scripts/Gabriel/session-stack/src/PlaybackHold.cs`
- `Custom/Scripts/Gabriel/session-stack/src/InitialExposureChange.cs`
- `Reference/VaM-Scene-Startup-And-Settle.md`

## Summary

The current Gabriel tree is already past the big cutover. The most recent
state change was a deliberate removal of obsolete snap, hotkey, and gesture
paths so the live behavior matches the user's current workflow:

- passenger possession only
- back-of-hand palm HUD pose retained
- no old snap-possession or dual-hand gesture paths
- no `P/C/Y/I` keyboard flow

Do not reintroduce the removed behaviors unless the user explicitly asks.
