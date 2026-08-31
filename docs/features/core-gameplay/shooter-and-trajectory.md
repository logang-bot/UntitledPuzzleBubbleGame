# Shooter & Trajectory

## Decision

- Aiming is touch-drag based: the player drags to set an aim angle from a
  fixed shooter position at the bottom of the board.
- The trajectory preview line is **always visible** while it's the player's
  turn (not only while actively dragging) — this matters because of the
  shot timer (see `shot-timer-and-ceiling-descent.md`): if the timer expires
  and the game auto-fires, the player must have already been able to see
  where that shot would go.
- The path is computed with **kinematic simulation**, not Rigidbody2D
  physics — see `architecture/overview.md` for why. The exact same
  simulation function produces both the preview line points and the actual
  fired bubble's motion path, so they can never disagree.

## Implementation sketch

- `TrajectoryPredictor.Simulate(Vector2 origin, float angle, int maxBounces)`
  → returns a list of `Vector2` points: straight segments from the origin,
  reflecting the direction vector off the left/right board walls (standard
  "angle of incidence = angle of reflection" — negate the x-component of
  the direction on wall contact) until it would hit an occupied grid cell or
  the ceiling, or `maxBounces` is exceeded (safety cap).
- `ShooterController` renders this as a `LineRenderer` (or dotted sprite
  trail) updated every frame the aim angle changes.
- Firing = disable the preview, move a bubble instance along the same point
  list (either via `Vector2.MoveTowards`/`Lerp` segment-by-segment over
  time, or instantly for a snappier feel — worth trying both once it's
  playable).
- On reaching the end of its path (touching an occupied cell or the
  ceiling), snap to the **nearest empty adjacent grid cell** — not the exact
  contact point — so it always lands cleanly on the hex grid. This is the
  handoff point into `matching-and-popping.md`.

## Open questions / tuning knobs

- Min/max aim angle (typically can't aim straight sideways — clamp to
  avoid a shot that only ever bounces forever or feels unfair).
- Bubble travel speed once fired (instant vs. animated) — a feel decision,
  test both.
- Whether to cap preview line length/bounce count for visual clarity on a
  small phone screen (a preview with many bounces can get cluttered).
