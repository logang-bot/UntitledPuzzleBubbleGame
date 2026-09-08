# Firing and Snapping

**Status: implemented.** `GameBoard`, `OccupancyCollision`,
`BubbleLandingResolver`, `FiredBubbleController`, and (added later, see
"Redesign" below) `LandingIndicator` live at `Assets/Scripts/Grid/` and
`Assets/Scripts/Shooter/`, with the pure-math pieces covered by EditMode
tests in `Assets/Tests/EditMode/` (`OccupancyCollisionTests.cs`,
`BubbleLandingResolverTests.cs`, `GridModelOccupiedCellsTests.cs`,
`GridModelDimensionsTests.cs`). `LandingIndicator` itself is thin Unity
rendering glue (a `SpriteRenderer` wrapper), consistent with
`GridDebugRenderer`/`CeilingRenderer` being untested today, so it has no
dedicated test file. This is Milestone 3 from `docs/ROADMAP.md`.

## Decision

- A fired bubble travels **animated**, not instant — it moves along the
  trajectory's segments frame-by-frame (`Vector2.MoveTowards`), so wall
  bounces stay visible and it reads as a real shot.
- The **landing indicator** (not the trajectory line — see "Redesign" below)
  and the fired bubble share one occupancy-truncated path, computed by the
  same call on both sides — this preserves the project's core invariant that
  the shown landing cell must never disagree with where a shot actually
  lands (`architecture/overview.md`). The trajectory line itself is
  deliberately *not* occupancy-truncated, so it stays a pure, zero-lag
  function of the aim angle; see "Redesign" below for why.
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
  `ShooterController.UpdateLandingIndicator` and `FiredBubbleController` both
  call this with the same inputs, so the shown landing cell and where a shot
  actually lands mechanically cannot disagree. The trajectory *line* itself
  no longer calls this at all - see "Redesign: decoupling the preview line
  from occupancy truncation" below. Note the
  truncated endpoint is the future bubble's *center* (exactly `cellWidth`
  from the struck cell's center) — correct for `FiredBubbleController`,
  whose flying bubble is a same-radius disc and so visually touches once
  centered there, and used identically by `LandingIndicator` to place the
  ghost bubble at the resolved landing cell's own center (not this raw
  contact point).
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

### Bug found and fixed (twice), then redesigned: preview line stopping short / feeling laggy near bubbles

First attempt: `ShooterController.DrawPreview` drew the occupancy-truncated
path with a `LineRenderer`. Its raw endpoint was the struck cell's future
center — visually a full bubble-radius short of the target's rendered edge,
since the line has no radius of its own (unlike the flying bubble, a real
disc). Fixed by `PreviewPointsCalculator.TrimToSurface`, a small pure
function that moved the truncated endpoint straight toward the struck
cell's actual center until exactly `cellWidth * 0.5` away, landing on the
bubble's rendered surface (a second bug in that first attempt — extending
along the incoming ray direction instead of straight at the center — was
also found and fixed; it undershot/overshot at any angled, non-head-on hit).

That approach was **fully superseded** after real-device and in-Editor
testing found the truncated-and-trimmed line's *speed* visibly kinked near
packed ceiling bubbles (looked/felt laggy) even though the aim angle itself
was changing at a perfectly constant rate. Root cause: `OccupancyCollision`'s
"nearest struck circle" selection is provably continuous in *position* at
the exact angle where the ray hands off from one touching bubble to its
neighbor (both candidate circles' contact points coincide there), but its
*velocity* has a kink at that same point — a real geometric property of
following the envelope of tangent circles, not a bug in the truncation math.
A chase-based smoother (tried first) could only trade added lag for
smoothness against a target whose speed is genuinely kinked; it didn't
remove the kink.

**Current design:** the trajectory line no longer calls `OccupancyCollision`
at all. `ShooterController.DrawPreview` renders `TrajectoryPredictor.Simulate`'s
raw, occupancy-unaware output directly — a pure, continuous, zero-lag
function of `_aimAngleDegrees`, with no kink possible by construction. The
line's `LineRenderer.sortingOrder` is set to `-1` (same convention as
`CeilingRenderer`, behind bubbles' implicit `sortingOrder = 0`), so opaque
bubble sprites visually occlude it wherever a shot would actually stop —
landing at essentially the same "touches the bubble's rendered edge" look
`TrimToSurface` used to compute explicitly, but via rendering instead of
geometry, so there's nothing to trim or smooth. The discrete "which cell
would this land in" information moved to a new **`LandingIndicator`**
(`Assets/Scripts/Shooter/LandingIndicator.cs`) — a small ghost-bubble
`SpriteRenderer` positioned every frame from `OccupancyCollision.Truncate` +
`BubbleLandingResolver.ResolveLandingCell` (identical to what
`FiredBubbleController.Land` uses, so it can't disagree with a real shot).
It's allowed to pop discretely between candidate cells with no smoothing —
a snapping discrete marker reads as normal; a continuously-tracked line
whose speed visibly changes does not.
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
