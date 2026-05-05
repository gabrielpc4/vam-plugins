# VaM Scripting Notes

Working reference for future custom scripting in this workspace.

## Scope

- **`Assembly-CSharp` decompile (where to read it):**
  - **Inside this workspace:** `Reference/Assembly-CSharp-decompiled` — decompiled from `VaM_Data/Managed/Assembly-CSharp.dll`, beside these notes.
  - **Same tree outside VaM (optional mirror):** `d:\Games\Assembly-CSharp-decompiled` — use for IDE search, Cursor `@` references, or tooling when you want a path that does not live under `VaM`. Keep it in sync if you rely on both (same ILSpy/dnSpy export).
- Do not edit files under `Reference/Assembly-CSharp-decompiled` (or the mirror); they are only for reverse-engineering how VaM works.
- The live scripts VaM actually loads in this workspace are under `Custom/Scripts`.

### C# language level (VaM plugin compiler)

VaM’s in-game script compilation is **not** a full modern C# toolchain. Treat plugin sources as **C# 6.0–style** unless you verify otherwise: features from **C# 7 and later** often fail with errors such as **“declaration expression cannot be used because it is not part of the C# 6.0 language specification”**.

Examples to **avoid** in VaM-loaded scripts:

- **Inline `out` declarations** (C# 7): `Foo(out int x);` or `GetWorld(head, out Vector3 p);` — use a **pre-declared** variable instead:

  ```csharp
  Vector3 p;
  GetWorld(head, out p);
  ```

- Other C# 7+ features (pattern `switch`, `throw` expressions, `ref` locals/returns, `default` without a type, **local functions** (nested `void Name(...)` inside a method — VaM may report *keyword void cannot be used in context*), etc.) — prefer older equivalents.

- **`System.IO`** (`File`, `Directory`, `Path`, etc.) — **prohibited** in typical VaM user plugin compilation; use **`SuperController.singleton.ReadFileIntoString(relativePath)`** and **`SaveStringIntoFile(relativePath, contents)`** with paths **relative to the VaM install** (e.g. `Custom/Scripts/...`, forward slashes). Append by read + concatenate + save.

The decompiled `Assembly-CSharp` reference and many community plugins assume Unity/VaM-era constraints; when adding new code under `Custom/Scripts`, match that conservative style.

- The main custom areas to keep in mind in this workspace are:
  - `Custom/Scripts/AutoMate`
  - `Custom/Scripts/Easy Mate`
  - `Custom/Scripts/LFE` — **`KeyboardShortcuts`** (session-style rebinding UI; see below)
  - `Custom/Scripts/Passenger`

## Most Relevant Live Scripts

### `Easy Mate`

- `Custom/Scripts/Easy Mate/src/AutoLoadEasyMate.cs`
  - Menu/bootstrap plugin.
  - Injects session plugins into the `CoreControl` `PluginManager`.
  - **Session load order** (first missing entries are prepended when merging):
    1. `Custom/Scripts/Easy Mate/VaMLogClipboardHud.cslist` — log clipboard HUD (**separate compile** from `EasyMate.cslist`).
    2. `Custom/Scripts/Easy Mate/EasyMate.cslist` — main Easy Mate stack (includes `MainUIButtons`).
    3. `Custom/Scripts/AutoMate/SESSION_PLUGINS/Auto_Load_Person_Plugins.cslist`.
  - In desktop mode, also adds `Custom/Scripts/prestigitis_DesktopClothGrab.cs`.
  - Directly sets `SuperController.singleton.navigationRig.position` during init, so it already affects initial camera placement.

- **`Custom/Scripts/Easy Mate/VaMLogClipboardHud.cslist`**
  - Single source: `src/VaMLogClipboardHud.cs`.
  - **Why it exists:** The three VaM log buttons (**Copy Errors**, **Copy Console**, **Clear logs**) must still load if `EasyMate.cslist` fails to compile or `MainUIButtons` fails at runtime. They use the same `mainHUD` placement math as Easy Mate column **0** so the combined grid lines up.
  - **Behavior:** Reads `SuperController.allErrorsText` / `allErrorsText2` and `allMessagesText` / `allMessagesText2`; copy uses `GUIUtility.systemCopyBuffer`; clear calls `ClearErrors()` and `ClearMessages()` (see decompiled `SuperController`). To **show/hide** those panels from the keyboard, **`LFE/KeyboardShortcuts`** exposes **`Error Log > Toggle`** and **`Message Log > Toggle`** (see **LFE / KeyboardShortcuts**).
  - **Not toggled** by Easy Mate `Show UI` / `Hide UI` (separate plugin); it always builds its small canvas in `Start()` if the plugin loads.
  - **Grid alignment:** Same canvas scale/position/`Translate(0, 0.2f, 0)` as `MainUIButtons`; button Y uses `0.50f - row * ySpacing` with `xSpacing = 0.22f`, `ySpacing = 0.05f`; log column only uses **column 0**, rows **0–2**.

- `Custom/Scripts/Easy Mate/src/EasyMate.cs`
  - Main manager for Easy Mate.
  - Owns `Show UI` and `Hide UI` actions.
  - Tracks scene changes by watching `SuperController.singleton.isLoading`.
  - Creates/configures the `_ResetVROrientation` atom and its plugin.
  - Calls `mainUIButtons.ClothingResetCycle()` when the load directory changes.

- `Custom/Scripts/Easy Mate/src/MainUIButtons.cs`
  - Builds **a second** world-space HUD canvas on `SuperController.singleton.mainHUD` (Easy Mate only). **Columns 1–3** of the 4-column grid; **column 0** is reserved for `VaMLogClipboardHud`.
  - **Layout** (`CreateButtons`): `column * 0.22f`, `0.50f - row * 0.05f` per button; widths — E‑Motion column **132px**, middle **118px**, right **132px**.
  - **Grid (MainUIButtons only):**

    | Row | Col 1 | Col 2 | Col 3 |
    |-----|--------|--------|--------|
    | 0 | E‑Motion Lite | *(empty)* | Remove underwear |
    | 1 | E‑Motion Original | *(empty)* | Remove All Clothes |
    | 2 | E‑Motion Final | *(empty)* | **+/‑ Spankings Male** |
    | 3 | Remove E‑Motion | *(empty)* | Remove Spankings |
    | 4 | E‑Motion M | E‑Motion F | *(empty)* |

  - **Spankings:** Toggle label **`+ `** / **`- `** + **`Spankings Male`** when every `Person` has the Spankings plugin filename; row 3 **Remove Spankings** clears the plugin from all Persons. Hotkey **Ctrl+Shift+S** still toggles the same merge/remove behavior.
  - **E‑Motion:** **Lite**, **Original**, **Final**, **Remove E‑Motion**; **`TryReplaceEmotionFamilyWithExactPath`** enforces one family pack per merge. Row 4 **E‑Motion M** / **E‑Motion F** are gender-specific merge entry points.
  - **VR palm HUD / hotkeys:** Rig alignment, female/male targeting cycles, and related clears live in **`MainUIButtons`**, **`EasyMateVrEulerPossessHandHud`**, and **`EasyMateFemalePassengerRuntime`** — see **`Custom/Scripts/Easy Mate/VR-POSSESSION-PALM-HUD-AND-SNAP.md`**.
  - **Remove All Clothes / Remove underwear:** Same `DAZCharacterSelector` logic as before (`StripAllClothesOnAllPersons`, `RemoveUnderwearOnAllPersons`).
  - Shared plugin path: `GetJSON` → normalize paths → `LateRestoreFromJSON`; empty plugin set uses empty `PluginManager` JSON.
  - **`Show UI` / `Hide UI`:** Toggle **only** MainUIButtons HUD elements (not `VaMLogClipboardHud`).
  - `ClothingResetCycle()` refreshes **Spankings** `+`/`-` when `EasyMate.cs` sees a `currentLoadDir` change.

#### E-Motion (full), E-MotionLite, E-Motion Final, and path-keyword merge

- **Full E-Motion / AutoMate original** (HUD **E‑Motion Original** — see `MainUIButtons.cs`):
  - Typical plugin path: **`Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist`** (`PluginEMotion`).
  - VRAdultFun **`EmotionEngine`** partial stack: facial morphs, gaze/state-machine logic, optional scripted **head/neck** movement when UI storables enable it. In the AutoMate-bundled sources, **`AllowTorsoAndLimbEffects`** is **`false`**, so torso/limb joint/morph drivers stay off while face/eyes/head (and remaining systems) still run.

- **E-MotionLite** (HUD **E‑Motion Lite** button, or merged onto every **`Person`** when the path rule matches — never replaces the AutoMate tree on disk):
  - **`Custom/Scripts/E-MotionLite/E-Motion_AddThisONLY.cslist`** (`PluginEMotionLite`).
  - Uses the **same cslist file name** as full E‑Motion (`E-Motion_AddThisONLY.cslist`). Easy Mate **`TryReplaceEmotionFamilyWithExactPath`** removes every known pack basename (`E-Motion_AddThisONLY.cslist`, **`E-Motion_Final_AddThisONLY.cslist`**) then adds the desired **full path**, so lite vs full vs Final never stack on the same atom when Easy Mate performs a merge.
  - Lite-only behavior lives in **`Custom/Scripts/E-MotionLite/Scripts/EmotionEngine.cs`**: **`EmotionLiteDisableHeadAndNeck`** keeps scripted head/neck motion off regardless of presets/UI toggles; **`EmotionLiteLeaveEyeTargetUntouched`** skips driving **`eyeTargetControl`** and skips forcing Eyes **`lookMode`** to **Target** every frame so VaM defaults, the loaded scene, or the user retain whatever eye aim setting already applies.

- **E-Motion Final** (VRAdultFun standalone tree — HUD **E‑Motion Final** button, post–mocap female merge, or scene JSON; **stripped** when another family pack is merged via Easy Mate):
  - **`Custom/Scripts/E-MotionFinal/E-Motion_Final_AddThisONLY.cslist`** (`PluginEMotionFinal`).
  - **Different basename** from full/lite so scenes and tooling do not confuse it with **`E-Motion_AddThisONLY.cslist`**. Lives under **`Custom/Scripts/E-MotionFinal/`** so edits to Original or Lite sources do not affect Final files when only Final is loaded.
  - Presets and defaults load from **`Custom/Scripts/E-MotionFinal/Presets/`** (same pattern as Original/Lite: **`GetPluginPath()`** + `\Presets\`), not from legacy **`Custom/E-Motion/Presets`**.
  - After a **long non-loop** scene mocap ends, **`EasyMateMotionAnimationEmotionEnd`** calls **`MergeEmotionFinalOnFemalePersonsOnly`** once per scene (EasyMate toggles **`mergeEmotionWhenLongMocapEndsNoLoop`** / **`longMocapMinSecondsForEmotionMerge`**).

- **`EasyMateEmotionPathKeywords.cs`** (`Custom/Scripts/Easy Mate/src/EasyMateEmotionPathKeywords.cs`, listed in **`EasyMate.cslist`**):
  - **`KeywordsFileRelative`**: **`Custom/Scripts/Easy Mate/emotion_path_keywords.txt`** — read with **`SuperController.ReadFileIntoString`**; `#` lines skipped; tokens trimmed and split on newline / comma / semicolon; stored lowercase for matching.
  - **Haystack:** **`SuperController.currentLoadDir`** + space + **`currentSaveDir`**, backslashes normalized to slashes, lowercased — **folder strings only** (no fingerprinting or reading scene `.json` for this rule).
  - **`EvaluatePathRule`** / **`MatchesCurrentScenePath`**: substring match of each keyword against the haystack; used by **`EasyMate.cs`** after the scene settles (and when **`Person`** atoms appear, if the rule matched) to decide **`MergeEmotionLiteForPathRuleOnAllPersonsOnly`** vs skipping automatic lite merge.

- `Custom/Scripts/Easy Mate/src/EasyMateGripHandVisibility.cs`
  - VR grip (Quest squeeze / OpenVR HoldGrab): toggles each controller side via **`MeshVR.HandModelControl`** — articulated **`Male2`** / **`Male 2`** vs **`SphereKinematic`** (same strings as User Preferences → VR Hands → hand choice list). While **any** `Person` **head** or **hand** control is **possessed**, both sides are forced to **`None`** (no sphere, no Male2). When not possessed: Keeps **`leftHandEnabled` / `rightHandEnabled`** on when **`SphereKinematic`** exists so the sphere proxy stays visible; **`useCollision`** is **`false`** only right after **`DisableVrHandModelsForSceneStart`** (both spheres, no grip yet); after **any** VR grip toggle this scene, **`useCollision`** stays **`true`** even when both sides return to spheres (VaM applies collision globally on that control). Applies to **`SuperController.singleton.commonHandModelControl`** and **`alternateControllerHandModelControl`**. If **`SphereKinematic`** is missing from the hand arrays for a side, falls back to **`leftHandEnabled`/`rightHandEnabled` = false** for that side only. **VaM conflict:** with **`allowGrabPlusTriggerHandToggle`** enabled, **`SuperController.GetRightToggleHand()`** / **`GetLeftToggleHand()`** can call **`ToggleRightHandEnabled`** / **`ToggleLeftHandEnabled`** (hide hands) on grip+trigger combos — grip polling uses **`OVRInput.Controller.Touch`** like **`GetRightHoldGrab`**, and an extra **`WaitForEndOfFrame`** re-apply **`ApplyBothControls`** so Easy Mate wins after **`SuperController.Update`**. **Spankings:** the **first** grip-driven transition in a scene from **no** articulated side (both sphere/off after scene reset) to **any** articulated side calls **`MainUIButtons.MergeSpankingsOnAllPersonsOnly()`** (same merge as HUD **`+ Spankings`** for Persons missing the plugin); happens once per scene until **`DisableVrHandModelsForSceneStart`** resets. **VR euler possess:** **`NotifyVrEulerPossessTenSecondSuppress`** applies **`None`** immediately and opens the grip-suppress window.

#### Atom `Cube` / `MaterialOptions`: diffuse tint from plugins

Primitive atoms (e.g. **`Cube`**) expose a **`materials`** storable (**`MaterialOptions`**). The **Params** UI label **Diffuse Color** is stored as JSON / a **`JSONStorableColor`** registered under **`"Diffuse Color"`** on that storable.

- **`MaterialOptions.SetColor1(Color)`** only updates the GPU if VaM registered **`color1JSONParam`** at init (it requires **`materialForDefaults.HasProperty(color1Name)`**, usually **`_Color`**). On some prefab paths that never happens, so **`SetColor1` does nothing** and the mesh keeps the default tint (e.g. blue).
- Prefer the same path as the UI: **`atom.GetStorableByID("materials")` as `JSONStorable`**; if **`IsColorJSONParam("Diffuse Color")`**, convert RGB with **`HSVColorPicker.RGBToHSV(r, g, b)`** to **`HSVColor`**, then **`SetColorParamValue("Diffuse Color", hsv)`**.
- **Fallback:** **`atom.gameObject.GetComponentsInChildren<Renderer>(true)`**, use **`renderer.materials`** (instances), then **`material.color`** and, when **`Material.HasProperty`** is true, **`_Color`**, **`_BaseColor`**, **`_TintColor`**, **`_DiffuseColor`** via **`SetColor`**.

Example: **`Custom/Scripts/DildoOnHands/DildoOnHands.cs`** — **`ApplySpawnToyMaterialLook`** (**`SetColor1`** on spawned atoms).

- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/EasyMate_VR_Head_Cylinder_Hide.cs`
  - Static helper: **`Camera.onPreRender` / `onPostRender`** when Easy Mate **VR head proximity hide** is on: if the HMD eye is inside a **finite cylinder** (moderate radial radius along possess **up** through **`headControl`**), temporarily hides face-related skin materials (shader/alpha), hats/glasses; **male** figures also use backup/restore hair unequip while in zone — **only** on VR eye cameras (`CenterEyeAnchor`, `Camera (eye)`), not **`MonitorRig`** or mirror/reflection cameras. Chooses the **closest** Person whose cylinder contains the camera. **Load order:** inactive while **`SuperController.singleton.isLoading`**; **`SnapSkinHandler.Configure`** preflights **`Shader.Find`** before swapping materials. **C# 6**: no inline `out` variable declarations (see **C# language level** above). Navigation rig / alignment flows are separate; possession clear may call **`RestoreTransientHeadHideState()`** without toggling this feature.

### `AutoMate`

- `Custom/Scripts/AutoMate/Load_Session_Plugins.cs`
  - Session plugin bootstrap for `CoreControl`.
  - Uses `MVRPluginManager.GetJSON(...)` and `LateRestoreFromJSON(...)` to merge missing session plugins into `CoreControl`.

- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/Auto_Load_Person_Plugins.cs`
  - Most important existing reference for scene-wide person/plugin automation.
  - Handles:
    - scanning all Person atoms
    - detecting male vs female
    - adding/removing person plugins
    - preserving existing plugins while merging desired plugins
    - creating/removing a male Person atom
    - finding loaded plugins by storable ID suffix
    - resetting expression/tongue morphs
  - Already contains canonical plugin path constants for:
    - `ImprovedPoV`
    - `ImprovedPoV_TongueLicking`
    - `E-Motion`
    - `Spankings`
    - `Possess Sex`
    - `VAM Launch`

- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/MainUIButtons.cs`
  - Another world-space HUD canvas on `mainHUD`.
  - Uses flags like `wantToLoadNow`, `wantsToCycleSet`, etc., which the main plugin consumes in `Update()`.
  - Good pattern for lightweight button-to-update-loop communication.

### `Passenger`

- Live target: `Custom/Scripts/Passenger.cs`

Notes:

- Scene JSON references in this workspace commonly point to:
  - `Passenger.cs`
  - `Custom/Scripts/Passenger.cs`
- The duplicate file `Custom/Scripts/Passenger/Passenger.cs` was removed on purpose.
- If the goal is "do what Passenger does" from a logic standpoint, `Custom/Scripts/Passenger.cs` remains the source of truth for future edits.

### `LFE / KeyboardShortcuts` (LFE#9677)

- **Root:** `Custom/Scripts/LFE/KeyboardShortcuts/`
- **Plugin entry / VaM load path:** `src/KeyboardShortcuts.cslist` → lists every `Commands/*.cs`, `Extensions/*.cs`, `Models/*.cs`, `Utils/*.cs`, and **`Main/Plugin.cs`** (class **`LFE.KeyboardShortcuts.Main.Plugin`** : `MVRScript`).
- **Purpose:** User-defined **keyboard (and axis) chords** bound to **commands** — camera move/look, Play/Edit, animation speed, atom add/select/delete/hide, selected-atom position/rotation nudges, **Selected Options** tab jumps (`AtomSelectTab`), scene new/open/save, world/time scale, graphics toggles (MSAA, pixel lights, soft body, etc.), **`HardReset`**, and per-atom / per-controller actions built from **`SuperController.GetSelectableAtoms()`** (including **Add Plugin**, **ShowUI** tabs per atom, **Plugin** actions on loaded plugins: bool/float/string chooser, show UI, call **`JSONStorableAction`**, etc.).
- **Log / menu overlap:** Built-in commands **`ErrorLogToggle`** and **`MessageLogToggle`** (`Commands/ErrorLogToggle.cs`, `Commands/MessageLogToggle.cs`) open/close the same VaM panels whose text buffers **`VaMLogClipboardHud`** copies/clears — use either the Easy Mate log HUD buttons or LFE binds (or both).
- **Runtime:** `Update()` matches bindings (`KeyChord.IsBeingPressed()`), respects **Unity UI** **`InputField`** focus (skips shortcuts while typing), and queues command execution via **`ViewModel`** / **`BindingEvent`**. **Axis** bindings get a final “reset” when keys release.
- **Catalog:** New commands are **`yield return`**’d from **`Models/CommandFactory.BuildCommands()`**; groups **`[Global Actions]`** / **`[Selected Atom Actions]`** come from **`CommandConst`**; atom-specific groups use **`atom.uid`**.
- **Caveat (from plugin header):** VaM’s **own** shortcuts (e.g. **T**, **E**) still fire; avoid overlapping chord choices when possible.
- **Language note:** This tree uses **modern C#** patterns (e.g. interpolated strings, `var`) in places. VaM’s in-game compiler is stricter for some plugins — if you **merge** or **port** snippets into **`Easy Mate`**-style scripts, follow **C# 6.0** rules in these notes.

### `ImprovedPoV`

- `Custom/Scripts/ImprovedPoV.cs`
  - Best reference for:
    - head/face/hair invisibility
    - camera depth/height/pitch offsets
    - clip plane adjustment
    - optional world scaling
  - Hides face/hair with shader/material tricks during camera render.

- `Custom/Scripts/ImprovedPoV_TongueLicking.cs`
  - Same core pattern plus tongue morph animation.
  - Good reference for morph access through `morphsControlUI`.

### `Possess Sex`

- `Custom/Scripts/Possess Sex/PossessSex.cs`
  - Good reference for:
    - `SelectLinkToRigidbody(...)`
    - `FreeControllerV3` possession/link state manipulation
    - enabling/disabling controller interaction flags
    - finding nearby tracked rigidbodies

### `Reset VR Orientation`

- `Custom/Scripts/Reset VR Orientation/ResetVROrientation.cs`
  - Creates another world-space HUD.
  - Toggles the Easy/AutoMate/PossessSex UIs by calling their `Show UI` and `Hide UI` actions.
  - Uses `lookCamera`, `MonitorCenterCamera`, and direct transform rotation/position changes.

## `.cslist` Conventions

- VaM can load:
  - `.cs`
  - `.cslist`
  - `.dll`
- `.cslist` is just a text file listing source files relative to the `.cslist` file.

Examples:

- `Custom/Scripts/Easy Mate/EasyMate.cslist`
  - `src/EasyMate.cs`
  - `../AutoMate/SESSION_PLUGINS/src/EasyMate_VR_Head_Cylinder_Hide.cs`
  - `src/MainUIButtons.cs`
  - (full list in the file — does **not** include `VaMLogClipboardHud.cs`)
- `Custom/Scripts/Easy Mate/VaMLogClipboardHud.cslist`
  - `src/VaMLogClipboardHud.cs` only — **separate** plugin for log copy/clear buttons (see **Easy Mate** notes above).
- `Custom/Scripts/Easy Mate/AutoLoadEasyMate.cslist`
  - `src/AutoLoadEasyMate.cs`
- `Custom/Scripts/LFE/KeyboardShortcuts/src/KeyboardShortcuts.cslist`
  - multi-file bundle under `Commands/`, `Extensions/`, `Models/`, `Utils/`, `Main/` (see **LFE / KeyboardShortcuts** above).
- `Custom/Scripts/Spankings/Spankings.cslist`
  - loads several source files under `octopussy`, `Deluxe`, and `Easy Moan`
- `Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist`
  - loads many `EmotionEngine` partial source files (full E‑Motion bundle used by Easy Mate **`PluginEMotion`**).
- `Custom/Scripts/E-MotionLite/E-Motion_AddThisONLY.cslist`
  - same **basename** as above; lite fork (`PluginEMotionLite`). Easy Mate swaps among **full**, **lite**, and **Final** with **`TryReplaceEmotionFamilyWithExactPath`** (removes all family basenames, then adds the target path).
- `Custom/Scripts/E-MotionFinal/E-Motion_Final_AddThisONLY.cslist`
  - VRAdultFun Final bundle (`PluginEMotionFinal`); separate folder and basename so it stays isolated from Original/Lite sources on disk.

This matters because:

- when adding a plugin path to a `PluginManager`, the path may point at a single `.cs` file or a `.cslist`
- if a plugin is packaged as `.cslist`, treat the whole `.cslist` as the plugin identity

## VaM Scripting Model

### Base plugin lifecycle

Most custom plugins here follow the same shape:

1. `Init()`
   - cache references
   - register JSONStorables
   - create normal plugin-panel UI if needed
2. `Start()`
   - build world-space HUD canvases
   - do setup that depends on scene objects already existing
3. `Update()`
   - poll scene state
   - poll keyboard input
   - consume button flags

### JSONStorables

Common types used throughout the codebase:

- `JSONStorableBool`
- `JSONStorableFloat`
- `JSONStorableString`
- `JSONStorableStringChooser`
- `JSONStorableAction`

Why they matter:

- values become savable/restorable in scene JSON
- values become accessible to VaM triggers/actions
- they are the standard bridge between code, UI, and saved scene state

### Standard plugin UI

On `MVRScript`, common helpers are:

- `CreateButton(...)`
- `CreateToggle(...)`
- `CreateSlider(...)`
- `CreateTextField(...)`
- `CreateScrollablePopup(...)`

### World-space HUD pattern already used here

Easy Mate, AutoMate, Possess Sex, and Reset VR Orientation all use the same HUD approach:

1. create a `GameObject`
2. add `Canvas`
3. parent it to `SuperController.singleton.mainHUD`
4. instantiate `plugin.manager.configurableButtonPrefab`
5. position buttons manually

That means we already have a working in-project pattern for adding our own scene HUD buttons.

## Core Engine Objects We Will Reuse

### `SuperController.singleton`

This is the main gateway object. The most relevant fields/properties/methods for us are:

- `GetAtoms()`
- `GetAtomByUid(string uid)`
- `GetAtomUIDs()`
- `AddAtomByType(string type, string uid)`
- `RemoveAtom(Atom atom)`
- `navigationRig`
- `lookCamera`
- `centerCameraTarget`
- `MonitorRig`
- `MonitorCenterCamera`
- `mainHUD`
- `worldScale`
- `playerHeightAdjust`
- `isLoading`
- **In-game log UI buffers (also exposed as `UnityEngine.UI.Text`):** `allErrorsText`, `allErrorsText2`, `allMessagesText`, `allMessagesText2` — kept in sync with internal strings when **`LogError`** / **`Message`** run. **`ClearErrors()`**, **`ClearMessages()`** wipe buffers and text. **`LogError`** / **`LogMessage`** static helpers append lines. Clipboard copy from plugins typically reads `.text` from those `Text` fields and sets `GUIUtility.systemCopyBuffer` (see **`VaMLogClipboardHud`**).

### `Atom`

Most common operations:

- `atom.type == "Person"`
- `atom.GetStorableByID("geometry")`
- `atom.GetStorableByID("PluginManager")`
- `atom.GetStorableByID("headControl")`
- `atom.GetStorableIDs()`
- `atom.freeControllers`
- `atom.rigidbodies`

### `MVRPluginManager`

Important behavior:

- `GetJSON(true, true, true)` returns the current plugin list.
- `LateRestoreFromJSON(...)` removes all current plugins first, then restores the provided list.

Critical consequence:

- If we call `LateRestoreFromJSON(...)` with only one new plugin, we will wipe the atom's other plugins.
- So any "add plugin if missing" feature must:
  1. read current plugin list
  2. merge desired plugin path(s)
  3. write back the complete merged list

This is exactly why `Auto_Load_Person_Plugins.cs` rebuilds the full plugin list instead of blindly adding one path.

## Scene and Plugin Discovery Patterns

### Finding people

Existing code uses:

- `SuperController.singleton.GetAtoms().Where(a => a.type == "Person")`

Female detection pattern already used:

- `!atom.GetComponentInChildren<DAZCharacter>().isMale`

### Recommended helper shape

```csharp
private static IEnumerable<Atom> GetPersonAtoms()
{
    return SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
}

private static IEnumerable<Atom> GetFemaleAtoms()
{
    return GetPersonAtoms().Where(a => !a.GetComponentInChildren<DAZCharacter>().isMale);
}

private static IEnumerable<Atom> GetMaleAtoms()
{
    return GetPersonAtoms().Where(a => a.GetComponentInChildren<DAZCharacter>().isMale);
}
```

### Getting a person's geometry/morph access

```csharp
private static DAZCharacterSelector GetCharacter(Atom atom)
{
    return atom?.GetStorableByID("geometry") as DAZCharacterSelector;
}

private static FreeControllerV3 GetHeadControl(Atom atom)
{
    return atom?.GetStorableByID("headControl") as FreeControllerV3;
}
```

### Finding loaded plugins

Two good patterns already exist:

1. compare script path in `PluginManager.GetJSON(...)`
2. scan `atom.GetStorableIDs()` for IDs like `plugin#N_FullClassName`

Example pattern from AutoMate:

```csharp
private JSONStorable FindPluginInAtom(string fullClassName, Atom atom)
{
    var pluginId = atom.GetStorableIDs()
        .Find(id => id.StartsWith("plugin#") && id.EndsWith(fullClassName));

    return pluginId == null ? null : atom.GetStorableByID(pluginId);
}
```

Use cases:

- detect if a class is already instantiated
- call its actions
- read/write its JSON parameters

## Clothing API Notes

### Main entry point

Use:

```csharp
var character = atom.GetStorableByID("geometry") as DAZCharacterSelector;
```

Then:

- `character.clothingItems`
- `character.EnableUndressAllClothingItems()`
- `character.SetActiveClothingItem(item, false)`
- `character.RemoveAllClothing()`

### What `SetActiveClothingItem(...)` actually does

From decompiled `DAZCharacterSelector`:

- sets `item.active`
- handles clothing `exclusiveRegion`
- calls `item.gameObject.SetActive(active)`
- updates clothing JSON/UI
- syncs anatomy state

So it is the correct built-in way to remove clothing from a person.

### Clothing metadata available on `DAZClothingItem`

From `DAZDynamicItem` / `DAZClothingItem`, useful fields include:

- `displayName`
- `uid`
- `tags`
- `tagsArray`
- `gender`
- `startActive`
- `disableAnatomy`
- `exclusiveRegion`

### Clothing `exclusiveRegion`

The enum values are:

- `None`
- `UnderHip`
- `UnderChest`
- `Hip`
- `Chest`
- `Shoes`
- `Glasses`
- `Gloves`
- `Hat`
- `Legs`

This is very useful for future clothing buttons:

- bra-like items are likely `UnderChest`
- panties/briefs/underwear are likely `UnderHip`
- skirts are more likely `Hip` or `Legs`

### Best heuristic for "remove underwear and bras, but not skirts"

Use this order:

1. prefer `exclusiveRegion`
   - remove `UnderChest`
   - remove `UnderHip`
2. fallback to `tagsArray` or `displayName`
   - likely matches: `bra`, `panty`, `panties`, `underwear`, `brief`, `bikini`, `lingerie`
3. explicitly skip likely skirt items
   - `skirt`, `dress`, maybe `gown`
4. avoid removing plain `Hip` / `Legs` items unless positively identified

### Clothing logic on Easy Mate HUD

`Easy Mate/src/MainUIButtons.cs` still exposes **Remove All Clothes** and **Remove underwear** (world HUD). Implementation uses `DAZCharacterSelector`, `EnableUndressAllClothingItems()`, `SetActiveClothingItem`, `LooksLikeSkirtDressOuterGarment`, `IsUnderwearLikeItem`, etc. For standalone clothing automation without the HUD, use the same APIs or **`Auto_Load_Person_Plugins`** patterns.

## Morph API Notes

The main pattern is:

```csharp
var character = atom.GetStorableByID("geometry") as DAZCharacterSelector;
var morphUI = character.morphsControlUI;
var morph = morphUI.GetMorphByDisplayName("Tongue Length");
```

Useful operations:

- `morph.SetValue(...)`
- `morph.Reset()`
- `morph.morphValue = ...`

Existing references:

- `Custom/Scripts/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/Auto_Load_Person_Plugins.cs`
- `Custom/Scripts/Easy Moan/src/EasyMoan.cs`

## Camera / Rig / Possession Notes

### Important transforms and objects

- `SuperController.singleton.navigationRig`
  - root movement/rotation basis for the player
- `SuperController.singleton.lookCamera`
  - main camera reference used for navigation math
- `SuperController.singleton.centerCameraTarget.targetCamera`
  - current center-eye target camera
- `SuperController.singleton.MonitorRig`
  - monitor mode rig object
- `SuperController.singleton.MonitorCenterCamera`
  - desktop monitor camera
- `Possessor.autoSnapPoint`
  - built-in snap target used when aligning possession
- `FreeControllerV3.possessPoint`
  - controller-side possession anchor

### Built-in possession behavior that matters later

`SuperController` stores `headPossessedController` in scene JSON when saving.

When loading a scene:

- it restores `monitorCameraRotation`
- if `headPossessedController` exists, it calls:
  - `HeadPossess(freeController, alignRig: true)`

That call eventually runs `AlignRigAndController(...)`.

### What `AlignRigAndController(...)` does

It:

- rotates `navigationRig`
- translates `navigationRig`
- adjusts `playerHeightAdjust`
- re-aims `MonitorCenterCamera`
- finally moves/aligns the possessed head controller to the `autoSnapPoint`

This is the reason scenes saved with head possession can snap both the VR viewpoint and the desktop monitor setup on load.

### What `HeadPossess(...)` does besides snapping

It also:

- marks the `FreeControllerV3` as `possessed`
- suspends motion animation playback on that controller
- optionally adjusts hold springs
- links the head controller to the motion controller rigidbody with `SelectLinkToRigidbody(...)`

So built-in possession is not a one-time teleport. It becomes a live link.

### One-shot camera move reference (rig math)

The old Easy Mate `MainUIButtons.LookAtPerson()` flow (removed with that HUD) was a useful template: find the head control of a chosen Person, offset from `possessor.autoSnapPoint`, move `navigationRig`, adjust `playerHeightAdjust`, and yaw the rig toward the target. That pattern is closer to a future "snap HMD to first/second female once, without remaining attached" than calling `HeadPossess(...)` and leaving possession linked.

### `Passenger` behavior

Passenger does not create an independent monitor camera.

It works by manipulating:

- `SuperController.singleton.navigationRig.position`
- `SuperController.singleton.navigationRig.rotation`
- sometimes `SuperController.singleton.worldScale`

So it is another reference for moving the player rig, not for separating desktop and VR cameras.

## Monitor Mode and Why It Is Coupled To VR

### Built-in monitor controls

From `SuperController`:

- `M` toggles monitor mode
- `Tab` enters free-move mouse mode
- `F` focuses selected controller
- `R` resets focus distance
- `WASD` moves
- `Z` / `X` raise / lower
- right mouse drags orbit around the focus point
- middle mouse drags pan
- mouse wheel dollies in/out

### The important coupling

All of those built-in monitor controls ultimately mutate:

- `navigationRig.position`
- `navigationRig.rotation`
- `playerHeightAdjust`

That means the built-in desktop monitor camera is **not** independent from the VR player rig.

### Why your future independent desktop camera feature is hard

If we keep using VaM's built-in monitor mode:

- the desktop free-move logic keeps moving the same rig the HMD depends on
- scene-load possession code also realigns that same rig
- `ImprovedPoV` currently treats `MonitorRig` as a POV camera too

### Most likely solution path later

The safest likely direction is:

1. stop relying on built-in free-move monitor behavior for the custom feature
2. create or hijack a separate desktop-only camera
3. drive that camera with our own keyboard/mouse input
4. leave `navigationRig` alone unless we explicitly want to move the VR player

Open technical question:

- VaM's window already uses `MonitorCenterCamera`, so we may either:
  - replace/hijack that camera's behavior while active, or
  - create a brand new camera and ensure it is what the desktop window renders

This is the biggest unknown for the future feature set.

## ImprovedPoV Head/Face/Hair Hiding

### Why this is our best reference

`Custom/Scripts/ImprovedPoV.cs` already solves:

- face invisibility
- head invisibility
- hair invisibility
- maintaining visibility outside POV

### How it works

- hooks `Camera.onPreRender` and `Camera.onPostRender`
- temporarily alters skin/hair materials right before render
- restores original values right after render

### What it hides

Skin materials with names starting with things like:

- `Face`
- `Head`
- `Lips`
- `Teeth`
- `InnerMouth`
- `Tongue`
- `Eyelashes`
- etc.

Hair hiding uses:

- `_AlphaAdjust = -1f`
- or `_StandWidth = 0f` for certain hair types

### Important current limitation

`ImprovedPoV.IsPovCamera(...)` currently returns true for:

- `CenterEyeAnchor`
- `Camera (eye)`
- `MonitorRig`

So if we want "hide only when the HMD enters a head" or "universal head hide based on proximity instead of possession", we should **not** reuse its activation condition as-is.

### Better future adaptation idea

Instead of:

- `headControl.possessed`

use:

- distance from current camera to candidate head position
- or a simple inside-head threshold around `headControl` / eye bones

But we should still reuse the existing shader/material handler strategy.

### Performance caution

Do not rebuild material handlers for every person every frame.

Better approach:

- cache per-person `DAZCharacterSelector`
- cache handler objects
- only activate handlers for:
  - the current person being snapped into, or
  - the closest person head that the camera is actually inside

## Plugin Loading / Injection Recipes

### Safe "add plugin if missing" recipe

Use this shape:

```csharp
private static MVRPluginManager GetPluginManager(Atom atom)
{
    return atom?.GetStorableByID("PluginManager") as MVRPluginManager;
}

private static List<string> GetCurrentPluginPaths(MVRPluginManager manager)
{
    var result = new List<string>();
    var current = manager.GetJSON(true, true, true);

    if (current["plugins"] != null && current["plugins"]["plugin#0"] != null &&
        current["plugins"]["plugin#0"].Value != "")
    {
        foreach (JSONNode pluginNode in current["plugins"].Childs)
            result.Add(pluginNode.Value);
    }

    return result;
}
```

Then:

1. read current plugin list
2. normalize/fix paths if needed
3. append the desired plugin path only if missing
4. rebuild a full `"PluginManager"` JSON object
5. call `LateRestoreFromJSON(...)`

### Existing plugin path constants already in project

From `Auto_Load_Person_Plugins.cs`:

- `Custom/Scripts/Spankings/Spankings.cslist`
- `Custom/Scripts/ImprovedPoV.cs`
- `Custom/Scripts/ImprovedPoV_TongueLicking.cs`
- `Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist`
- `Custom/Scripts/Possess Sex/PossessSex.cs`
- `Custom/Scripts/VAMLaunch/ADD_ME.cslist`

### Special warning about duplicate filenames

AutoMate already notes this problem:

- filename-only matching is unsafe for names like `ADD_ME.cslist`

So:

- prefer full-path matching
- only fall back to filename matching when necessary

## Existing Scene-Load and Scene-Change Patterns

Useful patterns already present in EasyMate/AutoMate:

- watch `SuperController.singleton.isLoading`
- when it changes from true to false, wait about 1 second before acting
- then refresh scene-dependent state

This is important because VaM often creates/restores objects over multiple frames during load.

For any future feature that scans atoms or modifies camera state on scene load, we should use the same delayed-post-load pattern.

## Future Request Mapping

### 1. Remove Easy Mate buttons and add our own custom buttons

**Status:** World HUD is split between **`VaMLogClipboardHud`** (log column) and **`MainUIButtons`** (columns 1–3): E‑Motion column (including **E‑Motion M/F**), clothing strip buttons, **+/‑ Spankings Male**, **Remove Spankings**, **Remove E‑Motion**. VR flows use the palm HUD and helpers documented in **`VR-POSSESSION-PALM-HUD-AND-SNAP.md`**. Further buttons: same files or follow **`AutoMate/SESSION_PLUGINS`** HUD patterns.

Likely touch points:

- `Custom/Scripts/Easy Mate/src/MainUIButtons.cs` — Easy Mate grid only.
- `Custom/Scripts/Easy Mate/src/VaMLogClipboardHud.cs` — log copy/clear only (own `.cslist`).
- `Custom/Scripts/Easy Mate/src/EasyMate.cs` — lifecycle, `Show UI` / `Hide UI` (MainUIButtons only).

Notes:

- Keep **`Show UI` / `Hide UI`** wiring intact for **`MainUIButtons`**; log HUD is a separate session plugin.
- To hide log buttons with Easy Mate, either extend **`EasyMate.cs`** / **`VaMLogClipboardHud`** or merge the plugins (not done today).

### 2. Button to remove underwear / bra only, but keep skirts

**Status:** Implemented in `Easy Mate/src/MainUIButtons.cs` as **Remove underwear** (all `Person` atoms). Uses `UnderChest` / `UnderHip` plus name/tag keywords; skips items whose name/tags look like skirt/dress/gown-style outer garments.

Likely touch points for tweaks:

- `Custom/Scripts/Easy Mate/src/MainUIButtons.cs` (`ClothingSearchBlob`, `LooksLikeSkirtDressOuterGarment`, `IsUnderwearLikeItem`)

### 3. Button to remove all clothes from all persons

**Status:** Implemented in `Easy Mate/src/MainUIButtons.cs` as **Remove All Clothes** (`EnableUndressAllClothingItems` + deactivate every clothing item per `Person`).

### 4. Rig alignment, palm HUD targets, and head-zone hide

**Status:** Use **`MainUIButtons`**, **`EasyMateVrEulerPossessHandHud`**, and **`EasyMateFemalePassengerRuntime`** for current VR entry points; see **`VR-POSSESSION-PALM-HUD-AND-SNAP.md`**. **`EasyMateVrHeadCylinderHide`** applies temporary face/material hide when the HMD is inside a Person head cylinder on VR eye cameras (Easy Mate storables **VR head proximity hide**); it does not depend on navigation-rig alignment.

Likely touch points for polish:

- `Easy Mate/src/MainUIButtons.cs`
- `Easy Mate/src/EasyMateVrEulerPossessHandHud.cs`
- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/EasyMate_VR_Head_Cylinder_Hide.cs`
- `ImprovedPoV` material hiding logic (still separate if desired)

### 5. Universal head hide when camera enters any person's head

Likely touch points:

- `ImprovedPoV.cs`
- maybe a new helper service or manager plugin

Feasibility:

- moderate

Recommended direction:

- reuse the render-time material handler idea from `ImprovedPoV`
- replace the activation condition with a camera-inside-head test
- keep caching aggressive to avoid per-frame full rescans

### 6. Button to add `E-Motion` to all persons if missing

**Status:** Implemented in `Easy Mate/src/MainUIButtons.cs` as **E‑Motion Lite**, **E‑Motion Original**, and **E‑Motion Final** HUD buttons (each replaces other family packs on all `Person` atoms first), plus **Remove E‑Motion**.

Known plugin path:

- `Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist`

Related: **E-MotionLite** is merged automatically when **`emotion_path_keywords.txt`** matches load/save folder paths — see **Easy Mate → E-Motion (full), E-MotionLite, and path-keyword merge** above (`EasyMateEmotionPathKeywords.cs`).

### 7. Button to add `Spankings` to all persons if missing

**Status:** Implemented in `Easy Mate/src/MainUIButtons.cs` as **+/‑ Spankings Male** (merge-add by filename when enabling).

Known plugin path:

- `Custom/Scripts/Spankings/Spankings.cslist`

### 8. Button to add `RealGaze` to all persons if missing

**Status:** **Not** on the current Easy Mate world HUD (`MainUIButtons.cs` has no RealGaze buttons). Use **`Auto_Load_Person_Plugins`** / merge patterns if you need scene-wide RealGaze; to restore HUD buttons, add merge helpers to `MainUIButtons` (paths under `Custom/Scripts/RealGaze`: **Looker** / **Target** `.cs` files).

### 9. World tilt using a button + right analog up/down

Feasibility:

- moderate

Relevant engine findings:

- built-in free navigation already maps thumbsticks to:
  - side movement
  - forward movement
  - yaw rotation
  - height adjustment
- `GetFreeNavigateVector(...)` returns:
  - `x` side
  - `y` forward
  - `z` yaw
  - `w` up/down

Recommended future direction:

- while a custom modifier button is held:
  - intercept right-stick up/down ourselves
  - rotate around the camera pivot instead of treating it as plain height input
- likely operate on `navigationRig` around `lookCamera.transform.position`

Open design question:

- whether the tilt should rotate around the player's current eye point, a scene anchor, or a custom pivot a little in front of the user

## Performance and Optimization Guidelines

These are the main performance rules to keep in mind when writing or modifying VaM scripts in this project.

### General approach

- Prefer event-driven or button-driven work over constant polling.
- For scene-wide features, prefer one manager/session plugin over duplicating heavy logic on every Person atom.
- Use dirty flags and state transitions so work only happens when something changed.
- When a feature only matters while active, keep its expensive logic disabled the rest of the time.

### Avoid heavy scene scans in hot paths

Do **not** do these every frame unless there is no better option:

- `SuperController.singleton.GetAtoms()`
- `GetAtomUIDs()`
- `GetStorableIDs()`
- `GetStorableByID(...)`
- `GetComponent(...)`
- `GetComponentInChildren(...)`
- `Resources.FindObjectsOfTypeAll(...)`
- repeated LINQ over all Person atoms

Better pattern:

- scan once on scene load
- refresh caches when `onAtomUIDsChangedHandlers` fires
- do full scans on explicit button press if the feature is infrequent

### Cache references aggressively

Cache once and reuse when practical:

- `SuperController.singleton`
- `navigationRig`
- `lookCamera`
- `MonitorCenterCamera`
- `centerCameraTarget`
- `DAZCharacterSelector`
- `FreeControllerV3`
- `MVRPluginManager`
- `DAZMorph`
- `DAZClothingItem[]`

Especially avoid repeatedly calling `GetMorphByDisplayName(...)` or `GetStorableByID("geometry")` inside `Update()`.

### Avoid per-frame allocations

Things to avoid in `Update()`, render hooks, or other hot paths:

- `new List<>`, `new Dictionary<>`, `new StringBuilder()`
- repeated `ToList()`
- repeated `Where(...).ToList().ForEach(...)`
- string concatenation for logs/debug text
- rebuilding JSON objects unless actually applying plugin changes

Prefer:

- plain `for` loops over cached arrays/lists
- reusable collections that get `.Clear()`ed
- cached string values for button labels when state changes

### Be careful with LINQ

LINQ is fine for setup code and infrequent actions, but avoid it in hot loops.

Examples to avoid in per-frame code:

- `.Where(...)`
- `.Select(...)`
- `.First(...)`
- `.FirstOrDefault(...)`
- `.Distinct().ToList()`

These are all over the existing codebase and are acceptable in setup/button code, but not ideal for frequently running logic.

### Do not reload plugins casually

`MVRPluginManager.LateRestoreFromJSON(...)` is expensive because it rebuilds the plugin list.

Do **not**:

- call it every frame
- call it repeatedly during scene load
- use it as a polling mechanism
- use it when only checking whether a plugin exists

Use it only when:

- a scene just stabilized after loading
- the user pressed a button
- plugin state truly needs to change

### Do not create/destroy UI repeatedly

World-space HUD canvases are relatively expensive.

Do **not**:

- destroy and recreate canvases every frame
- rebuild all buttons just to change one label
- instantiate new UI objects for small state updates

Prefer:

- create once in `Start()`
- toggle `gameObject.SetActive(...)`
- update button text only when state changes

### Be careful with cameras and render hooks

`Camera.onPreRender` and `Camera.onPostRender` are powerful but easy to misuse.

Rules:

- register once
- unregister in `OnDestroy()` / `OnDisable()`
- keep the per-call work minimal
- avoid scene-wide scans inside render hooks
- avoid allocating temporary collections inside render hooks

For future head-hide logic:

- do not rebuild face/hair handlers every frame
- cache handlers per person
- only update handlers when the active target changes, hair changes, or character skin changes

### Head-hide / proximity features need throttling

For "hide head when camera enters any person's head":

- do not test every material on every person every frame
- do not rescan all persons in every camera render callback

Better approach:

- cache candidate people/heads
- run proximity checks at a lower cadence, such as every few frames or every 0.05-0.1 seconds
- use squared distance checks where possible
- only activate expensive material changes for the one or two relevant nearby people

### Morph access should be cached

Morph lookups by display name are convenient but not cheap.

Do **not** repeatedly call:

- `morphsControlUI.GetMorphByDisplayName(...)`

inside animation loops unless necessary.

Prefer:

- resolve morph references once when the atom/character is ready
- refresh only if the character/look changed
- null-check once and store the result

### Clothing operations should be bursty, not constant

Clothing operations are a good fit for button presses or scene transitions.

Do **not**:

- rebuild clothing classification every frame
- convert `clothingItems` to new lists unnecessarily
- repeatedly toggle clothing items just to keep state in sync

Prefer:

- cache per-person clothing metadata if a feature will reuse it often
- run clothing classification only on demand
- use direct array iteration when possible

### File/path checks are not hot-path work

Path normalization and existence checks can be surprisingly expensive.

Do **not**:

- call `GetFilesAtPath(...)`
- scan folders
- normalize plugin paths

inside `Update()` or other frequent loops.

Keep file/path logic limited to:

- scene-load reconciliation
- button presses
- plugin injection time

### Logging can be expensive

Do **not** spam:

- `SuperController.LogMessage(...)`
- `SuperController.LogError(...)`

inside hot loops.

Prefer:

- guarded debug flags
- one-time warnings
- transition-based logging

Exceptions are also expensive, so do not rely on try/catch as normal control flow in per-frame code.

### Avoid repeated rig thrashing

Frequent writes to:

- `navigationRig.position`
- `navigationRig.rotation`
- `playerHeightAdjust`
- `worldScale`

can cause visible instability and unnecessary work.

Do **not** reapply the same transform values every frame unless the feature truly requires continuous motion.

Prefer:

- one-time teleports for snap features
- smooth updates only while a mode is actively engaged
- transition checks so repeated identical assignments are skipped

### Debounce scene-load reactions

VaM scene loads often settle over multiple frames.

Do **not** react immediately to the first sign that `isLoading` became false and assume all atoms/plugins are ready.

Prefer:

- the existing Easy Mate / AutoMate pattern
- a short delay after loading finishes
- cache refresh only once the scene is stable

### Use atom-change events carefully

`onAtomUIDsChangedHandlers` is useful, but it may fire multiple times during load.

Good pattern:

- set a dirty flag when atom lists change
- process the expensive refresh later in `Update()` once loading is over

### Prefer targeted loops over nested generic scans

Avoid patterns like:

- every frame: all atoms -> all storables -> all components -> all materials

These explode quickly in busy VaM scenes.

Instead:

- target only Person atoms
- target only atoms relevant to the current feature
- short-circuit early when no work is needed

### Good default rule of thumb

If a piece of code touches the whole scene, the whole plugin list, all clothing items, all morphs, or all materials, it probably should **not** run every frame.

## Known Unknowns / Risks

### Built-in monitor mode is fundamentally coupled

- built-in monitor free move changes `navigationRig`
- scene load head possession also changes `navigationRig`
- `ImprovedPoV` treats `MonitorRig` as a POV camera

So the independent desktop camera feature is the hardest request in the list.

### Easy Mate / AutoMate / ResetVROrientation already coordinate each other

- `ResetVROrientation` calls `Show UI` / `Hide UI` actions on Easy Mate, AutoMate, and Possess Sex
- `MainUIButtons.ShowUI` toggles **only** Easy Mate’s main HUD buttons; **`VaMLogClipboardHud`** is a separate session plugin and is **not** controlled by those actions

## Best Reusable Code References

When implementing later, revisit these first:

- `Custom/Scripts/Easy Mate/src/MainUIButtons.cs`
  - world-space HUD on `mainHUD` (columns 1–3; see grid in **Easy Mate** section)
  - merge-add / remove plugins; **Remove All Clothes** / **Remove underwear**; **E‑Motion M/F**; Spankings toggles; VR palm HUD and female runtime wiring (`EasyMateVrEulerPossessHandHud`, **`EasyMateFemalePassengerRuntime`**)
- `Custom/Scripts/Easy Mate/src/VaMLogClipboardHud.cs`
  - separate `.cslist`; **Copy Errors** / **Copy Console** / **Clear logs** aligned to HUD column 0

- `Custom/Scripts/AutoMate/SESSION_PLUGINS/src/Auto_Load_Person_Plugins.cs`
  - safe plugin-list merging
  - finding plugins by storable ID
  - person scanning
  - adding/removing male atom

- `Custom/Scripts/ImprovedPoV.cs`
  - face/hair hide
  - per-camera render hooks
  - camera offset handling

- `Custom/Scripts/Possess Sex/PossessSex.cs`
  - `FreeControllerV3` link states
  - possession/link flags
  - rigidbody/controller targeting

- `Reference/Assembly-CSharp-decompiled/SuperController.cs`
  - `AlignRigAndController(...)`
  - `HeadPossess(...)`
  - `ProcessControllerNavigation(...)`
  - `ProcessMouseControl()`
  - `ProcessKeyboardFreeNavigation()`
  - `SetSceneLoadPosition()`
  - `MoveToSceneLoadPosition()`

- `Reference/Assembly-CSharp-decompiled/DAZCharacterSelector.cs`
  - `EnableUndressAllClothingItems()`
  - `RemoveAllClothing()`
  - `SetActiveClothingItem(...)`

- `Reference/Assembly-CSharp-decompiled/MVRPluginManager.cs`
  - `GetJSON(...)`
  - `LateRestoreFromJSON(...)`

## Resolved Decisions

1. `Passenger` source of truth is `Custom/Scripts/Passenger.cs`, and the duplicate `Custom/Scripts/Passenger/Passenger.cs` was deleted.
2. Future keyboard shortcuts should avoid VaM's bare-key defaults and avoid numpad-only bindings. The default keybind family should be modifier-heavy, preferably `Ctrl+Shift+1` through `Ctrl+Shift+9`, unless a specific feature needs a better fit.
3. `RealGaze` scripts live under `Custom/Scripts/RealGaze`.
4. Easy Mate’s **primary** actions live on **`MainUIButtons`**; **log copy/clear** uses a **second** session plugin (**`VaMLogClipboardHud`**) so those controls survive **`EasyMate.cslist`** failures. Keep **`Show UI` / `Hide UI`** affecting **`MainUIButtons`** only unless you wire **`VaMLogClipboardHud`** separately.
5. "First female" and "second female" should be assigned by a deterministic ordering. The preferred default is sorting female atoms by `uid` so slot 1 and slot 2 stay stable as long as the scene's female atom set stays the same.
6. **RealGaze** is not on the current Easy Mate world HUD; scripts live under `Custom/Scripts/RealGaze` if you add merge buttons or use **`Auto_Load_Person_Plugins`**. Historical note: two-HUD actions (**Looker** / **Target**) were once described here; reintroduce via `MainUIButtons` if needed.
7. E-Motion on the Easy Mate HUD: a **column** of **E‑Motion Lite**, **E‑Motion Original**, **E‑Motion Final**, and **Remove E‑Motion** (each install uses **`TryReplaceEmotionFamilyWithExactPath`**). **Spankings Male** is a **`+`/`-`** toggle on **`MainUIButtons`**. Log copy/clear is **`VaMLogClipboardHud`**. **E-MotionLite** is also merged automatically on matching loads via **`EasyMateEmotionPathKeywords`** / **`emotion_path_keywords.txt`**. **E-Motion Final** is merged onto **female** Persons once after a **long non-loop** mocap ends (**`EasyMateMotionAnimationEmotionEnd`**).

## Short Conclusions

- The project already has solid patterns for HUD buttons, scene scanning, clothing control, plugin injection, and person-relative camera math.
- The hardest future task is the independent desktop monitor camera because VaM's built-in monitor mode shares the same `navigationRig` as VR.
- The easiest remaining HUD-style tasks are: extra gender/atom picker buttons if needed, optional head-hide when camera inside head, more plugin toggles.
- The best way to do "snap to female head once" is probably **not** built-in possession, but a one-time rig move using the same math that built-in possession uses.

