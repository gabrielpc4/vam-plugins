# VaM scene: initial camera / rig pose (JSON + tooling)

Reference for editing **`Saves/scene/.../*.json`** so **VR spawn** and **monitor peel** match a desired pose. Use this when aligning one scene to another or to a pose captured in play mode.

---

## What in the scene file actually drives “where I stand”

VaM persists several related concepts; they are **not** the same camera.

| Location in scene JSON | Role |
|------------------------|------|
| **`playerHeightAdjust`** (root) | Vertical peel of the player / navigation height (float string). Saved with the scene; drives feel of eye height vs floor. |
| **`monitorCameraRotation`** `{ x, y, z }` (root) | **Monitor** center camera **local Euler** (`SuperController.MonitorCenterCamera`). **Not** the HMD pose; affects flat/monitor view tilt. |
| **Atom `WindowCamera`** `position` / `rotation` (+ `container*`, `control`) | **Desktop/window** camera rig placement. Often unchanged when tuning **VR** spawn. |
| **Atom `[CameraRig]`** `position` / `rotation` | **`VRController`** atom; corresponds to the saved rig pose that loads into player navigation (along with `useSceneLoadPosition`, etc.). Primary lever for **VR world position/yaw** when matching `SuperController.navigationRig` after load. |
| **`headPossessedController`** (root, if present) | If set, spawn view can be **possessed head**—different problem from rig-only tweaks. |

At runtime, **`SuperController.navigationRig`** is what Easy Mate logs as **`SuperController.navigationRig`** beside **`[CameraRig]`**—that is the navigation pose VaM uses for load/save in many paths (see decompiled `SuperController` around scene load / `sceneLoadPosition`).

---

## Easy Mate: built-in pose snapshots

**`EasyMate`** logs pose blocks via **`EasyMateCameraPoseLog`** (`SuperController.LogMessage`):

| Tag | When |
|-----|------|
| **`loading started`** | First frame **`SuperController.isLoading`** becomes true. Values often **not** final (e.g. `playerHeightAdjust` may still be `0`; `[CameraRig]` may show defaults). |
| **`loading false`** | **`isLoading`** true → false. JSON may still be applying; closer than “started” but not necessarily final UI peel. |
| **`scene finished loading`** | After Easy Mate’s **~1 s** post-load delay (same moment its **`sceneChanged`** path runs). Good “what the scene settled to” snapshot. |
| **`key K`** | **`KeyCode.K`** while VaM is focused—manual snapshot anytime (best for “this is what I want saved”). |

Each block logs:

- `playerHeightAdjust`
- `monitorCameraRotation` (`MonitorCenterCamera.localEulerAngles`)
- **`WindowCamera`** (prefers `mainController.control`, else fallbacks)
- **`[CameraRig]`** atom transform + **`SuperController.navigationRig`**

Use **`[scene finished loading]`** vs **`[key K]`** to see drift you want to bake into JSON.

---

## Editing workflow (recommended)

1. **Capture targets**  
   Load scene, wait for **`scene finished loading`**, move to desired VR view, press **`K`**. Save log lines for **`key K`**.

2. **Match JSON to `key K`**  
   Update **root** `playerHeightAdjust`, **`monitorCameraRotation`**, and **`[CameraRig]`** `position` / `rotation` to match the log (see numeric notes below).  
   Leave **`WindowCamera`** alone unless you intentionally change the window camera too (often identical before/after VR tweaks).

3. **Prefer simple numeric strings for near-zero components**  
   Logs may show tiny scientific notation (e.g. `1.4E-21`). In JSON, **`"0"`** is safer and visually identical for euler/precision.  
   Avoid awkward forms like **`-2.74E-29`** unless you know VaM’s parser accepts them—when in doubt use **`"0"`**.

4. **Possession**  
   If spawn is head-possessed, also compare root **`headPossessedController`** and person **`uid`** numbering across scenes—wrong possession dominates over rig tweaks.

---

## Limitations & caveats

- **VR vs monitor**: Changing **`[CameraRig]`** mainly affects **VR / navigation** spawn; **`monitorCameraRotation`** is separate from HMD eye pose.
- **Load order**: **`loading started`** logs are misleading for “final” numbers; use **`scene finished loading`** or **`key K`** for authoring JSON.
- **`useSceneLoadPosition`**: If **`false`**, behavior may ignore saved load pose—confirm root flag matches intent.
- **`worldScale`**: Alters perceived scale; usually leave unchanged unless matching another scene deliberately.
- **Atom order**: Only **one** `"id" : "[CameraRig]"` block—edit that block only.

---

## Critical: huge scene JSON files (~30MB+)

Scene saves can be **tens of megabytes** and **hundreds of thousands of lines**.

- **Do not** rely on typical IDE / patch tools that rewrite the whole file if they truncate or cap output—symptom: **black scene**, **no parse error**, **JSON ends mid-object** (tail missing closing `}` / `]`).
- **Always keep a backup** (e.g. `SceneName - Copia.json`, VaM duplicate, or copy before edit).
- **Safe approaches**  
  - **`copy /y`** (Windows `cmd`) backup → main, then edit with a method that writes the **full** string length.  
  - **Python** (or similar): `read_text` → **exact string replaces** with `count == 1` assertions → `write_text`; verify **`s.rstrip().endswith('}')`** before write.  
- After any edit: confirm file **byte size** stayed in the same ballpark as backup and **tail** of file ends with scene closure (`}`).

---

## Quick checklist before saving a scene for Git / sharing

- [ ] Tail of `.json` closes all braces (open in editor end-of-file or `Get-Content -Tail 5`).
- [ ] File size not collapsed from ~30MB → ~8MB (truncation red flag).
- [ ] `[CameraRig]` numbers match intended **`key K`** log (or deliberate deltas).
- [ ] Reload scene in VaM and confirm **`scene finished loading`** vs **`key K`** if you need pixel-perfect confirmation.

---

## Related repo docs / code

- **`Reference/VaM-Scripting-Notes.md`** — Easy Mate session scripts, including camera pose logging context.
- **`Custom/Scripts/Easy Mate/src/EasyMateCameraPoseLog.cs`** — exact fields logged.
- **`Reference/Assembly-CSharp-decompiled/SuperController.cs`** — `playerHeightAdjust`, `MonitorCenterCamera`, `navigationRig`, scene load JSON keys.
