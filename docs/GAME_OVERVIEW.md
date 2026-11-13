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
mode (first variant built).

## Menus and your pets

- **Main menu:** New Game, Continue, 2 Players and Settings, over a deep
  space sky where galaxies and little cats and dogs drift by. The title
  wordmark is still a placeholder ("Untitled Pet Game").
- Screens change with a paw-print iris that closes and reopens.
- **New Game** opens **pet customization**, under a dusk sky, in two
  steps:
  1. Choose which pet aims (cat or dog) and style it.
  2. Style the other pet, the firer.

  Each pet gets a name, a fur color (6 options) and a shape (4 options).
  Start saves them and begins a fresh run at level 1. Superpower unlocks
  reset with the run.
- **Continue** resumes the solo run at the level you were last on, even
  after a game over. It's disabled until you've started a run.
- **Results:** clearing a level shows a celebration overlay. Losing knocks
  the pets out: after a short pause, a Retry screen shows them napping.
  Battle Mode shows win/lose banners, then each player's match stats.
- The pets appear on the menus, customization and the Retry screen, and
  they work the launcher in game: the aimer turns it (the dog cranks, the
  cat steers a wheel) and the firer pulls a lever or presses a button. They
  react to play: hurrying when time runs short, cheering on big pops,
  panicking before a ceiling push and getting knocked out on a loss, in
  their saved fur color and shape.

## Core gameplay loop

- **The board** is a hex grid that fills the phone's full width and runs
  from the top of the screen down to the danger line on top of the
  launcher deck (the bottom 169 pt of the guide's 360 pt-wide layout).
  The row count is computed per device: 15 rows on a 9:19.5 phone, 11 on
  9:16. The empty "headroom" rows each level starts with are a share of
  the board (45% at level 1, down to 15% by level 10), so every phone
  starts about equally full. The look changes every 5 levels: living room, garden, rooftop at
  dusk, park at night.
- **Aiming and firing**: a gear-driven launcher at the bottom of the
  screen. Hold one of the two rotate slabs (side by side on your thumb's
  side; Settings picks left or right) to turn the aim at a fixed speed
  (not drag-to-angle), follow the dotted aim line (including wall
  bounces, ending where the shot will land), and tap the launcher's
  pointer to shoot. The loaded bubble sits in the pointer and the next
  one waits in its ring; after each shot it feeds up the tube.
- **Landing**: the fired bubble snaps into the nearest empty grid cell at
  its point of contact, with a short slide-and-settle animation on arrival
  (two visual styles available, see Settings below).
- **Matching**: connecting 3 or more same-color bubbles pops them. Any
  bubbles left disconnected from the ceiling after a pop fall away too.
- **Shot timer**: a 12-second countdown runs each turn, shown as 12 pips
  around the pointer. If you don't fire in time, the game auto-fires at
  your current aim. In the last 4 seconds the pips flash and a shout
  bubble counts down 4 → 1.
- **Ceiling descent**: on a separate, fixed interval (which shortens as
  levels get harder), the entire board pushes down one row — a screen
  shake and a flashing glow under the ceiling warn you first, and the
  board slides down with the growing ceiling band.
- **Winning and losing**: clear the whole board to win a level; if the
  descending ceiling reaches your shooter line, you lose.
- **Levels**: the first five levels have hand-curated bubble patterns —
  level 1 fills with separated single-color hexagonal blobs, level 2 with
  vertical color stripes, level 3 with horizontal stripes, and levels 4-5
  mix those patterns in horizontal bands. Level 6 onward falls back to
  procedural generation (same level number always produces the same
  layout), with difficulty — color count, bubble density, starting row
  count, ceiling-descent speed — ramping up via an Inspector-editable
  curve as you progress. Early levels get extra "headroom" rows of
  breathing room.
- **Level transitions**: bubbles build in with a staggered top-to-bottom
  pop-in animation when a new level loads, instead of appearing all at
  once. Mid-level ceiling pushes still happen instantly, on purpose.
- **HUD**: score, shots fired and level sit in the deck beside the
  superpower buttons, opposite the rotate slabs. Score rewards bigger
  matches and cascade drops more. A win/loss screen lets you retry the
  same level or advance to the next one.
- **Settings**: a menu (reachable from the main menu) lets you pick
  between two landing-animation styles ("overshoot bounce" vs.
  "squash/pop") and which thumb works the rotate slabs (left or right),
  both saved between sessions.

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
Rainbow) can be armed at a time. An armed ability sits in the launcher's
pointer as its own bubble (a bomb with a lit fuse, a row-clear bubble, a
spinning rainbow), and each has its own effect: an explosion, a sweep
across the row, or a frosty frame and snowflakes while frozen.

## Battle Mode

A local split-screen 2-player versus mode (the first, simplest of
several planned variants), reachable via "2 Players" on the main menu:

- **Setup**: the phone lies flat between two players facing each other.
  The screen splits top/bottom, each player's board filling their half,
  with Player 2's half rendered upside-down (rotated 180°) so it reads
  right-side-up from their side of the device — their touch controls are
  mirrored to match.
- **The core loop is otherwise identical to solo play** — same hex grid,
  aiming, firing, matching, and shot timer (12s auto-fire) — played
  independently and simultaneously by both players.
- **No autonomous ceiling descent.** Instead, matching or dropping
  bubbles on your own board pushes rows onto your *opponent's* board —
  bigger matches and cascades push more rows.
- **Winning, losing, and draws**: clear your own board to win instantly;
  if your opponent's attacks push your wall to your shooter line, you
  lose (they win). If both happen at the same instant, it's a draw. A
  result screen offers "Rematch" (both boards regenerate with fresh,
  matching content) or a return to the main menu.
- **Superpowers are disabled** in this first variant.
- **Each half has its own launcher deck** with one pet: the cat at the
  bottom, the dog at the top. Each player taps their own pointer to fire.
  The halves are short (7 rows on a 9:19.5 phone, 5 on 9:16), so Battle
  uses the procedural level generator instead of the curated patterns:
  both boards still get the same content.

See `features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md`.

## Not yet built

- **Meta progression** — no level-select map, currency, or unlocks beyond
  the superpower level-thresholds above.
- **Monetization** — no ads or IAP.

## See also

- [`ROADMAP.md`](ROADMAP.md) — phased build order and status, with the
  full history of bugs found/fixed and decisions made per milestone.
- [`architecture/overview.md`](architecture/overview.md) — code structure,
  components, and conventions.
- `features/core-gameplay/`, `features/superpowers/`,
  `features/level-content/`, `features/battle-mode/`,
  `features/game-flow/`, and `features/art/` — the detailed
  design docs behind everything summarized above.
