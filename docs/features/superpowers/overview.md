# Superpowers — Placeholder

**Status: implemented.** The design session this file called for has
happened, and the resulting spec has been implemented — see
[`specs/2026-09-10-superpowers-design.md`](specs/2026-09-10-superpowers-design.md)
(now carrying an "Implementation notes" section) for the actual spec and
what shipped. This file is kept for its historical context below.

## Origin idea

From initial brainstorming: some bubbles carry special abilities (freeze the
screen, blow up sections of bubbles, etc.), on top of the normal
color-matching mechanic.

## Why this is deferred

Superpowers plug into the core match/pop loop (see
`../core-gameplay/matching-and-popping.md` and the event list in
`../../architecture/overview.md`) — abilities will most naturally be
designed as reactions to real events like `OnBubblesPopped`. Designing this
system before Phase 1 exists would mean guessing at hooks that don't exist
yet. Once Phase 1 is playable, bring this back to a dedicated brainstorming
session.

## Known open questions for that session (now answered)

- How players encounter superpower bubbles: random spawn chance, guaranteed
  placement by the level generator, player-chosen loadout before a level,
  or unlocked progression. → **Unlocked progression**, see the spec.
- The actual ability list beyond the two examples given (freeze screen,
  blow up a section) — what else fits, and how many should exist at launch.
  → **Freeze, Bomb, Row Clear, Rainbow** — 4 at launch, see the spec.
- How abilities interact with the shot timer and ceiling descent (e.g. does
  "freeze the screen" pause the ceiling timer too, or only the shot timer?).
  → **Freeze pauses both**; the other three don't touch timers at all.
- Whether abilities are single-use consumables, cooldown-based, or tied to
  matching the special bubble itself into a group. → **Fixed charges per
  level**, player-activated, not tied to matching.
- How this interacts with Battle Mode (see `../battle-mode/overview.md`) —
  abilities are likely to matter even more in a competitive context.
  → **Explicitly deferred** until Phase 3 is designed.

See [`../../ROADMAP.md`](../../ROADMAP.md) — this is Phase 2.
