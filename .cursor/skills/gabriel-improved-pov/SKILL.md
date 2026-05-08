---
name: gabriel-improved-pov
description: Specialize on Gabriel's vendored Improved PoV plugin, first-person possession rendering, and the repo-specific integration points that depend on it. Use when editing `Custom/Scripts/Gabriel/features/improved-pov/**` or debugging Improved PoV behavior.
---

# Gabriel Improved PoV

## Instructions
1. Read `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/features/improved-pov/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
