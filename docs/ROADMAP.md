# Roadmap

Status legend: ✅ designed (see feature docs) · 🚧 placeholder, needs its own
brainstorming session · ⏳ not yet scoped.

## Phase 0 — Project setup

- [x] `git init` this project and make an initial commit.
- [x] Create folder conventions under `Assets/`: `Scripts/`, `Prefabs/`,
      `Art/`, `ScriptableObjects/`, `Scenes/` (plus `Tests/` for EditMode
      tests, added once testing started).
- [x] `.gitignore` / `.gitattributes` set up for Unity (standard ignores,
      line-ending normalization, and a `Screenshots/` ignore for Editor/MCP
      debug captures).
- [ ] Import a free placeholder sprite pack for bubbles/UI (e.g.
      [Kenney.nl](https://kenney.nl/) — "Puzzle Pack" or similar). Not
      needed yet — the Milestone 1 debug renderer draws plain circles
      generated in code instead, see below.
- [ ] Confirm the existing URP 2D template settings are suitable (project
      already uses Unity 6000.5.1f1 with the 2D URP template — no changes
      expected here, just a sanity check once real content exists).
- [x] Player Settings default orientation set to Portrait (landscape
      autorotate disabled) to match the portrait-only design (see
      `battle-mode/overview.md` and
      `core-gameplay/screen-fit-and-difficulty-scaling.md`). Build target
      is still `StandaloneWindows64` for now — switching to Android/iOS is
      deferred to Milestone 11's device build.

## Phase 1 — Core single-player bubble shooter ✅ (design complete)

See `features/core-gameplay/`. Suggested build order — each milestone should
be playable/testable on its own before moving to the next:

1. **Hex grid data model** + static debug rendering of a board. ✅ **Done.**
   `GridModel` (occupancy, hex-neighbor lookup, world position) is
   implemented and unit-tested (`Assets/Scripts/Grid/`,
   `Assets/Tests/EditMode/`), with a temporary `GridDebugRenderer` that
   fills a board with random-colored circles (generated in code, no art
   asset needed) to visually confirm the hex packing. `GridDebugRenderer`
   is a Milestone-1 stand-in, not the final rendering layer described in
   `architecture/overview.md`.
   → [`hex-grid.md`](features/core-gameplay/hex-grid.md)
   - **Screen fit & device-independent difficulty scaling — decided and
     implemented.** Board fills the full phone screen in width and height
     (no letterbox bars) via a fixed-column/match-width camera plus a
     dynamically computed row count, with ceiling-descent fairness
     preserved across devices via a headroom-rows difficulty knob (for
     Milestone 9's `LevelGenerator`). Unblocks Milestone 2's need for real
     board bounds. Tablets explicitly out of scope for now.
     → [`screen-fit-and-difficulty-scaling.md`](features/core-gameplay/screen-fit-and-difficulty-scaling.md)
2. **Shooter + aim input** with kinematic trajectory preview (including wall
   bounces). ✅ **Done.** Aiming turned out to be a fixed-speed rotating gun
   (arcade Puzzle Bobble style, via on-screen hold zones) rather than
   drag-to-angle — the original write-up here was wrong and got corrected
   during implementation. `TrajectoryPredictor`, `BoardBoundsCalculator`,
   `HoldInputZone`, and `ShooterController` are implemented, with the pure
   math unit-tested (`Assets/Scripts/Shooter/`, `Assets/Tests/EditMode/`).
   Firing itself was **not** built yet at this point — `ShooterController`
   raised `OnFireRequested` with no subscriber, which became Milestone 3's
   hook-in point. Known tech debt at the time: `ShooterController` and
   `GridDebugRenderer` each had their own `cols`/`cellWidth` fields instead
   of sharing one board config — ~~resolved in Milestone 3~~ by introducing
   `GameBoard` as the single shared owner (see below).
   → [`shooter-and-trajectory.md`](features/core-gameplay/shooter-and-trajectory.md)
3. **Firing a bubble** — move it along the previewed path, snap to the
   nearest empty grid cell on collision. ✅ **Done.** `GameBoard` is now the
   single shared owner of the `GridModel`/bounds (resolving the
   `ShooterController`/`GridDebugRenderer` cols/cellWidth duplication
   tech debt from Milestone 2). `OccupancyCollision` truncates the
   trajectory at the first occupied cell so the preview and the fired
   bubble always agree, `BubbleLandingResolver` picks the nearest empty
   cell, and `FiredBubbleController` animates the fired bubble and hooks
   into `ShooterController.OnFireRequested`.
   → [`firing-and-snapping.md`](features/core-gameplay/firing-and-snapping.md)
   - **Two playtesting bugs found and fixed, well after this milestone
     first shipped.** A mis-snapping bug (a shot could visibly nestle into
     a pocket bounded by a bubble other than the one it technically
     contacted first, and land in the wrong slot) and a preview-line bug (a
     "ghost aura" gap between the aim line's tip and the bubble it was
     aiming at, worse at an angle — the line was drawn to a bubble's future
     *center*, then an initial fix extrapolated along the incoming ray
     instead of aiming at the bubble's actual center, which only happened
     to work for head-on shots). Both fixed in `BubbleLandingResolver`/the
     (since superseded) `PreviewPointsCalculator` — see
     `firing-and-snapping.md` for the full root causes and the later
     redesign that replaced `PreviewPointsCalculator` with an occlusion-based
     line plus a separate `LandingIndicator`.
   - **Landing settle animation added, well after this milestone first
     shipped.** The fired bubble used to teleport instantly from its
     flight-path contact point to the resolved landing cell (most visible
     on a straight-column shot into an offset row, which shifts one
     hex-cell to a side by design). `FiredBubbleController` now plays a
     short "slide + small overshoot bounce" (`EaseOutBack` easing) settle
     motion between the two, always — no special-casing for whether there
     was an actual shift. The shot timer was raised from 8s to 12s in the
     same pass to fit the more playful pacing. See
     `firing-and-snapping.md` and `shot-timer-and-ceiling-descent.md`.
4. **Match detection** (3+ connected same-color bubbles via flood fill) +
   popping. ✅ **Done**, together with Milestone 5 (built in the same pass,
   since the design doc treats them as one flood-fill-based component).
   `FloodFill` (generic BFS over `GridModel`) and `MatchResolver`
   (`FindMatchGroup`/`FindFloatingCells`) are static, pure logic classes in
   `Assets/Scripts/Grid/`, unit-tested, following the `BubbleLandingResolver`
   precedent. `GameBoard` gained `OnBubblesPopped`/`OnClusterDropped` events
   and `PopCells`/`DropCells` methods — kept on `GameBoard` rather than a new
   `MatchResolver` MonoBehaviour, so it stays the single event source for all
   grid-state changes (consistent with `OnBubblePlaced`). A new
   `MatchProcessor` listens to `OnBubblePlaced` (rather than being wired
   directly into `FiredBubbleController.Land()`), so matching/dropping
   applies to any future placement source, not just fired bubbles —
   confirmed safe since `GameBoard.Awake`'s initial fill calls
   `Grid.PlaceBubble` directly, bypassing the event. `GridDebugRenderer` now
   tracks its spawned sprites and reacts to both events: a pop destroys
   instantly, while a drop adds a new `FallingBubble` component (simple
   constant-gravity fall) so a bubble that loses its connection to the
   ceiling visibly falls instead of vanishing like a match.
   → [`matching-and-popping.md`](features/core-gameplay/matching-and-popping.md)
   - **Row-direction bug found and fixed.** `firing-and-snapping.md` and
     `BubbleLandingResolver` always assumed row 0 is the ceiling, but
     `GameBoard`'s actual positioning/fill code had it backwards — row 0
     rendered at the bottom near the shooter (always empty in practice),
     while real board content lived near `Rows-1`. Harmless before now
     (`BubbleLandingResolver`'s row-0 fallback path is rarely reachable
     since the initial fill blocks most straight-up shots), but it would
     have made `MatchResolver.FindFloatingCells` drop the entire board on
     every pop, since its ceiling-row seed set would almost always be
     empty. Fixed at the source: `GridModel.GetWorldPosition` now returns
     `y = -row * RowHeight` (row 0 = y = 0 = the ceiling anchor), and
     `GameBoard.PositionBoard`/`FillWithRandomBubbles`/`ShooterOrigin` were
     updated to match (anchor at the top, fill from row 0 down, shooter
     origin computed independently of the now-top-anchored transform).
     Verified with a live Play-mode check (occupied rows, world Y vs.
     camera bounds) and a screenshot before trusting the fix. Updated
     `GridModelWorldPositionTests` and three `OccupancyCollisionTests`
     scenarios that had hard-coded the old (wrong) direction.
   - **Shooter tweaks needed to actually test this.** Manually verifying
     matches required knowing which color was about to fire and being able
     to aim precisely, so two small `Shooter`-side changes landed alongside
     this milestone rather than as separate work: a "next bubble" indicator
     (see `firing-and-snapping.md`) and a rotation-speed retune (`90 → 45 →
     25`, see `shooter-and-trajectory.md`).
5. **Floating cluster detection** (bubbles disconnected from the ceiling
   after a pop) + drop. ✅ **Done** — see Milestone 4 above.
6. **Shot timer** — countdown per turn, auto-fires at current aim on
   expiry. ✅ **Done** (shot timer only — ceiling descent from the same doc
   is deferred to Milestone 7). `ShotTimer` is a pure C# countdown class
   (`Assets/Scripts/Gameplay/`), unit-tested like `FloodFill`/`MatchResolver`.
   `GameStateManager` (the "referee" slot reserved in
   `architecture/overview.md`) ticks it and calls `shooterController.Fire()`
   on expiry — a small `ShooterController` refactor that extracts the
   `OnFireRequested` invoke into a public `Fire()` method, callable by both
   a manual press and the auto-fire path. `GameStateManager` resets the
   timer only in response to `OnFireRequested`, so manual and auto fire
   share one reset path instead of two that could drift apart — this
   replaces the originally-sketched `OnShotTimerExpired()` event, which
   turned out unnecessary. Duration is 12s (raised from 8s alongside the
   landing settle animation, see the note below). `ShotTimerDisplay` shows a
   numeric countdown ("4"→"1"), hidden until `ShotTimeRemaining <= 4f`,
   built at runtime and anchored off the fire zone the same way
   `FiredBubbleController`'s next-bubble indicator is.
   → [`shot-timer-and-ceiling-descent.md`](features/core-gameplay/shot-timer-and-ceiling-descent.md)
7. **Ceiling descent timer** — pushes a new row down at a fixed interval;
   interval shortens with difficulty. ✅ **Done** (fixed interval only —
   difficulty scaling is deferred to Milestone 9's `LevelGenerator`).
   `GridModel.PushRowsDown` shifts row contents down in place and reports
   whether the shooter's line (`Rows - 1`) was occupied before the shift;
   `GameBoard.PushRowDown()` calls it and raises
   `OnRowPushedDown(bool wasLastRowOccupied)` — the single hook Milestone
   8's loss check will consume. `GameStateManager` reuses `ShotTimer` for
   the countdown (resetting it itself on expiry, rather than a separate
   self-resetting timer class) and currently only logs
   `wasLastRowOccupied`; no game-over flow yet. `GridDebugRenderer` reacts
   by destroying and rebuilding all its sprites from `GameBoard.Grid`
   rather than re-keying incrementally, since it's still the disposable
   Milestone-1 stand-in.
   → [`shot-timer-and-ceiling-descent.md`](features/core-gameplay/shot-timer-and-ceiling-descent.md)
   - **Reworked after playtesting, well after this milestone's original
     scope** — three bugs found and fixed in sequence, each well after
     Phase 1 first shipped: (1) `PushRowDown()` used to refill row 0 with a
     fresh random row every push (the sentence above described this at the
     time) — that was itself the bug: it silently injected bubbles the
     player never placed and overwrote the level's curated pattern within a
     few pushes. Removed entirely; a push now purely shifts existing rows
     down, nothing more. (2) Removing that refill exposed a hex row-parity
     bug (bubbles jogging sideways on every push) and a ceiling-connectivity
     bug (a pop anywhere could drop the *entire* board, since the
     "connected to ceiling" check hardcoded row 0 as the seed, and row 0
     now legitimately sits empty for stretches of play). (3) The wall's own
     *visual* advance was still missing even after (1)/(2): bubbles moved
     down correctly, but the ceiling band and the shot's stopping boundary
     never did, so new shots could land in space "behind" where the wall
     had already advanced to. A warning-gated trigger (`CameraShake` +
     landing-gated advance, instead of pushing the instant the timer
     expires) was added in the same pass. Full account, root causes, and
     fixes for all of this live in `shot-timer-and-ceiling-descent.md`,
     `hex-grid.md`, and `matching-and-popping.md` — not repeated here.
8. **Win/loss conditions** — board cleared = win, ceiling reaches the
   shooter line = loss. ✅ **Done.** `GridModel.IsEmpty` (unit-tested) added
   as the pure check the win condition needed. `GameStateManager` now
   subscribes to `GameBoard.OnBubblesPopped`/`OnClusterDropped` and checks
   `IsEmpty` after each; `HandleRowPushedDown` acts on
   `wasLastRowOccupied` instead of only logging it. Both routes go through
   a shared `EndGame` helper that guards against double-firing, stops both
   timers (`Update` no-ops once `_isGameOver` is set), logs, and raises the
   new `OnLevelWon`/`OnLevelLost` events. No level-complete/game-over UI or
   "advance to next level" yet — those wait on Milestone 9's
   `LevelGenerator` and Milestone 10's HUD.
   → [`win-loss-conditions.md`](features/core-gameplay/win-loss-conditions.md)
9. **Procedural level generator** with difficulty knobs (color count,
   density, row count). ✅ **Done.**
   `LevelGenerator.Generate(GridModel, int levelNumber, DifficultyConfig)`
   (`Assets/Scripts/Grid/`) fills `grid.Rows - HeadroomRows` initial rows,
   gating each cell on a per-cell density roll and restricting color choice
   to `BubbleColorPalette.AllColors[0..ColorCount)`, seeded via
   `System.Random(levelNumber)` for determinism. `DifficultyCurveConfig` (a
   `ScriptableObject`, asset at
   `Assets/ScriptableObjects/DefaultDifficultyCurve.asset`) resolves a
   `DifficultyConfig` per level via simple linear ramps — explicitly a rough
   placeholder curve pending playtesting (see `level-generation.md`'s open
   questions). `GameBoard.Awake` now calls `LevelGenerator.Generate` instead of the old
   fixed `filledRows`/`FillWithRandomBubbles`, and owns `levelNumber` and
   `CurrentDifficulty`; `GameStateManager`'s ceiling-descent `ShotTimer` is
   now constructed in `Start()` (not `Awake()`, since it needs
   `GameBoard.CurrentDifficulty`, which Unity doesn't guarantee is set
   before `GameStateManager.Awake()` runs) from
   `CurrentDifficulty.CeilingDropIntervalSeconds` instead of a hardcoded
   `20f`. (Ceiling-descent row refill — `GameBoard.RefillRow` — used to
   respect the level's color count here too; the whole refill was later
   found to be a bug and removed, see Milestone 7's update above.) A
   level-1-only override (`level1Density`/`level1HeadroomRows` on
   `DifficultyCurveConfig`) was added afterward as well, isolated from the
   `start*` ramp fields so easing level 1 for testing doesn't soften every
   later level's starting point too — see `level-generation.md`.
   - **Anti-pre-pop constraint pass, corrected during TDD.** The original
     plan's "reroll up to 8 times" approach turned out unsound: a
     same-color neighbor can itself already belong to a larger connected
     group elsewhere on the board, so with a low color count (tested down
     to `ColorCount=2`) *every* available color can trigger an instant
     match at a given cell, not just some. Caught by an EditMode stress
     test generating 100 levels at `ColorCount=2`/`Density=1` and
     asserting no `MatchResolver.FindMatchGroup` result is non-empty.
     Fixed by exhaustively trying every color in `[0, ColorCount)`
     (bounded by `ColorCount` itself, not a fixed attempt cap) and, in the
     genuinely-unavoidable case, leaving the cell empty rather than
     keeping an instant-popping placement.
   → [`level-generation.md`](features/core-gameplay/level-generation.md)
10. **Minimal HUD** (score, shots fired, level indicator) + level-complete
    and game-over screens. ✅ **Done.** No scoring system existed before
    this milestone; `ScoreCalculator` (pure, unit-tested) adds a weighted
    formula — quadratic per-match-size for pops, flat higher per-bubble for
    cascade drops — accumulated by `ScoreTracker`. `ShotsFiredCounter`
    tallies `ShooterController.OnFireRequested`. `HudDisplay` shows all
    three as a bottom bar anchored just above the fire zone (the board
    fills the full screen with no letterbox, so a top/floating overlay
    would sit on top of ceiling bubbles). `GameBoard.LoadLevel(int)` (new)
    lets a level be regenerated after the initial load; `GameStateManager`
    gained `RetryLevel()`/`AdvanceToNextLevel()`, turning the previously
    one-way `_isGameOver` latch into a real resume path — retry reloads
    the *same* level (deterministic seed) and resets score/shots, advance
    loads `LevelNumber + 1` and keeps score. `LevelResultScreen` is one
    component for both outcomes (win/lose panels are structurally
    identical), introducing the project's first `UnityEngine.UI.Button`.
    Verified end-to-end in Play Mode via Unity MCP (`execute_code`
    driving fire/pop/push-row/button-click, screenshots confirming the
    HUD and result panels).
    → [`hud-and-level-flow.md`](features/core-gameplay/hud-and-level-flow.md)
11. **First playable build on a physical device.** ✅ **Done.** Tested on a
    real device.
    - **Playtesting pass found and fixed, after this milestone first
      shipped.** Real-device testing found input felt laggy while aiming,
      and the rotate zones were hard to thumb-hit. Root causes and fixes:
      (1) `OccupancyCollision`/`BubbleLandingResolver` each independently
      re-scanned the entire grid via `GridModel.OccupiedCells()` every
      frame while aiming, plus LINQ allocations in the landing search —
      fixed by caching `OccupiedCells()` in `GridModel` itself (invalidated
      only at its actual mutation points) and rewriting the LINQ chains as
      manual loops; see `firing-and-snapping.md`. (2) No
      `Application.targetFrameRate` was set and `QualitySettings.vSyncCount`
      was inconsistent across quality tiers, causing uneven frame pacing
      independent of the aim-preview cost — fixed by disabling VSync
      project-wide and adding `FrameRateInitializer`; see
      `architecture/overview.md`'s "Frame pacing" section. (3) The
      rotate-left/rotate-right touch zones grew from 150×150 to 220×220,
      with the adjacent next-bubble/countdown labels' offsets made
      clearance-aware so they can't end up overlapping the (now bigger)
      zones on a narrow screen; see `shooter-and-trajectory.md` and
      `firing-and-snapping.md`. (4) The score/level HUD labels (bottom bar
      from Milestone 10), positioned near the screen edges directly above
      the rotate zones, were still clearing only the fire zone's (shorter)
      height, so they ended up sitting on top of the enlarged rotate zones
      and eating some of their touches too — fixed the same way, by having
      `HudDisplay` clear the tallest of all three bottom zones; see
      `hud-and-level-flow.md`.
    - **Settings screen added, to compare the landing settle animation
      against a second style.** The overshoot-bounce settle animation
      became swappable against a new "squash/pop" style (plain ease-out
      slide + sine-based squash-then-rebound scale on arrival) via a new
      `LandingAnimationStyle` enum read from a new `GameSettings` static
      class — this project's first persisted preference, backed by
      `PlayerPrefs`. A new `SettingsMenu.unity` scene (reached from a new
      "Settings" button on the main menu) lets the player pick between the
      two, live — this project's first screen outside Main Menu/gameplay/
      result. A bug where the new scene's Canvas ended up in World Space
      instead of Screen Space - Overlay (built by adding components
      directly rather than via the Editor's UI menu) made it render as a
      single oversized, full-screen button was found and fixed. See
      `firing-and-snapping.md`'s "Landing settle animation" section for
      the full split (`BubbleSettleMotion`), mechanics, and bug account.

## Phase 2 — Superpowers system ✅ (implemented)

Designed against the real Phase 1 codebase on 2026-09-10, implemented the
same day via an 11-task plan. Four player-activated abilities (Freeze,
Bomb, Row Clear, Rainbow), unlocked permanently as the player's highest
level reached crosses per-ability thresholds, with fixed charges per
level, shown on a new `SuperpowerHud`. Aimed abilities reuse the existing
rotate-and-fire mechanic; all board effects route through the existing
`GameBoard.PopCells`/event pipeline, so `GameStateManager`, `ScoreTracker`,
etc. need no special-casing — `GameBoard`'s public API was not touched.
Battle Mode interaction is explicitly deferred to Phase 3.

Two real bugs were found and fixed during implementation, well after the
individual tasks that introduced them first "shipped" within the same
implementation pass: (1) the new `FiredBubbleController.Land()` branch for
an armed superpower shot didn't clean up the flying-bubble sprite, unlike
the pre-existing normal-color path — fixed by extracting a shared
`ClearFlyingBubble()` helper called from both branches. (2) `SuperpowerHud`
built its button list once in `Start()`, with no guarantee
`SuperpowerController.Start()` (which populates the unlock data) ran
first — reproduced as the HUD silently showing no buttons depending on
Unity's arbitrary script execution order. Fixed by adding a
`SuperpowerController.OnAbilitiesChanged` event the HUD subscribes to and
rebuilds from, in addition to its original build-on-`Start()` attempt.

A final whole-branch review (after all 11 tasks individually passed their
own review) found the shipped code never implemented the spec's stated
"another ability already armed" guard — a player could double-arm aimed
abilities, silently wasting a charge. Fixed with
`FiredBubbleController.HasArmedOrInFlightSuperpower`, checked in
`SuperpowerController.TryActivate` before any charge is consumed. The same
review pass also made the `SuperpowerId` branching in `TryActivate` and
`SuperpowerEffectController.ResolveAffectedCells` exhaustive (throwing on
an unhandled value instead of silently doing nothing), so a careless future
5th ability fails loudly instead of compiling clean and doing nothing.

See [`features/superpowers/specs/2026-09-10-superpowers-design.md`](features/superpowers/specs/2026-09-10-superpowers-design.md)
for the full spec (now carrying an "Implementation notes" section with the
full account of the above), the plan at
[`features/superpowers/plans/2026-09-10-superpowers-implementation.md`](features/superpowers/plans/2026-09-10-superpowers-implementation.md),
and [`features/superpowers/overview.md`](features/superpowers/overview.md)
for the original open questions (now answered).

### Known follow-ups (untuned/non-blocking)

- ~~`GameStateManager.Freeze(0f)` or a negative duration would pause both
  timers permanently.~~ ✅ Fixed in the 2026-11-03 maintenance pass (see
  below).
- ~~Two small member-ordering nits (`ApplyActivation` over the line cap,
  `ArmSuperpower` among private methods).~~ ✅ Fixed in the 2026-11-03
  maintenance pass.
- Unlock levels (3/6/9/12), charges-per-level (1 for all four), and Bomb's
  radius are all untuned placeholders pending playtesting — see the spec's
  own "Open questions / tuning knobs" section.

## Phase 2.5 — Curated level patterns + level-transition animation ✅ (implemented)

Designed against the real Phase 1/2 codebase on 2026-09-11, implemented
the same day via a 10-task plan (subagent-driven development). Levels 1-5
now use hand-curated bubble patterns instead of pure-random fill —
separated hex blobs (1), vertical color stripes (2), horizontal stripes
(3), and horizontal-band mixes of those (4-5) — generated by new
`HexBlobPlacer`/`StripePlacer`/`PatternLevelGenerator` classes, routed by
a new `LevelContentGenerator` alongside the untouched `LevelGenerator`
(still used for level 6+, which has no curated recipe).
`DifficultyCurveConfig`'s four knobs became Inspector-editable
`AnimationCurve`s instead of hardcoded linear ramps, folding the old
level-1 special case into each curve's first keyframe. Level transitions
now play a staggered top-to-bottom bubble build-in animation
(`BubbleSpawnAnimator`) instead of popping in instantly; mid-level
ceiling pushes stay instant on purpose.

A real plan defect was found and fixed during implementation: the
original design called for deleting any bubble left disconnected from
the ceiling after generation (matching `LevelGenerator`'s existing
behavior). For curated patterns this is the *norm*, not a rare accident —
gap-separated blobs and banded gaps are deliberately disconnected by
design — so deleting on disconnect would have silently gutted most of
every pattern level's content. Replaced with
`ConnectFloatingCellsToCeiling`, which bridges each disconnected
component to the ceiling with a single same-color vertical stem instead.

A final whole-branch review (after all 10 tasks individually passed their
own review) found the shipped hex-blob radius (2) needed more rows than
levels 4-5's band math could guarantee on realistic device screens, so
their hex-blob region could silently render empty. Fixed by clamping the
effective blob radius to whatever a band can actually hold, down to a
single cell in the worst case, so a tight band always shows *something*.
The same pass reinstated value clamps on `DifficultyCurveConfig.ForLevel`
that the `AnimationCurve` refactor had dropped (a hand-edited curve in
the Inspector — which the design explicitly invites — could otherwise
throw `IndexOutOfRangeException`/`DivideByZeroException`), added a guard
against an empty/null region list, and restored a small testable
`LevelContentGenerator` router so the level-6+ fallback path isn't only
verified by an unautomated Play-mode pass.

See [`features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md`](features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md)
for the full spec and
[`features/level-content/plans/2026-09-11-curated-levels-and-transitions-implementation.md`](features/level-content/plans/2026-09-11-curated-levels-and-transitions-implementation.md)
for the implementation plan.

### Known follow-ups (untuned/non-blocking)

- `HexBlobRadius`, `HexBlobGapCells`, `StripeWidth`, `RegionGapRows` (on
  `DefaultPatternLevelCatalog.asset`) and the difficulty curve's exact
  keyframe shape are all first-guess defaults pending playtesting.
- The connectivity-bridging fix leaves a thin single-color stalk visible
  wherever a blob/band would otherwise be disconnected — an accepted
  trade-off, not a bug, but worth a look against real playtesting
  screenshots.
- ~~The shooter stays active during the ~1s level-load build-in
  animation.~~ ✅ Fixed in the 2026-11-03 maintenance pass. Firing is now
  locked during the build-in, while aiming stays live.
- ~~Leftover `FallingBubble` sprites can overlap a new level's build-in
  animation.~~ ✅ Fixed in the 2026-11-03 maintenance pass.

## Phase 3 — Local split-screen battle mode ✅ (implemented)

Designed against the real Phase 1/2 codebase on 2026-09-15, implemented
the same day via an 11-task plan (subagent-driven development). This is
explicitly the first and simplest of several battle-mode variants
planned — later variants get their own design sessions.

Two full instances of the existing Phase 1 stack run side by side, split
top/bottom, Player 2's half rotated 180° for face-to-face tabletop play —
achieved by rotating only the camera transform and a UI parent
`RectTransform`, with zero changes to any of Phase 1's board-fit or
trajectory math (both already rotation-invariant, reading only camera
position and orthographic size). The autonomous ceiling-descent timer is
replaced entirely: a new `Game.Battle` layer (attack economy, per-side
outcome tracking, match settlement) converts each player's matches/
cascades into rows pushed onto the *opponent's* board via the existing
`GameBoard.PushRowDown()`. Losing and winning-by-clearing both reuse
Phase 1's existing signals unchanged; a simultaneous-end case (both
boards finish the same frame) resolves as a draw. Superpowers are
disabled for this first variant.

Two deliberate simplifications from the spec's literal wording turned
out sound once building against the real generators: no new
`BattleBoardConfig` class was needed — `LevelContentGenerator`/
`LevelGenerator`/`PatternLevelGenerator` are already fully deterministic
from a shared level number alone, so picking one random level number per
match and loading it on both boards gives identical, varied content for
free. And the Player 2 180° rotation, flagged in the spec as needing a
feasibility spike, turned out fully specifiable in the implementation
plan without one — `Camera.rect`/`transform.rotation` and a rotated UI
parent `RectTransform` are both well-understood, orthogonal Unity
mechanisms.

**Three real bugs were found and fixed during the plan's final
verification task**, well after the individual tasks first "shipped":
(1) `PendingRowsMeter`'s accumulated attack value wasn't reset on
Rematch — fixed with `PendingRowsMeter.Reset()`/
`BattleAttackController.ResetForRematch()`, wired into
`BattleMatchController.Rematch()`. (2) Battle Mode's boards spawned
completely empty in real play — the boards reused the solo difficulty
curve, whose headroom-row tuning (9 rows at level 1) exceeds Battle
Mode's much shorter ~7-row boards, and the generators' existing,
deliberately-tested "headroom exceeds playfield → empty grid" behavior
correctly (if unhelpfully) kicked in. Fixed with a new
`BattleDifficultyCurve.asset` (same `DifficultyCurveConfig` type, a low
flat headroom curve) wired into both battle boards instead of the solo
default — no changes to the shared, tested generator code. (3)
`BattleAttackController` could push multiple rows onto the opponent in
one synchronous loop with nothing stopping it once the board was already
fully pushed, driving `GridModel.RowsPushed` past `Rows` and crashing
`BubbleLandingResolver`. Fixed by bounding the push loop
(`opponentBoard.Grid.RowsPushed < opponentBoard.Grid.Rows`) rather than
touching the shared `GridModel`/`BubbleLandingResolver` code.

A final whole-branch review (after all 11 tasks individually passed
their own review) found two more real bugs, both the same "leftover
per-match state survives Rematch" class as bug (1) above:
`BattleAttackController.enabled = false` at match end was a structural
no-op (the class has no `Update`/`OnEnable`/`OnDisable`, so Unity's
`enabled` flag never actually stopped its event-driven attack banking —
fixed with an explicit `if (!enabled) return;` guard in its handlers),
and `BattleShotClock`'s internal timer wasn't reset on Rematch either (a
regression against solo mode's own `GameStateManager.ResumeWithLevel`
precedent for the identical transition — fixed with
`BattleShotClock.ResetForRematch()`).

See
[`features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md`](features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md)
(now carrying an "Implementation notes" section) for the full spec, and
[`features/battle-mode/plans/2026-09-15-simple-attack-battle-mode-implementation.md`](features/battle-mode/plans/2026-09-15-simple-attack-battle-mode-implementation.md)
for the implementation plan.

### Known follow-ups (untuned/non-blocking)

- `BattleAttackConfig`'s formula constants and `BattleDifficultyCurve`'s
  headroom value are first-guess placeholders pending real 1v1
  playtesting — same caveat as every other tunable config in this
  project.
- ~~A narrow, deliberately-parked residual: a single-frame console
  exception at the exact instant a board's wall is fully pushed
  (`RowsPushed == Rows`).~~ ✅ Fixed in the 2026-11-03 maintenance pass
  with a single bounds check in `BubbleLandingResolver`. The deferred
  disable timing is untouched.
- No battle-mode shot-timer countdown UI exists — `BattleShotClock`
  exposes no `TimeRemaining` for a display to bind to, and none was
  promised by the spec for this first variant. `ShotTimerDisplay` (solo
  mode's equivalent) was excluded from the battle scene since it
  hard-depends on the excluded `GameStateManager`.

## Maintenance pass (2026-11-03) ✅

A bug-fix and cleanup pass covering one playtesting bug and the known
follow-ups above. No new features. All 148 EditMode tests pass, and each
fix was checked in Play mode via Unity MCP.

- **Bubbles left floating after popping a chunk (playtesting bug).** The
  player stacked bubbles beneath a same-color chunk that hung from the
  wall, then popped the chunk. The stack stayed in mid-air instead of
  falling.
  - **Root cause:** `MatchResolver.FindFloatingCells` treated the topmost
    occupied row as the ceiling. Once a pop cleared the whole
    wall-adjacent row, the next row down became a fake ceiling.
  - **Fix:** it now seeds from the row the wall actually touches,
    `GridModel.RowsPushed`.
  - It looked tied to the wall advancing only because push-triggering
    landings coincide with pops, and the push rebuilds every sprite.
  - Full account in
    [`matching-and-popping.md`](features/core-gameplay/matching-and-popping.md)
    ("Third bug").
- **Superpower pops never dropped floating clusters** (found while tracing
  the above). Bomb, Row Clear and Rainbow now run the same check through
  a new shared `GameBoard.DropFloatingCells()`.
- **Freeze guard.** `Freeze` ignores a duration ≤ 0. A freeze still
  active at level end no longer carries into the next level
  (`ClearFreeze` in `ResumeWithLevel`).
- **Style nits.** `SuperpowerController.ApplyActivation` is back under the
  line cap, and `FiredBubbleController.ArmSuperpower` now sits ahead of
  the private methods.
- **Battle Mode crash at a fully pushed wall.**
  `BubbleLandingResolver.UnoccupiedEffectiveCeilingRowCells` returns no
  candidates once `RowsPushed >= Rows`. This covers both the per-frame
  preview and a bubble still in flight on the losing side.
- **Level transitions:**
  - New `ShooterBuildInGate` (wired into both scenes) plus
    `ShooterController.IsFireLocked` block firing while
    `GridDebugRenderer.IsBuildingIn` is true. This applies to solo level
    loads and to Battle Mode match start and Rematch.
  - Leftover `FallingBubble` sprites are now tracked and destroyed on
    level load.

Spec "Implementation notes" for
[superpowers](features/superpowers/specs/2026-09-10-superpowers-design.md),
[battle mode](features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md)
and
[level content](features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md)
carry the details.

## Phase 4 — Game flow & pet customization ✅ (implemented, final screen art)

A proper front end for the game, ahead of the art pass. Details are in
[`features/game-flow/overview.md`](features/game-flow/overview.md).

- **Scene cleanup:**
  - removed the unused URP template leftovers
    (`Settings/Scenes/URP2DSceneTemplate.unity`,
    `Settings/Lit2DSceneTemplate.scenetemplate`);
  - renamed `SampleScene.unity` → `Game.unity`, keeping its GUID;
  - the build order is now MainMenu, PetCustomization, Game, BattleMode,
    SettingsMenu;
  - scene names are centralized in `Flow/SceneNames`.
- **Main menu:** a placeholder title plus New Game, Continue, 2 Players
  and Settings. Continue stays disabled until a solo run exists.
- **Pet customization (new scene):** the player names the cat and the
  dog and picks a fur color (6) and a shape (4) for each, from
  `DefaultPetAppearanceCatalog`. Start saves both pets, begins a fresh
  run (level 1, superpower unlocks reset) and loads the game.
- **Continue:** `SoloRunSave` and `SoloRunRecorder` remember the level
  the player was last on. `GameBoard.startAtSavedSoloLevel`, ticked only
  in `Game.unity`, opens there.
- **Victory and game over:** unchanged (`LevelResultScreen`). Distinct
  presentations come with the art pass.
- **Shared runtime UI factory** (`UI/RuntimeUi`): the new screens build
  their placeholder widgets through it.
- **Tests:** `CyclicIndexTests` covers the picker wraparound and the
  index clamping.

### Screen art (Art Phases 1b, 2 and 6b, screens only) ✅
- Every pre-game screen and result overlay is now skinned with the
  Claude Design menu art:
  - main menu;
  - two-step aimer/firer customization;
  - settings;
  - victory overlay;
  - knock-out → Retry screen;
  - 2-player banners and stats;
  - paw-iris transition.

  See the "Screens: as built" section of
  [`features/art/overview.md`](features/art/overview.md).
- New tests:
  - `FrameClockTests`
  - `TransitionTimelineTests`
  - `SpawnSlotTests`
  - `CrossingPathTests`
  - `FloaterPickTests`
  - `CustomizationFlowTests`
  - `MatchStatsTests`
  - `ParticleMotionTests`

### Known follow-ups
- Pet names and appearance aren't shown in-game yet. That waits on the
  in-game art (Art Phases 4 and 7), as do the launcher pets on the
  2-player result.
- The HUD canvas still uses Constant Pixel Size. It moves to the pixel
  canvas with Art Phase 6.

## Later / not yet scoped ⏳

- Meta progression (level select map, currency, unlocks).
- Monetization (ads/IAP) — architecture implications deferred until scope
  is chosen.
- Real art pass — swap placeholder assets for final art. **Now planned:**
  chibi cats and dogs in two pixel-art directions: 16-bit arcade for the
  board and HUD, and a Celeste-inspired atmospheric style for the menus,
  customization and result overlays. It runs in parallel with tuning.
  See [`features/art/overview.md`](features/art/overview.md) and the
  Claude Design prompts in
  [`features/art/claude-design-prompt.md`](features/art/claude-design-prompt.md)
  (in-game) and
  [`features/art/claude-design-prompt-menus.md`](features/art/claude-design-prompt-menus.md)
  (screens).
