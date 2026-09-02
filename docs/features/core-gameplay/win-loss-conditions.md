# Win / Loss Conditions

## Decision

- **Win**: the level ends in victory when the board is fully cleared (no
  occupied cells remain in `GridModel`).
- **Loss**: the level ends in defeat when the descending ceiling (see
  `shot-timer-and-ceiling-descent.md`) pushes the board down far enough
  that occupied cells reach the shooter's line.

No score threshold or shot-budget win condition — this was considered and
explicitly rejected in favor of the simpler, more classic "clear the board"
condition, which pairs naturally with the ceiling-descent pressure.

## Implementation sketch

✅ **Done as of Milestone 8** (win/loss detection and stopping; UI and
level-advance are still open, see below).

- `GridModel.IsEmpty` (unit-tested) is the pure check the win condition
  needed — true when `OccupiedCells()` is empty.
- `GameStateManager` checks the win condition after every pop/cluster drop
  resolves — it subscribes to `GameBoard.OnBubblesPopped`/`OnClusterDropped`
  and checks `gameBoard.Grid.IsEmpty` after each.
- Checks the loss condition on every `OnRowPushedDown(bool
  wasLastRowOccupied)` (✅ implemented as of Milestone 7, see
  `shot-timer-and-ceiling-descent.md`) — the event's payload already *is*
  the loss signal (whether the shooter's line held any bubbles right
  before the shift discarded them), computed during the row shift in
  `GridModel.PushRowsDown`. `HandleRowPushedDown` now acts on
  `wasLastRowOccupied` instead of only logging it.
- Both routes go through a shared `EndGame(raiseEvent, logMessage)` helper:
  guards against double-firing via an `_isGameOver` flag (also checked at
  the top of `Update`, so both timers stop ticking — no separate "stop
  timer" call needed on `ShotTimer` itself), disables `ShooterController`
  (`shooterController.enabled = false`) so the player can't keep
  aiming/firing after the game has ended, logs, then raises
  `OnLevelWon`/`OnLevelLost`.
- **Not yet built**: level-complete/game-over UI, and "advance to the next
  level" — both wait on Milestone 9's `LevelGenerator` and Milestone 10's
  HUD, since there's no next level to generate yet and no UI layer to show
  a screen on.

## Open questions / tuning knobs

- Whether there's a bubble/shot count limit as a *secondary* fail-safe
  (not the primary loss condition, just a guard against a pathological
  infinite level) — likely unnecessary given the ceiling always advances,
  revisit only if playtesting reveals a stalemate case.
- The "shooter's line" is row index `PlayfieldRows - 1` (the bottom-most
  row, per `GameBoard`/`GridModel`'s row-to-world-position convention: row 0
  is the ceiling, increasing row moves down toward the shooter — see
  `hex-grid.md`) — consistent across every device by construction, since
  `PlayfieldRows` is derived from the device's screen fit rather than a
  fixed magic value. See `screen-fit-and-difficulty-scaling.md`.
