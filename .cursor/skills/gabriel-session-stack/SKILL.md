---
name: gabriel-session-stack
description: Specialize on the Gabriel session stack, person-plugin injection, scene-settle holds, same-folder load handling, and startup keyboard shortcuts. Use when editing `Custom/Scripts/Gabriel/session-stack/**` or debugging person plugin wiring.
---

# Gabriel Session Stack

## Instructions
1. Read `Custom/Scripts/Gabriel/session-stack/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/session-stack/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
