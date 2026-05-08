# VaM scene startup and “settle” (core engine + Gabriel `OnSceneStartup`)

This note ties together **what VaM does while a scene loads** (from decompiled `Assembly-CSharp`) and **what this workspace adds** in Gabriel's session stack so exposure, simulation, and audio stay coherent until assets are ready.

**Decompile location:** `Reference/Assembly-CSharp-decompiled` (read-only; do not edit).

**Live plugin:** `Custom/Scripts/Gabriel/session-stack/src/SceneSettle.cs`
plus the adjacent `PlaybackHold.cs` and `InitialExposureChange.cs` partials,
ticked from
`GabrielSessionStack.LateUpdate()` (see
`Custom/Scripts/Gabriel/session-stack/src/GabrielSessionStack.cs`).

---

## 1. Signals that mean “load / not ready yet”

### `SuperController.isLoading` (`_isLoading`)

While the main load coroutine runs, **`isLoading`** is true. Near the end of that pipeline VaM clears **`_isLoading`** *before* it tears down the full-screen loading UI.

Relevant excerpt from `SuperController` load flow (`Reference/Assembly-CSharp-decompiled/SuperController.cs`):

```3811:3834:Reference/Assembly-CSharp-decompiled/SuperController.cs
		_isLoading = false;
		SyncSortedAtomUIDs();
		// ...
		if (loadingUI != null)
		{
			for (int l = 0; l < 10; l++)
			{
				yield return null;
			}
			loadingUI.gameObject.SetActive(value: false);
			if (loadingUIAlt != null)
			{
				loadingUIAlt.gameObject.SetActive(value: false);
			}
			if (loadingGeometry != null)
			{
				loadingGeometry.gameObject.SetActive(value: false);
			}
		}
```

So **`isLoading == false` does not imply** the big **`loadingUI` / `loadingUIAlt` / `loadingGeometry`** overlay is gone yet—there can still be **10 frames** (plus earlier yields) where the scene is “loaded” in code terms but the progress UI is still active.

### Full-screen loading transforms

Public transforms on **`SuperController`** (same file) include **`loadingUI`**, **`loadingUIAlt`**, **`loadingGeometry`**. While those **`GameObject`s are active**, VaM is still presenting the load overlay.

### Corner **`loadingIcon`**

Separate async work (e.g. threaded images, URL audio) can register flags via **`SuperController.SetLoadingIconFlag`**. **`Update`** drives **`CheckLoadingIcon()`**, which shows or hides **`loadingIcon`** while those flags are pending.

Treat **`loadingIcon.gameObject.activeSelf`** as “still draining deferred media,” alongside **`isLoading`** and the full-screen loaders.

### Hold-load async (`HoldLoadComplete` / `CheckHoldLoad`)

The load coroutine can wait on **`AsyncFlag`** instances for certain asset paths (e.g. custom unity asset loaders). That time still overlaps **`isLoading`** from the player’s perspective; plugins rarely need to duplicate that logic if they already gate on **`isLoading`** and the loading UI/icon.

---

## 2. Why “settle” is wider than `isLoading` alone

For UX (dimmed exposure, no motion/audio ahead of textures), **waiting only until `isLoading` flips false is too early**:

1. **`_isLoading`** clears **before** **`loadingUI`** is disabled (see §1).
2. **Textures / URL audio** may still run through **`loadingIcon`** after **`isLoading`** is false.

Gabriel therefore defines **“scene still settling”** as the OR of:

- `SuperController.singleton.isLoading`
- `loadingUI` / `loadingUIAlt` / `loadingGeometry` **`activeSelf`**
- `loadingIcon` **`activeSelf`**

That matches “VaM still showing load feedback or holding deferred loads,” not only the internal `_isLoading` flag.

---

## 3. Freeze animation vs pause simulation vs UI toggle (`SetFreezeAnimation`)

### Computed **`freezeAnimation`** getter

```1650:1650:Reference/Assembly-CSharp-decompiled/SuperController.cs
	public bool freezeAnimation => _freezeAnimation || _isLoading || _pauseSimulation;
```

Many gameplay paths consult **`freezeAnimation`** (physics / input helpers, etc.). **`_isLoading`** and **`_pauseSimulation`** make this **true** without touching the user’s freeze toggle.

### What **`SetFreezeAnimation`** actually does

It sets **`_freezeAnimation`** and syncs **UI toggles**. **`SyncFreezeAnimation`** only drives **`Animator.enabled`** from **`_freezeAnimation`**, not from `_isLoading` or `_pauseSimulation`:

```5070:5096:Reference/Assembly-CSharp-decompiled/SuperController.cs
	protected void SyncFreezeAnimation()
	{
		if (allAnimators == null)
		{
			return;
		}
		foreach (Animator allAnimator in allAnimators)
		{
			allAnimator.enabled = !_freezeAnimation;
		}
	}

	public void SetFreezeAnimation(bool freeze)
	{
		if (_freezeAnimation != freeze)
		{
			_freezeAnimation = freeze;
			SyncFreezeAnimation();
			if (freezeAnimationToggle != null)
			{
				freezeAnimationToggle.isOn = _freezeAnimation;
			}
			if (freezeAnimationToggleAlt != null)
			{
				freezeAnimationToggleAlt.isOn = _freezeAnimation;
			}
		}
	}
```

So:

- **`PauseSimulation`** raises **`_pauseSimulation`**, which makes **`freezeAnimation`** true for **code that reads the property**, but it does **not** flip **`Animator.enabled`** the same way the **Freeze Animation** menu does.
- **`SetFreezeAnimation`** would affect **animators and UI**, but it’s the **user preference** channel; temporarily forcing it from a plugin requires **careful snapshot/restore** of **`_freezeAnimation`** (not directly exposed as a clean “user value only” API while **`isLoading`** is true).

Gabriel **`OnSceneStartup`** uses **`PauseSimulation(AsyncFlag)`** so the hold is **stacked like other engine pauses** and does **not** rewrite the Freeze Animation toggles. **`AudioListener.pause`** is used separately to mute game audio during the same window.

---

## 4. `PauseSimulation` (engine pause stack)

Public entry point:

```4944:4957:Reference/Assembly-CSharp-decompiled/SuperController.cs
	public void PauseSimulation(AsyncFlag af, bool hidden = false)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(af);
		hiddenPause = hidden;
		pauseSimulation = true;
		foreach (Atom value in atoms.Values)
		{
			value.PauseSimulation(af);
		}
	}
```

Each **`Atom`** forwards to its physics/simulator components (`Atom.PauseSimulation` in `Reference/Assembly-CSharp-decompiled/Atom.cs`). **`CheckResumeSimulation`** (called from **`Update`**) clears entries whose **`AsyncFlag.Raised`** is true and drops **`pauseSimulation`** when the wait list is empty.

Gabriel creates a dedicated **`AsyncFlag`**, calls **`PauseSimulation(flag, hidden: true)`** when settle **starts**, and **`Raise()`** on that flag **after** **`camExposure`** has been restored when settle **ends**. **`hidden: true`** avoids treating this like a user-visible “wait” reason in the normal wait UI where applicable.

---

## 5. Gabriel `OnSceneStartup`: behavior summary

### When settle **starts** (false → true)

1. **`BeginSceneSettleSimulationPauseHold()`** — **`AsyncFlag.Lower()`**, then **`SuperController.PauseSimulation(flag, true)`**.
2. **`MaintainSceneSettleAudioPauseDuringTick()`** — snapshot **`AudioListener.pause`** once, then force **`AudioListener.pause = true`** each LateUpdate while settling (so nothing else unmutes mid-load).
3. **`ApplyCamExposureWhileSceneSettling()`** — read **`CoreControl` → `GlobalLighting` → `camExposure`**, update backup rules, then **`SetFloatParamValue("camExposure", 0)`**.

### **`camExposure` backup** (why not only the first read)

Scene JSON may apply **after** the first visible value (default **~1** vs scene
**0.03**). Logic (see live Gabriel session-stack partials):

- Reads **`>= ~0.99`** (“bright”) **overwrite** the backup anytime (works while **`isLoading`** is still true; fixes short loads).
- Reads **below ~0.99`** tighten with **`Mathf.Min`** so a transient bright flash then the real dim scene value still restores **low** exposure (e.g. passenger scene).

### **`LateUpdate`**

The tick runs from **`LateUpdate`** so **`GlobalLighting`** tends to reflect the loaded scene **after** normal **`Update`** mutations.

### When settle **ends** (true → false)

Order is intentional:

1. Restore **`camExposure`** (from backup, or **`JSONStorableFloat.defaultVal`** fallback if no backup was captured).
2. **`Raise()`** the pause **`AsyncFlag`** so **`CheckResumeSimulation`** can clear **`pauseSimulation`** on the next appropriate frame.
3. Restore **`AudioListener.pause`** to the snapshot taken at settle start.

### Plugin destroy

**`OnOwningPluginDestroy`** restores exposure if a backup still exists, then **always** releases simulation/audio hold so the session is not left paused.

---

## 6. Related docs

- **`Reference/VaM-Scripting-Notes.md`** — C# 6 constraints, file APIs, and Gabriel cutover mapping notes.
- **`Reference/VaM-Camera-Initial-Scene-Pose.md`** — camera pose separate from load/settle timing.

---

## 7. Files quick reference

| Topic | Where |
|--------|--------|
| Load coroutine, `_isLoading`, loading UI | `Reference/Assembly-CSharp-decompiled/SuperController.cs` (~3811+) |
| `freezeAnimation`, `SetFreezeAnimation`, `PauseSimulation` | same |
| `Atom.PauseSimulation` | `Reference/Assembly-CSharp-decompiled/Atom.cs` |
| `AsyncFlag` | `Reference/Assembly-CSharp-decompiled/AsyncFlag.cs` |
| Gabriel settle + exposure + pause + audio | `Custom/Scripts/Gabriel/session-stack/src/SceneSettle.cs` + `PlaybackHold.cs` + `InitialExposureChange.cs` |
| Runtime caller | `Custom/Scripts/Gabriel/session-stack/src/GabrielSessionStack.cs` |
