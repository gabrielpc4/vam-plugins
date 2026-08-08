# VaMScripts palm HUD and passenger handoff notes

Hand-off doc for another LLM or developer. Paths live under
`Custom/Scripts/features/`, namespace **`geesp0t`**, VaM plugin C# **6.0**.

---

## 1. Runtime entry

- **`Hud.cs`** calls **`VrEulerPossessHandHud.Tick()`** from **`Update`**.
- **`Hud.LateUpdate`** runs **`MonitorModeLaserRestore.Tick`** (beams +
  **`PassengerLaserPossess`** trigger).
- Desktop hotkeys: **`Hotkeys.cs`**.

---

## 2. Palm HUD (right hand only)

**File:** `palm-hud/VrEulerPossessHandHud.cs`

- **Visibility:** **`VrEulerPossessPoseCheck.RightHandOnlyMatchTriggerWindow`**
  (back-of-hand / watch euler toward HMD). Leaving the pose hides the HUD.
- **Attach:** Parent **`SuperController.rightHand`**, **`LocalHandHudOffset`**
  (positive **X** = dorsum side).
- **Rows:**
  - **Próxima cena:** upper row when the scene resolves a matching next
    **`UIButton`** (see **`NextSceneUiButton`**). Poll **face B / menu**.
  - **Despossuir:** lower row **only while already possessed**
    (**`PassengerRuntime.IsPassengerModeActiveOrPending()`** OR any Person
    head/hand possessed). Poll **face A / Select**.
- **Does not start passenger:** there is **no Possuir**, no gender submenu.
  Passenger **start** uses the **right** laser + **A** only (**§7**).

---

## 3. HMD-relative euler (pose math)

**File:** `palm-hud/VrEulerPossessPoseCheck.cs`

| Axis | Palm HUD window (`RightPalmHudMatches`) |
|------|----------------------------------------|
| X | `> 300°` |
| Z | between `110°` and `150°` |

HMD basis: **`lookCamera`** else **`centerCameraTarget`**
(**`ResolveHmdTransform`**).

---

## 4. Controller input on the palm canvas

**File:** `palm-hud/VrInput.cs`

- **Face A:** **`PollRightFaceADown`** / **`PollPalmHudPossessRowFaceADown`** —
  delegates to **`SuperController.GetRightSelect`** (OVR and OpenVR); VaM
  suppresses select while interacting with VR UI (`rightGUIInteract`).
  Despossuir when that row exists.
- **Face B:** **`PollPalmHudProximaCenaFaceBDown`** — next scene.
- Lasers often miss the **`WorldSpace`** canvas; code **polls buttons**, not only
  ray hits.

---

## 5. Passenger start: lasers + A

**Files:**

- **`passenger-possession/PassengerLaserPossess.cs`** — raycast resolver +
  **`TryTriggerFromRightBeamPersonHit`** (**`PollRightFaceADown`** →
  **`GetRightSelect`** + cooldown +
  **`PassengerRuntime.RequestPassengerForSpecificPerson`**). Skips **Edit**
  mode and when the palm HUD is **visible** (right beam aim is suppressed
  anyway while palm is up). **`GetRightSelect`** also skips frames where VaM
  consumes right select for VR UI (e.g. laser on a UI button).
- **`scene-camera/MonitorModeLaserRestore.cs`** — draws forward cylinders when
  **UI aim** capacitive/state is held (Quest **X**/ **A touch**, OpenVR
  **GetLeft/RightUIPointerShow**); passes hit **`Person`** atoms into
  **`PassengerLaserPossess`**.

Only the **right** beam can arm possession (Person along that ray).

---

## 6. Clearing possession

**File:** **`Hud.RequestClearAllPossession`**

Clears grips, **`PassengerRuntime`**, VaM **`ClearPossess`**, head-hide restore,
unlink stray HMD links, **`SelectModeOff`**. Used from scene-load cleanup,
etc. **Despossuir** on the palm uses **`PassengerRuntime.RequestStopForPalmHud`**,
not necessarily this full-path unless you wire it elsewhere.

---

## 7. File index

| Topic | Path |
|-------|------|
| Palm HUD UI + tick | `palm-hud/VrEulerPossessHandHud.cs` |
| Euler windows | `palm-hud/VrEulerPossessPoseCheck.cs` |
| VR input polls | `palm-hud/VrInput.cs` |
| Laser + A passenger | `passenger-possession/PassengerLaserPossess.cs` |
| Aim beams glue | `scene-camera/MonitorModeLaserRestore.cs` |
| Passenger runtime | `passenger-possession/PassengerRuntime.cs` |
| Next-scene UIButton helpers | `ui-hud/NextSceneUiButton.cs` |
| Desktop hotkeys | `ui-hud/Hotkeys.cs` |
| Session **`Hud`** | `ui-hud/Hud.cs` |
| Compile list | `ui-hud/Hud.cslist` |

---

## 8. Design notes for changes

- Prefer **SuperController** public API over reflection (project rule).
- Keep **C# 6**; see **`Reference/VaM-Scripting-Notes.md`** and **`.cursor`**
  rules.
- Passenger **start**: change **`PassengerLaserPossess`** and/or beam logic in
  **`MonitorModeLaserRestore`**. Palm HUD is **exit + next scene** only.
