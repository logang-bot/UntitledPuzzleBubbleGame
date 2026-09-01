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
2. **Shooter + aim input** (touch drag) with kinematic trajectory preview
   (including wall bounces).
   → [`shooter-and-trajectory.md`](features/core-gameplay/shooter-and-trajectory.md)
3. **Firing a bubble** — move it along the previewed path, snap to the
   nearest empty grid cell on collision.
4. **Match detection** (3+ connected same-color bubbles via flood fill) +
   popping.
   → [`matching-and-popping.md`](features/core-gameplay/matching-and-popping.md)
5. **Floating cluster detection** (bubbles disconnected from the ceiling
   after a pop) + drop.
6. **Shot timer** — countdown per turn, auto-fires at current aim on
   expiry.
   → [`shot-timer-and-ceiling-descent.md`](features/core-gameplay/shot-timer-and-ceiling-descent.md)
7. **Ceiling descent timer** — pushes a new row down at a fixed interval;
   interval shortens with difficulty.
8. **Win/loss conditions** — board cleared = win, ceiling reaches the
   shooter line = loss.
   → [`win-loss-conditions.md`](features/core-gameplay/win-loss-conditions.md)
9. **Procedural level generator** with difficulty knobs (color count,
   density, row count).
   → [`level-generation.md`](features/core-gameplay/level-generation.md)
10. **Minimal HUD** (score, shots fired, level indicator) + level-complete
    and game-over screens.
11. **First playable build on a physical device** — verify touch input
    feels right and performance is acceptable.

## Phase 2 — Superpowers system 🚧 (placeholder)

Not yet designed. The plan is to brainstorm this once Phase 1 is playable,
so the ability system can be designed against real match/pop events instead
of speculative ones.

Known constraints from the original idea (to be scoped properly in that
session): special bubbles carrying multiple abilities (freeze the screen,
blow up sections of bubbles, etc.); still open — how they're introduced
(spawn rate vs. player-chosen loadout vs. unlock progression), and how they
interact with the shot timer/ceiling descent.

See [`features/superpowers/overview.md`](features/superpowers/overview.md).

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
