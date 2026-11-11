# Claude Design prompt — pre-game screens, vector cartoon

Replaces the pixel-art direction of
[`claude-design-prompt-menus.md`](claude-design-prompt-menus.md) for the
pre-game screens and result overlays only. In-game art stays 16-bit pixel art
(see [`overview.md`](overview.md)).

**Why:** the pixel-art screens felt choppy on device and their runtime fonts
never sat on the art's pixel grid. This prompt asks for art that the engine
animates at 60 fps, with typography and color designed as part of the style.

**How to use:** paste the prompt into the "Mobile bubble-shooter art system"
Claude Design project, so it can read `Menu Art Guide.dc.html`,
`menus-scenes.js` and the `Knocked Out Cartoon Style.dc.html` study. Review the
mockups, ideally on a phone. Then click "Download Unity bundle". Put the
sheets in `Assets/Art/Cartoon/Sheets/` and the JSON spec files in
`Assets/Art/Cartoon/Spec/`, then run Tools ▸ Art ▸ Import Menu Sheets. If
the bundle doesn't include the fonts, download the TTFs it names from
Google Fonts into `Assets/Art/Fonts/`.

**What changed after this prompt:** we decided to keep the pets in pixel
art. A follow-up prompt asked Claude Design to replace its cartoon pets
with the existing pixel portraits, floaters and Retry nap, drawn at
whole-pixel scales and recolored with `colorways.json`. It also asked for
the cartoon outline to match the pixel ink. So the delivered guide,
`Cartoon Menu Guide.dc.html`, has no `rig.json` and no gradient-map
colorways: the pet sections below were not used. The follow-up prompt:

> In `Cartoon Menu Guide.dc.html`, replace the cartoon pets with the pixel
> pets from `Menu Art Guide.dc.html` (the `chars_cat` / `chars_dog`
> portraits, the space floaters and the `retry_nap` illustration). Draw
> them at a whole-number scale with no smoothing, and recolor them with
> the existing `colorways.json`. Keep everything else cartoon. Adjust the
> stages and frames around the pets so the two styles sit well together,
> and match the cartoon outline color to the pixel outline ink. Remove the
> pets from the Unity bundle (no `rig.json`, no gradient-map colorways);
> we keep the existing pixel pet sheets.

---

## Prompt

Redesign the out-of-game screens of this mobile bubble shooter in a new
**vector cartoon style**. Start from our study `Knocked Out Cartoon Style.dc.html`
/ `ko-cartoon.svg` and develop it into a full style. Keep the **screen content
and flow** from `Menu Art Guide.dc.html` and `menus-scenes.js`:

- main menu: space backdrop, drifting galaxies and planets, floating pets, the
  logo, and 4 buttons (New Game, Continue, 2 Players, Settings);
- pet customization in 2 steps, the aimer and then the firer: CAT/DOG role
  toggle, name field, 6 fur swatches, 4 shapes with arrows, preview pet, Back,
  and Next/Start;
- settings: one toggle row, Landing animation BOUNCE/SQUASH;
- the victory overlay over the board;
- knock-out → the Retry screen with the napping cat and dog;
- the 2-player result: YOU WIN / YOU LOSE / DRAW per half, then a stats card
  per player with Rematch and Home. The top half is rotated 180°;
- the paw-print screen transition.

Only the style changes, plus anything you need to change for the style to
work.

### Style

- Smooth vector art: chunky dark outlines (one outline color and weight
  family), glossy gradient fills with a soft highlight, and a warm dusk/night
  palette with saturated warm accents. Cozy and playful. Game over is gently
  sad, never harsh.
- Our own look: don't copy the UI, logo, characters or layouts of any existing
  game.
- The cat and dog are the same characters as in the in-game Art Guide (the
  ginger tabby with forehead stripes and green eyes; the pup with an eye patch
  and a big nose), redrawn as cartoon portraits. Keep the 4 shapes per
  species: cat round shorthair, fluffy longhair, slim, chonky; dog floppy ears,
  pointy ears, fluffy, short legs.

### Canvas

- Layout grid 360 × 780 pt (9:19.5). Export PNGs at **3×** (1080 × 2340 for a
  full screen).
- Safe area: nothing interactive in the top 32 pt or the bottom 22 pt.
- Background layers bleed 32 pt beyond every edge, and horizontally scrolling
  layers tile seamlessly.

### Text and color

- Choose **one display font and one UI font** from Google Fonts under the SIL
  Open Font License. Use them in the mockups too, not a placeholder.
- Deliver **text style tokens**: font, size (pt), fill (solid or 2-stop
  gradient), outline width and color, drop-shadow offset and color, and letter
  spacing. Cover title, button caption (primary, secondary, disabled), label,
  small label, number/score, and the name-field text and placeholder.
- Deliver **color tokens** (named hex values) for every UI surface, dim overlay
  (alpha allowed) and text color.
- No baked text except the logo wordmark. The game renders every label, pet
  name and number.

### Motion: the engine animates, not the sprite sheet

- Unity runs at 60 fps and moves, rotates, scales and fades sprites every
  frame. Draw **static parts**, and describe the motion in a **motion spec**:
  speeds in pt/s, amplitudes, periods, easing curves and durations. Don't bake
  movement into frames.
- Pets are **rigged from parts**: body, head, each ear, tail, each leg/paw, and
  eye and mouth variants (open, closed/blink, happy ^^, sad, asleep). Each part
  is a separate image with a pivot point and a draw order. Give per-state
  motion for:
  - idle: breathing, tail sway, a blink every 3–5 s;
  - happy: a hop with squash and stretch, played on selection;
  - cheer, sad, and asleep (for Retry);
  - the floating poses for the main menu: spread, swim, wave, curl, star,
    tumble.
- **Fur recolor:** draw all fur parts in **grayscale**, with the shading baked
  as gray values. Outline, eyes, nose, inner ears and belly stay in color on
  separate parts. Give each of the 12 colorways as a 4-stop gradient map,
  darkest to lightest, that the engine maps gray onto:
  - cat: Ginger, Cream, Silver, Midnight, Cocoa, Lilac;
  - dog: Golden, Chocolate, Cream, Charcoal, Rust, Snow.

  Show every colorway in the guide using that mapping.
- Space backdrop: a static deep layer, a seamless star layer, and galaxies,
  planets, comets and shooting stars as separate sprites. Twinkle, glow pulse
  and rotation are motion-spec entries, not frames.
- Bake frame-by-frame only where it can't be done otherwise (for example
  firework bursts): **at least 8 frames at 12 fps or more**.
- Paw-print transition: deliver the paw silhouette as one clean shape. The
  engine scales it as a mask, and the motion spec gives the close/open duration
  and easing.

### UI kit

- 9-slice buttons: primary, secondary and disabled, each normal and pressed.
- Back, rematch and home icon buttons.
- Panel, title plate, and name field (empty, focused, filled).
- Segmented toggle: track, selected, unselected.
- Swatch frame and shape slot, each normal and selected; arrow buttons.
- Win and lose banners, draw plate, stats card.
- The stage: a cushion for the cat, a mat for the dog.
- Give 9-slice borders in px at 3×.
- The design also owns the dims: give the victory/stats dim and the settings
  backdrop as color tokens.

### Deliverable: a "Download Unity bundle" button

Same idea as the Menu Art Guide's button. It zips:

- `Sheets/*.png` at 3×, plus one legend JSON per sheet using the **same schema
  as the current menu bundle**: `id, name, x, y, frameW, frameH, frames, stepX,
  fps, pivot, pivotX, pivotY, border {l,t,r,b}, note`;
- `rig.json`: per pet shape, its parts (sprite id, parent, pivot, offset, draw
  order) and the eye/mouth variants;
- `motion.json`: every motion-spec entry from above, by id;
- `tokens.json`: color tokens, text style tokens and the 12 gradient-map
  colorways;
- `layout.json`: per screen, each element's rect in pt;
- `fonts.txt`: the Google Fonts family names and weights used;
- `MANIFEST.json`.

Show each screen as an animated mockup at 60 fps in the guide, built only from
the delivered parts plus the motion spec, so what we see is what Unity will
play.
