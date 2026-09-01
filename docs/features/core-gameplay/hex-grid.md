# Hex Grid

**Status: implemented.** `GridModel` lives at
`Assets/Scripts/Grid/GridModel.cs`, covered by EditMode tests in
`Assets/Tests/EditMode/GridModelOccupancyTests.cs` and
`GridModelNeighborsTests.cs`/`GridModelWorldPositionTests.cs`. The sketch
below now describes the actual shipped shape, not just a plan.

## Decision

Hex grid with alternating row offset (each bubble touches up to 6
neighbors), matching the classic Puzzle Bobble/Bust-a-Move look — not a
square grid. Rows alternate being shifted half a bubble-width left/right so
each bubble nestles between two bubbles in the row above/below.

## Why

This is the shape players expect from the genre, and it's what makes a shot
"snap" satisfyingly into a pocket between two existing bubbles rather than
just landing in a square cell. A square grid would be simpler math but
wouldn't look or feel like the source material.

## Implementation sketch

- `GridModel` stores cells keyed by **offset coordinates** `(row, col)`,
  backed by a `bool[,]` occupancy array and a `BubbleColor[,]` color array
  sized `rows x cols`. Odd/even rows are visually shifted but stored as
  plain integer columns (avoids needing full axial/cube hex-coordinate math
  for a shooter-style grid, which — unlike a board game — never needs
  diagonal movement, only "which cells are adjacent").
- `PlaceBubble(row, col, color)` / `ClearCell(row, col)` / `IsOccupied` /
  `GetColor` are the occupancy API.
- `GetNeighbors(row, col)` returns the up-to-6 adjacent cells as
  `List<(int Row, int Col)>`, clipped to grid bounds. Implemented as a
  static even-row/odd-row offset table (`EvenRowOffsets`/`OddRowOffsets`)
  plus a bounds check, rather than one long branching method — this is the
  one place hex-grid math actually shows up, and keeping it as small
  composed functions (candidate offsets → bounds filter) made it easy to
  unit-test each row parity and the corner-clipping case separately.
- `GetWorldPosition(row, col)` returns a `Vector2`: `x = col * cellWidth +
  (row % 2 == 0 ? 0 : cellWidth * 0.5f)`, `y = row * HexGridMath.RowHeight
  (cellWidth)` (row-height factor `0.8660254f` = sin 60°, for hex row
  packing — lives in `HexGridMath` so `PlayfieldSizer` can reuse the same
  constant, see `screen-fit-and-difficulty-scaling.md`). `cellWidth` is an
  optional constructor parameter (default `1`), so tests can use simple
  round numbers.
- Rendering is a separate component that listens for grid changes and
  instantiates/pools bubble sprites at each occupied cell's world position —
  `GridModel` itself has no Unity dependencies beyond `Vector2`, so it can
  be unit-tested without a scene. The current renderer
  (`GridDebugRenderer`, `BubbleColorPalette`, `CircleSpriteFactory`) is a
  temporary Milestone-1 stand-in that generates a plain circle sprite in
  code and spawns one `GameObject` per bubble directly — it proved the grid
  math visually, but isn't the pooled/event-driven rendering layer the rest
  of this doc describes; expect it to be replaced once real bubble art and
  the match/pop events exist.

## Open questions / tuning knobs

- Exact column count is still a "tune once placeholder art is in" value —
  but *how* it maps to a real device is now decided: column count is fixed
  and device-independent, matched to screen width via the camera, while
  row count is computed dynamically from screen height. See
  `screen-fit-and-difficulty-scaling.md`.
- Whether the grid needs to support a "half bubble" edge case for the
  offset rows at the board's left/right boundary (classic games either wall
  it off or allow it — pick during implementation once it's visible).
