# Gabriel Improved PoV

## Purpose
Vendored AcidBubbles Improved PoV plugin with repo-specific defaults and integrations. This is the only surviving Gabriel-owned first-person possession plugin in the tree; the TongueLicking fork has been removed.

## Live Files
- `ImprovedPoV.cs`

## Load Path
- Managed as a person plugin by `Custom/Scripts/Gabriel/session-plugins/src/GabrielSessionPlugins.cs`.
- Also merged/prepared directly by `PassengerRuntime` when passenger mode starts.

## Responsibilities
- Handle first-person camera positioning, face/hair hiding, and render-time material swaps for possession.
- Expose the repo's active camera depth and height defaults through the plugin UI.
- Stay compatible with `HeadProximityHide` and passenger possession expectations.

## Dependencies And Coupling
- `session-plugins` treats this as a managed male person-plugin path.
- `passenger-possession` depends on this plugin being mergable and configurable at runtime.
- `head-hide` intentionally borrows and mirrors some of its material/hair handling strategies.

## References
- `Custom/Scripts/Gabriel/session-plugins/FEATURE.md`
- `Custom/Scripts/Gabriel/features/passenger-possession/FEATURE.md`
- `Custom/Scripts/Gabriel/features/head-hide/FEATURE.md`
- https://github.com/acidbubbles/vam-improved-pov

## Update Checklist
Update this file in the same turn whenever any of these change:

- Default offsets or possession-only behavior change.
- Any integration assumption with passenger mode or head-hide changes.
- A new fork/variant is introduced or removed.
