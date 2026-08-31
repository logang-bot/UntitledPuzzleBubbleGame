# Hex Grid

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
  where odd/even rows are visually shifted but stored as plain integer
  columns (avoids needing full axial/cube hex-coordinate math for a
  shooter-style grid, which — unlike a board game — never needs diagonal
  movement, only "which cells are adjacent").
- Each cell holds: `bool IsOccupied`, `BubbleColor Color` (enum).
- `GetNeighbors(row, col)` returns the up-to-6 adjacent cells, accounting
  for the row's offset direction (even rows and odd rows compute their
  neighbor offsets differently — this is the one place hex-grid math
  actually shows up).
- World-space position of a cell = grid origin + `col * bubbleWidth +
  (row % 2 == 0 ? 0 : bubbleWidth / 2)` horizontally, `row * rowHeight`
  vertically (`rowHeight` is slightly less than `bubbleWidth` for hex
  packing — roughly `bubbleWidth * 0.866` (sin 60°)).
- Rendering is a separate component that listens for grid changes and
  instantiates/pools bubble sprites at each occupied cell's world position —
  `GridModel` itself has no Unity dependencies, so it can be unit-tested
  without a scene.

## Open questions / tuning knobs

- Exact grid width (columns per row) for the target phone aspect ratio —
  needs a device/resolution decision, tune once placeholder art is in.
- Whether the grid needs to support a "half bubble" edge case for the
  offset rows at the board's left/right boundary (classic games either wall
  it off or allow it — pick during implementation once it's visible).
