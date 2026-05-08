---
name: gabriel-bootstrap
description: Specialize on the Gabriel bootstrap session entry plugin, session plugin injection order, and initial menu scene wiring. Use when editing `Custom/Scripts/Gabriel/bootstrap/**` or diagnosing Gabriel startup and PluginManager loading.
---

# Gabriel Bootstrap

## Instructions
1. Read `Custom/Scripts/Gabriel/bootstrap/FEATURE.md` before changing this area.
2. Read linked feature docs from that file when the change crosses feature boundaries.
3. Keep VaM plugin code C# 6 compatible and use `SuperController` APIs instead of forbidden desktop-only file APIs or reflection.
4. Update `Custom/Scripts/Gabriel/bootstrap/FEATURE.md` in the same turn whenever behavior, owned files, load paths, inputs, settings, or dependencies change.
