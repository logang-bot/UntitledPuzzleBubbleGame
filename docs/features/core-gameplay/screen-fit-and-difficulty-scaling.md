# Screen Fit & Device-Independent Difficulty Scaling

**Status: implemented.** `PlayfieldSizer` and `HexGridMath` live at
`Assets/Scripts/Grid/`, covered by EditMode tests in
`Assets/Tests/EditMode/PlayfieldSizerTests.cs`. `GameBoard` uses them to
size the camera and grid at runtime instead of a hardcoded 8x8 board — this
was `GridDebugRenderer`'s job in Milestone 1, moved to `GameBoard` in
Milestone 3 when a single shared board owner became necessary (see
`firing-and-snapping.md`).

## Decision

The board fills the full screen in both width and height on every phone —
no letterbox/pillarbox bars — while keeping the ceiling-descent mechanic
(the primary loss condition, see `shot-timer-and-ceiling-descent.md`)
equally fair regardless of device height.

- **Width**: a fixed column count, matched to screen width via the camera
  (orthographic "match width" sizing). The board always spans exactly full
  screen width on every phone; only the on-screen bubble size in pixels
  varies slightly per device, never the column count.
- **Height**: real board content fills the screen height too — no top/
  bottom padding. Taller phones simply start with more rows of bubbles
  visible, computed at runtime from the device's screen height
  (`PlayfieldRows`).
- **Fairness via headroom, not pixel-based descent**: `GridModel` is a
  discrete row/column grid, and ceiling descent (per
  `shot-timer-and-ceiling-descent.md`) is a discrete "push down exactly one
  row" operation — a continuous/fractional-pixel descent amount doesn't fit
  that model. Instead, the per-level difficulty knob in `LevelGenerator`
  (Milestone 9, not yet built) is defined as **headroom rows** — empty rows
  between the initial board's bottom edge (the initial board sits at the
  top, near the ceiling) and the shooter's line — rather than a raw
  starting row count:
  `InitialRowCount = PlayfieldRows - HeadroomRows(level)`.
  Since `HeadroomRows` is a pure per-level value independent of device, and
  the descent step itself is unchanged (always exactly 1 row), the number
  of ceiling-pushes before the board reaches the shooter's line is
  identical on every phone — a taller phone just starts with a taller
  initial board (more starting content), not more/less breathing room.

## Why

Filling the whole screen with real board content (rather than fixed-size
board + padding) makes better use of every device's screen without
disadvantaging or advantaging any particular aspect ratio. Reframing the
difficulty knob as headroom rows gets this without needing to change the
discrete, row-based descent/grid model at all — it only changes what number
`LevelGenerator` computes for the starting row count.

## Implementation sketch

- `HexGridMath.RowHeightFactor` (`0.8660254f`, sin 60°) — extracted from
  `GridModel` into a shared static class so both `GridModel` and
  `PlayfieldSizer` use the same row-height math without duplicating the
  constant.
- `PlayfieldSizer` — pure static math, no `Camera`/`Screen` calls inside
  the functions themselves (mirrors `GridModel`'s "no Unity dependencies
  beyond `Vector2`" approach, so it stays unit-testable):
  - `OrthographicSizeForWidth(boardWidth, screenWidth, screenHeight)` → the
    orthographic size that makes the camera's visible world width exactly
    equal `boardWidth`.
  - `RowsForWorldHeight(worldHeight, cellWidth)` → row count that fits
    (floored, since rows are discrete).
- `GameBoard` calls these from `Awake()`: sets `Camera.main
  .orthographicSize` from the fixed `cols`/`cellWidth` board width and the
  live `Screen.width`/`Screen.height`, then positions itself relative to
  the camera (horizontally centered, bottom row anchored near the bottom
  of the visible frustum) instead of relying on a hand-placed Transform in
  the scene — a hardcoded scene position goes stale the moment the
  computed orthographic size changes for a different aspect ratio (this
  bit us during implementation: the debug renderer initially rendered
  nothing because its old hand-placed position from the fixed-size-camera
  era put the board entirely above the new dynamic frustum).
  `filledRows` is clamped to the computed row count. Row 0 is the
  bottom-most row (the shooter's line); higher row indices are toward the
  ceiling. `FillWithRandomBubbles` fills the top `filledRows` rows (nearest
  the ceiling), leaving the bottom rows empty — matching the actual game's
  initial state (board near the ceiling, empty shooting lane below it)
  rather than filling from the shooter's line upward. `GridDebugRenderer`
  no longer owns any of this — it just renders whatever `GameBoard` holds
  (see `firing-and-snapping.md`).

## Tablets — explicitly out of scope for now

This project targets phones only. Tablets are far less elongated in
portrait (~4:3) than phones (~19.5:9-20:9), so under this scheme they'd
actually show *fewer* rows, not more — and naively reusing "match width"
on a physically much larger panel would render each bubble as a huge
circle. If tablet support is ever added, the expected fix is **more
columns at the same physical bubble size**, not the same column count
scaled up to a bigger bubble. Deferred; revisit only if tablets become a
real target.

## Open questions / tuning knobs

- Exact column count (`cols` on `GameBoard`) is still a "tune once real art
  exists" value, same as before — this doc only settles *how* width/height
  are matched to the device, not the final number. See `hex-grid.md`.
- Whether the board needs a top/bottom safety margin beyond a single
  bubble radius once the shooter and HUD exist (Milestones 2 and 10) — the
  current bottom-anchor offset is a first approximation, not tuned against
  real shooter/HUD dimensions yet.
