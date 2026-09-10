# Superpowers System — Design Spec

**Status: implemented.** This is Phase 2 of
`docs/ROADMAP.md`, produced by a brainstorming session on 2026-09-10 against
the real Phase 1 codebase (event hooks, `GameBoard`/`GameStateManager`
internals) rather than the speculative version described in
`../overview.md`. This doc supersedes that placeholder as the actual spec;
`../overview.md`'s open questions are answered below.

## Origin & why now

From initial brainstorming: some bubbles carry special abilities (freeze the
screen, blow up sections of bubbles, etc.) on top of the normal
color-matching mechanic. Design was deliberately deferred until Phase 1 was
playable, so abilities could be designed against real events
(`OnBubblePlaced`, `OnBubblesPopped`, `OnClusterDropped`, `OnRowPushedDown`)
instead of guessed-at hooks — see `architecture/overview.md`'s "Events"
section, which already anticipated this.

## Decisions

These were the open questions in `../overview.md`; each is now resolved:

- **How players encounter superpower bubbles:** unlock progression, not
  random spawn, level-generator placement, or a pre-level loadout screen.
  Abilities are permanently unlocked as the player's furthest level reached
  crosses per-ability thresholds.
- **Ability list at launch (4):** Freeze, Bomb, Row Clear, Rainbow — see
  "Per-ability effects" below.
- **Trigger mechanism:** abilities are player-activated consumables (a HUD
  button), not bubbles sitting in the grid that get matched into a group.
  There is deliberately no new "special bubble" concept in `GridModel` —
  color remains the only per-cell data (see "Why no grid-level bubble
  type" below).
- **Interaction with the shot timer / ceiling descent:** only Freeze
  touches the timers, and it pauses both — see "Freeze" below. Bomb, Row
  Clear, and Rainbow don't touch timers at all; they resolve through the
  same board-mutation pipeline a normal match does.
- **Consumable model:** fixed charges per level (not a global
  cooldown or a persistent spendable currency) — simplest to reason about
  and fits the same "per-level config" shape `DifficultyCurveConfig`
  already uses.
- **Battle Mode interaction:** explicitly deferred — see "Out of scope."

## Why no grid-level bubble type

`GridModel` currently stores exactly two parallel arrays per cell —
occupancy and `BubbleColor` — nothing else. Adding a per-cell "this bubble
is special" flag would touch `LevelGenerator`, `MatchResolver`, and every
renderer that reads `GridModel`, for a feature that (per the trigger
decision above) doesn't need it: abilities are activated by the player, not
discovered by firing into the right bubble. Keeping `GridModel` untouched
is deliberate scope control, not an oversight.

## Progression & unlocking

- New enum `SuperpowerId { Freeze, Bomb, RowClear, Rainbow }`
  (`Scripts/Superpowers/SuperpowerId.cs`).
- New `ScriptableObject SuperpowerCatalog` (same shape as
  `DifficultyCurveConfig`), listing one `SuperpowerDefinition` per ability
  (`SuperpowerId`, `UnlockLevel`, `ChargesPerLevel`). Default asset values:
  Freeze unlocks at level 3, Bomb at 6, Row Clear at 9, Rainbow at 12; all
  start at 1 charge per level — untuned placeholders, same caveat
  `DefaultDifficultyCurve.asset`'s ramps carry.
- New static `SuperpowerProgress` class, mirroring the `GameSettings`/
  `PlayerPrefs` precedent from Milestone 11 (the project's first persisted
  preference): persists `HighestLevelReached`, updated whenever
  `GameStateManager` raises `OnLevelWon`. Exposes
  `IsUnlocked(SuperpowerId id, SuperpowerCatalog catalog)` comparing
  against the catalog's `UnlockLevel`.
- Tracking against *highest level ever reached* rather than the currently
  loaded level is deliberate: `GameStateManager.RetryLevel()` reloads an
  earlier level with the same deterministic seed, and an ability earned by
  reaching level 6 must stay available on a level-3 retry.

## Charge economy & activation

- New `SuperpowerController` `MonoBehaviour`: owns remaining charges per
  *unlocked* ability for the current level, as a small dictionary keyed by
  `SuperpowerId`. Reset from `SuperpowerCatalog` whenever `GameBoard`
  raises `OnLevelLoaded`.
- `TryActivate(SuperpowerId id)`:
  - Returns false with no effect if the ability is locked, has 0 charges
    remaining, or (for the aimed abilities) another ability is already
    armed.
  - **Freeze**: consumes a charge and calls
    `GameStateManager.Freeze(duration)` immediately — no aiming step.
  - **Bomb / Row Clear / Rainbow**: consumes a charge and arms
    `FiredBubbleController` for the next shot (see below) instead of firing
    anything itself.

## Targeting: aimed like a normal shot

Bomb, Row Clear, and Rainbow reuse the existing rotate-and-hold aim
mechanic (`ShooterController`) rather than introducing a tap-to-target
input mode. Pressing the ability button arms the *next* fired shot:

- `FiredBubbleController` gains a nullable `SuperpowerId? ArmedAbility`
  field, set by `SuperpowerController.TryActivate`.
- While armed, the "next bubble" indicator (currently a color swatch, see
  `firing-and-snapping.md`) shows the ability's icon instead.
- On `OnFireRequested`, if `ArmedAbility` is set, `FiredBubbleController`
  skips normal color placement on landing. It still uses the existing
  `OccupancyCollision`/`BubbleLandingResolver` path to resolve a landing
  cell (same trajectory, same truncation rules a normal shot gets), but
  instead of calling `GameBoard.PlaceBubble`, it raises a new event
  `OnSuperpowerLanded(SuperpowerId id, (int Row, int Col) cell)` and clears
  `ArmedAbility`.

## Per-ability effects & board integration

A new static, pure `SuperpowerEffectResolver` (same shape as
`MatchResolver` — no mutation, just cell-set computation) computes the
affected cells for each ability given the landing cell and the current
`GridModel`:

- **Bomb** — all occupied cells within a fixed radius of the landing cell.
- **Row Clear** — all occupied cells in the landing cell's row.
- **Rainbow** — flood-fills the same-color group of whichever *neighboring*
  bubble the landing cell is adjacent to (the largest such adjacent group),
  reusing the same `FloodFill` primitive `MatchResolver.FindMatchGroup`
  already uses, just seeded from a neighbor's color instead of the landing
  bubble's own (the landing cell itself has no color, since it never
  becomes a real bubble).

A new `SuperpowerEffectController` `MonoBehaviour` subscribes to
`FiredBubbleController.OnSuperpowerLanded`:

1. Calls `SuperpowerEffectResolver` for the resolved cell set.
2. Groups the resolved cells by their current `BubbleColor` (read from
   `GridModel` before popping).
3. Calls `GameBoard.PopCells(cells, color)` once per color group.

`GameBoard.PopCells(cells, color)` takes a single `BubbleColor` for its
whole batch (used for the pop event/VFX color) — Bomb and Row Clear can
span multiple colors in one activation, which that signature doesn't
support directly. Grouping by color and calling `PopCells` once per group
avoids changing `GameBoard`'s signature or its contract with existing
listeners. Because this goes through the same `PopCells`/`OnBubblesPopped`
path a normal match uses, `GameStateManager`'s win check, `ScoreTracker`,
and `GridDebugRenderer` all react correctly with zero special-casing —
consistent with the precedent `MatchProcessor`'s own doc comment set for
Phase 2 (see `architecture/overview.md`).

## Freeze

- `ShotTimer` gains two one-line methods, `Pause()`/`Resume()`, gating
  whether `Tick(float deltaTime)` advances `TimeRemaining`.
- New `GameStateManager.Freeze(float duration)` pauses both `_shotTimer`
  and `_ceilingTimer`, and resumes both once `duration` elapses — driven
  by a small countdown in `GameStateManager`'s existing `Update()` loop
  (no new timer class needed).
- Freeze deliberately pauses *both* timers, not just the shot timer: the
  player's mental model is "the whole board pauses," and pausing only one
  timer while the other keeps counting down would be a confusing partial
  freeze.

## UI & testing

- New `SuperpowerHud`: builds one button per *unlocked* ability at
  runtime, same "no prefab, built in code" pattern `HudDisplay`/
  `ShotTimerDisplay` already use. Each button shows the ability's icon and
  remaining charge count, and is disabled at 0 charges or while any
  ability is currently armed. Positioned clear of the existing bottom bar
  and rotate zones, following `HudDisplay`'s "clear the tallest zone"
  precedent from the Milestone 11 playtesting fixes.
- Pure logic — `SuperpowerEffectResolver`, `SuperpowerProgress`'s unlock
  check, `SuperpowerCatalog` charge lookups — gets EditMode unit tests,
  same as `MatchResolver`/`LevelGenerator`.
- The `MonoBehaviour` wiring (`SuperpowerController`,
  `SuperpowerEffectController`, the `FiredBubbleController` extension)
  gets a Play Mode check via the Unity MCP bridge, following Milestone
  10's HUD verification (`execute_code` driving activation/landing,
  screenshots confirming HUD state).

## New files

Each under the project's ~200-line file convention; function bodies under
~7 lines, ≤3 parameters (see `architecture/overview.md`'s "Code style"):

- `Scripts/Superpowers/SuperpowerId.cs`
- `Scripts/Superpowers/SuperpowerDefinition.cs`
- `Scripts/Superpowers/SuperpowerCatalog.cs`
- `Scripts/Superpowers/SuperpowerProgress.cs`
- `Scripts/Superpowers/SuperpowerController.cs`
- `Scripts/Superpowers/SuperpowerEffectResolver.cs`
- `Scripts/Superpowers/SuperpowerEffectController.cs`
- `Scripts/UI/SuperpowerHud.cs`

## Small additions to existing files

- `ShotTimer` — `Pause()`/`Resume()`.
- `GameStateManager` — `Freeze(float duration)`.
- `FiredBubbleController` — `ArmedAbility` state + `OnSuperpowerLanded`
  event.

## Out of scope

- **Battle Mode interaction** (`../../battle-mode/overview.md`) — Phase 3
  is still an undesigned placeholder. This design only requires abilities
  to go through the same generic `GameBoard` events Phase 3 will already
  consume (`OnBubblesPopped` etc.), so it doesn't block that future
  design, but it doesn't attempt to answer battle-mode-specific questions
  (e.g. do abilities affect the opponent's board) either.
- **Real art** — icons reuse the placeholder-shape convention already used
  for bubbles (see `ROADMAP.md` Phase 0), pending a later art pass.
- **Meta-progression beyond unlock thresholds** — no currency, no
  loadout-selection screen. Once unlocked, an ability is simply available
  every level, subject only to its per-level charge count.

## Implementation notes

Implemented via an 11-task plan
(`plans/2026-09-10-superpowers-implementation.md`) the same day as this
spec, using subagent-driven development (a fresh implementer per task, a
scoped task review after each, a final whole-branch review, one fix wave,
and a scoped re-review of that fix wave). Everything below the "Decisions"
section above matched what shipped; this section records what changed or
was discovered only once the pieces existed together.

- **`FiredBubbleController` flying-bubble cleanup (found during Task 7).**
  The plan's original sketch for the new superpower-landing branch in
  `Land(...)` didn't account for the flying-bubble sprite — unlike the
  pre-existing normal-color path, it never destroyed `_flyingBubble` or
  cleared `_settleCell`, which would have left a ghost sprite on screen
  after every armed shot. Fixed by extracting a shared
  `ClearFlyingBubble()` helper, called from both branches (this also
  brought the combined `Land(...)` method back under this project's
  function-body line cap, which the naive fix alone had breached).
- **`SuperpowerHud` build-order bug (found during Task 11's Play Mode
  verification).** `SuperpowerHud` built its button list once in `Start()`
  by reading `SuperpowerController.UnlockedAbilities` at that instant —
  but Unity does not guarantee `SuperpowerController.Start()` (which
  populates that data) runs first. Reproduced directly: with
  `SuperpowerHud.Start()` running first, the HUD showed zero buttons even
  with unlocks already persisted before Play began. Fixed by adding
  `SuperpowerController.OnAbilitiesChanged` (`public event Action`),
  raised at the end of its level-loaded handler after the charge tracker
  resets; `SuperpowerHud` subscribes in `Start()` and rebuilds its button
  set (clearing old buttons first) whenever it fires, in addition to its
  original build-on-`Start()` attempt — this closes the gap regardless of
  which component's `Start()` happens to run first.
- **Missing "already armed" guard (found by the final whole-branch
  review).** This spec's own "Charge economy & activation" section says
  `TryActivate` "Returns false with no effect if... another ability is
  already armed" — but neither Task 9's `SuperpowerController` nor Task
  10's `SuperpowerHud` actually implemented that check, so a player with
  charges for two aimed abilities could press both before firing,
  silently overwriting `FiredBubbleController`'s armed-ability state and
  wasting the first charge with zero board effect. Fixed with a new
  `FiredBubbleController.HasArmedOrInFlightSuperpower` property
  (`_armedAbility.HasValue || _firedAbility.HasValue`), checked in
  `TryActivate` before any charge is consumed. A related edge case — an
  armed shot that misses the board entirely (`landingCell == null`)
  wastes its charge with no feedback — was deliberately left unfixed: the
  final reviewer confirmed it self-heals (`_firedAbility` is unconditionally
  overwritten by the next `HandleFireRequested` call) and is the same class
  of pre-existing, accepted edge case as the near-full-board missed-shot
  question already noted in `firing-and-snapping.md`.
- **Non-exhaustive `SuperpowerId` branching (found by the final review).**
  `SuperpowerEffectController.ResolveAffectedCells` and
  `SuperpowerController.TryActivate` both silently did nothing/guessed on
  an unhandled `SuperpowerId` instead of failing loudly. Both now throw
  `ArgumentOutOfRangeException` on a value they don't explicitly handle,
  so a careless future 5th ability compiles clean but fails loudly the
  first time it's actually activated, instead of silently popping nothing
  with a charge already spent.
- **Two residual Minor style nits**, introduced by the final-review fix
  wave itself and left as-is (no second fix wave in this project's
  process): `SuperpowerController.ApplyActivation` (extracted from
  `TryActivate` to fix a different line-length breach) is itself still
  slightly over this project's function-body line cap; and
  `FiredBubbleController.ArmSuperpower`'s new position sits among private
  methods instead of ahead of them, per this project's public-before-
  private member-order convention. Both are purely cosmetic — no
  behavioral impact — and are candidates for a future small cleanup pass.

## Open questions / tuning knobs

- Unlock levels (3/6/9/12) and charges-per-level (1 for all four) are
  untuned placeholders, same caveat as `DifficultyCurveConfig`'s ramps —
  expect retuning after playtesting once Phase 2 is implemented.
- Bomb's radius is not yet a concrete number — needs a value picked during
  implementation and tuned against real board density.
- Whether an armed-but-unfired ability should be cancelable (e.g. a second
  tap on the same button) isn't decided; the simplest version (no cancel,
  charge is spent the moment the button is pressed) is assumed unless this
  comes up as a usability problem in playtesting.
