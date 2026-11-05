# Claude Design prompt — Art Phase 1b (pre-game screens and result overlays)

Paste everything below the line into Claude Design. See `overview.md`
("Art direction B") for why these screens use a different style from the
board. Run it **after** the in-game prompt (`claude-design-prompt.md`), and
attach or paste that result's palette and cat/dog model sheets so the
characters match.

---

I'm making a mobile bubble-shooter puzzle game in Unity (portrait, phones
only) starring original chibi **cats and dogs** that the player names and
customizes. The in-game board art is chunky 16-bit arcade pixel art; that
set already exists, and I've attached its palette and character model
sheets. Please create the **out-of-game screens and result overlays** in
a different, more atmospheric pixel-art style.

## Style

Take inspiration from the overall feel of modern indie pixel games like
*Celeste*:
- higher-detail pixel art than the in-game sprites;
- layered parallax backgrounds with real depth;
- rich dusk and night palettes built from **stepped, banded or dithered
  color bands**, not smooth gradients;
- soft glowing pixel particles: fireflies, drifting petals, snow, dust
  motes;
- saturated warm accents against cool, muted backdrops;
- crisp, minimal pixel UI with clear shapes;
- a gentle, heartfelt mood.

**Everything must be original.** Don't use or closely imitate *Celeste*'s
mountain, characters, collectibles, logo, fonts or UI layouts, or those of
any other game. Our world is a **cozy hillside pet town at dusk**: tiled
rooftops, warm glowing windows, string lights, chimneys, a big moon, the
first stars coming out. Tone: cozy and playful. Even the game-over screen
is gently melancholic, never harsh or grim.

## Technical specs (important)

- **Native canvas: 270×585 px** (9:19.5 portrait), displayed at 4× →
  1080×2340. Backgrounds need ~24 px of bleed on every side, because
  other phone aspect ratios show more or less of them.
- **Safe area:** keep the top 24 px and bottom 16 px native free of
  buttons and important content (notches and gesture bars).
- **Hard pixels only:** no anti-aliasing, no soft brushes, no alpha
  gradients. Glows are built from stepped pixel halos. Transparent
  backgrounds on every sprite.
- **One shared master palette** of about 48 colors for these screens. It
  must **harmonize with the attached in-game palette** and reuse its cat
  and dog colors.
- **No baked text except the logo.** The game renders every label,
  button caption, pet name, score and level number. Leave clear, empty
  text areas.
- **Buttons and panels are 9-slice:** mark the slice borders.
- **Backgrounds come as separate parallax layers**: sky, far hills, mid
  town, near rooftops, foreground particles. Each layer must loop
  horizontally. Give the frame count and suggested speed for animated
  layers.
- **Export:** each sprite sheet as PNG at **1× native** plus a 4× preview,
  with a short legend (sprite sizes, frame counts, frame rates, 9-slice
  borders, pivots) and the palette as hex values.

## Characters for these screens

Larger "portrait" versions, **about 64–80 px tall**, of the same chibi cat
and dog from the attached model sheets.

- **4 shape variants per species:**
  - cat: round shorthair, fluffy longhair, slim pointy-eared, chonky;
  - dog: floppy-eared pup, pointy-eared, fluffy, short-legged.
- **Recolorable fur:** draw all fur with **one dedicated 4-shade ramp**
  (darkest to lightest) used nowhere else in the sprite, so the game can
  palette-swap it at runtime. Eyes, nose, outline and accessories stay
  outside the ramp. Show **6 fur colorways per species** as 4-shade ramps:
  - cat: ginger, cream, silver, midnight, cocoa, lilac;
  - dog: golden, chocolate, cream, charcoal, rust, snow.
- **Animations per variant:** idle (4 frames, gentle breathing or tail
  sway), happy/selected (4 frames), cheer (4 frames), sad (2–4 frames).

## Screens

### 1. Main menu
- The **game logo**: the game is still untitled, so use the placeholder
  wordmark "Untitled Pet Game", plus a **logomark** that works on its
  own without text (e.g. a paw plus a bubble).
- An animated dusk-town background (the parallax layers above).
- The cat and the dog idling together on a rooftop or windowsill.
- **4 stacked buttons**: New Game, Continue, 2 Players and Settings.
  Continue needs a clearly **disabled state** for when no save exists.
- Button states: normal, pressed, disabled.

### 2. Pet customization
Shown after New Game. The player names and styles **both** pets, then
starts.
- **Layout:** cat section on top and dog section below, or two swipeable
  cards. Pick whichever reads better on a phone and show it.
- **Each pet section has:**
  - a big preview of the pet standing on a small stage, cushion or
    doormat;
  - a **name field** (empty with placeholder, focused, filled), up to 12
    characters;
  - a **fur-color swatch row** of 6 swatches, with a selected state;
  - a **shape picker**: ◀ ▶ arrows plus 4 small silhouette icons, with a
    selected state.
- **Back** in the top-left corner.
- A prominent **Start** button at the bottom, with a disabled state for
  when a name is empty.
- When the player picks a pet, its preview plays the happy/selected
  animation.

### 3. Settings
- A panel over a dimmed, blurred-looking (pixel-dithered) main menu
  background.
- A back button.
- A two-option toggle row with selected and unselected states. Today the
  only setting is the landing animation (Bounce / Squash). Design the
  row so more settings can stack below it.

### 4. Victory overlay (over the gameplay board)
- A full-screen dim layer (dithered, not alpha) so the board stays faintly
  visible.
- A celebratory banner motif, the cat cheering, and fireworks, confetti
  or rising lantern particles.
- Empty slots for "Level complete" text, score and level number.
- Primary button: Continue. Secondary button: Menu.

### 5. Game-over overlay (over the gameplay board)
- A softer, gently melancholic banner motif with light rain or falling
  leaves.
- The cat sad but hopeful (ears down, small smile).
- Empty slots for "Game over" text, score and level number.
- Primary button: Retry. Secondary button: Menu.

### 6. 2-player result overlay
- Cat vs. dog: winner, loser and draw poses for each.
- Two players sit on opposite sides of a phone lying flat, so the layout
  must be **symmetric and text-free**, reading fine from either end.
- Buttons: Rematch and Menu.

### 7. Screen transition
- A short scene-to-scene wipe of 8–12 frames, e.g. a paw-print iris or a
  curtain of stars, covering the full 270×585 canvas.

## Presentation

1. A style page: the palette with hex values, the light and particle
   rules, the button and panel kit, and a size comparison of the portrait
   cat and dog next to their in-game sprites.
2. **One full mockup per screen** at 9:19.5, with realistic placeholder
   text drawn in, so I can judge the layout. The exported sprites stay
   text-free.
3. Each element as a separate downloadable sprite sheet:
   - buttons (normal, pressed, disabled);
   - panels and frames;
   - swatches and picker icons;
   - portrait characters (all variants and animations);
   - background layers;
   - particles;
   - the transition.

   Include the frame/size legend for each.
