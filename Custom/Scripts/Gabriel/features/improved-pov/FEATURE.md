# Gabriel Improved PoV

## Purpose
Vendored AcidBubbles Improved PoV plugin with repo-specific defaults and integrations. This is the only surviving Gabriel-owned first-person possession plugin in the tree; the TongueLicking fork has been removed.

## Live Files
- `ImprovedPoV.cs`

## Load Path
- Merged onto the target person by `PassengerRuntime` when passenger mode
  starts (`PluginManager.TryMergePluginOntoPerson`). Session **host** compile is
  **`GabrielSessionPlugins.cslist`** (orchestrator + HUD + shared utils).

## Responsibilities
- Handle first-person camera positioning, face/hair hiding, and render-time material swaps for possession.
- Register camera render hooks only while the effect is active.
- Expose the repo's active camera depth and height defaults through the plugin UI (depth default **0.11**; vendor stock is 0.17).
- Stay compatible with `HeadProximityHide` and passenger possession expectations.

## Dependencies And Coupling
- `session-plugins` treats this as a managed male person-plugin path.
- `passenger-possession` depends on this plugin being mergable and configurable at runtime.
- `head-hide` intentionally borrows and mirrors some of its material/hair handling strategies.

## References
- `Custom/Scripts/Gabriel/FEATURE.md`
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`
- https://github.com/acidbubbles/vam-improved-pov

## Update Checklist
Update this file in the same turn whenever any of these change:

- Default offsets or possession-only behavior change.
- Any integration assumption with passenger mode or head-hide changes.
- A new fork/variant is introduced or removed.
