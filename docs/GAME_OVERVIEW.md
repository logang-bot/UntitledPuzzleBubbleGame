# Game Overview

What this game currently does, from a player's perspective. This is the
one place to check "what mechanics does the game already have" before
starting a new session — update it whenever a session changes what the
game *does*, not just how it's built. For the technical/code side, see
[`architecture/overview.md`](architecture/overview.md); for how a
mechanic was designed and built, see the relevant doc under `features/`.

## What the game is

A Puzzle Bobble/Bust-a-Move-style single-player bubble shooter for mobile
(portrait orientation), with two planned additions: bubbles with special
"superpower" abilities (built), and a local split-screen 2-player battle
mode (not yet built).

## Core gameplay loop

- **The board** is a hex grid that fills the entire phone screen, width
  and height, with no letterbox bars — the exact row count is computed
  per device so difficulty stays consistent across screen sizes.
- **Aiming and firing**: an arcade-style rotating gun at the bottom of the
  screen. Hold the left/right zones to rotate the aim at a fixed speed
  (not drag-to-angle), see a live trajectory preview line (including wall
  bounces), and tap the fire zone to shoot. A "next bubble" indicator
  shows the color of the upcoming shot before you fire.
- **Landing**: the fired bubble snaps into the nearest empty grid cell at
  its point of contact, with a short slide-and-settle animation on arrival
  (two visual styles available, see Settings below).
- **Matching**: connecting 3 or more same-color bubbles pops them. Any
  bubbles left disconnected from the ceiling after a pop fall away too.
- **Shot timer**: a 12-second countdown runs each turn. If you don't fire
  in time, the game auto-fires at your current aim. A numeric countdown
  only appears in the last 4 seconds.
- **Ceiling descent**: on a separate, fixed interval (which shortens as
  levels get harder), the entire board pushes down one row — a screen
  shake warns you just before it happens.
- **Winning and losing**: clear the whole board to win a level; if the
  descending ceiling reaches your shooter line, you lose.
- **Levels**: procedurally generated per level number (same level number
  always produces the same layout), with difficulty — color count, bubble
  density, starting row count, ceiling-descent speed — ramping up as you
  progress. Early levels get extra "headroom" rows of breathing room.
- **HUD**: score, shots fired, and current level shown in a bottom bar.
  Score rewards bigger matches and cascade drops more. A win/loss screen
  lets you retry the same level or advance to the next one.
- **Settings**: a menu (reachable from the main menu) lets you pick
  between two landing-animation styles ("overshoot bounce" vs.
  "squash/pop"), saved between sessions.

## Superpowers

Four special abilities the player can activate during a level, layered on
top of the core loop above:

- **Freeze** — activates instantly. Pauses both the shot timer and the
  ceiling descent for a few seconds, buying you breathing room.
- **Bomb** — arms your next shot. Aim and fire like normal; wherever it
  lands, it clears every bubble within a radius of the impact point,
  regardless of color.
- **Row Clear** — arms your next shot. Wherever it lands, it clears every
  bubble in that entire row.
- **Rainbow** — arms your next shot. Wherever it lands, it matches and
  clears the largest connected same-color group adjacent to the impact
  point, as if it were that color.

**Unlocking and using them:** each ability unlocks permanently once you
reach a certain level for the first time (currently Freeze at level 3,
Bomb at 6, Row Clear at 9, Rainbow at 12 — untuned placeholder values).
Once unlocked, an ability is available every level with a small, fixed
number of charges (currently 1) that refill each level — press its button
in the HUD to use a charge. Only one aimed ability (Bomb/Row Clear/
Rainbow) can be armed at a time.

## Not yet built

- **Battle Mode** — local split-screen 2-player versus mode. Only a
  placeholder idea exists: portrait split top/bottom, two independent
  boards, likely a garbage-bubble mechanic (clearing bubbles sends rows to
  the opponent). Not designed in detail yet.
- **Meta progression** — no level-select map, currency, or unlocks beyond
  the superpower level-thresholds above.
- **Monetization** — no ads or IAP.
- **Real art** — bubbles and UI are still placeholder shapes generated in
  code; no final art pass yet.

## See also

- [`ROADMAP.md`](ROADMAP.md) — phased build order and status, with the
  full history of bugs found/fixed and decisions made per milestone.
- [`architecture/overview.md`](architecture/overview.md) — code structure,
  components, and conventions.
- `features/core-gameplay/` and `features/superpowers/` — the detailed
  design docs behind everything summarized above.
