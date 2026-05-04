# Easy Mate: VR possession, palm HUD, snapping, and related triggers

Hand-off doc for another LLM or developer. All paths live under
`Custom/Scripts/Easy Mate/`, namespace **`geesp0t`**, VaM plugin C# **6.0**.

---

## 1. Runtime entry

- **`EasyMate.cs`** (session plugin) calls **`EasyMateVrEulerPossessHandHud.Tick()`**
  from its main **`Update`** path (same block as other VR helpers).
- **`MainUIButtons.ProcessHotkeysUpdate()`** calls
  **`EasyMateVrGestureRuntime.ProcessUpdate(_vrGestureBindings)`** after
  keyboard handling.

---

## 2. Palm HUD (right hand only)

**File:** `src/EasyMateVrEulerPossessHandHud.cs`

- **Visibility:** The HUD is shown only while
  **`EasyMateVrEulerPossessPoseCheck.RightHandOnlyMatchTriggerWindow(sc)`**
  is true. There is **no** bypass when the gender submenu is active: if the
  right hand leaves the euler window, **`SetVisible(false)`** runs and
  **`_genderChooseStepActive` is cleared** (no separate “Back” control).
- **Attach point:** Canvas parented to **`SuperController.rightHand`** with a
  fixed **local** offset (`LocalPalmOffset`), billboard-style toward the HMD.
- **World-space `Canvas` + `GraphicRaycaster`:** VaM’s VR UI lasers often
  **do not** click these buttons; gender choices are driven by **polled**
  controller input (see §5).
- **Rows:**
  - **Main:** `Possuir` / `Despossuir` (one row) and **`Próxima cena`**
    (fires next-scene UI button via `MainUIButtons.RequestFireNextSceneUiButton`).
  - **Gender:** **`Mulher (B)`** (top) and **`Homem (A)`** (bottom). Shown
    after `Possuir` when at least one `Person` exists (see §4).
- **VaM system menu (`GetMenuShow`):** While **not** in the gender step and
  **not** possessed, if **`VrPalmHudNeedsGenderChoiceStep()`** and user opens
  the menu, the HUD dismisses **`activeUI`**, sets **`RequestGenderChooseStep()`**,
  and refreshes to the gender row (must still hold palm pose to see it).

**After choosing Mulher or Homem:** `DismissVaMOverlayUiIfAny()` sets
`SuperController.activeUI = None` to avoid the OpenVR menu binding leaving
the overlay up when “Mulher” used `GetMenuShow`.

---

## 3. HMD-relative euler (pose math)

**File:** `src/EasyMateVrEulerPossessPoseCheck.cs`

- Hand rotation relative to HMD:
  `Quaternion.Inverse(hmd.rotation) * hand.rotation` → euler per axis in
  **[0, 360)**.
- **HMD transform:** `lookCamera` if set, else `centerCameraTarget`
  (`ResolveHmdTransform`).

**Right-hand window (palm HUD)** — `RightPalmHudMatches(e)` (`RightHandOnlyMatchTriggerWindow`):

| Axis | Condition |
|------|-----------|
| X | `> 300°` |
| Z | strictly between `110°` and `150°` (back of hand toward HMD; palm-facing window +180° on Z) |

**Right-hand window (dual-hand gesture, when enabled)** — `RightMatches(e)`:

| Axis | Condition |
|------|-----------|
| X | `> 300°` |
| Z | strictly between `290°` and `330°` |

**Left-hand window (dual-hand gesture module, when enabled)** —
`LeftMatches(e)`:

| Axis | Condition |
|------|-----------|
| X | `> 300°` |
| Z | strictly between `30°` and `90°` |

`RightHandOnlyMatchTriggerWindow` = right hand only + `RightPalmHudMatches`.  
`BothHandsMatchTriggerWindow` = left `LeftMatches` **and** right
`RightMatches`.

---

## 4. Gender lists, cycling, Mulher vs Homem

**File:** `src/MainUIButtons.cs`

- **Caches:** `_cachedFemalePersonsByUid`, `_cachedMalePersonsByUid` — all
  `Person` atoms, **female** = `DAZCharacter` exists and **`!d.isMale`**.
  Sorted by **`Atom.uid`** (`StringComparer.Ordinal`). Invalidated on
  `SuperController.onAtomUIDsChangedHandlers`.
- **`VrPalmHudNeedsGenderChoiceStep()`:** `true` if **at least one** `Person`
  exists (`nF + nM >= 1`). So **every** scene with a figure uses the
  **Mulher / Homem** step (including a solo Person).
- **Indices:** `_vrPalmHudFemaleCycleIndex`, `_vrPalmHudMaleCycleIndex`.
  Target atom: `list[index % list.Count]`.
- **`RequestPossessVrPalmHudByGender(true)` — Mulher:**  
  **`StartAutoPossessRoutine(target, VrEulerPossessLabel)`**  
  - Constant **`MainUIButtons.VrEulerPossessLabel`** = `"VR euler"`.  
  - Triggers VR-euler-specific behavior inside the possess routine (e.g.
    `RemoveSpankingsFromAllPersonsStatic`, notify grip visibility suppress,
    different Spankings merge rules vs HUD “F” possess).
- **`RequestPossessVrPalmHudByGender(false)` — Homem:**  
  **`SnapRigToMalePersonHeadWithPostSteps(target)`** — same pipeline as HUD
  **“Passenger Male”**: rig snap to male head (possession-match head snap
  point), **`EnsureSnapMEndsWithoutPossessionOrTargetHud`**
  (`ClearPossess`, unlink stray HMD-linked FCs, `SelectModeOff`), hide
  possessor alignment preview meshes. **Not** full possess+align+select.

**`RequestPossessVrPalmHudAutoWithoutGenderMenu`:** If zero persons → log;
else **`EasyMateVrEulerPossessHandHud.RequestGenderChooseStep()`** only (no
direct possess).

**Menu coroutine** (`RequestVrPalmHudMenuButtonPossessAfterDismissMenu`):
after 100 ms, `activeUI = None`; if a Person exists →
`RequestGenderChooseStep()`, else log no person.

---

## 5. Controller input for gender row (not UI raycasts)

**File:** `src/EasyMateVrInput.cs`

| Action | OVR (`sc.isOVR`) | OpenVR (`sc.isOpenVR`) |
|--------|------------------|-------------------------|
| **Mulher** | `OVRInput.GetDown(Button.Two, RTouch)` only | `SuperController.GetMenuShow()` (SteamVR **Menu**, may be **Any** hand per VaM) |
| **Homem** | `OVRInput.GetDown(Button.One, RTouch)` only | `SuperController.GetRightSelect()` (right-hand **Select**) |

If neither OVR nor OpenVR but XR seems on, code **falls back** to OVR
`RTouch` reads (e.g. some Link setups).

**Conflict:** If both Mulher and Homem fire same frame, **Mulher** wins
(HandHud `Tick` order).

---

## 6. Clearing possession and advancing cycle indices

**File:** `src/MainUIButtons.cs` — **`ClearAllPossession`**, **`RequestClearAllPossession`**

On clear:

1. Record **`hadPossessed`** =
   `EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed()`.
2. **`StopAutoPossessRoutine()`**
3. **`EasyMateHeadSnapPovRuntime.EndSnapSession()`**
4. **`sc.ClearPossess()`**
5. **`UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads`**
6. **`sc.SelectModeOff()`** (try/catch + log)
7. **`EasyMateHeadSnapPovRuntime.HidePossessorAlignmentPreviewMeshes()`**
8. If **`advanceVrPalmHudGenderCycle && hadPossessed`** → increment **both**
   `_vrPalmHudFemaleCycleIndex` and `_vrPalmHudMaleCycleIndex`.

**Callers (typical):**

| Source | `advanceVrPalmHudGenderCycle` |
|--------|--------------------------------|
| Palm **Despossuir** | `true` |
| **O** hotkey | `true` |
| Over-head unpossess gesture (when enabled) | `true` |
| **`EasyMatePossessFootDistanceAutoRelease`** | `true` |
| Scene load cleanup in **`EasyMate.cs`** | **`false`** |

---

## 7. VR gestures (over-head unpossess, dual-hand possess)

**File:** `src/EasyMateVrGestureRuntime.cs`

Bindings are set in **`MainUIButtons.Init`**:

- **`TriggerVrOverHeadHandUnpossessAll`** →
  `RequestClearAllPossession(..., advanceVrPalmHudGenderCycle: true)`.
- **`TriggerPossessAlignSelectClosestFemaleByHead`** →
  **`PossessAlignSelectClosestFemaleByHeadToCamera()`** (closest female by
  head, **`VrEulerPossessLabel`**, not the palm Mulher/Homem menus).

**Master switch (currently off):**

```csharp
private const bool EnableOverHeadUnpossessAndDualHandPossessGestures = false;
```

When **`false`**, **`ProcessUpdate` returns immediately** — no over-head zone
evaluation, no dual-hand dwell. Saves CPU; palm HUD remains the only VR
auto path for those flows. Set to **`true`** to restore old behavior.

**When enabled (reference only):**

- **OverHeadRightHandGesture:** Right hand in a vertical “cap” above HMD
 along headset **up** (~1 Hz eval, 4 s cooldown, latched per visit).
- **DualHandHmdRelativeEulerPossessClosestFemale:** Both hands in euler
 windows ~**3 s** dwell, **10 s** cooldown, skipped while anyone possessed.

---

## 8. Other related pieces

- **`EasyMatePossessFootDistanceAutoRelease.cs`:** Optional auto-unpossess
  when look camera moves too far from possessed person’s feet; calls
  **`RequestClearAllPossession` with advance `true`**.
- **Desktop / HUD:** Possess Female / Male / **P** / **O** etc. still live in
  **`MainUIButtons`** (not duplicated here).
- **Passenger Female / Male** HUD buttons:** Snap rig to head; **Male** uses
  `SnapRigToMalePersonHeadWithPostSteps` (same family as palm **Homem**).

---

## 9. File index

| Topic | Path |
|-------|------|
| Palm HUD UI + tick | `src/EasyMateVrEulerPossessHandHud.cs` |
| Euler windows | `src/EasyMateVrEulerPossessPoseCheck.cs` |
| Possess / snap / palm API / clear | `src/MainUIButtons.cs` |
| VR face/menu/select polling | `src/EasyMateVrInput.cs` |
| Over-head + dual-hand modules | `src/EasyMateVrGestureRuntime.cs` |
| Foot-distance unpossess | `src/EasyMatePossessFootDistanceAutoRelease.cs` |
| Calls `HandHud.Tick` | `src/EasyMate.cs` |
| Plugin file list | `EasyMate.cslist` |

---

## 10. Design notes for changes

- Prefer **SuperController** public API over reflection (project rule).
- Keep **C# 6** (no inline `out var`, etc.); see
  `Reference/VaM-Scripting-Notes.md` and `.cursor` rules.
- Palm **Homem** is **Snap M**, not possess; palm **Mulher** is **VR euler**
  possess+align+select with label **`VrEulerPossessLabel`**.
