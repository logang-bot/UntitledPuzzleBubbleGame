# Untitled Unity Mobile Game — Docs

A Puzzle Bobble/Bust-a-Move-inspired mobile bubble shooter, with two planned
additions: bubbles with special "superpower" abilities, and a local
split-screen 2-player battle mode.

## Start here

- [`GAME_OVERVIEW.md`](GAME_OVERVIEW.md) — what the game currently does,
  from a player's perspective. Check this first to see what mechanics
  already exist before starting a new session.
- [`ROADMAP.md`](ROADMAP.md) — phased build order, what's designed vs. still
  needs its own brainstorming session.
- [`architecture/overview.md`](architecture/overview.md) — the overall code
  structure and conventions for the project.

## Feature docs

- `features/core-gameplay/` — fully implemented. The single-player bubble
  shooter: grid, shooter, matching, timers, level generation, win/loss.
- `features/superpowers/` — fully implemented (Phase 2). See
  `features/superpowers/specs/` for the design spec and
  `features/superpowers/plans/` for the implementation plan.
- `features/level-content/` — fully implemented. Curated bubble patterns
  for levels 1-5 and the level-transition build-in animation. See
  `features/level-content/specs/` for the design spec and
  `features/level-content/plans/` for the implementation plan.
- `features/battle-mode/` — first variant **fully implemented** (Phase 3).
  See `features/battle-mode/specs/` for the design spec and
  `features/battle-mode/plans/` for the implementation plan.

## How these docs work

Each feature doc captures the decisions made during brainstorming, an
implementation sketch (not full code — just enough to know what
classes/fields to start with), and open questions to revisit while
building. Once a feature has its own dedicated design session (like
`superpowers/`), its docs live under a `specs/` (and, once implementation
starts, `plans/`) subfolder instead of a single flat file. As
implementation progresses, keep these docs in sync with reality rather
than treating them as frozen specs — update them when a decision changes.
