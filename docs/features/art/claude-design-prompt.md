# Claude Design prompt — Art Phase 1

Paste everything below the line into Claude Design. See `overview.md` for
the art roadmap this feeds into.

---

I'm making a mobile bubble-shooter puzzle game in Unity (portrait,
phones only). Please create an **art style guide and a complete sprite
asset set** in **16-bit pixel art**. It should feel like a cheerful
1990s arcade puzzle game: chunky dark outlines, limited palettes, bouncy
and expressive animation.

The cast is **original chibi cats and dogs**. A small chibi **cat**
operates the bubble launcher in single player. A chibi **dog** is the
rival in 2-player mode. Tone: cozy, playful, pet-filled household and
neighborhood. All characters, logos and layouts must be original
designs. Don't copy or closely imitate any existing game's characters,
launcher, UI or backgrounds.

## How the game works (so the art fits)
- The board is a hex-packed grid of bubbles, **8 columns** wide. Odd
  rows are offset by half a bubble. The board fills the full width of a
  portrait phone screen.
- The player rotates a launcher at the bottom center and fires colored
  bubbles upward. Bubbles can bounce off the side walls. Matching 3+ of
  the same color pops them, and anything left disconnected from the
  ceiling falls.
- The ceiling periodically pushes down one row. The screen shakes as a
  warning first. You lose if the bubbles reach the launcher line.
- There are four superpowers: **Freeze** (pauses timers), **Bomb**
  (clears a radius), **Row Clear** (clears a whole row) and **Rainbow**
  (a wildcard color).
- 2-player mode splits the screen top/bottom. The top player's half is
  rotated 180° (the phone lies flat between two players). So **no text
  may be baked into board or character art**, and everything must read
  fine upside-down.

## Technical specs (important)
- **Base bubble size: 24×24 px.** Use even dimensions everywhere so the
  half-cell offset lands on whole pixels.
- Native board width is about 216 px including side walls. Size every
  other element relative to that.
- **Transparent backgrounds**, no anti-aliasing, no soft shadows, no
  gradients. Use hard pixels only, with dithering allowed.
- One **shared master palette** of about 32–48 colors, used across
  every asset.
- **Build each sprite as an exact pixel grid**, for example from a
  palette-indexed pixel matrix rendered to a canvas with
  `image-rendering: pixelated`.
- **Export:**
  - each sprite sheet as PNG at **1× native size**, plus a 4× preview;
  - for each animation: frame size, frame count and suggested frame
    rate;
  - the palette as hex values.

## Assets needed

### 1. Style guide page
- The master palette with hex values.
- Outline and shading rules.
- A size comparison of a bubble, the cat and the dog next to each
  other.
- The pixel font recommendation: an existing free, OFL-licensed pixel
  font, or a small custom set of digits 0–9 plus a few UI glyphs.

### 2. Bubbles
- **Six bubble colors:** red, orange, yellow, green, blue and purple,
  each 24×24.
  - Each gets a glossy pixel highlight.
  - Each carries a **distinct small inner glyph** for colorblind
    players: e.g. paw print, fish, bone, yarn ball, heart, star. You
    may pick better pet-themed glyphs.
- **Pop animation:** 4–6 frames per color, or one shared burst overlay
  tinted per color.
- A "falling/scared" variant or overlay for disconnected bubbles.
- A **ghost landing indicator**: a translucent-looking outline built
  from pixels (dithered, not alpha).
- **Superpower bubbles:**
  - Bomb: a fused bubble;
  - Row Clear: a bubble with a horizontal arrow or sweep motif;
  - Rainbow: a 4-frame color-cycling bubble.
- **Superpower effects:**
  - Freeze: a full-screen icy frame overlay or border, plus a
    snowflake burst;
  - Bomb: a 6-frame explosion;
  - Row Clear: a horizontal sweep strip.

### 3. Launcher and aim
- A launcher device (an original design: e.g. a pet-themed cannon or
  slingshot) with a separate rotatable **pointer/arrow** sprite. Pivot:
  bottom center.
- An **aim-line dot** sprite (2–4 px) for a dotted trajectory.
- A **next-bubble holder** frame.

### 4. Characters (chibi, about 32–48 px tall)
- **Cat (player 1 / solo):**
  - idle: 2–4 frames;
  - cranking/turning the launcher: 4 frames;
  - fire: 2–3 frames;
  - cheer on a big combo: 4 frames;
  - panic during the ceiling warning: 2–4 frames;
  - win pose and lose pose.
- **Dog (player 2):** the same set of animations.
- A **model sheet** for each: front view plus expressions.

### 5. Board dressing
- Left and right **side walls**: tileable vertically, 8–12 px wide.
- A **ceiling band**: tileable horizontally, plus a bottom edge piece.
  It moves down one bubble row at a time, so its texture must tile
  vertically too as it grows.
- A launcher-line marker (the "danger line").
- **Four background themes**, portrait, sized to the native board area:
  1. living room;
  2. garden;
  3. rooftop at dusk;
  4. park at night.

  Keep them low-contrast and desaturated so bubbles stay readable on
  top.

### 6. UI (pixel style, no baked text except on the title screen)
- **Touch buttons**, each 48×48 native with normal and pressed states:
  rotate-left, rotate-right and fire.
- A **HUD bar** frame with icon slots for score, shots fired and level.
- Countdown digits 1–4: big, bold, readable.
- **Superpower buttons**, one icon per ability (Freeze, Bomb, Row
  Clear, Rainbow), with normal, pressed and disabled/empty states, plus
  charge pips.
- A **result panel** frame, plus a win banner motif and a lose banner
  motif (icons and decoration only; text is rendered by the game).
- A generic **9-slice button** and **9-slice panel**.

## Presentation
Show everything on one organized sheet per category. Include a **mockup
of a full gameplay screen**: board with bubbles, walls, ceiling, the cat
at the launcher, an aim line, the HUD and buttons, at a 9:19.5 phone
aspect ratio. Also show a **2-player mockup** with the dog's half
rotated 180°. Then provide each category as a separate downloadable PNG
sprite sheet with a short frame/size legend.
