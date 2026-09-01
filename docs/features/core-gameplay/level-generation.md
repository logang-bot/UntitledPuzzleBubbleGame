# Level Generation

## Decision

Levels are **procedurally generated**, not hand-authored. A generator
algorithm produces a starting `GridModel` for a given level number, driven
by difficulty knobs. Hand-authored/milestone levels are an explicitly
possible future addition, not part of Phase 1 — don't build toward them yet.

## Why

Procedural generation gets a large number of playable levels quickly without
needing a level editor or manual design pass, which fits building Phase 1 as
a single solo developer newer to Unity. Hand-authoring can be revisited
later for special/milestone levels once the core loop is proven fun.

## Implementation sketch

- `LevelGenerator.Generate(int levelNumber, DifficultyConfig config)` →
  returns a populated `GridModel`.
- Difficulty knobs (likely a `ScriptableObject` per difficulty tier, or a
  curve keyed by level number):
  - **Color count** — how many distinct bubble colors are in play (fewer =
    easier to find matches).
  - **Density** — what fraction of the starting rows are filled vs. left
    empty gaps.
  - **Headroom rows** — empty rows between the initial board's bottom edge
    and the shooter's line, *not* a raw starting row count. The initial
    board is placed at the top (near the ceiling); headroom is the empty
    space below it that the player starts with. Actual
    starting row count is `PlayfieldRows - HeadroomRows(level)`, where
    `PlayfieldRows` comes from the device's screen fit (see
    `screen-fit-and-difficulty-scaling.md`). This keeps the number of
    ceiling-pushes before reaching the shooter's line consistent across
    every phone, regardless of how many rows actually fit on screen.
  - **Ceiling descent interval** (see `shot-timer-and-ceiling-descent.md`)
    — generated/looked-up alongside the board layout since it's also a
    per-level difficulty value.
- Generation approach: fill the starting rows cell-by-cell, picking a
  random color from the level's color count, with a light constraint pass
  to avoid generating a level that's already "pre-solved" (e.g. avoid
  accidentally creating large same-color runs of 3+ that would trivially
  auto-pop before the player even takes a shot) or that's unsolvable
  (guaranteeing at least the color count present is reachable/poppable is
  usually enough — full solvability proofs are out of scope for Phase 1).

## Open questions / tuning knobs

- The exact difficulty curve (how color count/density/row count scale
  across levels 1, 5, 10, 20...) — needs playtesting, start with a rough
  linear ramp and adjust.
- Whether to seed the RNG per level (so a given level number always
  generates the same board) — recommended for consistency/debugging, cheap
  to add (store the seed, not the generated grid).
