# Shot Timer & Ceiling Descent

These are two **independent** timers, both owned by `GameStateManager`.
They were initially discussed as one mechanic during brainstorming but are
deliberately separate systems with different triggers.

## Shot timer ✅ Implemented (Milestone 6)

- Each turn, an 8-second countdown runs. Rather than a depleting gauge
  visible the whole time, a numeric countdown ("4", "3", "2", "1") only
  appears in the final 4 seconds — enough warning without cluttering the
  screen during normal aiming.
- If the player fires before it expires, the timer resets for the next
  turn as normal.
- If it reaches zero, the game **auto-fires** at whatever aim angle the
  player currently has set, then the timer resets.
- Because of this, the trajectory preview line must always be visible
  during the player's turn (see `shooter-and-trajectory.md`) — an auto-fire
  should never show the player a shot they hadn't already seen coming.
- Implementation deviates from the sketch below in one way: there's no
  `OnShotTimerExpired()` event. Both manual and auto fire now go through
  a single `ShooterController.Fire()` method that raises the existing
  `OnFireRequested` — `GameStateManager` resets the timer only from that
  event, so there's one reset path instead of two that could drift apart.
  See `Assets/Scripts/Gameplay/ShotTimer.cs`, `GameStateManager.cs`, and
  `ShotTimerDisplay.cs`.

## Ceiling descent ✅ Implemented (Milestone 7), reworked into a warning-gated push

- Independent of the shot timer and of how many shots have been taken.
- On a fixed interval, the entire board pushes down one row — a flat "wall"
  advance, not a row addition. `GameBoard.PushRowDown()` now only calls
  `Grid.PushRowsDown` and raises `OnRowPushedDown`; it no longer refills row
  0. **Bug found and fixed**: the original implementation called
  `RefillRow(0)` after every shift, injecting a full row of brand-new random
  bubbles on every push. Besides visibly "adding bubbles" the player never
  placed, this overwrote the level's curated `LevelGenerator` pattern within
  a few pushes, reading as the whole board "shuffling." Removing the refill
  fixes both — row 0 just stays empty after the shift, and the original
  pattern (position and color) survives untouched, only moving down.
  **Second bug found and fixed**: even with the refill removed, bubbles
  still visibly jogged sideways on every push — a hex row-parity bug in
  `GridModel`, not this mechanic itself. See `hex-grid.md`'s "row parity
  flipping on every ceiling-descent push" section for the fix
  (`GridModel.IsShiftedRow`/`_rowsPushed`).
  **Third bug found and fixed**: bubbles moved down correctly, but the wall
  itself never did — `CeilingRenderer`'s band and `GameBoard.Bounds.CeilingY`
  (the world-Y boundary a shot's raw trajectory stops at) were both computed
  once and never touched again, so they stayed pinned at the level's
  starting position forever. Visually this meant the ceiling band never
  covered the space the wall had supposedly advanced into (plain background
  showing where there should have been solid wall), and an unobstructed shot
  would sail all the way up to that stale boundary and land at literal array
  row 0 — which is *behind* the wall by then, well above the actual receding
  bubbles, with a growing gap between them. Fixed by having `GameBoard`
  recompute `Bounds` after every push (and level load) using
  `ceilingHeight + Grid.RowsPushed * HexGridMath.RowHeight(cellWidth)` as the
  effective reserved height, and `CeilingRenderer` grow its band by the same
  amount on `OnRowPushedDown`/`OnLevelLoaded` — its *top* edge is the
  screen's fixed physical edge and never moves, its *bottom* edge is what
  advances, landing exactly on `GridModel.RowsPushed`'s top edge (see
  `hex-grid.md`). `BubbleLandingResolver`'s unobstructed-shot fallback now
  targets row `RowsPushed` instead of hardcoded row 0, and its general
  neighbor search excludes any candidate with `row < RowsPushed` (rows
  behind the wall are permanently vacated — see `GridModel.RowsPushed` —
  and could otherwise still be picked by raw distance alone). One more
  wrinkle: `TrajectoryPredictor` takes a *snapshot* of `BoardBounds` in its
  constructor, so `ShooterController` and `FiredBubbleController` (which
  each built one once in `Start()`) now rebuild it whenever
  `OnRowPushedDown`/`OnLevelLoaded` fires, or they'd keep simulating shots
  against the stale, pre-push boundary even after `GameBoard.Bounds` itself
  was correctly updated.
- The interval **shortens at higher levels/difficulty** — early levels give
  more breathing room, later levels apply more pressure. ✅ **Tied to
  difficulty as of Milestone 9**: `GameStateManager` now constructs the
  ceiling `ShotTimer` in `Start()` (not `Awake()`, since it needs
  `GameBoard.CurrentDifficulty`, only set once `GameBoard.Awake()` has run —
  Unity doesn't guarantee `Awake` ordering across scripts, only that all
  `Awake`s finish before any `Start`) from
  `gameBoard.CurrentDifficulty.CeilingDropIntervalSeconds`, replacing the
  old hardcoded `[SerializeField] ceilingDropIntervalSeconds` field. See
  `level-generation.md` for the `DifficultyCurveConfig` curve that produces
  this value (still a rough placeholder linear ramp, level 1 = 20s to match
  the old default, floor of 8s).
- This is the primary way a level can be lost (see
  `win-loss-conditions.md`): if the pushed-down board reaches the shooter's
  line, the game ends. `GameBoard.OnRowPushedDown(bool wasLastRowOccupied)`
  fires after every push, `wasLastRowOccupied` reporting whether the row
  about to be discarded (the shooter's line, `GridModel.Rows - 1`) held any
  bubbles right before the shift; `GameStateManager` ends the level on it.
- Implementation deviates from the sketch below in two ways: the push
  logic reuses `ShotTimer` for the countdown (no separate
  `CeilingDescentTimer` class), and there's no separate
  `OnShotTimerExpired()`-style wrapper event — the row-shift mutation and
  its event live on `GameBoard` (`GridModel.PushRowsDown` does the array
  shift, `GameBoard.PushRowDown()` calls it and raises `OnRowPushedDown`),
  consistent with the Milestone 4 precedent that grid-mutating operations
  are the single event source on `GameBoard`. See
  `Assets/Scripts/Grid/GridModel.cs`, `GameBoard.cs`,
  `Assets/Scripts/Gameplay/GameStateManager.cs`.

### Warning-gated trigger ✅ Done

Timer expiry no longer pushes immediately. `GameStateManager` now runs a
small `CeilingState { Countdown, Warning }` state machine:

- **Countdown**: the ceiling `ShotTimer` ticks normally. On expiry, instead
  of pushing, it switches to **Warning** and starts a camera shake
  (`CameraShake.StartShaking()`, `Assets/Scripts/Gameplay/CameraShake.cs`) —
  the earthquake feedback that a push is imminent. The timer isn't reset yet
  and stops ticking for the rest of the warning.
- **Warning**: the player keeps shooting normally. `GameStateManager`
  subscribes to `gameBoard.OnBubblePlaced` and, while in Warning, sets a
  `_landingOccurredDuringWarning` flag on the next placement — it does
  **not** push synchronously from inside that handler. `OnBubblePlaced` is
  dispatched synchronously alongside `MatchProcessor`'s own pop/drop
  handling in an order Unity doesn't guarantee across sibling components;
  mutating the grid mid-dispatch could step on `MatchProcessor`'s in-flight
  match computation. Instead, the flag is checked on the *next* `Update()`
  tick — by then all same-frame event dispatch for that placement has fully
  unwound — which stops the shake, pushes, resets the ceiling timer, and
  returns to Countdown.
- `CameraShake` shakes the camera, not `GameBoard.transform`: gameplay math
  (trajectory, occupancy collision, landing snap) is anchored to the
  board's transform, not the camera, so this keeps the shake purely visual
  with zero coupling to live shot-aiming. `GameStateManager.EndGame()` and
  `ResumeWithLevel()` both call a `StopCeilingWarning()` helper so an active
  shake never keeps running past game-over or into a freshly loaded level.

Verified in Play mode and via direct state-machine invocation: a push no
longer adds any bubbles or disturbs the existing pattern (confirmed by
diffing `GameBoard.Grid.OccupiedCells()` before/after — every cell just
moves down one row, same color, row 0 left empty); the state machine stays
in Warning across multiple ticks with no landing; a landing sets the flag
without an immediate push; and the actual push only happens on the
following tick. `CameraShake` was confirmed to displace the camera while
shaking and restore its exact base position on stop.

## Implementation sketch

- `GameStateManager` runs two independent `float` countdowns in `Update`
  (or via coroutines) — `shotTimeRemaining` and `ceilingDropTimeRemaining` —
  each reset to a configurable duration on trigger.
- `shotTimeRemaining` reset value is constant per level (not tied to
  difficulty in the current design — only the ceiling interval scales).
- `ceilingDropTimeRemaining` reset value comes from the level's difficulty
  config (see `level-generation.md`) — e.g. 10s at level 1, tightening
  toward some floor value (say 4-5s) at higher levels. Exact curve is a
  tuning knob, not a hard decision yet.
- Raises `OnShotTimerExpired()` and `OnRowPushedDown()` respectively for
  other systems (UI countdown display, board-push animation) to react to.

This discrete "always exactly one row" push is unchanged by and compatible
with the device-independent screen-fit scheme — see
`screen-fit-and-difficulty-scaling.md` for how row capacity and starting
row count are derived per device without touching the descent step itself.

## Open questions / tuning knobs

- Shot timer duration is 8s for now — may need retuning after more
  playtesting.
- Ceiling descent interval curve by level/difficulty is now implemented
  (`DifficultyCurveConfig`, Milestone 9 — see `level-generation.md`), but
  the actual start/floor values and how quickly it tightens are still an
  untuned placeholder linear ramp, not validated by playtesting.
- `CameraShake.amplitude` (currently `0.08`) is a feel value, not yet tuned
  against a real device/build.
- A warning that never resolves: if every shot after the warning starts
  misses the board entirely (`BubbleLandingResolver.ResolveLandingCell`
  returning `null` on a near-full board — a pre-existing open question in
  `firing-and-snapping.md`), `OnBubblePlaced` never fires and the shake
  keeps looping. This was already possible before the warning gate; it's
  just more visible now (an endless shake vs. a silent stuck state).
