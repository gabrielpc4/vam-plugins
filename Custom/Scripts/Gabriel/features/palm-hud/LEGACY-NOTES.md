# Gabriel palm HUD and passenger handoff notes

Hand-off doc for another LLM or developer. All paths live under
`Custom/Scripts/Gabriel/features/`, namespace **`geesp0t`**, VaM plugin
C# **6.0**.

---

## 1. Runtime entry

- **`GabrielHud.cs`** (session plugin) calls **`VrEulerPossessHandHud.Tick()`**
  from its main **`Update`** path (same block as other VR helpers).
- **`GabrielHudButtons.ProcessHotkeysUpdate()`** only handles the remaining
  desktop hotkeys; there is no separate VR gesture runtime anymore.

---

## 2. Palm HUD (right hand only)

**File:** `src/VrEulerPossessHandHud.cs`

- **Visibility:** The HUD is shown only while
  **`VrEulerPossessPoseCheck.RightHandOnlyMatchTriggerWindow(sc)`**
  is true. There is **no** bypass when the gender submenu is active: if the
  right hand leaves the euler window, **`SetVisible(false)`** runs and
  **`_genderChooseStepActive` is cleared** (no separate “Back” control).
- **Attach point:** Canvas parented to **`SuperController.rightHand`** with a
  fixed **local** offset (`LocalHandHudOffset`, **positive X** = back of hand /
  watch side; palm-era HUD used **negative X** with same Y,Z), billboard-style toward the HMD.
- **World-space `Canvas` + `GraphicRaycaster`:** VaM’s VR UI lasers often
  **do not** click these buttons; gender choices are driven by **polled**
  controller input (see §5).
- **Rows:**
  - **Main:** `Possuir` / `Despossuir` (one row) and **`Próxima cena`**
    (fires next-scene UI button via `GabrielHudButtons.RequestFireNextSceneUiButton`).
  - **Gender:** **`Mulher (B)`** (top row of the gender step). Shown after
    `Possuir` when at least one `Person` exists (see §4).
- **VaM system menu (`GetMenuShow`):** While **not** in the gender step and
  **not** possessed, if **`VrPalmHudNeedsGenderChoiceStep()`** and user opens
  the menu, the HUD dismisses **`activeUI`**, sets **`RequestGenderChooseStep()`**,
  and refreshes to the gender row (must still hold the hand HUD pose to see it).

**After choosing Mulher:** `DismissVaMOverlayUiIfAny()` sets
`SuperController.activeUI = None` to avoid the OpenVR menu binding leaving
the overlay up when Mulher used `GetMenuShow`.

---

## 3. HMD-relative euler (pose math)

**File:** `src/VrEulerPossessPoseCheck.cs`

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

`RightHandOnlyMatchTriggerWindow` = right hand only +
`RightPalmHudMatches`.

---

## 4. Gender lists, cycling, palm Mulher

**File:** `src/GabrielHudButtons.cs`

- **Caches:** `_cachedFemalePersonsByUid`, `_cachedMalePersonsByUid` — all
  `Person` atoms, **female** = `DAZCharacter` exists and **`!d.isMale`**.
  Sorted by **`Atom.uid`** (`StringComparer.Ordinal`). Invalidated on
  `SuperController.onAtomUIDsChangedHandlers`.
- **`VrPalmHudNeedsGenderChoiceStep()`:** `true` if **at least one** `Person`
  exists (`nF + nM >= 1`). So **every** scene with a figure uses the palm
  gender step (including a solo Person).
- **Indices:** `_vrPalmHudFemaleCycleIndex`, `_vrPalmHudMaleCycleIndex`.
  Target atom: `list[index % list.Count]`.
- **`RequestPossessVrPalmHudByGender(true)` — Mulher:**  
  **`StartAutoPossessRoutine(target, VrEulerPossessLabel)`**  
  - Constant **`GabrielHudButtons.VrEulerPossessLabel`** = `"VR euler"`.  
  - Triggers VR-euler-specific behavior inside the possess routine (e.g.
    `RemoveSpankingsFromAllPersonsStatic`, notify grip visibility suppress,
    different Spankings merge rules vs HUD “F” possess).

**`RequestPossessVrPalmHudAutoWithoutGenderMenu`:** If zero persons → log;
else **`VrEulerPossessHandHud.RequestGenderChooseStep()`** only (no
direct possess).

**Menu coroutine** (`RequestVrPalmHudMenuButtonPossessAfterDismissMenu`):
after 100 ms, `activeUI = None`; if a Person exists →
`RequestGenderChooseStep()`, else log no person.

---

## 5. Controller input for Mulher on the gender row (not UI raycasts)

**File:** `src/VrInput.cs`

| Action | OVR (`sc.isOVR`) | OpenVR (`sc.isOpenVR`) |
|--------|------------------|-------------------------|
| **Mulher** | `OVRInput.GetDown(Button.Two, RTouch)` only | `SuperController.GetMenuShow()` (SteamVR **Menu**, may be **Any** hand per VaM) |

If neither OVR nor OpenVR but XR seems on, code **falls back** to OVR
`RTouch` reads (e.g. some Link setups).

---

## 6. Clearing possession and advancing cycle indices

**File:** `src/GabrielHudButtons.cs` — **`ClearAllPossession`**, **`RequestClearAllPossession`**

On clear:

1. Record **`hadPossessed`** =
   `GripHandVisibility.IsAnyPersonHeadOrHandPossessed()`.
2. **`StopAutoPossessRoutine()`**
3. **`HeadProximityHide.RestoreTransientHeadHideState()`** (restores materials / clears hide-target state; camera hooks stay if VR proximity hide is enabled)
4. **`sc.ClearPossess()`**
5. **`UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads`**
6. **`sc.SelectModeOff()`** (try/catch + log)
7. **`HeadProximityHide.HidePossessorAlignmentPreviewMeshes()`**
8. If **`advanceVrPalmHudGenderCycle && hadPossessed`** → increment **both**
   `_vrPalmHudFemaleCycleIndex` and `_vrPalmHudMaleCycleIndex`.

**Callers (typical):**

| Source | `advanceVrPalmHudGenderCycle` |
|--------|--------------------------------|
| Palm **Despossuir** | `true` |
| **O** hotkey | `true` |
| Scene load cleanup in **`GabrielHud.cs`** | **`false`** |

---

## 7. Removed gesture runtime

The old over-head unpossess gesture and dual-hand auto-possess detection were
removed. Current VR behavior depends only on the right-hand back-of-hand pose
window used by `VrEulerPossessHandHud`.

---

## 8. Other related pieces

- **Desktop / HUD:** Remaining keyboard handling lives in
  `GabrielHudButtonsHotkeys.cs`. Palm **Mulher** and **Homem** use the shared
  passenger runtime path (`PassengerRuntime`).

---

## 9. File index

| Topic | Path |
|-------|------|
| Palm HUD UI + tick | `src/VrEulerPossessHandHud.cs` |
| Euler windows | `src/VrEulerPossessPoseCheck.cs` |
| Palm HUD actions / clear | `src/GabrielHudButtons.cs` |
| Desktop hotkeys | `src/GabrielHudButtonsHotkeys.cs` |
| VR face/menu/select polling | `src/VrInput.cs` |
| Calls `HandHud.Tick` | `src/GabrielHud.cs` |
| Plugin file list | `GabrielHud.cslist` |

---

## 10. Design notes for changes

- Prefer **SuperController** public API over reflection (project rule).
- Keep **C# 6** (no inline `out var`, etc.); see
  `Reference/VaM-Scripting-Notes.md` and `.cursor` rules.
- Palm **Mulher** / **Homem** now route directly into
  `PassengerRuntime.RequestStartForFemale` /
  `PassengerRuntime.RequestStartForMale`.
