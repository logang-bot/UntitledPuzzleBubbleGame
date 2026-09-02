# Firing and Snapping

**Status: implemented.** `GameBoard`, `OccupancyCollision`,
`BubbleLandingResolver`, and `FiredBubbleController` live at
`Assets/Scripts/Grid/` and `Assets/Scripts/Shooter/`, with the pure-math
pieces covered by EditMode tests in `Assets/Tests/EditMode/`
(`OccupancyCollisionTests.cs`, `BubbleLandingResolverTests.cs`,
`GridModelOccupiedCellsTests.cs`, `GridModelDimensionsTests.cs`). This is
Milestone 3 from `docs/ROADMAP.md`.

## Decision

- A fired bubble travels **animated**, not instant — it moves along the
  trajectory's segments frame-by-frame (`Vector2.MoveTowards`), so wall
  bounces stay visible and it reads as a real shot.
- The preview line and the fired bubble share one occupancy-truncated path,
  computed by the same call on both sides — this preserves the project's
  core invariant that the preview must never disagree with where a shot
  actually lands (`architecture/overview.md`).
- On landing, the bubble snaps to the **nearest empty cell**, not the exact
  contact point — matching the classic Puzzle Bobble feel described in
  `hex-grid.md`.

## Why

Milestone 2 left `TrajectoryPredictor` aware only of walls and the ceiling;
it had no way to stop a shot early when it reaches an existing bubble. That
gap blocked Milestone 3 by design — see the tech-debt note in
`shooter-and-trajectory.md`. It also exposed that `ShooterController` and
`GridDebugRenderer` each privately computed their own board geometry
(cols/cellWidth/camera-fit) and only `GridDebugRenderer` ever held a real
`GridModel` — there was no single source of truth to check occupancy
against. Both problems needed solving together.

## Implementation sketch

- **`GameBoard`** (`Assets/Scripts/Grid/GameBoard.cs`) is now the single
  owner of the board's `GridModel`, camera-fit geometry, and `BoardBounds`.
  It performs the initial random fill (moved from `GridDebugRenderer`) and
  raises `OnBubblePlaced(row, col)` — the event named in
  `architecture/overview.md`'s event list — whenever `PlaceBubble` is
  called. `ShooterController` and `GridDebugRenderer` both take a
  `[SerializeField] GameBoard` reference instead of computing their own
  geometry.
- **`OccupancyCollision.Truncate(rawPoints, grid, cellWidth)`**
  (`Assets/Scripts/Shooter/OccupancyCollision.cs`) is a pure function that
  layers occupancy awareness on top of `TrajectoryPredictor.Simulate`'s
  output, rather than teaching `TrajectoryPredictor` itself about
  `GridModel` (keeps its existing wall/ceiling-only tests untouched). It
  walks the raw path's segments and finds the earliest point where the
  path comes within `cellWidth` of an occupied cell's center (exact
  ray/circle intersection, not a sampled approximation — two bubbles touch
  when their centers are `cellWidth` apart), truncating the path there and
  returning which cell was struck (or `null` if the path reaches its
  original wall/ceiling endpoint unobstructed).
  `ShooterController.DrawPreview` and `FiredBubbleController` both call
  this with the same inputs, so they mechanically cannot disagree. Note the
  truncated endpoint is the future bubble's *center* (exactly `cellWidth`
  from the struck cell's center) — correct for `FiredBubbleController`,
  whose flying bubble is a same-radius disc and so visually touches once
  centered there, but `DrawPreview`'s bare `LineRenderer` tip has no radius
  of its own and would stop a full bubble-radius short of the target's
  rendered edge. See `PreviewPointsCalculator.TrimToSurface`
  (`Assets/Scripts/Shooter/PreviewPointsCalculator.cs`) below.
  `GridModel.GetWorldPosition` is board-**local** (relative to `GameBoard`'s
  own transform, not Unity world space — see `hex-grid.md`), while
  trajectory points are true world space, so both `OccupancyCollision` and
  `BubbleLandingResolver` take a `(GridModel Grid, Vector2 Origin) board`
  tuple and add `Origin` (`gameBoard.transform.position`) to every cell
  position before comparing. Missing this was an actual bug hit during
  implementation — shots sailed straight past the real board and landed at
  row 0 — caught by testing in Play mode, not by the EditMode tests (which
  all defaulted to a zero origin). `OccupancyCollisionTests` and
  `BubbleLandingResolverTests` each have a dedicated non-zero-origin case
  to guard against regressing this.
- **`BubbleLandingResolver.ResolveLandingCell(grid, contactPoint, struckCell, cellWidth)`**
  (`Assets/Scripts/Grid/BubbleLandingResolver.cs`) picks the landing cell:
  the nearest unoccupied neighbor of any occupied cell within
  `cellWidth * 1.3` of the contact point (not just the struck cell — see bug
  note below), or — if the path was unobstructed and hit the ceiling instead
  — the nearest unoccupied cell in row `grid.RowsPushed` (the current
  effective ceiling row, *not* hardcoded row 0 — see `hex-grid.md`) by
  x-position. Every neighbor candidate is also filtered to `row >=
  grid.RowsPushed`, since rows behind the advanced wall are permanently
  vacated and could otherwise still be picked by raw distance alone.
  Returns `null` if no empty candidate exists (board nearly full); see
  "Open questions" below.

### Bug found and fixed: mis-snapping to the wrong pocket

Restricting landing candidates to only the struck cell's own neighbors (the
original design) meant a shot could visibly nestle into a pocket bounded
mostly by a *different* nearby bubble than the one `OccupancyCollision`
happened to register contact with first (by time along the trajectory) —
the resolver would then snap to the nearest cell adjacent to the wrong
bubble instead. Fixed by gathering every occupied cell within
`cellWidth * 1.3` of the contact point (not just the struck cell) and
considering all of their unoccupied neighbors together. `1.3` sits strictly
between the hex lattice's first-ring (`1.0x`) and second-ring (`~1.73x`)
distances, so it catches a second touching bubble without reaching a full
ring further out.

### Bug found and fixed: preview line stopping short of the bubble it's aiming at

- **`ShooterController.DrawPreview`** (`Assets/Scripts/Shooter/ShooterController.cs`)
  draws the occupancy-truncated path with a `LineRenderer`. Its raw endpoint
  is the struck cell's future center — visually a full bubble-radius short
  of the target's rendered edge, since the line has no radius of its own
  (unlike the flying bubble, a real disc). Fixed by
  **`PreviewPointsCalculator.TrimToSurface(truncatedPoints, targetCenter, cellWidth)`**
  (`Assets/Scripts/Shooter/PreviewPointsCalculator.cs`), a small pure
  function `DrawPreview` runs the truncated points through before handing
  them to the `LineRenderer`: when there's a struck cell, it moves the
  final point directly toward the struck cell's actual world-space center
  until it's exactly `cellWidth * 0.5` away, landing precisely on the
  bubble's surface. Purely cosmetic — `OccupancyCollision`,
  `BubbleLandingResolver`, and `FiredBubbleController` are untouched, so
  where a shot actually truncates/lands is unchanged.
  **Second bug found and fixed**: the first attempt extended the endpoint
  further along the *incoming segment's direction* instead of toward the
  actual center — correct only for a head-on shot, where the ray happens to
  pass through the target's center. For an angled/grazing hit (the ray
  merely grazes the `cellWidth`-radius circle around the center, not
  passing through it), extending along the ray direction over/undershoots
  the true surface point, so the gap reappeared at an angle. Fixed by
  computing the direction from the endpoint straight to the struck cell's
  known center (`GameBoard.Grid.GetWorldPosition(row, col) + Origin`) and
  moving along *that* instead — geometrically correct at any approach
  angle, since it no longer depends on the incoming ray at all.
- **`FiredBubbleController`** (`Assets/Scripts/Shooter/FiredBubbleController.cs`)
  subscribes to `ShooterController.OnFireRequested`. On fire it builds the
  truncated path, spawns a temporary flying-bubble `GameObject` (reusing
  `CircleSpriteFactory`/`BubbleColorPalette`), and moves it along the path
  each `Update`. On reaching the end it destroys the flying bubble and calls
  `BubbleLandingResolver` + `GameBoard.PlaceBubble` — the permanent rendered
  sprite then comes from `GridDebugRenderer` reacting to `OnBubblePlaced`,
  the same rendering path every other bubble on the board uses (including
  the initial fill).
- **Next-bubble indicator** (added while testing `matching-and-popping.md` —
  without it there was no way to plan a shot toward a match): the fired
  bubble's color is no longer randomized at the moment of firing. Instead
  `FiredBubbleController` pre-rolls the next shot's color and shows it as a
  small UI `Image` anchored to the left of the fire-zone square (same
  `RectTransform` anchors/pivot as the fire zone, offset by its half-width
  plus a margin — a UI element rather than a world-space sprite, since a
  Screen Space Overlay canvas always draws over world sprites regardless of
  sorting order, which hid an earlier world-space attempt). On fire, that
  pre-rolled color becomes `_color` and the indicator hides; on landing, a
  new color is rolled and the indicator reappears with it.

## Open questions / tuning knobs

- `FiredBubbleController.bubbleSpeed` is a feel value to tune once
  playable, not final.
- `BubbleLandingResolver` returning `null` (no empty cell found near a
  nearly-full board) is currently a silent no-op — the fired bubble is
  destroyed without being placed. Revisit once Milestone 8 (win/loss /
  board-full) is designed; a full board should probably end the game
  before this case is reachable.
