# Matching & Popping

> **Updated by the game screen art (2026-11-12):** the renderer named below
> (`GridDebugRenderer`) is now `BoardView`/`BubbleLayer`: pops burst in
> breadth-first order with the baked pop frames (`PopSchedule`), and
> dropped bubbles get a scared face and a toss (`FallingBubble`). The
> matching logic is unchanged. See `../art/overview.md` ("Game screen: as
> built").

**Status: implemented.** Covers Milestones 4 and 5 together — built in one
pass, since both center on the same flood-fill traversal. `FloodFill` and
`MatchResolver` live at `Assets/Scripts/Grid/`, fully unit-tested
(`Assets/Tests/EditMode/FloodFillTests.cs`,
`MatchResolverMatchGroupTests.cs`, `MatchResolverFloatingCellsTests.cs`).

## Decision

Standard Puzzle Bobble matching: when a fired bubble snaps into a grid cell,
if it connects **3 or more** same-color bubbles (including itself) via
adjacency, they all pop. After a pop, any bubbles left disconnected from the
ceiling (not reachable by any path of adjacent occupied cells back to the
top row) fall and are also cleared.

## Implementation sketch

- **`FloodFill.Run(grid, seeds, include)`** (`Assets/Scripts/Grid/FloodFill.cs`)
  is the one generic BFS helper the original sketch called for — pure, static,
  no Unity dependency. It starts from any number of seed cells and expands
  through `GridModel.GetNeighbors` only into cells satisfying `include`, so
  the same function serves both flood-fills below with different predicates.
- **`MatchResolver`** (`Assets/Scripts/Grid/MatchResolver.cs`) is a static,
  pure query class (mirrors `BubbleLandingResolver`'s style — no grid
  mutation, just answers):
  - `FindMatchGroup(grid, placedCell)`: flood-fills from `placedCell` through
    same-color occupied neighbors; returns the group if `Count >= 3`, else an
    empty set.
  - `FindFloatingCells(grid)`: flood-fills from every occupied cell in the
    **wall row, `grid.RowsPushed`**: the row actually touching the
    advanced wall. That is neither a hardcoded row 0 nor "whatever row is
    topmost" (see the second and third bugs below). It floods through any
    occupied neighbor and returns every occupied cell *not* in that
    reachable set.
- **`GameBoard`** owns the actual mutation and events, mirroring its existing
  `PlaceBubble`/`OnBubblePlaced` pattern: `PopCells(cells, color)` and
  `DropCells(cells)` each clear the given cells via `Grid.ClearCell` and
  raise `OnBubblesPopped`/`OnClusterDropped`. Kept on `GameBoard` rather than
  a `MatchResolver` MonoBehaviour so it stays the single event source for
  every grid-state change.
- **`MatchProcessor`** (`Assets/Scripts/Grid/MatchProcessor.cs`) is the thin
  MonoBehaviour hook-in: it subscribes to `GameBoard.OnBubblePlaced`, calls
  `MatchResolver.FindMatchGroup`, and if non-empty, calls `GameBoard.PopCells`
  followed by `GameBoard.DropFloatingCells()`. That is the
  `FindFloatingCells`/`DropCells` pair, moved onto `GameBoard` so superpower
  pops share it (see the third bug below). Listening to the
  general placement event (rather than wiring straight into
  `FiredBubbleController.Land()`) means any future placement source
  (superpower effect, garbage row) gets matching for free. This is safe
  because `GameBoard.Awake`'s initial random fill calls `Grid.PlaceBubble`
  directly, bypassing the event, so the debug board never auto-pops on load.
- **`GridDebugRenderer`** now tracks its spawned sprites in a
  `Dictionary<(int Row, int Col), GameObject>` and reacts to both events: a
  pop destroys the sprite instantly; a drop instead adds a `FallingBubble`
  component (`Assets/Scripts/Grid/FallingBubble.cs` — simple constant-gravity
  fall, self-destroys after a short duration) so a disconnected bubble
  visibly falls rather than vanishing like a match.

**Bug found and fixed while building this**: `firing-and-snapping.md` and
`BubbleLandingResolver` always assumed row 0 is the ceiling, but
`GameBoard`'s positioning/fill code had it backwards (row 0 rendered at the
bottom, always empty). Harmless before now, but it would have made
`FindFloatingCells`'s ceiling-row seed set almost always empty, dropping the
entire board on every pop. Fixed at the source — see `hex-grid.md` and the
Milestone 4/5 write-up in `docs/ROADMAP.md` for the full account.

**Second bug found and fixed, later**: once the ceiling-descent push
(`shot-timer-and-ceiling-descent.md`) stopped refilling row 0 on every
advance, row 0 started legitimately sitting empty for stretches of real
play — not a bug in itself, just the wall having pushed past it with no new
bubble landed there yet. But `FindFloatingCells` still hardcoded its seed
set to `cell.Row == 0`, so the instant row 0 was empty, *every* remaining
bubble on the board — including ones with a perfectly good connection to
each other, "above" whatever had just been popped — was flagged as floating
and dropped on the next pop. Reported as "even bubbles above the matched
pattern are falling." Fixed by seeding from the **topmost occupied row**
instead of a hardcoded 0: nothing can be floating in the gap between the
wall and that row, since by definition nothing occupies it (if something
did, it would be the topmost row instead). For a freshly generated level
this is always row 0 (`LevelGenerator`'s `RescueRowZeroIfOrphaned` guarantees
that), so behavior is unchanged until a push actually empties it.
`MatchResolverFloatingCellsTests.cs` has dedicated cases for a row-0-empty
board (topmost row anchors correctly) and for a genuinely disconnected
island below that row (still correctly dropped) to guard against
regressing either direction. The third bug below superseded this: the
topmost-row rule itself turned out to be wrong.

**Third bug found and fixed (2026-11-03, during playtesting).**

*What happened.* On level 1, the player stacked bubbles beneath a large
same-color chunk that hung from the wall, then popped the chunk. The
stacked bubbles stayed floating in mid-air.

*Root cause.* The second fix assumed that nothing can float in the gap
between the wall and the topmost row. That's false, because the gap is
open air. If a pop removed *every* bubble in the wall-adjacent row, the
next row down became the "topmost row" and was treated as a stand-in
ceiling. Everything hanging from it stayed put.

*Fix.* Seed from the row the wall actually touches, `GridModel.RowsPushed`.
That property didn't exist yet when the second fix was made;
`BubbleLandingResolver` and `CeilingRenderer` already used it as the
effective ceiling.

*Why it looked tied to the wall advancing.* Pop and push never interleave.
`MatchProcessor`'s pop/drop pass runs synchronously inside `PlaceBubble`,
and the warning-gated push runs a frame later. The landings that trigger a
push are exactly the shots fired during the warning, and the push rebuilds
all sprites from the grid. That makes the floaters obvious at that moment.

*Tests.* The two tests where row 0 is empty now empty it the way real play
does, via `PushRowsDown`. Two new cases reproduce the report directly: the
wall row fully cleared, before and after a push.

*Level generation is unaffected.*
`PatternLevelGenerator.ConnectFloatingCellsToCeiling` bridges stems to row
0, which equals `RowsPushed` at generation time. Levels 1-6 were checked in
Play mode and generate with zero floaters.

**Related gap found in the same pass.** Superpower pops
(`SuperpowerEffectController`) called `GameBoard.PopCells` but never ran
the floating-cluster check. Bomb, Row Clear and Rainbow could therefore
leave disconnected bubbles hanging, whatever the anchor rule. Fixed by
moving `MatchProcessor`'s drop step onto `GameBoard.DropFloatingCells()`
and calling it after a superpower's pops too.

## Open questions / tuning knobs

- Scoring: points per bubble popped, and whether floating-cluster bubbles
  score differently/more (classic games often reward chain drops higher) —
  not decided yet, tune once there's a HUD to show it.
- Whether combo/chain bonuses exist for consecutive matches — out of scope
  for the first playable version, revisit after Phase 1 milestone 8.
- Pop and drop currently look different (instant vs. falling) but neither
  has a distinct sound/particle effect yet — revisit once there's real art
  and audio.
