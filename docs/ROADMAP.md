# Roadmap

Status legend: ✅ designed (see feature docs) · 🚧 placeholder, needs its own
brainstorming session · ⏳ not yet scoped.

## Phase 0 — Project setup

- [x] `git init` this project and make an initial commit.
- [x] Create folder conventions under `Assets/`: `Scripts/`, `Prefabs/`,
      `Art/`, `ScriptableObjects/`, `Scenes/` (plus `Tests/` for EditMode
      tests, added once testing started).
- [x] `.gitignore` / `.gitattributes` set up for Unity (standard ignores,
      line-ending normalization, and a `Screenshots/` ignore for Editor/MCP
      debug captures).
- [ ] Import a free placeholder sprite pack for bubbles/UI (e.g.
      [Kenney.nl](https://kenney.nl/) — "Puzzle Pack" or similar). Not
      needed yet — the Milestone 1 debug renderer draws plain circles
      generated in code instead, see below.
- [ ] Confirm the existing URP 2D template settings are suitable (project
      already uses Unity 6000.5.1f1 with the 2D URP template — no changes
      expected here, just a sanity check once real content exists).
- [x] Player Settings default orientation set to Portrait (landscape
      autorotate disabled) to match the portrait-only design (see
      `battle-mode/overview.md` and
      `core-gameplay/screen-fit-and-difficulty-scaling.md`). Build target
      is still `StandaloneWindows64` for now — switching to Android/iOS is
      deferred to Milestone 11's device build.

## Phase 1 — Core single-player bubble shooter ✅ (design complete)

See `features/core-gameplay/`. Suggested build order — each milestone should
be playable/testable on its own before moving to the next:

1. **Hex grid data model** + static debug rendering of a board. ✅ **Done.**
   `GridModel` (occupancy, hex-neighbor lookup, world position) is
   implemented and unit-tested (`Assets/Scripts/Grid/`,
   `Assets/Tests/EditMode/`), with a temporary `GridDebugRenderer` that
   fills a board with random-colored circles (generated in code, no art
   asset needed) to visually confirm the hex packing. `GridDebugRenderer`
   is a Milestone-1 stand-in, not the final rendering layer described in
   `architecture/overview.md`.
   → [`hex-grid.md`](features/core-gameplay/hex-grid.md)
   - **Screen fit & device-independent difficulty scaling — decided and
     implemented.** Board fills the full phone screen in width and height
     (no letterbox bars) via a fixed-column/match-width camera plus a
     dynamically computed row count, with ceiling-descent fairness
     preserved across devices via a headroom-rows difficulty knob (for
     Milestone 9's `LevelGenerator`). Unblocks Milestone 2's need for real
     board bounds. Tablets explicitly out of scope for now.
     → [`screen-fit-and-difficulty-scaling.md`](features/core-gameplay/screen-fit-and-difficulty-scaling.md)
2. **Shooter + aim input** with kinematic trajectory preview (including wall
   bounces). ✅ **Done.** Aiming turned out to be a fixed-speed rotating gun
   (arcade Puzzle Bobble style, via on-screen hold zones) rather than
   drag-to-angle — the original write-up here was wrong and got corrected
   during implementation. `TrajectoryPredictor`, `BoardBoundsCalculator`,
   `HoldInputZone`, and `ShooterController` are implemented, with the pure
   math unit-tested (`Assets/Scripts/Shooter/`, `Assets/Tests/EditMode/`).
   Firing itself was **not** built yet at this point — `ShooterController`
   raised `OnFireRequested` with no subscriber, which became Milestone 3's
   hook-in point. Known tech debt at the time: `ShooterController` and
   `GridDebugRenderer` each had their own `cols`/`cellWidth` fields instead
   of sharing one board config — ~~resolved in Milestone 3~~ by introducing
   `GameBoard` as the single shared owner (see below).
   → [`shooter-and-trajectory.md`](features/core-gameplay/shooter-and-trajectory.md)
3. **Firing a bubble** — move it along the previewed path, snap to the
   nearest empty grid cell on collision. ✅ **Done.** `GameBoard` is now the
   single shared owner of the `GridModel`/bounds (resolving the
   `ShooterController`/`GridDebugRenderer` cols/cellWidth duplication
   tech debt from Milestone 2). `OccupancyCollision` truncates the
   trajectory at the first occupied cell so the preview and the fired
   bubble always agree, `BubbleLandingResolver` picks the nearest empty
   cell, and `FiredBubbleController` animates the fired bubble and hooks
   into `ShooterController.OnFireRequested`.
   → [`firing-and-snapping.md`](features/core-gameplay/firing-and-snapping.md)
4. **Match detection** (3+ connected same-color bubbles via flood fill) +
   popping. ✅ **Done**, together with Milestone 5 (built in the same pass,
   since the design doc treats them as one flood-fill-based component).
   `FloodFill` (generic BFS over `GridModel`) and `MatchResolver`
   (`FindMatchGroup`/`FindFloatingCells`) are static, pure logic classes in
   `Assets/Scripts/Grid/`, unit-tested, following the `BubbleLandingResolver`
   precedent. `GameBoard` gained `OnBubblesPopped`/`OnClusterDropped` events
   and `PopCells`/`DropCells` methods — kept on `GameBoard` rather than a new
   `MatchResolver` MonoBehaviour, so it stays the single event source for all
   grid-state changes (consistent with `OnBubblePlaced`). A new
   `MatchProcessor` listens to `OnBubblePlaced` (rather than being wired
   directly into `FiredBubbleController.Land()`), so matching/dropping
   applies to any future placement source, not just fired bubbles —
   confirmed safe since `GameBoard.Awake`'s initial fill calls
   `Grid.PlaceBubble` directly, bypassing the event. `GridDebugRenderer` now
   tracks its spawned sprites and reacts to both events: a pop destroys
   instantly, while a drop adds a new `FallingBubble` component (simple
   constant-gravity fall) so a bubble that loses its connection to the
   ceiling visibly falls instead of vanishing like a match.
   → [`matching-and-popping.md`](features/core-gameplay/matching-and-popping.md)
   - **Row-direction bug found and fixed.** `firing-and-snapping.md` and
     `BubbleLandingResolver` always assumed row 0 is the ceiling, but
     `GameBoard`'s actual positioning/fill code had it backwards — row 0
     rendered at the bottom near the shooter (always empty in practice),
     while real board content lived near `Rows-1`. Harmless before now
     (`BubbleLandingResolver`'s row-0 fallback path is rarely reachable
     since the initial fill blocks most straight-up shots), but it would
     have made `MatchResolver.FindFloatingCells` drop the entire board on
     every pop, since its ceiling-row seed set would almost always be
     empty. Fixed at the source: `GridModel.GetWorldPosition` now returns
     `y = -row * RowHeight` (row 0 = y = 0 = the ceiling anchor), and
     `GameBoard.PositionBoard`/`FillWithRandomBubbles`/`ShooterOrigin` were
     updated to match (anchor at the top, fill from row 0 down, shooter
     origin computed independently of the now-top-anchored transform).
     Verified with a live Play-mode check (occupied rows, world Y vs.
     camera bounds) and a screenshot before trusting the fix. Updated
     `GridModelWorldPositionTests` and three `OccupancyCollisionTests`
     scenarios that had hard-coded the old (wrong) direction.
   - **Shooter tweaks needed to actually test this.** Manually verifying
     matches required knowing which color was about to fire and being able
     to aim precisely, so two small `Shooter`-side changes landed alongside
     this milestone rather than as separate work: a "next bubble" indicator
     (see `firing-and-snapping.md`) and a rotation-speed retune (`90 → 45 →
     25`, see `shooter-and-trajectory.md`).
5. **Floating cluster detection** (bubbles disconnected from the ceiling
   after a pop) + drop. ✅ **Done** — see Milestone 4 above.
6. **Shot timer** — countdown per turn, auto-fires at current aim on
   expiry. ✅ **Done** (shot timer only — ceiling descent from the same doc
   is deferred to Milestone 7). `ShotTimer` is a pure C# countdown class
   (`Assets/Scripts/Gameplay/`), unit-tested like `FloodFill`/`MatchResolver`.
   `GameStateManager` (the "referee" slot reserved in
   `architecture/overview.md`) ticks it and calls `shooterController.Fire()`
   on expiry — a small `ShooterController` refactor that extracts the
   `OnFireRequested` invoke into a public `Fire()` method, callable by both
   a manual press and the auto-fire path. `GameStateManager` resets the
   timer only in response to `OnFireRequested`, so manual and auto fire
   share one reset path instead of two that could drift apart — this
   replaces the originally-sketched `OnShotTimerExpired()` event, which
   turned out unnecessary. Duration is 8s. `ShotTimerDisplay` shows a
   numeric countdown ("4"→"1"), hidden until `ShotTimeRemaining <= 4f`,
   built at runtime and anchored off the fire zone the same way
   `FiredBubbleController`'s next-bubble indicator is.
   → [`shot-timer-and-ceiling-descent.md`](features/core-gameplay/shot-timer-and-ceiling-descent.md)
7. **Ceiling descent timer** — pushes a new row down at a fixed interval;
   interval shortens with difficulty. ✅ **Done** (fixed interval only —
   difficulty scaling is deferred to Milestone 9's `LevelGenerator`).
   `GridModel.PushRowsDown` shifts row contents down in place and reports
   whether the shooter's line (`Rows - 1`) was occupied before the shift;
   `GameBoard.PushRowDown()` calls it, refills row 0 with random bubbles,
   and raises `OnRowPushedDown(bool wasLastRowOccupied)` — the single hook
   Milestone 8's loss check will consume. `GameStateManager` reuses
   `ShotTimer` for the countdown (resetting it itself on expiry, rather
   than a separate self-resetting timer class) and currently only logs
   `wasLastRowOccupied`; no game-over flow yet. `GridDebugRenderer` reacts
   by destroying and rebuilding all its sprites from `GameBoard.Grid`
   rather than re-keying incrementally, since it's still the disposable
   Milestone-1 stand-in.
   → [`shot-timer-and-ceiling-descent.md`](features/core-gameplay/shot-timer-and-ceiling-descent.md)
8. **Win/loss conditions** — board cleared = win, ceiling reaches the
   shooter line = loss. ✅ **Done.** `GridModel.IsEmpty` (unit-tested) added
   as the pure check the win condition needed. `GameStateManager` now
   subscribes to `GameBoard.OnBubblesPopped`/`OnClusterDropped` and checks
   `IsEmpty` after each; `HandleRowPushedDown` acts on
   `wasLastRowOccupied` instead of only logging it. Both routes go through
   a shared `EndGame` helper that guards against double-firing, stops both
   timers (`Update` no-ops once `_isGameOver` is set), logs, and raises the
   new `OnLevelWon`/`OnLevelLost` events. No level-complete/game-over UI or
   "advance to next level" yet — those wait on Milestone 9's
   `LevelGenerator` and Milestone 10's HUD.
   → [`win-loss-conditions.md`](features/core-gameplay/win-loss-conditions.md)
9. **Procedural level generator** with difficulty knobs (color count,
   density, row count). ✅ **Done.**
   `LevelGenerator.Generate(GridModel, int levelNumber, DifficultyConfig)`
   (`Assets/Scripts/Grid/`) fills `grid.Rows - HeadroomRows` initial rows,
   gating each cell on a per-cell density roll and restricting color choice
   to `BubbleColorPalette.AllColors[0..ColorCount)`, seeded via
   `System.Random(levelNumber)` for determinism. `DifficultyCurveConfig` (a
   `ScriptableObject`, asset at
   `Assets/ScriptableObjects/DefaultDifficultyCurve.asset`) resolves a
   `DifficultyConfig` per level via simple linear ramps — explicitly a rough
   placeholder curve pending playtesting (see `level-generation.md`'s open
   questions). `GameBoard.Awake` now calls `LevelGenerator.Generate` instead of the old
   fixed `filledRows`/`FillWithRandomBubbles`, and owns `levelNumber` and
   `CurrentDifficulty`; `GameStateManager`'s ceiling-descent `ShotTimer` is
   now constructed in `Start()` (not `Awake()`, since it needs
   `GameBoard.CurrentDifficulty`, which Unity doesn't guarantee is set
   before `GameStateManager.Awake()` runs) from
   `CurrentDifficulty.CeilingDropIntervalSeconds` instead of a hardcoded
   `20f`. Ceiling-descent row refill (`GameBoard.RefillRow`) also now
   respects the level's color count.
   - **Anti-pre-pop constraint pass, corrected during TDD.** The original
     plan's "reroll up to 8 times" approach turned out unsound: a
     same-color neighbor can itself already belong to a larger connected
     group elsewhere on the board, so with a low color count (tested down
     to `ColorCount=2`) *every* available color can trigger an instant
     match at a given cell, not just some. Caught by an EditMode stress
     test generating 100 levels at `ColorCount=2`/`Density=1` and
     asserting no `MatchResolver.FindMatchGroup` result is non-empty.
     Fixed by exhaustively trying every color in `[0, ColorCount)`
     (bounded by `ColorCount` itself, not a fixed attempt cap) and, in the
     genuinely-unavoidable case, leaving the cell empty rather than
     keeping an instant-popping placement.
   → [`level-generation.md`](features/core-gameplay/level-generation.md)
10. **Minimal HUD** (score, shots fired, level indicator) + level-complete
    and game-over screens.
11. **First playable build on a physical device** — verify touch input
    feels right and performance is acceptable.

## Phase 2 — Superpowers system 🚧 (placeholder)

Not yet designed. The plan is to brainstorm this once Phase 1 is playable,
so the ability system can be designed against real match/pop events instead
of speculative ones.

Known constraints from the original idea (to be scoped properly in that
session): special bubbles carrying multiple abilities (freeze the screen,
blow up sections of bubbles, etc.); still open — how they're introduced
(spawn rate vs. player-chosen loadout vs. unlock progression), and how they
interact with the shot timer/ceiling descent.

See [`features/superpowers/overview.md`](features/superpowers/overview.md).

## Phase 3 — Local split-screen battle mode 🚧 (placeholder)

Not yet designed. Depends on Phase 1's grid/shooter/match systems being
solid enough to run as two simultaneous instances.

Known constraints from the original idea (to be confirmed/expanded in that
session): portrait orientation, screen split top/bottom, two independent
boards, goal is to clear the opponent's board — most likely via a
garbage-bubble mechanic where clearing bubbles sends rows to the opponent's
board, in the style of the arcade version's versus mode.

See [`features/battle-mode/overview.md`](features/battle-mode/overview.md).

## Later / not yet scoped ⏳

- Meta progression (level select map, currency, unlocks).
- Monetization (ads/IAP) — architecture implications deferred until scope
  is chosen.
- Real art pass — swap placeholder assets for final art.
