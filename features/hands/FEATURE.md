# SceneControlSuite hands (HUD grip helpers)

## Purpose
HUD-compiled VR hand / overlap helpers shared by SceneControlSuite HUD routing: VR grip
cycles hand proxies **None → Male 2 → SphereKinematic (collision on) → None**.
While a Person **head or hand** is possessed, or passenger mode is
active/pending, proxies stay **None** (character mesh only). Deferred
first-grip Spankings merges, overlap full-grab auto-release without reflection.

## Live Files
- `GripHandVisibility.cs`
- `OverlapFullGrabRelease.cs`
- `SpankingsGripDeferredMerge.cs`

## Load Path
- Compiled in **`SessionPlugins.cslist`** (mirrored by
  `Hud.cslist`). Sources sit under `features/hands/`; no separate
  hands-only `.cslist`.

## Dependencies And Coupling
- Invoked from `Hud` / `Hotkeys`; clothing touch-fall deferral
  routes to **`SessionOrchestrator`** (`TryMerge…` /
  `GripHandVisibility.SetMerge…`) with same-folder suppression so grip merge runs
  at most once per load-folder navigation chain.
- Uses the shared per-frame possession snapshot from `util/PersonAtomCache` so
  hand visibility and palm-HUD gating share one Person scan.
- Uses palm-hud input, passenger runtime,
  animation-no-loop-detection heuristics, and
  `SpankingsGripBlockPathKeywords`.

## References
- `Custom/Scripts/FEATURE.md`
- `Custom/Scripts/features/ui-hud/FEATURE.md`
- `Custom/Scripts/features/clothing-interactions/FEATURE.md`
- `Custom/Scripts/features/spankings/FEATURE.md`

## Update Checklist
Update this file in the same turn whenever behavior, HUD wiring paths, grip
overlap rules, or load path assumptions change.
