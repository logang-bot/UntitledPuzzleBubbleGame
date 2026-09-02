# HUD + Level-Complete/Game-Over Flow

## Decision

- **HUD placement**: a bottom bar grouped with the existing fire/rotate-zone
  controls, not overlaid on the board — the board fills the full screen
  width/height with no letterbox (`screen-fit-and-difficulty-scaling.md`),
  so a top or floating overlay would sit on top of ceiling bubbles.
- **Scoring**: no scoring system existed anywhere before this milestone.
  Built one now, weighted for cluster size and cascades rather than a flat
  per-bubble tally, in the spirit of classic Puzzle Bobble scoring —
  `ScoreCalculator.PointsForPop` grows quadratically with match size,
  `PointsForDrop` pays a flat higher per-bubble bonus for cascade drops
  (free chain-reaction points).
- **Level-complete flow**: explicit tap-to-continue button (`Continue`),
  not auto-advance.
- **Game-over flow**: `Retry` reloads the *same* level — `LevelGenerator`
  is seeded by `levelNumber` (`System.Random(levelNumber)`), so retrying
  reproduces the identical layout rather than resetting progression to
  level 1.
- Score **persists** across a level-complete → next-level transition (it's
  a running total for the play session) but is explicitly reset to 0 on
  retry, so the game-over screen can still show the failed run's final
  score before it clears.

## Implementation sketch

✅ **Done as of Milestone 10.**

- `GameBoard.LoadLevel(int newLevelNumber)` extracts the grid-generation
  work out of `Awake()` so it can be re-run for both retry
  (`LoadLevel(LevelNumber)`) and advance (`LoadLevel(LevelNumber + 1)`).
  Raises the new `OnLevelLoaded(int levelNumber)` event.
  `GridDebugRenderer` reacts the same way it already did for
  `OnRowPushedDown` — destroy all sprites, respawn from `GameBoard.Grid`
  (extracted into a shared `RebuildAll()` helper).
- `GameStateManager` gained `RetryLevel()`/`AdvanceToNextLevel()`, both
  routed through a private `ResumeWithLevel(int)` that calls
  `gameBoard.LoadLevel`, resets both timers (the ceiling timer is
  recreated from `gameBoard.CurrentDifficulty`, since difficulty can
  differ between levels), re-enables `ShooterController`, and clears
  `_isGameOver` — the one-way latch from Milestone 8 is now a real
  resume path. `RetryLevel()` additionally resets `ScoreTracker` and
  `ShotsFiredCounter` before resuming.
- `ScoreCalculator` (pure, unit-tested in `ScoreCalculatorTests`) holds the
  point formula; `ScoreTracker` (MonoBehaviour) subscribes to
  `GameBoard.OnBubblesPopped`/`OnClusterDropped` and accumulates.
- `ShotsFiredCounter` subscribes to `ShooterController.OnFireRequested`
  (fires once per shot attempt, manual or auto-fire-on-timeout — a true
  "shots fired" signal, not "shots landed").
- `HudDisplay` builds three runtime `Text` elements (score/shots/level) in
  a row anchored just above an existing bottom RectTransform (wired to the
  fire zone), following `ShotTimerDisplay`'s runtime-build/anchor recipe —
  legacy `UnityEngine.UI.Text`, no TMP, no prefab.
- `LevelResultScreen` is one component for both outcomes (win/lose panels
  are structurally identical — dim full-screen panel + message + one
  button), subscribing to `GameStateManager.OnLevelWon`/`OnLevelLost` and
  wiring its button to `AdvanceToNextLevel`/`RetryLevel`. Introduces the
  project's first `UnityEngine.UI.Button` (prior input was all
  `HoldInputZone`'s custom pointer handlers) — standard fit for a one-shot
  tap, and the scene already has an `EventSystem`.

## Open questions / tuning knobs

- `ScoreCalculator`'s point constants (`PopPointsPerBubble = 10`,
  `DropPointsPerBubble = 20`) are a first guess, not playtested — revisit
  once there's real play data on how big matches/cascades typically run.
- The HUD is plain text with no visual polish (no icons, no animation on
  change) — intentionally minimal per the roadmap's "Minimal HUD" framing;
  a real art/UX pass is out of scope until Phase 1's placeholder-art
  backlog is addressed.
- `LevelResultScreen`'s panel/button sizes are fixed pixel constants tuned
  by eye, not derived from screen size — fine for the current
  fixed-column/match-width camera fit, revisit if that changes.
