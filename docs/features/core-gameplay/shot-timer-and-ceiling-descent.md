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

## Ceiling descent ✅ Implemented (Milestone 7)

- Independent of the shot timer and of how many shots have been taken.
- On a fixed interval, the entire board pushes down one row and a new row
  of bubbles is added at the top.
- The interval **shortens at higher levels/difficulty** — early levels give
  more breathing room, later levels apply more pressure. Not tied to a
  difficulty config yet (Milestone 9's `LevelGenerator` doesn't exist), so
  for now it's a single `[SerializeField] ceilingDropIntervalSeconds` on
  `GameStateManager` (default 20s) — a tuning knob, not a curve.
- This is the primary way a level can be lost (see
  `win-loss-conditions.md`): if the pushed-down board reaches the shooter's
  line, the game ends. The actual loss/game-over flow is Milestone 8 — not
  built yet. This milestone only produces the signal that flow will need:
  `GameBoard.OnRowPushedDown(bool wasLastRowOccupied)` fires after every
  push, `wasLastRowOccupied` reporting whether the row about to be
  discarded (the shooter's line, `GridModel.Rows - 1`) held any bubbles
  right before the shift. `GameStateManager` currently just logs it.
- Implementation deviates from the sketch below in two ways: the push
  logic reuses `ShotTimer` for the countdown (no separate
  `CeilingDescentTimer` class — `GameStateManager` calls `Reset()` itself
  right after `Tick()` reports expiry, rather than `ShotTimer`
  self-resetting), and there's no separate `OnShotTimerExpired()`-style
  wrapper event — the row-shift mutation and its event live on
  `GameBoard` (`GridModel.PushRowsDown` does the array shift,
  `GameBoard.PushRowDown()` calls it, refills row 0, and raises
  `OnRowPushedDown`), consistent with the Milestone 4 precedent that
  grid-mutating operations are the single event source on `GameBoard`.
  See `Assets/Scripts/Grid/GridModel.cs`, `GameBoard.cs`,
  `Assets/Scripts/Gameplay/GameStateManager.cs`.
- Verified in Play mode: board grows downward one row per interval, each
  push visibly shifts every existing bubble down (including the hex
  half-cell x-offset that comes with the row-parity change) and adds a
  fresh random row at the ceiling; `GridDebugRenderer` handles this by
  destroying and rebuilding all tracked sprites from `GameBoard.Grid` on
  `OnRowPushedDown` rather than incrementally re-keying, since it's an
  explicitly disposable Milestone-1 stand-in. Ran the board to full
  saturation (136/136 cells on a 17x8 grid) with no index errors, and
  confirmed `OnRowPushedDown` reports `wasLastRowOccupied = true`
  repeatedly once the bottom row started staying occupied.

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
- Ceiling descent interval curve by level/difficulty (start value, floor
  value, how quickly it tightens).
- Whether the ceiling descent timer should visually warn the player a
  couple seconds before dropping (common in the genre to reduce
  frustration) — worth adding once the core loop is playable.
