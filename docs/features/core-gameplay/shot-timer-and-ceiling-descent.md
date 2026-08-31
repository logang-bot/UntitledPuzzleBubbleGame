# Shot Timer & Ceiling Descent

These are two **independent** timers, both owned by `GameStateManager`.
They were initially discussed as one mechanic during brainstorming but are
deliberately separate systems with different triggers.

## Shot timer

- Each turn, a countdown starts (e.g. a depleting gauge shown in the UI).
- If the player fires before it expires, the timer resets for the next
  turn as normal.
- If it reaches zero, the game **auto-fires** at whatever aim angle the
  player currently has set, then the timer resets.
- Because of this, the trajectory preview line must always be visible
  during the player's turn (see `shooter-and-trajectory.md`) — an auto-fire
  should never show the player a shot they hadn't already seen coming.

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

## Open questions / tuning knobs

- Exact shot timer duration (e.g. 5-8 seconds is typical for the genre —
  needs playtesting once it's implemented).
- Ceiling descent interval curve by level/difficulty (start value, floor
  value, how quickly it tightens).
- Whether the ceiling descent timer should visually warn the player a
  couple seconds before dropping (common in the genre to reduce
  frustration) — worth adding once the core loop is playable.
