# Claude Design prompt: in-game art, vector cartoon

Replaces the 16-bit direction of
[`claude-design-prompt.md`](claude-design-prompt.md) for the game screen:
board, bubbles, launcher, effects, backgrounds and the in-game HUD. **The pets
stay pixel art**, as on the menus (see [`overview.md`](overview.md)).

**Why:** the reasons for the menu rework apply even more to the board:
- The camera fits the board to the screen width, so on most phones pixel art
  would be scaled by an uneven amount.
- Almost everything on the board moves between pixel positions: the bubble in
  flight, the rotating aim, falling bubbles, the settle and build-in
  animations.
- The HUD numbers would have the same font problem the pixel menus had.

Cartoon art exported at 3× and scaled smoothly avoids all three, and the game
screen then matches the menus.

**How to use:** paste the prompt below the line into the Claude Design project
that holds `Art Guide.dc.html` (with `sprites.js` and `support.js`) and
`Cartoon Menu Guide.dc.html`. If they're in different projects, use the one
with the Art Guide and attach or paste the cartoon guide's tokens, so it can
match them. Review the mockups on a phone, then click "Download Unity bundle".

**What came back (2026-11-12):** `Cartoon Game Guide.dc.html`, with
`game-art.js` and `game-engine.js`. We accepted most of it, including the new
controls: both rotate slabs side by side on the thumb side, fire by tapping the
launcher's pointer, and right- and left-hand layouts, which need a new setting.
We also accepted dropping the ghost landing bubble, since the aim line ends at
the landing cell. Two things changed in a follow-up prompt:
- The guide put the HUD and superpower row in a top strip, with the ceiling
  starting 112 pt down. That costs 2–3 board rows per phone, so we kept the
  board full-height and moved the HUD into the bottom deck.
- Its motion spec's gameplay numbers (rotate 70°/s, bomb radius 1.6 cells, a
  10 s Battle shot clock) didn't match our tuning, which wins.

The follow-up prompt:

> In `Cartoon Game Guide.dc.html`, move the HUD (score, shots, level) and the
> superpower buttons out of the top strip and into the bottom deck, so the
> board fills the screen from the top edge down to the danger line, as it does
> in the game today.
>
> - The ceiling band starts at y 0. It may sit behind the top 32 pt safe area,
>   since nothing interactive goes there.
> - Fit the HUD values and the 4 superpower buttons into the deck, on the side
>   opposite the rotate slabs, mirrored for the left-hand layout. If they don't
>   fit beside the mechanism, a slim row directly above it is fine.
> - Keep the deck as short as you can: ideally no taller than the current
>   140 pt mechanism, at most about 170 pt. Every extra point costs board rows.
> - Keep the HUD visually separate from the board: the dark menu track and
>   cream icons, as now. Superpower buttons may shrink, but stay at least 44 pt
>   touch targets with a readable charge badge.
> - Put the countdown and timer pips somewhere that still reads at a glance
>   while aiming.
> - The Battle half doesn't change.
>
> Also set the motion spec's "code" entries to our tuned gameplay values, so
> the mockups match the game: `launcher_rotate` 25°/s, bomb radius 2 cells,
> Battle shot clock 12 s. Keep everything else. Then update `layout.json` and
> the Unity bundle to match.

**Follow-up for the pet shapes (sent 2026-11-12, imported):** the game pet sheets only
have one shape per species. Paste this into the same project:

> In `Cartoon Game Guide.dc.html`, the in-game pixel pets (`sprites.js`,
> exported as `PixelSheets/game_pets_cat` and `game_pets_dog`) only come
> in one shape per species. Add the other three shapes of each, matching
> the menu portraits in `Menu Art Guide.dc.html`: cat round shorthair
> (the current one), fluffy longhair, slim, chonky; dog floppy ears (the
> current one), pointy ears, fluffy, short legs. Every strip needs all four
> shapes, with the same frame sizes, frame counts and fps, and the same fur
> key colors so `colorways.json` still recolors them. Name the strips
> `<species>_<shape>_<anim>` with the shape ids round, fluffy, slim, chonk
> (cat) and floppy, pointy, fluffy, short (dog), keep the current strips as
> the round and floppy shapes, and re-export the pixel sheets.

---

## Prompt

Redesign the **in-game art** of this mobile bubble shooter in the **same
vector cartoon style as `Cartoon Menu Guide.dc.html`**, keeping **the pets in
pixel art**. Create a new guide, `Cartoon Game Guide.dc.html`. Keep the
**content and gameplay** from `Art Guide.dc.html`, `sprites.js` and
`support.js`. Only the style changes, plus anything you need to change for the
style to work on a busy board.

### What the game screen is

- Portrait phone. A hex-packed board of bubbles, **8 columns** wide, with odd
  rows offset by half a bubble, so one row spans 8.5 bubble widths. The board
  **fills the full screen width**, with no letterbox. Taller phones get more
  rows.
- At the bottom center, a launcher rotates while the player holds the
  rotate-left or rotate-right button, at a fixed speed. A dotted trajectory
  line shows the path, including bounces off the side walls, and a ghost
  bubble shows where the shot will land. The fire button shoots. A
  next-bubble holder shows the upcoming color.
- 3+ connected bubbles of the same color pop. Anything left disconnected from
  the ceiling falls.
- A 12 s shot timer runs every turn. A big countdown appears only for the last
  4 seconds, then the game auto-fires.
- On a fixed interval the ceiling pushes the whole board down one row. A
  screen shake warns just before it. You lose when bubbles reach the launcher
  line. You win by clearing the board.
- Four superpowers, with charges: **Freeze** (pauses the timers), **Bomb**
  (clears a radius), **Row Clear** (clears a whole row) and **Rainbow** (a
  wildcard color).
- **Two pets work the launcher in solo play.** The player picks which one
  aims and which one fires (cat or dog, either way round). The aimer turns the
  launcher and the firer shoots.
- **2-player mode:** the screen splits into two 360×390 pt halves, each with
  its own board and launcher. The top half is rotated 180° (the phone lies
  flat between the players). The cat plays the bottom half, the dog the top
  half. Superpowers are off. Big pops send rows to push the opponent's
  ceiling down, and the timer is a shot clock.
- The menus, the victory overlay, the Retry screen and the 2-player result
  screens are **already done** in `Cartoon Menu Guide.dc.html`. Leave them
  out. Draw the board underneath them so they sit on top naturally.

### Style: same family as the cartoon menu guide

- Reuse the cartoon guide's **ink (`#1a1424`), outline family, color tokens,
  and its two fonts (Lilita One and Baloo 2)**. Add new tokens only where the
  board needs them, such as the bubble colors and background themes. Don't add
  a third font.
- Our own look: don't copy the board, launcher, characters, UI or layouts of
  any existing bubble shooter.
- Cozy and playful, like a pet-filled home and neighborhood at dusk and night.
  Never grim.

### The board must stay readable: rules stricter than the menus

The board is the busiest screen in the game, and the player reads it at a
glance mid-shot. Treat it as its own layer with tighter rules:

- **Bubbles:** a chunky ink outline, a flat base color, one simple highlight
  and at most a 2-stop shade. No textures and no full glossy 3-stop menu
  gradient if it adds noise.
- **Six bubble colors** (red, orange, yellow, green, blue, purple; you may
  tune the hues). They must differ in **value as well as hue**. Show all six
  in grayscale and under deuteranopia and protanopia simulations in the guide.
- Each color keeps a **distinct inner glyph** for colorblind players. Keep the
  glyph set from `Art Guide.dc.html` (paw, fish, bone, …), redrawn as cartoon
  shapes, readable at the smallest bubble size.
- The UI's gold and coral accents must not be confused with the yellow, orange
  or red bubbles. Keep the HUD visually separate from the board.
- **Backgrounds** sit behind the bubbles across the whole screen. Keep them
  low-contrast and dim, with no busy detail at bubble size. Show every theme
  with a full board on top to prove it.
- Also show one comparison card: the bubbles with **tiny faces**, next to the
  glyph-only version. We haven't decided whether faces help or add noise.

### Pixel pets

- Use the pixel cat and dog **from `Art Guide.dc.html` / `sprites.js`**, with
  their frames and animations (idle, turning the launcher, fire, cheer, panic,
  win, lose), all 4 shapes per species and the fur recolor. Don't redraw them
  as cartoon.
- Draw them at a **whole number of device pixels per art pixel** on the 3×
  reference, with no smoothing, the same rule the menu guide uses for its
  pixel pets. State the scale in the guide and in `layout.json`.
- Their fur must recolor through the **same 4 fur-key colors and ramps as the
  menu pets' `colorways.json`**, so one recolor shader covers both. If the Art
  Guide sprites use different key colors, remap them.
- Around the pets, step the cartoon art down to thinner lines and flatter
  fills, as the menu guide does, so the launcher, the stools or cushions under
  the pets and the pets sit well together.
- The engine may add motion on top of the frames, such as a bob or a hop with
  squash. Put those in the motion spec.

### Canvas and sizes

- **Layout grid 360 × 780 pt** (9:19.5), the same as the menus. Export PNGs at
  **3×**.
- **Bubbles:** the board is 8.5 bubbles wide, so a bubble is about **42 pt**
  across (126 px at 3×). The engine scales the board to the real screen width,
  so bubbles must look good from about 0.8× to 1.2× of that.
- Side walls, if you add them, are thin rails at the screen edges (at most
  6 pt). The engine shrinks the board slightly to make room.
- Safe area: nothing interactive in the top 32 pt or the bottom 22 pt.
- Backgrounds bleed 32 pt beyond every edge, like the menu backdrops.
- **Battle Mode:** every board sprite must read correctly upside-down. No text
  is baked into board art.

### Motion: the engine animates, not the sprite sheet

Unity runs at 60 fps and moves, rotates, scales and fades sprites every frame.
Draw static parts and describe motion in a **motion spec**, in the same format
as the menu guide's `motion.json`: speeds in pt/s, amplitudes, periods, easing
curves (from the 7 the menu guide already uses) and durations. Give entries
for:

- **landing settle**, in two selectable styles: overshoot bounce, and squash
  and pop;
- **pop** (a burst from parts: rings, sparkles, shards tinted per color) and
  the **falling** bubble, including a "scared" look for disconnected bubbles;
- **level build-in:** bubbles scale in, staggered top to bottom;
- the **aim line:** dots scrolling along the path, fading toward the end;
- the **ghost landing bubble:** a soft pulse (alpha is fine now);
- the **next-bubble swap** after each shot;
- the **ceiling warning** before a push: the shake itself is code, so give its
  amplitude and duration plus any warning flash on the ceiling band;
- the **countdown** digits 4 → 1: a pop-in per second, getting more urgent;
- **superpower effects:** Freeze (an icy frame over the screen plus a
  snowflake burst), Bomb (fuse, then explosion), Row Clear (a sweep across
  one row), Rainbow (a color-cycling bubble);
- button **press and release**, reusing the menu guide's button motion where
  it fits.

Bake frame-by-frame only where it can't be done otherwise, such as the
explosion: **at least 8 frames at 12 fps or more**.

### Assets needed

**Bubbles and board**
- 6 bubbles with glyphs; the pop parts; the falling/scared overlay; the ghost
  landing bubble.
- Superpower bubbles: Bomb with a fuse, Row Clear with a sweep motif, Rainbow
  as layered parts the engine can cycle.
- The **ceiling band**: it grows downward one bubble row per push, so it must
  tile **horizontally and vertically**. Add a bottom edge piece.
- The **danger line** at the launcher's height.
- Side rails (optional, see above).
- **Four background themes:** living room, garden, rooftop at dusk, park at
  night. Each is static layers with optional slow motion in the motion spec,
  kept calm.

**Launcher and aim**
- The launcher (our own design, e.g. a pet-themed cannon or slingshot) with a
  separate rotating pointer. Give the pivot.
- A stool, cushion or platform per pet next to it, for the aimer and the
  firer.
- The aim-line dot.
- The next-bubble holder.

**Bottom control deck and HUD**

Design the bottom of the screen as **one control deck** under the danger
line: launcher, both pets, the next bubble, the rotate-left and rotate-right
buttons, the fire button, the HUD (score, shots fired, level), the countdown,
and the superpower buttons. The touch areas today are about 73 pt square for
rotate and 40 pt for fire. You may resize them, but keep rotate at least
64 pt and fire at least 56 pt so they're easy thumb targets. Rotate is held
down and fire is tapped, so make that difference clear in the design.

- Rotate-left, rotate-right and fire buttons, normal and pressed.
- A HUD plate or bar with icon slots for score, shots and level. The game
  renders every number.
- A countdown badge. The digits are rendered with a text token, not baked.
- 4 superpower buttons (Freeze, Bomb, Row Clear, Rainbow), each normal,
  pressed and disabled/empty, plus a charge badge. The game renders the
  charge count.
- Reuse the menu guide's 9-slice panels and plates where they fit rather than
  drawing near-duplicates.
- **Battle Mode deck:** the same deck squeezed into a 360×390 pt half, with
  one pet per half, no superpower buttons, the shot clock, and an
  incoming-rows indicator showing rows queued against that player.

**Text and color tokens**
- New text tokens in the menu guide's format for: HUD value, HUD label,
  countdown digit, charge count. Reuse existing tokens where they already fit.
- New color tokens: the 6 bubbles (base, shade, highlight), glyph color, ghost,
  aim dot, danger line, freeze tint, and each background theme's key colors.

### Deliverable: a "Download Unity bundle" button

Same button and **same schema as the cartoon menu bundle**. It zips:

- `Sheets/*.png` at 3×, plus one legend JSON per sheet: `id, name, x, y,
  frameW, frameH, frames, stepX, fps, pivot, pivotX, pivotY, border {l,t,r,b},
  note`. **Sprite ids must not clash with any id in the menu bundle**, unless
  the sprite really is the same one.
- `PixelSheets/*.png`: the pixel pet sheets at **1× native**, with legend JSON
  in the same schema (frame size, frame count, fps, pivot), and the fur-key
  colors used;
- `motion.json`: every entry above, by id;
- `tokens.json`: the new color and text tokens only, named so they merge with
  the menu tokens;
- `layout.json`: the solo game screen and one Battle half, each element's rect
  in pt, plus the pets' device-px-per-art-px scale;
- `MANIFEST.json`.

The fonts are the menu guide's, so no `fonts.txt` is needed unless something
changes.

### Presentation

Show an **animated mockup at 60 fps** of a full solo game screen, built only
from the delivered parts plus the motion spec: a full board, both pets at the
launcher, the aim line bouncing off a wall, a shot landing and popping a
cluster, a cascade falling, the countdown, a ceiling warning and push, and a
superpower in use. Show one mockup per background theme, and a **2-player
mockup** with the top half rotated 180°. Then lay out each sheet with a short
size and pivot legend.
