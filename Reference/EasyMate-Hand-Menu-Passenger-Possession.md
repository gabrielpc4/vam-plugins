# Gabriel hand-menu passenger possession

Reference for the current hand-menu passenger possession flow used by Gabriel.

This documents the path that starts from the VR hand menu and ends in the Passenger-style female possession runtime. It reflects the latest behavior in:

- `Custom/Scripts/Gabriel/features/ui-hud/GabrielHudButtons.cs`
- `Custom/Scripts/Gabriel/features/passenger-possession/PassengerRuntime.cs`
- `Custom/Scripts/Gabriel/features/passenger-possession/PassengerHandPrePossessSnapshot.cs`
- `Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV.cs`

---

## Entry point

The active entry point is the VR palm / hand menu flow.

- The hand HUD opens the female choice step.
- Choosing `Mulher` does not use the old world-button flow.
- It finds the closest female `Person` by head-to-camera distance.
- It then starts `PassengerRuntime.RequestStartForFemale(target)` for female targets, or `PassengerRuntime.RequestStartForMale(target)` for male targets.

The old scene-world passenger button flow is no longer the path to reason about here.

---

## High-level behavior

This possession mode is not "possess the head and keep following the model head forever".

Instead, it works like this:

1. Prepare `ImprovedPoV` on the target person.
2. Snapshot the person head state before Gabriel starts overriding it.
3. Freeze scene animation temporarily.
4. Force the target `headControl` to a neutral rotation once.
5. Wait a short fixed delay (`100 ms`) so the mesh catches up.
6. Teleport / align the player rig to the target head.
7. Stop forcing the neutral rotation.
8. Keep following the target head position.
9. Handle head rotation after startup with special rules:
   - if the scene contains a `ReflectiveSlate` or `Glass` atom, the head follows the HMD rotation
   - otherwise, the target `headControl` rotation is kept at the scene's original rotation instead of being driven by the HMD

So the initial head neutralization is only a startup step. It is not a continuous "fight the animation forever" loop anymore.

---

## ImprovedPoV behavior

If the target person does not already have `ImprovedPoV`, Gabriel merges it first and waits until it is available before continuing.

When passenger mode starts, Gabriel prepares `ImprovedPoV` like this:

- `Hide face = true`
- `Hide hair = true`
- `Activate only when possessed = false`

Current `ImprovedPoV` defaults in this repo are:

- `Camera depth = 0.17`
- `Camera height = 0.06`

Gabriel no longer zeroes those offsets during this flow, so the possession uses the same configured `ImprovedPoV` offset system instead of its own old offset path.

---

## What gets captured before possession

Before the runtime starts overriding anything, Gabriel stores:

- the current navigation rig rotation
- the current navigation rig position
- the current `playerHeightAdjust`
- the target person's `headControl` rotation and rotation state

Later, when passenger mode stops and `ClearPossess()` runs, Gabriel restores the saved head state and the saved navigation rig state.

This matters because passenger mode is intentionally invasive while active, but it should leave the scene in a sane state after it stops.

---

## Head neutralization before first teleport

At startup, Gabriel captures the target head's current downward pitch and preserves it if it is:

- greater than `0`
- less than or equal to `90`

That preserved downward pitch is then reapplied to the neutralized startup rotation.

The neutralized startup rotation is built from a "forward" direction that deliberately ignores the current animated head yaw. The forward source order is:

1. `chestControl.forward`
2. `pelvisControl.forward`
3. `abdomenControl.forward`
4. `person.transform.forward`
5. `headRigidbody.forward`

That means the startup alignment is based on the person's torso / atom forward, not on wherever the animated head happens to be twisted.

After computing that neutral target:

- Gabriel turns on `freezeAnimation` if it was not already on
- writes the neutral rotation to `headControl`
- waits `100 ms`
- teleports anyway after the delay
- restores the previous `freezeAnimation` state

There is no longer a per-frame "wait until perfectly stable" loop.

---

## First teleport alignment

On the first teleport, Easy Mate computes a desired head-facing rotation from:

- the body-based neutral forward described above
- the target head rigidbody rotation
- the built-in X rotation offset used by the runtime
- the preserved downward pitch, if one was captured

Important details:

- If there is a preserved downward pitch, Gabriel keeps that downward tilt during the first teleport.
- If there is no preserved downward pitch, the runtime can fall back to the current HMD pitch.
- Roll is zeroed on the navigation rig during the first snap.
- The first snap also recenters laterally against the torso midline so the HMD lands on the person's center plane.

So the first teleport is meant to face the person's body forward, keep a real downward look if the scene head was already looking down, and avoid inheriting random animated head yaw / roll.

---

## What follows after the first teleport

### Position

After startup, the player rig keeps following the target head position.

The target position is based on:

- the target head rigidbody world position
- plus a small offset in the head rigidbody forward direction

After the first snap, movement is smoothed instead of hard-snapped every frame.

### Rotation

After startup, the rig is not continuously reoriented to match whatever the model head animation does.

That was a deliberate outcome of this chat. The initial teleport uses a body-based forward so the player starts facing the character's front correctly, but the ongoing runtime is not supposed to keep inheriting animated head yaw drift.

For the target person's actual `headControl`, the current rule is:

- If the scene contains an active `ReflectiveSlate` or `Glass` atom, Easy Mate drives the person's head rotation from the HMD.
- Otherwise, Easy Mate keeps the person's `headControl` at the rotation state captured from the original scene.

This special-case exists so mirrors / reflective setups can still show the player-driven head orientation, while normal scenes keep the authored head rotation instead of flattening or replacing it.

---

## Hand possession after passenger starts

Passenger body mode starts first.

Hand possession is a second step:

- once passenger mode is active, Easy Mate waits for any VR trigger or grip press
- then it snapshots both hand states
- then it starts the hand possession routine

That is why the runtime logs:

`Easy Mate passenger: press any VR grip or trigger to possess the model's hands.`

So "be the girl" body alignment and "take over the hands" are related, but they are not the same instant.

---

## What this flow intentionally ignores

These are intentional properties of the current implementation:

- It does not use the old legacy world-button passenger path.
- It does not keep recomputing startup neutralization every frame.
- It does not use the animated head's current yaw as the authoritative facing direction for startup.
- It does not keep rotating the whole experience to follow ongoing model head rotation after the first alignment.
- It does not zero `ImprovedPoV` depth / height offsets during passenger startup.

---

## Practical summary

If you want to reason about current Gabriel passenger possession, the mental model is:

- Start from the hand menu.
- Choose the nearest female person.
- Make sure `ImprovedPoV` is active and configured.
- Save the original head state.
- Neutralize the head once using body forward.
- Preserve an already-authored downward look.
- Freeze animation briefly, wait `100 ms`, then teleport.
- Follow head position afterward.
- Only follow HMD head rotation in reflective / glass scenes.
- Otherwise keep the scene's original head-control rotation.

That is the current intended behavior.
