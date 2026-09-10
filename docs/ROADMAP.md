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

- `GameStateManager.Freeze(0f)` or a negative duration would pause both
  timers permanently (`TickFreeze` never reaches the point where it calls
  `Unfreeze()`) — dormant, since `SuperpowerController.freezeDurationSeconds`
  defaults to 5 in the Inspector; worth a one-line guard whenever this file
  is next touched.
- Two small member-ordering nits, introduced by the final-review fix pass
  itself and left as-is since there's no further fix round in that
  process: `SuperpowerController.ApplyActivation` (extracted to fix a
  different line-length breach) is itself still slightly over this
  project's function-body line cap, and `FiredBubbleController.ArmSuperpower`
  sits among private methods instead of ahead of them.
- Unlock levels (3/6/9/12), charges-per-level (1 for all four), and Bomb's
  radius are all untuned placeholders pending playtesting — see the spec's
  own "Open questions / tuning knobs" section.

## Phase 3 — Local split-screen battle mode 🚧 (placeholder)

Not yet designed. Depends on Phase 1's grid/shooter/match systems being
solid enough to run as two simultaneous instances.

Known constraints from the original idea (to be confirmed/expanded in that
session): portrait orientation, screen split top/bottom, two independent
boards, goal is to clear the opponent's board — most likely via a
garbage-bubble mechanic where clearing bubbles sends rows to the opponent's
board, in the style of the arcade version's versus mode.

See [`features/battle-mode/overview.md`](features/battle-mode/overview.md).

## Later / not yet scoped ⏳

- Meta progression (level select map, currency, unlocks).
- Monetization (ads/IAP) — architecture implications deferred until scope
  is chosen.
- Real art pass — swap placeholder assets for final art.
