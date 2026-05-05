# Easy Mate: “next scene” UIButton + VR palm HUD — handoff for crash diagnosis

This note summarizes work done in chat (another agent) on this repo’s **Easy Mate** plugin. Use it to judge whether a **VaM crash soon after scene load** could be related, and what to toggle or read next.

---

## Goal (feature)

1. **Resolve** a VaM **`UIButton`** atom whose label looks like a “next scene” control (English `next`, Portuguese `proxima` / `próxima`; also reads the **`Text`** storable param `"text"` via `GetStringJSONParam` because `GetStorableByID("Text")` is `TextStorable`, not `JSONStorableString`).
2. **Pulse** its `UIButtonTrigger` the same way VaM does on click: `trigger.active = true` for one frame, then `false` (`FireUIButtonTriggerActivePulseCo`).
3. **VR palm HUD** (“Próxima cena”, **B**): call that pulse when the pose-based hand HUD is visible and B is pressed — including **while possessed** (earlier bug: B only unpossessed).

**Optional / later behavior:** show the palm **Próxima cena** row only when `HasNextSceneUiButtonInScene()` is true (atom scan). **This scan is temporarily disabled** for diagnosis — see below.

**Scene example used in chat:** `Saves/scene/sapuzex/jessika's bedroom intro.json` — UIButton id `UIButton` with `text` `"next scene >>"` and `Trigger` → `CoreControl` / `SceneLoader` / `MergeLoadScene`.

---

## Files changed (only these in this workstream)

### `Custom/Scripts/Easy Mate/src/MainUIButtons.cs`

- **`TryResolveNextSceneUIButtonTrigger(SuperController sc)`** (private): prefers atom UID `nxtUIButton` if valid; else scans **`sc.GetAtoms()`** for `type == "UIButton"`, `activeInHierarchy`, matches label on child **`UnityEngine.UI.Text`** or **`Text` storable** param `"text"`, returns first `UIButtonTrigger` on storable id **`Trigger`** with non-null `trigger`.
- **`NextSceneUIButtonLabelMatches`** (private): substring checks for `next`, `proxima`, `próxima` (\u00F3).
- **`RequestFireNextSceneUiButton()`**: existing public entry; resolves trigger and starts coroutine on plugin host.
- **`HasNextSceneUiButtonInScene()`** (public): `!sc.isLoading` guard, then same resolution as above; used for palm HUD visibility/gating when diagnostics flag is off.

**Compile fix:** Do **not** cast `GetStorableByID("Text")` to `JSONStorableString` — use **`GetStringJSONParam("text")`** on the `JSONStorable` (`TextStorable`).

### `Custom/Scripts/Easy Mate/src/EasyMateVrEulerPossessHandHud.cs`

- **Possessed branch:** **B** → `RequestFireNextSceneUiButton()`; **A** → unpossess / possess row action (no longer maps B to unpossess-only).
- **Conditional UI (when scan enabled):** `RefreshGenderVersusMainRows` shows **Próxima cena** only when cached `HasNextSceneUiButtonInScene()` is true (and not on gender-choose panel). **Stretching** the lower row when hiding Próxima was **reverted** — empty top half preserved.
- **Presence recheck interval:** `NextSceneButtonPresenceRecheckSeconds = 1f` (was 0.35s briefly).
- **Diagnostics — scan off:**  
  `TempDisableNextSceneUiButtonPresenceScan = true`  
  When **true**: never calls `HasNextSceneUiButtonInScene()`; sets `_cachedSceneHasNextSceneUIButton = true` whenever the palm HUD tick runs (so UI/shortcuts match “always show Próxima” behavior). **Firing** still runs full resolution inside `RequestFireNextSceneUiButton()` on B press.

**When does the scan run (if enabled)?** Only inside `Tick()` after **VR is on**, `rightHand` exists, passenger block clear, and **right-hand euler window** matches — not on every load and not on desktop if those fail early.

---

## Git commits (this thread, `main`)

Approximate messages (newest diagnostic commit last):

1. `fix(easy-mate): palm HUD B fires next-scene UIButton while possessed`
2. `fix(easy-mate): read UIButton label via TextStorable string param, not cast`
3. `feat(easy-mate): show palm Próxima cena only when next UIButton exists`
4. `revert(easy-mate): keep palm HUD layout when Próxima cena hidden`
5. `chore(easy-mate): palm HUD next-button presence recheck every 1s`
6. `chore(easy-mate): temp disable palm HUD next-scene presence scan (diagnostics)` — **current “is it the scan?” test**

---

## Crash diagnosis checklist

| Observation | Implication |
|-------------|-------------|
| Crash with **`TempDisableNextSceneUiButtonPresenceScan == true`** | Unlikely to be the **periodic `GetAtoms` + UIButton scan**; consider **in-scene load / MergeLoadScene**, **`RequestFireNextSceneUiButton`** path on input, other plugins, GPU/driver, `logs/patch.log` / Player.log. |
| Crash only with flag **false** | Suspect **`HasNextSceneUiButtonInScene` / `TryResolveNextSceneUIButtonTrigger`** during palm HUD ticks (VR + pose + hand HUD active). |
| No VR / palm pose never active | Palm HUD code path (including scan) may **never** run; crash likely **elsewhere**. |

**Re-enable presence detection:** set `TempDisableNextSceneUiButtonPresenceScan` to **`false`** in `EasyMateVrEulerPossessHandHud.cs`.

**Full disable of “smart” next resolution** (if you need a stronger A/B test): would require also short-circuiting or gating `RequestFireNextSceneUiButton` / `TryResolveNextSceneUIButtonTrigger` — not done in current diagnostic flag.

---

## VaM plugin constraints (this repo)

Scripts under `Custom/Scripts` target **C# 6** for VaM compilation (no inline `out var`, etc.). See `.cursor/rules/vam-csharp-6-plugins.mdc` and `Reference/VaM-Scripting-Notes.md`.

---

## What was *not* edited

- Saved scenes under `Saves/` (e.g. `jessika's bedroom intro.json`) were only **read** as reference; no JSON changes from this workstream for the feature itself.
