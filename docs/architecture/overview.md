# Architecture Overview

## Approach

Manager-based MonoBehaviours, each with one clear responsibility, wired
together with plain C# events rather than direct references. This was chosen
over a full ScriptableObject event-bus (too much Inspector-driven indirection
for a first Unity project) and over ECS/DOTS (steep learning curve, and
overkill for a board of ~100 bubbles).

The event-based wiring is still worth doing even though it adds a small step
over "just call the method directly" — it means Phase 2 (superpowers) and
Phase 3 (battle mode) can hook into core gameplay (e.g. "bubbles were just
popped") without editing the core systems themselves.

## Core components (Phase 1)

| Component | Responsibility |
|---|---|
| `GridModel` | Owns the hex grid data (which cell holds which color, empty vs. occupied). No rendering, no Unity physics — pure data + queries (neighbors, flood fill). |
| `ShooterController` | Reads touch input, tracks aim angle, tells `TrajectoryPredictor` to simulate, fires bubbles. |
| `TrajectoryPredictor` | Given a start point and aim angle, simulates the kinematic path (straight line + wall-bounce reflections) and returns points for both the preview line and the actual fired bubble to follow. |
| `MatchResolver` | Given a newly-placed bubble's grid cell, flood-fills same-color neighbors, decides what pops, and detects floating (disconnected) clusters afterward. |
| `LevelGenerator` | Produces a `GridModel` populated for a given level/difficulty (color count, density, row count knobs). |
| `GameStateManager` | Owns the shot timer, ceiling descent timer, and win/loss checks; the "referee" that ties the other systems together and raises high-level events like `OnLevelWon` / `OnLevelLost`. |

Rendering (turning `GridModel` cells into actual bubble sprites/prefabs) is a
separate, thin layer that listens to grid-change events rather than being
part of the model — keeps the data model testable without needing a scene.

## Events (initial set — expand as needed)

- `OnBubblePlaced(cell)`
- `OnBubblesPopped(cells, color)`
- `OnClusterDropped(cells)`
- `OnRowPushedDown()`
- `OnShotTimerExpired()`
- `OnLevelWon()` / `OnLevelLost()`

These are the seams Phase 2/3 will subscribe to later (e.g. a superpower
bubble reacting to `OnBubblesPopped`, or battle mode turning
`OnBubblesPopped` on one board into garbage rows added to the other).

## Folder conventions (`Assets/`)

- `Scripts/` — all C# code, organized by the components above (e.g.
  `Scripts/Grid/`, `Scripts/Shooter/`, `Scripts/Gameplay/`).
- `Prefabs/` — bubble prefab, UI prefabs, etc.
- `Art/` — placeholder and (later) real sprites.
- `ScriptableObjects/` — level generation difficulty configs, color palettes.
- `Scenes/` — gameplay scene(s).

## Trajectory: kinematic simulation, not Rigidbody2D physics

The shooter does **not** use Unity's Rigidbody2D/real physics for the fired
bubble. Instead, `TrajectoryPredictor` computes the path itself (straight
line, reflect off side walls) using the same math for both the preview line
and the actual shot. This guarantees the preview the player sees always
matches exactly where the bubble goes — real physics engines have enough
non-determinism (fixed timestep quantization, collision resolution order)
that a physics-simulated preview can occasionally diverge from the real
shot, which is fatal for a game where precision aiming is the whole point.
