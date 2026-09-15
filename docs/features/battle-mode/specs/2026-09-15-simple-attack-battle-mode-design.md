# Battle Mode (Simple Attack Variant) — Design Spec

**Status: implemented.** This is Phase 3 of `docs/ROADMAP.md`, produced
by a brainstorming session on 2026-09-15 against the real Phase 1/2
codebase, implemented the same day via an 11-task plan. This is
explicitly the *first* and simplest of several battle-mode variants the
user has in mind — later variants get their own future design sessions.
Supersedes the open questions in `../overview.md`, which stays as
historical context.

## Origin & why now

From `../overview.md`: a same-device local multiplayer mode, screen split
top/bottom, two independent boards, goal of destroying the opponent's
board. Deferred until Phase 1's core systems (`GridModel`,
`ShooterController`, `MatchResolver`) were proven solid enough to run as
two simultaneous instances — confirmed feasible per
`architecture/overview.md`: Phase 1 is manager-based MonoBehaviours wired
by local C# events with no singletons, and Phase 2 (Superpowers) already
demonstrated hooking new systems in via events with zero changes to
`GameBoard`'s public API.

## Decisions

Answers to `../overview.md`'s open questions, plus new questions raised
during this session:

- **Win condition mechanic:** the existing autonomous ceiling-descent
  timer is **replaced**, not supplemented. The only way a player's wall
  advances is via the opponent's matches — reusing the existing
  `GameBoard.PushRowDown()` (already used by solo mode's ceiling timer)
  as the mechanism, just re-triggered by a different source.
- **Input handling:** two independent per-player instances of the
  existing input stack (see "Architecture" below), not a new input
  scheme.
- **Shot timer carryover:** kept, applies independently per player,
  unchanged from solo (12s auto-fire at current aim).
- **Superpowers:** **disabled** for this first battle-mode variant.
  Revisit in a follow-up design pass once the core battle loop is proven.
- **Match pacing:** no artificial time limit — the match is bounded
  naturally by the win/loss conditions below.

## Architecture: two parallel instances, no core-system changes

Each player gets a complete instance of the existing Phase 1 stack —
`GridModel`, `GameBoard`, `ShooterController`, `TrajectoryPredictor`,
`OccupancyCollision`, `BubbleLandingResolver`, `LandingIndicator`,
`FiredBubbleController`, `MatchProcessor`, and a battle-specific state
manager (see "Win, loss, and draw" below) — bundled under one root (e.g.
`BattleSide`), instantiated twice. No singleton or static state blocks
this; the only static classes in the project (`GameSettings`,
`SuperpowerProgress`) are `PlayerPrefs`-backed persisted preferences,
irrelevant here since superpowers are off.

## Screen layout & input

- Each side's camera renders to its half of the screen via `Camera.rect`
  (top/bottom viewport split), reusing the existing fixed-column/
  match-width board-fit logic (`BoardBoundsCalculator`) computed against
  a half-height viewport instead of the full screen.
- **Player 2's half is rotated 180°**, so the phone can lie flat between
  two players facing each other, each reading their own half right-side-
  up — standard for same-device tabletop multiplayer.
- This rotation is the one genuinely new, unproven piece of the design —
  everything else above is direct reuse of shipped, tested code.
  Rotating the render output 180° for one player is a solved problem in
  Unity; rotating *touch input* to match (so Player 2's physical "left"
  zone still maps correctly after the visual flip) carries real
  implementation risk and is worth validating with a short feasibility
  spike before committing to the full implementation plan.

## Attack economy

- New `BattleAttackConfig` (`ScriptableObject`, same pattern as
  `DifficultyCurveConfig`/`SuperpowerCatalog`): a small formula mapping
  match size (pop count) to an attack value, and cascade-drop count to a
  separately-weighted attack value — deliberately **decoupled from the
  solo score formula** (`ScoreCalculator`), so the two can be tuned
  independently without one breaking the other.
- New pure `BattleAttackResolver` (same shape as `MatchResolver` — no
  mutation, just computation), turning a pop/drop event into an attack
  value via the config above.
- New `PendingRowsMeter` per side (small pure C# class, mirrors
  `ShotTimer`'s "small pure countdown class" pattern): accumulates
  attack value and exposes whole rows once ≥1 is banked, carrying the
  fractional remainder forward. `GridModel.PushRowsDown` only moves whole
  rows, so partial attack value can never trigger a partial push.
- **The one new cross-board wiring Phase 3 needs:** a controller listens
  to *my* board's `OnBubblesPopped`/`OnClusterDropped`, resolves attack
  value via `BattleAttackResolver`, adds it to *my* `PendingRowsMeter`,
  and once a whole row is banked, calls `PushRowDown()` on the
  **opponent's** `GameBoard` that many times. Every other event stays
  single-board, exactly as in solo mode.

## Win, loss, and draw

Both win/loss signals already exist in Phase 1 and need no new logic —
just re-pointed at a new trigger source:

- **Loss:** reuse `GameBoard.OnRowPushedDown(wasLastRowOccupied)` — same
  check solo mode already does, now triggered by opponent attacks
  instead of a timer.
- **Win by clearing your board:** reuse `GridModel.IsEmpty`, same check
  solo's `GameStateManager` already does after
  `OnBubblesPopped`/`OnClusterDropped`. Clearing your own board is an
  **instant win** — no mid-match refill needed, since the match ends
  immediately.
- **Simultaneous end (new edge case, no solo-mode analog):** if both
  players' win/loss conditions trigger in the same frame (e.g. both
  boards clear simultaneously), the match is a **draw**.
- Because both boards' outcomes must be reconciled into one authoritative
  result (avoiding a race where each side's manager independently
  declares itself the winner), a single top-level `BattleMatchController`
  listens to both sides' win/loss signals and settles the match once,
  rather than two independent per-side managers each unilaterally
  declaring an outcome.

## Board content

- Both boards generate from **one shared seed** picked at match start
  (e.g. derived from match start time), so layouts are always identical
  between the two players within a match, while varying match-to-match.
  Reuses `LevelContentGenerator`/`LevelGenerator` directly.
- Since there's no "level number" within a single match to drive
  `DifficultyCurveConfig`'s ramps, a small fixed `BattleBoardConfig`
  (same shape as `DifficultyConfig` — color count, density, headroom
  rows) replaces the curve lookup.
- Recommend seeding initial content from one of the existing curated
  patterns (levels 1-5 already have tested, visually distinct layouts)
  rather than inventing new pattern logic for this first variant.

## Result screen & rematch

Reuse the existing `LevelResultScreen`-shaped component (win/lose panels
are already structurally identical per `ROADMAP.md`'s Milestone 10
notes), with a third **draw** state added, and a "Rematch" action
(regenerates both boards from a fresh shared seed) instead of "Next
Level."

## Out of scope (this variant)

- Superpowers (Freeze/Bomb/Row Clear/Rainbow) — deferred to a follow-up
  design pass.
- More than 2 players, or any other battle-mode variant — this is
  explicitly the first and simplest of several the user has planned;
  later variants get their own design sessions.
- Online/matchmaking play — local split-screen only, per the original
  constraint in `../overview.md`.
- Any non-touchscreen input scheme.
- Real art and battle-specific VFX — reuse existing pop/drop visuals for
  now, per the project's existing "placeholder art, real art pass later"
  convention.

## Implementation notes

Implemented via an 11-task plan
(`plans/2026-09-15-simple-attack-battle-mode-implementation.md`) the same
day as this spec, using subagent-driven development (a fresh implementer
per task, a scoped task review after each, a final whole-branch review,
one fix wave, and a scoped re-review of that fix wave). Two sections
above didn't match what shipped, both found to be sound simplifications
once building against the real codebase — see "Board content" and
"Screen layout & input" above for the original text; this section
records what actually happened.

- **No `BattleBoardConfig` class exists.** Reading the real generators
  (`LevelContentGenerator`/`LevelGenerator`/`PatternLevelGenerator`)
  showed they're already fully deterministic from a shared level number
  alone (`new System.Random(levelNumber)`/`new System.Random(plan.LevelNumber)`).
  Picking one random level number in `[1, 5]` per match and calling
  `GameBoard.LoadLevel(n)` on both boards gives byte-identical, match-to-
  match-varied content for free, reusing `DefaultDifficultyCurve.asset`/
  `DefaultPatternLevelCatalog.asset` directly — no new content-generation
  config or code needed. (A later fix — see below — replaced the
  *difficulty curve* asset used, but board content is still driven purely
  by a shared level number, not a bespoke config class.)
- **No feasibility spike was run for the Player 2 180° rotation** before
  the implementation plan locked in a mechanism. Reasoning through
  Unity's actual semantics while writing the plan showed it decomposes
  into two independent, well-documented mechanisms: rotating only the
  physical camera `transform.rotation` (all of `GameBoard`'s board-fit
  math reads only camera *position* and *orthographic size*, both
  rotation-invariant, so this changes nothing about where `GameBoard`
  *thinks* things are — only what direction on screen is "up" for that
  camera's rendered output), and rotating a UI parent `RectTransform`
  180° for the touch zones (uGUI's raycasting already handles rotated
  `RectTransform`s correctly). Both held up in the actual build with no
  surprises.

**Three real bugs were found and fixed during the plan's final
verification task (Task 11)**, well after the individual tasks first
"shipped":

- `PendingRowsMeter`'s accumulated attack value wasn't reset on
  `BattleMatchController.Rematch()`, leaking leftover fractional attack
  value into the next match. Fixed with `PendingRowsMeter.Reset()` and
  `BattleAttackController.ResetForRematch()`, wired into `Rematch()`.
- Battle Mode's boards spawned **completely empty** in real play. Root
  cause: both boards reused `DefaultDifficultyCurve.asset`, whose
  headroom-row curve was tuned only for solo mode's 16-row board
  (headroom 9 at level 1) — Battle Mode's boards are only ~7 rows
  (half-height viewport), so headroom exceeded the playfield, and the
  generators' existing, *deliberately tested* "headroom exceeds
  playfield → produce an empty grid, without throwing" behavior
  (`LevelGeneratorTests`/`PatternLevelGeneratorTests`'
  `Generate_HeadroomExceedsPlayfieldRows_ClampsToEmptyGridWithoutThrowing`)
  correctly, if unhelpfully, kicked in every time. This is exactly the
  scenario the original `BattleBoardConfig` concept (see above) would
  have existed for — its removal's real cost, discovered here. Fixed
  with a new `Assets/ScriptableObjects/BattleDifficultyCurve.asset`
  (same `DifficultyCurveConfig` type, a low flat headroom curve; other
  three curves unchanged from default) wired into both battle boards
  instead of the solo default — an asset-and-scene-wiring fix only, no
  changes to the shared, tested generator code.
- `BattleAttackController` could call `opponentBoard.PushRowDown()`
  multiple times in one synchronous loop (a big match banks several rows
  of attack at once) with nothing stopping it once the opponent's board
  was already fully pushed — driving `GridModel.RowsPushed` past `Rows`
  and crashing `BubbleLandingResolver` (which assumes `RowsPushed <=
  Rows`). This was never reachable before Battle Mode: solo mode's
  `GameStateManager` only ever pushes one row per ceiling-timer tick and
  stops entirely once the game ends. Fixed by bounding the push loop
  (`opponentBoard.Grid.RowsPushed < opponentBoard.Grid.Rows`) rather than
  touching the shared, untested-for-this-case `GridModel`/
  `BubbleLandingResolver`. A narrower residual was found and deliberately
  **not** fixed: because `BubbleLandingResolver`'s indexing is already
  out of bounds at `RowsPushed == Rows` itself (not just greater than),
  and the losing side's `ShooterController` isn't disabled until a frame
  later (`BattleMatchController.LateUpdate`, by design — needed for
  simultaneous-draw detection), one single-frame, non-recurring,
  non-corrupting console exception can still occur at the exact instant a
  board's wall is fully pushed. Fully closing this would mean touching
  the shared `GridModel`/`BubbleLandingResolver` files, or reworking the
  `LateUpdate`-deferred disable timing the draw-detection design depends
  on — parked as a known, bounded limitation for a future pass.

**A final whole-branch review** (after all 11 tasks individually passed
their own review) found two more bugs, both the same "leftover per-match
state survives Rematch" class as the `PendingRowsMeter` bug above:

- `BattleAttackController.enabled = false` at match end was a structural
  no-op — the class has no `Update`/`OnEnable`/`OnDisable`, only event
  subscriptions made once in `Start()`, so Unity's `enabled` flag never
  actually stopped it from banking attacks and pushing rows after a
  match had already ended (reachable via a bubble still mid-flight when
  the match ends). Fixed by adding `if (!enabled) return;` as the first
  line of both `HandlePopped` and `HandleDropped`.
- `BattleShotClock`'s internal `ShotTimer` wasn't reset on `Rematch()`
  either — confirmed as a regression against solo mode's own
  `GameStateManager.ResumeWithLevel` precedent for the identical
  transition (which resets both the level *and* the shot timer
  together). Fixed with `BattleShotClock.ResetForRematch()`, wired into
  `Rematch()` alongside the other two per-side resets via a small
  `ResetSideState(BattlePlayerSide side)` helper.

## Open questions / tuning knobs

- `BattleAttackConfig`'s formula constants (`popAttackPerBubble = 0.05`,
  `dropAttackPerBubble = 0.15`) and `BattleDifficultyCurve`'s flat
  headroom value are first-guess placeholders pending real 1v1
  playtesting, same caveat as `DifficultyCurveConfig`'s ramps and
  `SuperpowerCatalog`'s thresholds.
- Which curated level(s) get shown is left to chance (a random pick in
  `[1, 5]` per match) rather than a deliberate choice — fine for now,
  worth revisiting once there's real playtesting feedback on whether all
  five curated patterns feel fair/fun in a competitive 1v1 context.
- The parked `RowsPushed == Rows` single-frame exception noted above is
  worth a proper fix in a future pass, once it's clear whether it's
  worth reworking `BattleMatchController`'s disable timing or the shared
  landing-resolution code.
