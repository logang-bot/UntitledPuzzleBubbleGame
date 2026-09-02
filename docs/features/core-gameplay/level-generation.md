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

## Implementation ✅ Done (Milestone 9)

`LevelGenerator.Generate(GridModel grid, int levelNumber, DifficultyConfig
config)` (`Assets/Scripts/Grid/LevelGenerator.cs`) — a static, pure-logic
class in the `FloodFill`/`MatchResolver` style — fills an already-sized
`GridModel` (constructed by `GameBoard.Awake`, which already knows the
device-fit row/col count and `cellWidth`) rather than building the grid
itself, so `LevelGenerator` stays free of rendering concerns. Seeded via
`new System.Random(levelNumber)` — the level number doubles directly as the
seed, resolving the open question below in favor of the simplest option (no
separate stored seed field).

`DifficultyCurveConfig` (`Assets/Scripts/Grid/DifficultyCurveConfig.cs`) is
the `ScriptableObject` per-difficulty-tier config the sketch below
anticipated, resolved via `ForLevel(int levelNumber) → DifficultyConfig`.
The shipped asset is `Assets/ScriptableObjects/DefaultDifficultyCurve.asset`,
assigned to `GameBoard`'s `difficultyCurve` field. Every ramp in it is a
straightforward linear curve — a deliberate placeholder per the open
question below, not yet tuned by playtesting.

`GameBoard.Awake` now calls `LevelGenerator.Generate` instead of the old
fixed `filledRows`/`FillWithRandomBubbles`, and exposes the resolved
`CurrentDifficulty` so `GameStateManager` can read
`CeilingDropIntervalSeconds` for the ceiling timer (see
`shot-timer-and-ceiling-descent.md`).

### Floating-cell cleanup ✅ Done

Because each cell's density roll is independent, `Generate` could produce
bubbles with no path back to row 0 — invisible at level start (they render
like any other bubble) but wrong per the game's own connectivity rule, and
never checked until some unrelated later pop happened to trigger a full-board
scan. `Generate` now finishes with `MatchResolver.FindFloatingCells` and
clears whatever it returns, so a level can never start with an already-
disconnected bubble. If row 0 itself happens to roll entirely empty (and the
level isn't genuinely meant to be empty — i.e. something *was* generated
below it), `RescueRowZeroIfOrphaned` force-places one bubble there first, via
the same anti-instant-match placement the rest of generation uses, so the
cleanup pass has something to anchor to instead of wiping the whole level.

### Level 1 override ✅ Done

Level 1 is otherwise just the difficulty curve's zero point — `(levelNumber
- 1) == 0` drops every ramp term, so it used the same `start*` fields levels
2+ ramp up from. `DifficultyCurveConfig` now short-circuits `ForLevel(1)` to
its own `level1Density`/`level1HeadroomRows` fields (sparser than
`startDensity`/`startHeadroomRows`), deliberately isolated from the ramp so
easing level 1 for testing doesn't also soften every later level's starting
point.

### Implementation sketch (original, for reference)

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

### Anti-pre-pop constraint: corrected during implementation

The constraint pass reuses `MatchResolver.FindMatchGroup` directly against
each newly-placed cell (rather than a hand-rolled same-color-neighbor count)
since a local neighbor-count heuristic is provably weaker — a connected
same-color run can form as a path where the new cell only directly touches
one prior neighbor, yet the full flood-filled group is still ≥3.

The first implementation attempt (reroll the color up to a fixed 8 times)
turned out unsound and was caught by an EditMode test generating 100 levels
at `ColorCount=2`/`Density=1` and asserting every occupied cell's match
group is empty: a same-color neighbor can itself already belong to a
*larger* connected group elsewhere on the board (not just be a lone cell),
so at low color counts **every** available color can trigger an instant
match at a given cell — not just some, as the original reasoning assumed.
Fixed by exhaustively trying every color in `[0, ColorCount)` (bounded by
`ColorCount` itself, not a fixed attempt cap) and, in the genuinely
unavoidable case, leaving that cell empty rather than keeping an
instant-popping placement — an acceptable outcome since density < 1 already
means gaps are expected.

## Open questions / tuning knobs — resolved

- The exact difficulty curve (how color count/density/headroom/ceiling
  interval scale across levels) started as a rough linear ramp
  (`DifficultyCurveConfig`'s Inspector-tunable fields) per the plan here —
  **still needs playtesting and adjustment**, not yet done.
- RNG seeding: resolved as `new System.Random(levelNumber)` — the level
  number itself is the seed, no separate seed field stored.

## Open questions

- `RescueRowZeroIfOrphaned` only guarantees row 0 isn't the reason a level
  gets wiped empty; it doesn't try to maximize how much of the rest of the
  board survives the floating-cell cleanup. Revisit if playtesting turns up
  levels that feel unexpectedly sparse.
