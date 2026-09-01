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
| `GridModel` | Owns the hex grid data (which cell holds which color, empty vs. occupied). No rendering, no Unity physics — pure data + queries (neighbors, flood fill, occupied-cell enumeration). |
| `GameBoard` | Single shared owner of the `GridModel` instance, camera-fit board geometry, and `BoardBounds` — everything else (shooter, renderer, fired-bubble logic) reads board state through this rather than computing its own. Raises `OnBubblePlaced`, and (via `PopCells`/`DropCells`) `OnBubblesPopped`/`OnClusterDropped`. |
| `ShooterController` | Rotates the aim angle at a fixed speed while an on-screen rotate zone is held (arcade-style, not drag-to-angle — see `features/core-gameplay/shooter-and-trajectory.md`), tells `TrajectoryPredictor` to simulate for the preview, and raises `OnFireRequested` on a fire-zone press. |
| `TrajectoryPredictor` | Given a start point and aim angle, simulates the kinematic path (straight line + wall-bounce reflections only — no occupancy) and returns points for both the preview line and the actual fired bubble to follow. |
| `OccupancyCollision` | Truncates a raw `TrajectoryPredictor` path at the first occupied cell it touches, so the preview and the fired bubble both stop at the same point (see `features/core-gameplay/firing-and-snapping.md`). |
| `BubbleLandingResolver` | Picks the nearest empty cell to a truncated path's contact point for a fired bubble to snap into. |
| `FiredBubbleController` | Subscribes to `OnFireRequested`, animates the fired bubble along the truncated path, places it on `GameBoard` when it lands, and shows the upcoming shot's color via a "next bubble" UI indicator next to the fire zone. |
| `FloodFill` | Generic BFS over `GridModel` from any number of seed cells, expanding only through cells satisfying a caller-supplied predicate. Shared by both of `MatchResolver`'s checks. |
| `MatchResolver` | Pure query class (no mutation): given a newly-placed bubble's cell, flood-fills same-color neighbors to find what pops (`FindMatchGroup`); separately finds bubbles disconnected from the ceiling row (`FindFloatingCells`). |
| `MatchProcessor` | Subscribes to `GameBoard.OnBubblePlaced`, calls `MatchResolver`, and drives `GameBoard.PopCells`/`DropCells` — see `features/core-gameplay/matching-and-popping.md`. |
| `LevelGenerator` | Produces a `GridModel` populated for a given level/difficulty (color count, density, row count knobs). |
| `GameStateManager` | Owns the shot timer (✅ implemented), ceiling descent timer, and win/loss checks; the "referee" that ties the other systems together and raises high-level events like `OnLevelWon` / `OnLevelLost`. |

Rendering (turning `GridModel` cells into actual bubble sprites/prefabs) is a
separate, thin layer that listens to grid-change events rather than being
part of the model — keeps the data model testable without needing a scene.

## Events (initial set — expand as needed)

- `OnBubblePlaced(cell)` — ✅ implemented, on `GameBoard`.
- `OnBubblesPopped(cells, color)` — ✅ implemented, on `GameBoard`.
- `OnClusterDropped(cells)` — ✅ implemented, on `GameBoard`.
- `OnRowPushedDown()` — not yet implemented (Milestone 7).
- `OnFireRequested(origin, angle)` — ✅ implemented, on `ShooterController`.
  Milestone 6 routes both manual and auto-fire through this single event
  (via `ShooterController.Fire()`) rather than adding a separate
  `OnShotTimerExpired()` event as originally sketched.
- `OnLevelWon()` / `OnLevelLost()` — not yet implemented (Milestone 8).

These are the seams Phase 2/3 will subscribe to later (e.g. a superpower
bubble reacting to `OnBubblesPopped`, or battle mode turning
`OnBubblesPopped` on one board into garbage rows added to the other).

## Folder conventions (`Assets/`)

- `Scripts/` — all C# code, organized by the components above (e.g.
  `Scripts/Grid/`, `Scripts/Shooter/`, `Scripts/Gameplay/`), under a single
  `Game` assembly (`Scripts/Game.asmdef`).
- `Tests/EditMode/` — Unity Test Framework tests for pure C# logic, under a
  `Game.EditModeTests` assembly that references `Game`. See Testing below.
- `Prefabs/` — bubble prefab, UI prefabs, etc.
- `Art/` — placeholder and (later) real sprites.
- `ScriptableObjects/` — level generation difficulty configs, color palettes.
- `Scenes/` — gameplay scene(s).
- `Screenshots/` — gitignored; Editor/MCP debug captures land here.

## Code style

Files stay under ~200 lines and function/method bodies under ~7 lines with
at most 3 parameters (group extras into a tuple or small type, as
`GridModel.GetNeighbors`'s internals do). Comments explain non-obvious
*why* (e.g. the hex-offset math), never restate what a well-named symbol
already says. This is enforced by the `general-code-style` Claude Code
plugin, not by a Unity analyzer — keep it in mind when writing code outside
that workflow too.

## Trajectory: kinematic simulation, not Rigidbody2D physics

The shooter does **not** use Unity's Rigidbody2D/real physics for the fired
bubble. Instead, `TrajectoryPredictor` computes the path itself (straight
line, reflect off side walls) using the same math for both the preview line
and the actual shot. This guarantees the preview the player sees always
matches exactly where the bubble goes — real physics engines have enough
non-determinism (fixed timestep quantization, collision resolution order)
that a physics-simulated preview can occasionally diverge from the real
shot, which is fatal for a game where precision aiming is the whole point.

## Testing

Pure C# logic (`GridModel` and friends) is covered by Unity Test Framework
**EditMode** tests under `Assets/Tests/EditMode/`, split into a `Game`
runtime assembly and a `Game.EditModeTests` test assembly (see their
`.asmdef` files).

There are two ways to run them — pick based on whether the Editor is
already open, since **they conflict with each other** (Unity refuses to
open the same project twice):

- **Editor closed**: run `.\run-edittests.ps1` at the project root
  (PowerShell). It wraps a Unity batch-mode invocation and works around two
  quirks discovered while setting this up:
  - `-runTests` must **not** be combined with `-quit` — the test runner
    quits on its own when done, and `-quit` makes Unity exit before tests
    run.
  - Unity clears its own project `Temp/` folder on a clean shutdown, so
    results/logs must be written outside it (the script uses `$env:TEMP`)
    or they get deleted before you can read them.
  - The script also polls for the results file rather than trusting the
    launched process to block, because Unity's own process hands off to a
    child process and returns early.
- **Editor already open** (e.g. via the Unity MCP bridge, see Tooling
  below, or just working in the Editor UI): use
  **Window > General > Test Runner**, or have an MCP-connected agent call
  the `run_tests`/`get_test_job` tools against the live instance. A batch
  run started while the Editor has the project open will hang/fail with
  "another Unity instance is running with this project open".

## Tooling: Unity MCP bridge

This project has the [Unity MCP](https://github.com/CoplayDev/unity-mcp)
bridge installed (`com.coplaydev.unity-mcp` in `Packages/manifest.json`),
which lets an MCP-connected agent drive an open Unity Editor directly —
creating/inspecting GameObjects, managing scenes, entering Play mode,
taking screenshots, and running tests. It got added automatically the
first time the project was opened (via a global Claude Code + Unity
integration), not as a manual dependency choice. Useful in practice for
verifying anything that needs an actual scene (rendering, prefabs) rather
than pure-logic unit tests.
