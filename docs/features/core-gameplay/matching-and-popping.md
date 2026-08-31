# Matching & Popping

## Decision

Standard Puzzle Bobble matching: when a fired bubble snaps into a grid cell,
if it connects **3 or more** same-color bubbles (including itself) via
adjacency, they all pop. After a pop, any bubbles left disconnected from the
ceiling (not reachable by any path of adjacent occupied cells back to the
top row) fall and are also cleared.

## Implementation sketch

`MatchResolver`, given the cell the new bubble just landed in:

1. **Match check**: flood-fill from that cell through same-color neighbors
   (`GridModel.GetNeighbors`). If the connected same-color group size is
   `>= 3`, mark all of them for popping. If `< 3`, the bubble just stays —
   no pop, turn ends normally.
2. **Pop**: clear those cells in `GridModel`, raise `OnBubblesPopped(cells,
   color)` for the rendering layer to animate/destroy the sprites.
3. **Floating cluster check** (only runs if step 2 popped anything): from
   every occupied cell in the **top row**, flood-fill through all occupied
   neighbors to find every cell reachable from the ceiling. Any occupied
   cell *not* in that reachable set is floating — clear it too and raise
   `OnClusterDropped(cells)` (rendering layer can animate these falling
   instead of just popping, for the classic "chain reaction" feel).

Both flood-fills reuse the same `GridModel.GetNeighbors` traversal — worth
writing one generic flood-fill helper (`FloodFill(startCell, predicate)`)
that both the match check and the ceiling-reachability check call with
different predicates (same-color vs. any-occupied).

## Open questions / tuning knobs

- Scoring: points per bubble popped, and whether floating-cluster bubbles
  score differently/more (classic games often reward chain drops higher) —
  not decided yet, tune once there's a HUD to show it.
- Whether combo/chain bonuses exist for consecutive matches — out of scope
  for the first playable version, revisit after Phase 1 milestone 8.
