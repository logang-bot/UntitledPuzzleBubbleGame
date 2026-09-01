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

- `GameStateManager` checks for the win condition after every pop/cluster
  drop resolves (subscribe to `OnBubblesPopped`/`OnClusterDropped` and check
  `GridModel.IsEmpty` after both have finished for that turn).
- Checks for the loss condition after every `OnRowPushedDown()` — compare
  the lowest occupied row's world-space Y position (or row index) against
  the shooter line's position.
- On win: raise `OnLevelWon()`, stop both timers, show level-complete UI,
  advance to the next level (re-run `LevelGenerator` for `levelNumber + 1`).
- On loss: raise `OnLevelLost()`, stop both timers, show game-over UI
  (retry same level / back to menu — exact UX TBD when the HUD/menu flow is
  built).

## Open questions / tuning knobs

- Whether there's a bubble/shot count limit as a *secondary* fail-safe
  (not the primary loss condition, just a guard against a pathological
  infinite level) — likely unnecessary given the ceiling always advances,
  revisit only if playtesting reveals a stalemate case.
- The "shooter's line" is row index `0` (the bottom-most row, per
  `GridDebugRenderer`'s row-to-world-position convention: row 0 sits at the
  bottom of the screen, row `PlayfieldRows - 1` near the ceiling) —
  consistent across every device by construction, since `PlayfieldRows` is
  derived from the device's screen fit rather than a fixed magic value.
  See `screen-fit-and-difficulty-scaling.md`.
