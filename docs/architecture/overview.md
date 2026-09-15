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
| `GameBoard` | Single shared owner of the `GridModel` instance, camera-fit board geometry, and `BoardBounds` — everything else (shooter, renderer, fired-bubble logic) reads board state through this rather than computing its own. Raises `OnBubblePlaced`, (via `PopCells`/`DropCells`) `OnBubblesPopped`/`OnClusterDropped`, and (via `PushRowDown`) `OnRowPushedDown`. |
| `ShooterController` | Rotates the aim angle at a fixed speed while an on-screen rotate zone is held (arcade-style, not drag-to-angle — see `features/core-gameplay/shooter-and-trajectory.md`), tells `TrajectoryPredictor` to simulate for the preview, and raises `OnFireRequested` on a fire-zone press. |
| `TrajectoryPredictor` | Given a start point and aim angle, simulates the kinematic path (straight line + wall-bounce reflections only — no occupancy) and returns points for both the preview line and the actual fired bubble to follow. |
| `OccupancyCollision` | Truncates a raw `TrajectoryPredictor` path at the first occupied cell it touches; used by `LandingIndicator` and `FiredBubbleController` so the shown landing cell and the fired bubble both stop at the same point. The preview *line* itself is drawn unoccupancy-truncated (see `features/core-gameplay/firing-and-snapping.md`). |
| `LandingIndicator` | Ghost-bubble `SpriteRenderer` shown during aiming at the cell a shot would land in, resolved via `OccupancyCollision` + `BubbleLandingResolver` every frame — allowed to pop discretely between cells, unlike the (always smooth, occupancy-unaware) preview line. See `features/core-gameplay/firing-and-snapping.md`. |
| `BubbleLandingResolver` | Picks the nearest empty cell to a truncated path's contact point for a fired bubble to snap into, excluding any cell behind the wall's current advance (`GridModel.RowsPushed`). |
| `FiredBubbleController` | Subscribes to `OnFireRequested`, animates the fired bubble along the truncated path, places it on `GameBoard` when it lands, and shows the upcoming shot's color via a "next bubble" UI indicator next to the fire zone. |
| `FloodFill` | Generic BFS over `GridModel` from any number of seed cells, expanding only through cells satisfying a caller-supplied predicate. Shared by both of `MatchResolver`'s checks. |
| `MatchResolver` | Pure query class (no mutation): given a newly-placed bubble's cell, flood-fills same-color neighbors to find what pops (`FindMatchGroup`); separately finds bubbles disconnected from the ceiling row (`FindFloatingCells`). |
| `MatchProcessor` | Subscribes to `GameBoard.OnBubblePlaced`, calls `MatchResolver`, and drives `GameBoard.PopCells`/`DropCells` — see `features/core-gameplay/matching-and-popping.md`. |
| `LevelGenerator` | ✅ Implemented (Milestone 9). Static pure-logic class: fills an existing `GridModel` for a given level/difficulty (color count, density, headroom-row knobs from `DifficultyCurveConfig`), seeded by level number for determinism, then clears anything left disconnected from the ceiling. See `features/core-gameplay/level-generation.md`. |
| `GameStateManager` | Owns the shot timer and ceiling descent timer, and the win/loss checks (✅ all implemented) — the "referee" that ties the other systems together and raises `OnLevelWon` / `OnLevelLost`. Also runs the ceiling-push warning state machine (camera shake, gated on the next landed bubble rather than firing the instant the timer expires). |
| `CameraShake` | Hand-rolled camera jitter, started/stopped by `GameStateManager` as the ceiling-push warning. See `features/core-gameplay/shot-timer-and-ceiling-descent.md`. |
| `CeilingRenderer` | Draws the solid ceiling/wall band and grows it by one row height on every push, so the wall's visual footprint actually advances instead of only the bubbles moving. See `features/core-gameplay/hex-grid.md`. |
| `FrameRateInitializer` | Pins `Application.targetFrameRate` at startup so frame pacing is deterministic across Android devices instead of left to platform defaults. See "Frame pacing" below. |

Rendering (turning `GridModel` cells into actual bubble sprites/prefabs) is a
separate, thin layer that listens to grid-change events rather than being
part of the model — keeps the data model testable without needing a scene.

## Superpowers components (Phase 2) ✅ implemented

Confirms the "hook in without editing core systems" claim above: this
entire phase was added with zero changes to `GameBoard`'s public API,
plus small, additive extensions to `ShotTimer`, `GameStateManager`, and
`FiredBubbleController`. See
`features/superpowers/specs/2026-09-10-superpowers-design.md` for the
full design and implementation account.

| Component | Responsibility |
|---|---|
| `SuperpowerId` / `SuperpowerDefinition` | Plain enum (`Freeze, Bomb, RowClear, Rainbow`) and a small `[Serializable]` data class (`Id`, `UnlockLevel`, `ChargesPerLevel`). |
| `SuperpowerCatalog` | `ScriptableObject` holding the list of `SuperpowerDefinition`s — the per-ability equivalent of `DifficultyCurveConfig`. Default asset: `Assets/ScriptableObjects/DefaultSuperpowerCatalog.asset`. |
| `SuperpowerProgress` | Static, `PlayerPrefs`-backed `HighestLevelReached`, following the `GameSettings` precedent — tracks the player's furthest level reached so an unlocked ability stays unlocked across level retries. |
| `SuperpowerChargeTracker` | Pure C# class (no Unity dependency) owning per-level remaining charges for unlocked abilities — mirrors `LevelGenerator` taking a plain `DifficultyConfig` rather than the ScriptableObject wrapper directly. |
| `HexRadius` | Pure BFS utility: all cells within N hex-steps of a center cell, used by `SuperpowerEffectResolver.ResolveBomb`. |
| `SuperpowerEffectResolver` | Pure query class (no mutation, same shape as `MatchResolver`): given a landing cell, resolves the affected cell set per ability (Bomb radius, Row Clear, Rainbow same-color neighbor group via `FloodFill`). |
| `SuperpowerController` | Owns charge state for unlocked abilities, resets on `GameBoard.OnLevelLoaded`, updates `SuperpowerProgress` on `GameStateManager.OnLevelWon`. `TryActivate(id)` either calls `GameStateManager.Freeze(duration)` (Freeze) or `FiredBubbleController.ArmSuperpower(id)` (the other three), guarded so a second aimed ability can't be armed while one is already armed or in flight. Raises `OnAbilitiesChanged` whenever unlocked/charge state changes. |
| `SuperpowerEffectController` | Subscribes to `FiredBubbleController.OnSuperpowerLanded`, calls `SuperpowerEffectResolver`, groups the resolved cells by color, and calls `GameBoard.PopCells` once per color group — so `GameStateManager`'s win-check, `ScoreTracker`, and rendering all react through the exact same pipeline a normal match uses. |
| `SuperpowerHud` | Builds one button per unlocked ability at runtime (same code-built pattern as `HudDisplay`), showing charge count and disabling at 0 charges; rebuilds on `SuperpowerController.OnAbilitiesChanged` (not just once in `Start()`, since Unity doesn't guarantee cross-component `Start()` ordering). |

Small, additive extensions to existing Phase 1 files: `ShotTimer` gained
`Pause()`/`Resume()`/`IsPaused`; `GameStateManager` gained
`Freeze(float duration)` (pauses both its shot and ceiling timers);
`FiredBubbleController` gained an armed-ability state
(`ArmSuperpower(SuperpowerId)`, `HasArmedOrInFlightSuperpower`) and the
`OnSuperpowerLanded` event.

## Level-content components (curated patterns + transitions) ✅ implemented

Layered on top of the existing `LevelGenerator`, which stays completely
unmodified and still handles level 6+ (any level without a curated
recipe). See
`features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md`
for the full design and implementation account.

| Component | Responsibility |
|---|---|
| `PatternType` | Enum (`HexBlob, VerticalStripe, HorizontalStripe`). |
| `LevelPatternPlan` | Small `[Serializable]` data class: `LevelNumber`, `Regions` (ordered `PatternType[]`, one per horizontal band). |
| `PatternGenerationSettings` | Plain data class for the shared tunables (`HexBlobRadius`, `HexBlobGapCells`, `StripeWidth`, `RegionGapRows`). |
| `PatternPlacementContext` | Groups `GridModel`/`PatternGenerationSettings`/`ColorCount`/`System.Random` for the placers below — mirrors `DifficultyConfig` taking plain data rather than a `ScriptableObject`. |
| `HexBlobPlacer` | Fills a row band with separated, single-color hexagonal blobs (reuses `HexRadius` from Superpowers). Clamps its blob radius to whatever the band can actually hold, so a short band still places something instead of silently nothing. |
| `StripePlacer` | Fills a row band solid with repeating same-width color stripes, vertical or horizontal. |
| `PatternLevelGenerator` | Orchestrates `HexBlobPlacer`/`StripePlacer` across a `LevelPatternPlan`'s row bands, then bridges (not deletes) anything `MatchResolver.FindFloatingCells` finds disconnected — gap-separated blobs/bands are disconnected by design here, unlike `LevelGenerator`'s rare accidental case. |
| `PatternLevelCatalog` | `ScriptableObject` holding the ordered `LevelPatternPlan` list (levels 1-5) and the shared `PatternGenerationSettings` tunables — the per-pattern equivalent of `DifficultyCurveConfig`. Default asset: `Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset`. |
| `LevelContentGenerator` | Routes to `PatternLevelGenerator` when the catalog has a plan for the level, otherwise falls through to the unmodified `LevelGenerator` — the single seam `GameBoard.GenerateGrid()` calls into. |
| `BubbleSpawnMotion` | Pure ease-out-cubic scale-in function for the level-load build-in animation. |
| `BubbleSpawnAnimator` | Small self-contained `MonoBehaviour`, added per spawned bubble on level load — same `Update()`-driven elapsed-time pattern as `FallingBubble`/`FiredBubbleController`'s settle motion, no tweening library. |

`DifficultyCurveConfig`'s four knobs (`ColorCount`, `Density`,
`HeadroomRows`, `CeilingDropIntervalSeconds`) changed from independent
linear-ramp formulas to Inspector-editable `AnimationCurve`s, each
evaluated at the level number and clamped to a safe range — the old
level-1 special case is now just each curve's first keyframe. See
`features/core-gameplay/level-generation.md`.

`GridDebugRenderer`'s single `RebuildAll()` split into an instant path
(kept for `OnRowPushedDown`, which should still read as abrupt) and an
animated path (for `OnLevelLoaded`) that attaches a `BubbleSpawnAnimator`
to each spawned bubble, delayed by its row.

## Battle mode components (Phase 3, simple attack variant) ✅ implemented

Confirms the "hook in without editing core systems" claim above: this
entire phase reuses Phase 1's public events unchanged, plus one small
additive extension to `GameBoard` (see below) needed to run two
simultaneous board instances. See
`features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md`
for the full design and implementation account.

| Component | Responsibility |
|---|---|
| `PendingRowsMeter` | Pure C# class: accumulates fractional attack value and releases only whole rows, carrying the remainder forward — `GridModel.PushRowsDown` only moves whole rows. |
| `BattleAttackConfig` | `ScriptableObject`: match-size/cascade-drop-count → attack value, deliberately decoupled from solo's `ScoreCalculator`. Default asset: `Assets/ScriptableObjects/DefaultBattleAttackConfig.asset`. |
| `BattleAttackController` | Listens to *my* board's `OnBubblesPopped`/`OnClusterDropped`, banks attack value via `BattleAttackConfig`/`PendingRowsMeter`, and calls `PushRowDown()` on the *opponent's* `GameBoard` — the one new cross-board wiring this feature needs. |
| `BattleEndReason` / `BattleSideOutcome` | Enum (`Cleared, WallReachedLine`) and a `MonoBehaviour` reusing solo mode's exact win/loss signals (`OnRowPushedDown`, `GridModel.IsEmpty`), scoped to one side, raising a one-shot `OnSideEnded` event. |
| `BattleMatchResult` / `BattlePlayerSide` | Enum (`Player1Wins, Player2Wins, Draw`) and a `[Serializable]` grouping class bundling one side's `GameBoard`/`ShooterController`/`BattleShotClock`/`BattleAttackController`/`BattleSideOutcome`. |
| `BattleMatchController` | Reconciles both sides' `OnSideEnded` signals into one authoritative result (same-frame endings resolve as a draw), picks one shared random level number for both boards each match, and drives Rematch. `[DefaultExecutionOrder(1000)]` so its `LoadLevel` calls run after every other component has subscribed to `OnLevelLoaded` — see the spec's implementation notes for the (already-hit-once, in Superpowers) bug class this avoids. |
| `BattleShotClock` | Per-side shot timer only (no ceiling timer) — thin wrapper around the existing `ShotTimer`. |
| `BattleResultScreen` | Win/lose/draw panel with Rematch, same runtime-built recipe as `LevelResultScreen`. |

Small, additive extension to an existing Phase 1 file: `GameBoard` gained
`targetCamera`/`viewportHeightFraction` fields, letting an instance
target a specific camera and a fraction of the screen height instead of
always using `Camera.main`/the full screen — default values reproduce
solo mode's exact prior behavior untouched. No other Phase 1 file
(`GridModel`, `MatchProcessor`, `ShooterController`,
`BubbleLandingResolver`, etc.) was changed.

Player 2's 180°-rotated half needed no coordinate-math changes anywhere:
`GameBoard`'s board-fit math (`PositionBoard`, `BoardBoundsCalculator`,
`ShooterOrigin`) only ever reads camera *position* and *orthographic
size* — both rotation-invariant — so rotating only the physical camera
transform (and, separately, a UI parent `RectTransform` for the touch
zones) achieves the full visual/input mirror for free.

## Events (initial set — expand as needed)

- `OnBubblePlaced(cell)` — ✅ implemented, on `GameBoard`.
- `OnBubblesPopped(cells, color)` — ✅ implemented, on `GameBoard`.
- `OnClusterDropped(cells)` — ✅ implemented, on `GameBoard`.
- `OnRowPushedDown(bool wasLastRowOccupied)` — ✅ implemented, on
  `GameBoard`. The payload reports whether the shooter's line was occupied
  right before the shift, discarded by it — the signal Milestone 8's loss
  check will consume; `GameStateManager` currently only logs it.
- `OnFireRequested(origin, angle)` — ✅ implemented, on `ShooterController`.
  Milestone 6 routes both manual and auto-fire through this single event
  (via `ShooterController.Fire()`) rather than adding a separate
  `OnShotTimerExpired()` event as originally sketched.
- `OnLevelWon()` / `OnLevelLost()` — ✅ implemented, on `GameStateManager`
  (Milestone 8).
- `OnSuperpowerLanded(SuperpowerId, (int Row, int Col) cell)` — ✅
  implemented, on `FiredBubbleController` (Phase 2). Raised in place of
  normal color placement when an armed superpower shot lands;
  `SuperpowerEffectController` is the only subscriber.
- `OnAbilitiesChanged()` — ✅ implemented, on `SuperpowerController`
  (Phase 2). Raised whenever unlocked/charge state changes (level load,
  activation); `SuperpowerHud` subscribes to know when to rebuild its
  buttons.

Phase 2 (superpowers) and Phase 3 (battle mode) both demonstrate this
pattern working as intended: Phase 3 in particular hooks in via
`OnBubblesPopped`/`OnClusterDropped`/`PushRowDown` for its cross-board
attack wiring (see "Battle mode components" above), turning one board's
pops into rows pushed onto the other, with zero changes to `GameBoard`'s
public API beyond the one additive camera/viewport extension.

## Folder conventions (`Assets/`)

- `Scripts/` — all C# code, organized by the components above (e.g.
  `Scripts/Grid/`, `Scripts/Shooter/`, `Scripts/Gameplay/`,
  `Scripts/Superpowers/`), under a single `Game` assembly
  (`Scripts/Game.asmdef`).
- `Tests/EditMode/` — Unity Test Framework tests for pure C# logic, under a
  `Game.EditModeTests` assembly that references `Game`. See Testing below.
- `Prefabs/` — bubble prefab, UI prefabs, etc.
- `Art/` — placeholder and (later) real sprites.
- `ScriptableObjects/` — level generation difficulty configs, color
  palettes, superpower catalogs.
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

## Frame pacing: explicit target frame rate, not device/VSync defaults

Found during real-device playtesting (general "not smooth" feeling,
separate from the aim-preview lag fixed in
`features/core-gameplay/firing-and-snapping.md`): the project never called
`Application.targetFrameRate`, and `QualitySettings` had inconsistent
`vSyncCount` across quality tiers (0 on Very Low/Low, 1 on Medium-Ultra).
Since VSync overrides `targetFrameRate` on most platforms, this meant frame
pacing depended on which quality tier was active and the device's own
refresh rate, rather than being consistent. Fixed by disabling VSync
(`vSyncCount = 0`) on every quality tier and adding `FrameRateInitializer`
(`Assets/Scripts/Bootstrap/`), a single-purpose `MonoBehaviour` that sets
`Application.targetFrameRate = 60` in `Awake`.

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
