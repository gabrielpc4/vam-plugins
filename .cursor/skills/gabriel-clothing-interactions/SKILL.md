---
name: gabriel-clothing-interactions
description: Specialize on Gabriel clothing-touch fall-off, proximity strip, and ClothingTouchFallOff person-plugin wiring. Grip visibility, overlap release, and Spankings deferral HUD sources live under `Custom/Scripts/Gabriel/features/hands/`. Use when editing `Custom/Scripts/Gabriel/features/clothing-interactions/**` or debugging those behaviors when they touch grip merge paths.
---

# Gabriel Clothing Interactions

## Session layout
- See `Custom/Scripts/Gabriel/FEATURE.md` for `GabrielSessionPlugins.cslist`
  clothing rows vs person-only `ClothingTouchFallOff.cs`.

## Instructions
1. Read `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/features/clothing-interactions/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
