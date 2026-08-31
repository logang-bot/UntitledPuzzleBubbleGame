# Local Split-Screen Battle Mode — Placeholder

**Status: not yet designed.** This file exists to reserve the folder
structure. Do not implement against this document — it is not a spec.

## Origin idea

From initial brainstorming: a same-device local multiplayer mode where the
screen splits in half and two players battle, with the goal of destroying
all of the opponent's bubbles.

## Decided so far (constraints, not full design)

- **Orientation**: portrait, screen split **top/bottom** (confirmed during
  Phase 1 brainstorming since it affects overall app orientation strategy —
  see `../../ROADMAP.md`). Each player's board occupies half the screen,
  shooter aiming toward the middle.
- Depends on Phase 1's `GridModel`/`ShooterController`/`MatchResolver`
  running as two independent, simultaneous instances (one per player) — the
  manager-based architecture with events (see `../../architecture/overview.md`)
  was chosen partly to make this feasible without a rewrite.

## Why this is deferred

This mode needs Phase 1's core systems to exist and be proven solid before
designing how two instances of them interact (garbage-bubble exchange,
input handling for two simultaneous local players on one device, win/loss
across two boards). Bring this back to a dedicated brainstorming session
once Phase 1 is playable.

## Known open questions for that session

- Win condition mechanics: most likely a garbage-bubble exchange (clearing
  N bubbles sends rows to the opponent's board, arcade-versus-mode style)
  rather than literally "destroy all bubbles" via direct attack — needs
  confirming.
- Input handling: two players sharing one device (e.g. touch zones split by
  screen half) — needs its own input scheme, likely reusing Unity's Input
  System but with two independent action maps/pointers.
- Does the shot timer / ceiling descent from Phase 1 carry over unchanged,
  or does battle mode replace/modify them (e.g. no independent ceiling
  descent per board, only garbage-driven pressure)?
- Whether superpowers (see `../superpowers/overview.md`) are enabled in
  this mode from the start, or added later.
- Match length/pacing for a 1v1 session on mobile (should a match be short
  enough for quick sessions?).

See [`../../ROADMAP.md`](../../ROADMAP.md) — this is Phase 3.
