# Curated Levels 1–6 + Level-Transition Animation — Design Spec

**Status: implemented.** Produced by a brainstorming session on 2026-09-11
against the real Phase 1 codebase (`LevelGenerator`, `DifficultyCurveConfig`,
`GridDebugRenderer`), following the same dedicated-design-session precedent
as `../../superpowers/specs/2026-09-10-superpowers-design.md`. Tracked as
Phase 2.5 in `../../../ROADMAP.md` — an addition on top of Phase 1's
already-complete level generation and HUD/level-flow work, implemented via
the plan at
`../plans/2026-09-11-curated-levels-and-transitions-implementation.md`.
See "Implementation notes" at the end of this doc for what changed during
implementation.

## Origin & why now

`docs/features/core-gameplay/level-generation.md` explicitly deferred
hand-authored content: "Hand-authored/milestone levels are an explicitly
possible future addition, not part of Phase 1 — don't build toward them
yet." Pure procedural generation (random per-cell fill, gated by
color-count/density knobs) works but can't express a designed feel level
to level. Separately, `docs/features/core-gameplay/hud-and-level-flow.md`
never designed any transition for the win → next-level flow — it's an
instant board swap today.

## Decisions

### Level scope

Curated pattern recipes apply to **levels 1–5 only**. Level 6 ("random
bubbles filling a bit more than 3/4 of the screen") needs **no new
generation code** — it has no entry in the new pattern catalog, so it
falls through to the existing `LevelGenerator` procedural path, tuned via
that level's `Density` curve keyframe (see "Difficulty curve" below).
Level 7 onward already used that same fallback before this spec.

### Pattern recipes per level

Each level maps to an ordered list of regions, laid out as equal
horizontal bands (ceiling-down) across the level's fillable rows
(`PlayfieldRows - HeadroomRows`, the same range `LevelGenerator` already
fills), separated by a small configurable row gap:

| Level | Regions (top → bottom) |
|---|---|
| 1 | `HexBlob` |
| 2 | `VerticalStripe` |
| 3 | `HorizontalStripe` |
| 4 | `HexBlob`, `VerticalStripe` |
| 5 | `HexBlob`, `VerticalStripe`, `HorizontalStripe` |
| 6+ | *(no entry — plain `LevelGenerator`)* |

- **`HexBlob`**: separated, single-color hexagonal blobs (one color per
  blob), filling as much of the band as fits.
- **`VerticalStripe`** / **`HorizontalStripe`**: repeating 3-cell-wide
  color stripes (columns or rows respectively), cycling through the
  level's available colors.

Bands are split by *rows*, not columns, even for vertical-stripe regions —
this keeps region layout consistent with the existing row-based
`HeadroomRows` concept rather than introducing a second, column-based
partitioning scheme.

### Hex blob sizing

Fixed radius, reusing the existing `HexRadius` BFS utility (built for the
Bomb superpower — see `../../superpowers/overview.md`). As many
non-overlapping, gap-separated blobs as fit in a band; blob count is
whatever the packing yields, not a fixed target.

### Difficulty curve

`DifficultyCurveConfig`'s four knobs (`ColorCount`, `Density`,
`HeadroomRows`, `CeilingDropIntervalSeconds`) — currently independent
linear start/max/rate formulas plus a one-off level-1 override — become
one Unity `AnimationCurve` each, evaluated at `levelNumber` (rounded/
clamped for the integer knobs). The existing level-1 override
(`level1Density`/`level1HeadroomRows`) is removed; it's just that curve's
first keyframe now. This is a visual, Inspector-editable graph rather than
a set of numeric ramp fields — the tool for hand-tuning a progression,
which is exactly what both this spec's level recipes and future
non-pattern levels need.

Pattern levels (1–5) still source `ColorCount`, `HeadroomRows`, and
`CeilingDropIntervalSeconds` from this curve. `Density` stops applying to
pattern levels — a pattern fills its band fully (subject to its own
blob/stripe rules); `HeadroomRows` alone controls how much of the screen
is used, so no separate "fill fraction" concept is needed. Level 6's
"a bit more than 3/4" is just that level's `Density` keyframe set higher
than levels 1–5's effective ~0.75 fill (which itself comes from
`HeadroomRows`, not `Density` — the two knobs are independent, and only
`Density` needs hand-tuning for level 6 specifically).

### Pre-formed match groups: allowed by design

Blobs and stripes are large same-color connected groups on purpose — that
is the point of the pattern. `MatchProcessor` only reacts to *new*
placements (`GameBoard.OnBubblePlaced`), never to the initial fill, so a
pre-formed group is never auto-popped; it simply sits until the player's
first adjacent shot triggers a flood-fill match that clears the whole
group at once. Pattern generation therefore **skips** the anti-pre-pop
exhaustive-reroll pass `LevelGenerator` uses for its pure-random fill
(see `level-generation.md`'s "Anti-pre-pop constraint" section) —
that pass exists specifically to *prevent* what pattern levels want.

Ceiling-connectivity cleanup (`MatchResolver.FindFloatingCells`, already
run by `LevelGenerator` after every generation) still runs unconditionally
after pattern generation too, so a level can never start with a bubble
disconnected from row 0, pattern or not.

### Level-transition animation

`GridDebugRenderer.OnLevelLoaded` currently shares the same instant
`RebuildAll()` (destroy all sprites, respawn from `GameBoard.Grid` in one
frame) as `OnRowPushedDown`. These get split:

- `OnRowPushedDown` keeps the instant rebuild — a mid-level ceiling push
  should read as abrupt, warned by the existing `CameraShake`, not eased
  in.
- `OnLevelLoaded` gets a new staggered build-in: each spawned bubble
  starts at scale 0 and eases up to scale 1, with a start delay derived
  from its row so the board visibly fills in top-to-bottom rather than
  popping in all at once.

This reuses the project's existing hand-rolled easing convention
(`BubbleSettleMotion`'s pure easing functions, `FiredBubbleController`'s
`Update()`-driven elapsed-time state, no tweening library dependency)
rather than introducing a new animation approach.

## Implementation sketch

### New data/config

- `Assets/Scripts/Grid/PatternType.cs` — enum `HexBlob | VerticalStripe |
  HorizontalStripe`.
- `Assets/Scripts/Grid/LevelPatternPlan.cs` — `[Serializable]`: `LevelNumber`,
  `PatternType[] Regions`.
- `Assets/Scripts/Grid/PatternLevelCatalog.cs` — `ScriptableObject`
  holding the ordered `LevelPatternPlan` list (levels 1–5) plus shared
  tunables: `HexBlobRadius` (default 2), `HexBlobGapCells`, `StripeWidth`
  (default 3), `RegionGapRows`. Exposes `TryGetPlan(int levelNumber, out
  LevelPatternPlan plan)`. Default asset:
  `Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset`.
- `DifficultyCurveConfig`: four `AnimationCurve` fields replace the
  start/max/rate groups; `level1Density`/`level1HeadroomRows` removed;
  `ForLevel` becomes per-knob curve evaluation.
  `Assets/ScriptableObjects/DefaultDifficultyCurve.asset` gets keyframes
  reproducing roughly today's ramp, hand-tunable afterward.

### New generation logic (pure, static — matches `FloodFill`/`MatchResolver`/`HexRadius`)

- `HexBlobPlacer` — places one `HexRadius`-sized blob per random empty
  center within a band (one random color per blob), rejecting centers
  that would overlap or come within `HexBlobGapCells` of an existing
  blob; stops when no valid center remains.
- `StripePlacer` — fills a band solid in repeating `StripeWidth`-wide
  stripes along the given axis, cycling
  `colors[(index / StripeWidth) % ColorCount]`.
- `PatternLevelGenerator.Generate(GridModel, LevelPatternPlan,
  DifficultyConfig, PatternLevelCatalog, System.Random)` — splits the
  fillable-row range into equal, gap-separated bands per
  `plan.Regions`, dispatches each to `HexBlobPlacer`/`StripePlacer`, then
  runs `MatchResolver.FindFloatingCells` cleanup. Seeded identically to
  `LevelGenerator` (`new System.Random(levelNumber)`).
- `LevelContentGenerator` — new routing entry point:
  `PatternLevelCatalog.TryGetPlan(levelNumber, ...)` found →
  `PatternLevelGenerator`; not found → today's `LevelGenerator.Generate`
  unchanged. `GameBoard.Awake`/`GameBoard.LoadLevel` call this instead of
  `LevelGenerator.Generate` directly. `LevelGenerator` itself is not
  modified beyond what the difficulty-curve change requires.

### Level-transition animation

- `Assets/Scripts/Grid/BubbleSpawnAnimator.cs` — small `MonoBehaviour`,
  `Update()`-driven elapsed-time scale-in (0→1, eased), same shape as
  `FiredBubbleController`'s settle-motion driving code; takes a start
  delay (derived from the bubble's row).
- `GridDebugRenderer` — `RebuildAll()` splits into the existing instant
  path (kept for `OnRowPushedDown`) and a new animated path for
  `OnLevelLoaded` that attaches a `BubbleSpawnAnimator` to each freshly
  spawned bubble instead of leaving it at full scale immediately.

## Testing

EditMode, mirroring `LevelGeneratorTests`' existing precedent:

- Hex blobs: one color per blob; blobs never touch/overlap; configured
  gap respected.
- Stripes: column/row index → color matches the configured-width cycle.
- Mixed levels: each band contains only its assigned pattern type's cells.
- All pattern levels: no floating cells after generation (existing
  invariant); same `levelNumber` regenerates an identical grid
  (determinism), matching `LevelGenerator`'s existing precedent.
- `DifficultyCurveConfig`: `ForLevel` returns curve-evaluated values
  matching hand-picked keyframe expectations at a few sample levels.
- Level-6/7+ fallback: `PatternLevelCatalog.TryGetPlan` returns false for
  those levels and generation goes through the unmodified `LevelGenerator`
  path.

The transition animation itself isn't unit-testable (visual/timing) —
verify via a Unity MCP Play-mode check (advance from level 1 to 2,
screenshot the staggered build-in), the same way Milestone 10's HUD work
was verified.

## Out of scope

- Levels 6 and beyond getting their own curated patterns — deliberately
  left to the existing procedural generator, tuned via the difficulty
  curve.
- Any change to `MatchProcessor`, `GameBoard`'s public event surface, or
  the anti-pre-pop logic `LevelGenerator` uses for its own (non-pattern)
  levels — both are reused as-is, not modified.
- Real art for bubbles/blobs/stripes — still placeholder circles via
  `GridDebugRenderer`, per the roadmap's "Later / not yet scoped" art
  pass.

## Open questions / tuning knobs

- `HexBlobRadius`, `HexBlobGapCells`, `StripeWidth`, and `RegionGapRows`
  default values are first guesses pending playtesting, same caveat every
  other `ScriptableObject` config in this project carries.
- The new `AnimationCurve` keyframes for `DefaultDifficultyCurve.asset`
  are set to roughly reproduce today's ramp at first; exact shape is left
  to the user's hand-tuning in the Inspector afterward.

## Implementation notes

Built via a 10-task subagent-driven-development plan on 2026-09-11. Two
things changed from this spec during implementation, both recorded in
full (with the reasoning) in `../../../ROADMAP.md`'s Phase 2.5 entry:

- **Ceiling-connectivity handling.** This spec's plan originally called
  for deleting any bubble left disconnected from the ceiling after
  generation, matching `LevelGenerator`'s existing cleanup. That turned
  out wrong for pattern levels specifically — gap-separated blobs and
  banded gaps are disconnected *by design* here, not by rare accident, so
  deleting would have gutted most of every pattern level's content.
  `PatternLevelGenerator` bridges each disconnected component to the
  ceiling with a single same-color vertical stem instead
  (`ConnectFloatingCellsToCeiling`).
- **`LevelContentGenerator`.** The implementation plan initially dropped
  this class in favor of routing logic inline in `GameBoard`. A final
  whole-branch review restored it as this spec originally described — the
  level-6+ fallback path needed to be testable without a live scene — so
  the shipped code matches this doc's "Implementation sketch" section
  above after all.
- The same final review found the shipped `HexBlobRadius` (2) needed more
  rows than levels 4-5's band math could guarantee on realistic device
  screens, silently rendering their hex-blob region empty. Fixed by
  clamping the effective radius to whatever a band can actually hold
  (down to a single cell in the worst case) inside `HexBlobPlacer`, and
  reinstated defensive value clamps on `DifficultyCurveConfig.ForLevel`
  that the `AnimationCurve` refactor had dropped.

All other decisions in this spec shipped as designed.
