# Shooter & Trajectory

**Status: implemented (Milestone 2 scope only).** `TrajectoryPredictor`,
`BoardBounds`, `BoardBoundsCalculator`, `HoldInputZone`, and
`ShooterController` live at `Assets/Scripts/Shooter/`, with the pure-math
pieces covered by EditMode tests in `Assets/Tests/EditMode/`
(`BoardBoundsCalculatorTests.cs`, `TrajectoryPredictorStraightShotTests.cs`,
`TrajectoryPredictorWallBounceTests.cs`,
`TrajectoryPredictorTerminationTests.cs`). Actually firing/moving/snapping a
bubble is Milestone 3 and is **not** implemented yet — see "Implementation
sketch" below for the exact scope boundary.

## Decision

- Aiming follows the classic arcade Puzzle Bobble/Bust-a-Move scheme, not
  free drag-to-angle: the gun rotates left or right at a **fixed angular
  speed** while an on-screen rotate-left/rotate-right zone is held, and
  stops at the current angle on release. A third dedicated fire zone fires
  the current aim (press, not hold). All three use the same hold-tracking
  input component, which works identically for mouse (Editor) and touch
  (device).
- The trajectory preview line is **always visible** while it's the player's
  turn (not only while actively rotating) — this matters because of the
  shot timer (see `shot-timer-and-ceiling-descent.md`): if the timer expires
  and the game auto-fires, the player must have already been able to see
  where that shot would go.
- The path is computed with **kinematic simulation**, not Rigidbody2D
  physics — see `architecture/overview.md` for why. The exact same
  simulation function produces both the preview line points and the actual
  fired bubble's motion path, so they can never disagree.

## Implementation sketch

- `TrajectoryPredictor.Simulate(Vector2 origin, float angleDegrees, int maxBounces)`
  → returns a list of `Vector2` points: straight segments from the origin,
  reflecting the direction vector off the left/right board walls (standard
  "angle of incidence = angle of reflection" — negate the x-component of
  the direction on wall contact) until it hits the ceiling or `maxBounces`
  is exceeded (safety cap). **Milestone 2 scope note:** it does not
  terminate on an occupied grid cell yet — there's no fired bubble to
  collide with anything until Milestone 3, and the only `GridModel`
  instance is private inside the temporary `GridDebugRenderer` (see
  `hex-grid.md`). Wall/ceiling bounds instead come from `ShooterController`'s
  own `cols`/`cellWidth` fields via `BoardBoundsCalculator`, computed
  independently of (but redundantly with) `GridDebugRenderer`'s. Flagged as
  tech debt to resolve once Milestone 3 needs real occupied-cell awareness
  for snapping — see `docs/ROADMAP.md`.
- `ShooterController` renders this as a `LineRenderer`, redrawn every frame.
  Aiming input comes from three on-screen `HoldInputZone` components
  (rotate-left, rotate-right, fire) rather than raw touch/drag — see
  "Decision" above.
- Firing is not implemented in Milestone 2. `ShooterController` raises
  `OnFireRequested(Vector2 origin, float angleDegrees)` on a fire-zone press
  with no subscriber yet — this event is the intended Milestone 3 hook-in
  point, not a stub to fill in blindly. Milestone 3's job: subscribe to it,
  move a bubble instance along `TrajectoryPredictor.Simulate`'s point list
  (either via `Vector2.MoveTowards`/`Lerp` segment-by-segment over time, or
  instantly for a snappier feel — worth trying both once it's playable),
  and on reaching the end of the path, snap to the **nearest empty adjacent
  grid cell** — not the exact contact point — so it always lands cleanly on
  the hex grid. This is the handoff point into `matching-and-popping.md`.

## Open questions / tuning knobs

- Min/max aim angle and rotation speed — implemented as serialized fields on
  `ShooterController` (`maxAimAngleDegrees = 60`, `rotateSpeedDegreesPerSecond
  = 90`), but the actual values are still a feel decision to tune once
  playable, not final.
- Bubble travel speed once fired (instant vs. animated) — a feel decision,
  test both.
- Whether to cap preview line length/bounce count for visual clarity on a
  small phone screen (a preview with many bounces can get cluttered).
