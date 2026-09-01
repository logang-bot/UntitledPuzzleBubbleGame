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

## Ceiling descent

- Independent of the shot timer and of how many shots have been taken.
- On a fixed interval, the entire board pushes down one row and a new row
  of bubbles is added at the top.
- The interval **shortens at higher levels/difficulty** — early levels give
  more breathing room, later levels apply more pressure.
- This is the primary way a level can be lost (see
  `win-loss-conditions.md`): if the pushed-down board reaches the shooter's
  line, the game ends.

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
