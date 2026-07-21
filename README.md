# 3D Platformer Prototype

A prototype of a 3D platformer game built in **Unity** with a focus on **modular, scalable gameplay architecture**.

The core of the project is a **dual-layer hierarchical state machine system** that cleanly separates player locomotion from combat/interaction logic — enabling complex, composable player states without spaghetti code.

---

## Architecture

### Dual-layer state machines

The player controller runs two independent state machines on the same GameObject:

| Layer | Responsibility | States |
|-------|---------------|--------|
| **Movement** | Physics, gravity, locomotion | `Grounded`, `Jump`, `Fall`, `WallSliding`, `Idle`, `Walk`, `Run` |
| **Action** | Combat, interactions | `Idle`, `Attack`, `PunchLeft`, `PunchRight`, `Grab` |

Both layers use the same base pattern but never interfere with each other. You can punch while falling, grab while walking, or slide down a wall while charging an attack.

### Hierarchical states

Every state inherits from a base class that handles the tree structure:

- **Super-states** manage broad behavior (e.g., `Attack`)
- **Sub-states** handle specific actions (e.g., `PunchLeft`, `PunchRight`)
- `UpdateStates()` recursively ticks the entire tree each frame
- `SwitchState()` exits the whole branch before entering a new one
- `SetSubState()` swaps a sub-state while keeping the same super-state alive

This distinction is what enables **combo chains**: `PunchLeft` → `PunchRight` stays inside `Attack`, but exiting the combo calls `SwitchState` to return to `Idle`.

---

## Key Features

- **Input buffering** — Attack inputs pressed during an animation are buffered and evaluated at the end of the current punch, enabling smooth combo chains without frame-perfect timing.
- **Triple jump system** — Three consecutive jumps with escalating velocity and gravity curves, plus coyote time and jump buffering for forgiving platforming.
- **Slope detection & wall sliding** — Raycast-based terrain analysis triggers wall-sliding states when slopes exceed the CharacterController limit.
- **Grab system** — Raycast + overlap sphere detection for grabbable objects, with continuous distance validation and auto-release.
- **Animation layering** — Three Animator layers (idle attacks, moving attacks, grab) with smooth weight transitions via coroutine-based lerping.
- **Animation events** — Dash forces and particle spawns triggered directly from animation keyframes via `AnimationEventReceiver`.

---

## Tech Stack

- Unity 2022.3 LTS
- C# 10
- Unity Input System (new)
- Animator Controller (layered)
- Character Controller

---

## Project Structure
