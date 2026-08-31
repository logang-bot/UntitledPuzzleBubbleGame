# Untitled Unity Mobile Game — Docs

A Puzzle Bobble/Bust-a-Move-inspired mobile bubble shooter, with two planned
additions: bubbles with special "superpower" abilities, and a local
split-screen 2-player battle mode.

## Start here

- [`ROADMAP.md`](ROADMAP.md) — phased build order, what's designed vs. still
  needs its own brainstorming session.
- [`architecture/overview.md`](architecture/overview.md) — the overall code
  structure and conventions for the project.

## Feature docs

- `features/core-gameplay/` — fully designed. The single-player bubble
  shooter: grid, shooter, matching, timers, level generation, win/loss.
- `features/superpowers/` — **placeholder only**. Not yet designed.
- `features/battle-mode/` — **placeholder only**. Not yet designed.

## How these docs work

Each feature doc under `core-gameplay/` captures the decisions made during
brainstorming, an implementation sketch (not full code — just enough to know
what classes/fields to start with), and open questions to revisit while
building. As implementation progresses, keep these docs in sync with reality
rather than treating them as frozen specs — update them when a decision
changes.
