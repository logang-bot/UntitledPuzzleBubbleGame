# Art Pass — Roadmap

**Status: screens done, in-game art not started.** Art Phase 1b (the
screen art) is generated, and the screen half of Phases 2 and 6b is
implemented: every pre-game screen and result overlay now uses it (see
"Screens: as built" below). Phases 1, 3–7 (in-game art) are still
planned. This replaces the "Real art pass" bullet in
`../../ROADMAP.md`'s "Later / not yet scoped" section. It runs in
parallel with playtest/tuning work (see "Working in parallel" below).

## Art direction

The game uses **two related art directions**. Both are pixel art and share
the same original cat and dog cast, but they serve different jobs.

### A. In-game: 16-bit arcade (board, bubbles, HUD, launcher)

- **Style:** 16-bit pixel art, evoking 1990s arcade puzzle games in
  general: chunky outlines, limited palettes, bouncy and cheerful
  animation. The inspiration is the genre's overall feel. We don't copy
  the inspiration game's specific characters, logo, UI layout, launcher
  design or backgrounds. Every character and screen is our own design.
- **Why:** the board must stay readable at a glance during fast play, so
  flat colors, hard outlines and a small palette win over atmosphere.
- **Prompt:** [`claude-design-prompt.md`](claude-design-prompt.md).

### B. Pre-game screens and result overlays: atmospheric indie pixel art

- **Covers:** the main menu, pet customization, settings, the
  victory and game-over overlays, the 2-player result overlay, and
  screen transitions.
- **Style:** inspired by modern indie pixel games such as *Celeste*:
  - higher-detail pixel art than the in-game sprites;
  - layered parallax backgrounds with real depth;
  - rich, banded dusk and night palettes (stepped or dithered color
    bands rather than smooth gradients);
  - soft glowing pixel particles: fireflies, drifting petals, snow,
    dust motes;
  - saturated warm accents against cool, muted backdrops;
  - crisp, minimal pixel UI and a gentle, heartfelt mood.
- **Originality rule:** the inspiration is the overall feel only. No
  mountain, characters, collectibles, logo, fonts or UI layouts from
  *Celeste* or any other game. Our setting is a **cozy hillside pet town
  at dusk**: rooftops, warm windows, string lights, a big moon.
- **Why:** these screens are where the player meets their pets and
  feels the game's mood, and nothing here competes with a busy board, so
  they can afford atmosphere.
- **Consistency:** the screen palette must harmonize with the in-game
  palette, and the cat and dog must read as the same characters in both
  styles. The screen versions are larger "portrait" renders of the same
  designs.
- **Prompt:** [`claude-design-prompt-menus.md`](claude-design-prompt-menus.md).

### Shared rules

- **Characters:** chibi **cats and dogs**, in two roles:
  - a **cat** runs the launcher in solo mode and as Player 1 in Battle
    Mode;
  - a **dog** is Player 2 in Battle Mode.
  - Optionally, other cat and dog breeds can appear as background
    "audience" or level-theme cameos.
- **Player-customized pets** (see
  [`../game-flow/overview.md`](../game-flow/overview.md)). New Game lets
  the player name both pets and pick a fur color and a shape for each.
  So every cat and dog sprite, in-game and on screens, needs:
  - **4 shape variants per species**, matching
    `DefaultPetAppearanceCatalog`:
    - cat: Shorthair, Longhair, Slim, Chonky;
    - dog: Floppy Ears, Pointy Ears, Fluffy, Short Legs;
  - a **dedicated 4-shade fur ramp** that the game recolors at runtime
    (a palette swap). Eyes, outlines and accessories stay outside the
    ramp, so they never change color;
  - **6 fur colorways per species**: the names in the catalog (Ginger,
    Cream, Silver, Midnight, Cocoa, Lilac; Golden, Chocolate, Cream,
    Charcoal, Rust, Snow). The art pass may tune the hex values.
- **Tone:** cozy and playful, like a pet-filled household or
  neighborhood. Nothing grim. The game-over screen is gently
  melancholic, never harsh.
- **Accessibility:** each of the 6 bubble colors also carries a distinct
  inner glyph, such as a paw, a fish or a bone, so colorblind players
  can still tell bubbles apart. This matters for a matching game and
  costs little at this pixel size.
- **No baked text** except the logo. The game renders every label, pet
  name and number, so it can localize and so pet names can be shown.

## Technical constraints the art must fit

These come from the existing code, so the art must work within them
rather than the other way round.

**Board and bubbles**
- The board is **8 columns** wide (`GameBoard.cols`), portrait only, and
  fills the full screen width. Odd rows sit half a cell to the side
  (hex packing), so one row spans 8.5 bubble widths.
- Bubble colors are a fixed 6-entry enum (`BubbleColor`: Red, Orange,
  Yellow, Green, Blue, Purple). Difficulty uses the first N. Art may
  restyle the hues but needs exactly 6 bubble kinds.
- **Proposed base bubble size: 24×24 px.** The board then works out to
  about 216 px of native width including walls, which upscales cleanly
  to phone widths and feels like 16-bit. Even sizes keep the half-cell
  hex offset on whole pixels.
- **Pixel scaling:** the camera fits the board to the screen width, so
  the upscale factor isn't a whole number on most phones. Options:
  1. point filtering with non-integer scaling (simplest, with slight
     pixel unevenness);
  2. Unity's Pixel Perfect Camera with integer scaling and cropped or
     padded edges (crisper, but conflicts with
     `screen-fit-and-difficulty-scaling.md`'s "board fills the full
     screen, no letterbox" decision).

  To be decided in Art Phase 2. Option 1 is the default unless it looks
  bad on a device.

**Pre-game screens**
- **Native canvas: 270×585 px** (9:19.5), shown at 4× (1080×2340).
  Other aspect ratios extend the background layers sideways and
  vertically, so backgrounds need bleed beyond the 270×585 frame and UI
  anchors to the safe area.
- **Safe area:** keep the top 24 px and the bottom 16 px native free of
  critical UI (notches and gesture bars).
- Buttons and panels are **9-slice**, so the same art serves every label
  length.
- Backgrounds come as **separate parallax layers** (sky, far, mid, near,
  foreground particles), so code can scroll and loop them.

**Things on screen today that need art**
- **Board:** bubbles, the fired bubble in flight, the next-bubble
  indicator, the ghost landing indicator (`LandingIndicator`), the aim
  line (`ShooterController`'s `LineRenderer`), and the advancing
  ceiling/wall band (`CeilingRenderer`, which grows one row per push).
- **Motion:** pop, fall (`FallingBubble`), landing settle (two styles,
  see Settings), the level build-in scale-in (`BubbleSpawnAnimator`),
  and the ceiling-warning camera shake.
- **Touch controls:** rotate-left, rotate-right and fire zones
  (`HoldInputZone`, 220×220 reference px).
- **In-game UI:** the HUD bottom bar (score, shots, level), the shot
  countdown (`ShotTimerDisplay`) and the superpower buttons with charge
  counts (`SuperpowerHud`).
- **Screens and overlays (direction B):**
  - the main menu (`MainMenuController`: New Game, Continue, 2 Players,
    Settings);
  - pet customization (`PetCustomizationController` and
    `PetEditorPanel`: a name field, fur-color and shape pickers, and a
    preview per pet);
  - the settings menu (`SettingsMenuController`);
  - the victory and game-over overlays (`LevelResultScreen`, which today
    is one shared panel and will become two distinct looks);
  - the battle result screen (`BattleResultScreen`).
- **Battle Mode:** Player 2's half is rendered rotated 180°. Every
  sprite must read correctly upside-down, so text is never baked into
  board art.

## Phases

### Art Phase 1 — In-game style guide and asset generation
- Generate a style guide and the asset set with Claude Design, using
  `claude-design-prompt.md` in this folder.
- Lock down:
  - the master palette;
  - the bubble size;
  - the outline rules;
  - the character model sheets (cat and dog: front, idle, happy, sad,
    panic), **including all 4 shape variants and the recolorable fur
    ramp**;
  - the pixel font choice.
- **Deliverable:** PNG sprite sheets plus a palette file, reviewed on a
  real phone screen before anything is wired in.

### Art Phase 1b — Pre-game screens and result overlays
- Generate the screen art with Claude Design, using
  `claude-design-prompt-menus.md`. Run it **after Phase 1** so the
  portrait cat and dog match the locked in-game model sheets.
- Lock down:
  - the screen palette and how it maps to the in-game palette;
  - the logo or logomark (the game is still untitled, so the logomark
    must work without the wordmark);
  - the UI font for screens, which may differ from the in-game digits
    font;
  - the background parallax layers and their loop timings.
- **Deliverable:** one mockup per screen at 9:19.5, plus sprite sheets
  for buttons, panels, swatches, icons, portrait characters, background
  layers and the screen transition.
- **Review the mockups on a real phone** before anything is wired in,
  just as in Phase 1. Check:
  - legibility at 4×;
  - that the safe-area margins clear the notch and gesture bar;
  - that the screens sit comfortably next to the in-game look when you
    switch between them.

### Art Phase 2 — Import pipeline
- Folder layout under `Assets/Art/`: `Bubbles/`, `Characters/`,
  `Board/`, `UI/`, `Screens/`, `Effects/`, `Backgrounds/`, `Fonts/`.
- Import settings:
  - Sprite (2D and UI);
  - point filtering, no compression, no mipmaps;
  - one shared pixels-per-unit;
  - a Sprite Atlas per group.
- Decide the pixel scaling option (see constraints above).
- A **fur-recolor material/shader**: it maps the 4 fur-ramp shades to
  the 4 shades of the chosen `PetColorOption`. `PetColorOption` grows
  from one `Color` to a 4-shade ramp at this point.

### Art Phase 3 — Real board renderer (code)
- `GridDebugRenderer` is no longer pure visuals. It also owns:
  - the build-in timing (`IsBuildingIn`, which `ShooterBuildInGate`
    reads);
  - falling-bubble tracking and cleanup on level load;
  - the full rebuild after a wall push.
- **Extract that behavior first**, then add a sprite-based renderer that
  reuses it. Bubble sprites come from a `BubbleSkin` ScriptableObject
  that maps each color to its sprite and pop frames.
- **Keep the plain-circle debug renderer as a toggle.** It's still the
  clearest view when chasing grid bugs.
- Replace the remaining `CircleSpriteFactory.CreateWhiteCircle()` call
  sites with skin sprites:
  - `FiredBubbleController`, for the flying bubble and the next-bubble
    indicator;
  - `LandingIndicator`.
- Add pop animation frames and the falling-bubble sprite.
- Animate with the project's existing `Update()`-driven style: no
  coroutines and no tween library.

### Art Phase 4 — Launcher and characters
- A launcher and pointer sprite that rotates with the aim angle.
- A cat operator next to it, reacting to game state:
  - idle;
  - turning the launcher while aiming;
  - fire;
  - a cheer on big pops or cascades;
  - panic during the ceiling warning;
  - win and lose poses on the result screen.
- The operator uses the **player's saved cat** (`PetProfileStore.Load`):
  its shape variant and its fur color through the recolor material.
- The animation driver listens to existing events (`OnFireRequested`,
  `OnBubblesPopped`, `OnClusterDropped`, the ceiling warning,
  `OnLevelWon`/`OnLevelLost`). No new game-logic hooks are expected.

### Art Phase 5 — Board dressing and backgrounds
- Side walls and frame, and the ceiling band (a themed advancing
  "ceiling" object).
- **Background themes** by level range: for example living room,
  garden, rooftop at dusk and park at night. Each needs to work behind
  a busy board, so keep contrast low.
- Superpower bubble variants and effects:
  - Freeze: an ice overlay and screen tint;
  - Bomb: a fuse bubble and explosion;
  - Row Clear: a row-sweep effect;
  - Rainbow: a cycling-color bubble.

### Art Phase 6 — In-game UI skin
- A pixel font.
- The HUD bar, countdown digits, and superpower buttons with charge
  pips.
- The rotate and fire control graphics.
- The next-bubble frame.

### Art Phase 6b — Screens and overlays skin (code)
- **Canvas scaling first.** All menu scenes (MainMenu, PetCustomization,
  SettingsMenu), and the overlay canvases in Game and BattleMode, use
  `CanvasScaler` in **Constant Pixel Size** mode today. For the
  270×585 native art, switch them to **Scale With Screen Size**:
  - reference resolution 1080×2340 (the 4× native size);
  - match width, so the horizontal layout stays fixed and taller phones
    get more background.

  Every placeholder size in code (button sizes, font sizes) is tuned
  for constant pixels, so re-check them after the switch. In Game and
  BattleMode the HUD and touch zones (`HoldInputZone`, 220×220
  reference px) probably share that canvas. Either give the result
  overlays their own canvas, or switch the scaler and re-tune the HUD
  together with Art Phase 6.
- **Safe-area component.** Add a small `SafeAreaFitter` that insets a
  root `RectTransform` to `Screen.safeArea`, re-checked when the
  resolution or orientation changes. All buttons and text go under it;
  backgrounds stay outside it so they still bleed edge to edge.
- Replace the placeholder `RuntimeUi` widgets with skinned prefabs from
  Phase 1b. `RuntimeUi` is the single place the menus build their UI,
  so this is mostly one swap point.
- Main menu: parallax background driver, logo, the idling cat and dog
  (using the saved pets once a run exists), and the 4 buttons.
- Pet customization: the live preview uses the portrait sprite for the
  chosen shape, recolored with the chosen fur. The color cycler becomes
  a swatch row, and the shape cycler becomes silhouette icons.
- Settings: the panel over the dimmed menu background, and the
  selected/unselected toggle row. Move `SettingsMenuController` onto
  `RuntimeUi` (or the skinned prefabs) at the same time, so it stops
  carrying its own builder code.
- Split `LevelResultScreen` into distinct **victory** and **game-over**
  presentations (banner, cat pose, particles), keeping its current
  buttons and `GameStateManager` hooks.
- A screen transition (paw-print iris or similar) between scenes.

### Art Phase 7 — Battle Mode pass
- The dog as Player 2's operator, plus the cat-vs-dog result screen
  (direction B).
- Battle uses the **player's saved cat and dog** as P1 and P2.
- Check every Player 2 asset upside-down, since the whole half is
  rotated.
- An incoming-attack indicator, if the pending-rows meter ever gets a
  display.

### Deferred (wait on other decisions)
- **Final game title and wordmark.** The logomark from Phase 1b works
  without it.
- **Level map and meta-progression screens.** They wait on the meta
  progression design.
- **Audio.** It's a separate pass.

## Screens: as built

The screens follow the **latest Claude Design source** ("Mobile
bubble-shooter art system", `Menu Art Guide.dc.html` and
`menus-scenes.js`). Where it differs from the plan above, it wins.

**Import pipeline (screen half of Phase 2)**
- Claude Design exports a bundle: 1× PNG sheets plus legend JSONs in
  `Assets/Art/Screens/Sheets/`, and `colorways.json` and `palette.gpl` in
  `Assets/Art/Screens/`.
  - Each legend gives every sprite id its frame rect, frame count, fps,
    a numeric pixel pivot and, for 9-slices, its border.
- **Tools ▸ Art ▸ Import Menu Sheets** (`Editor/Art/MenuArtImportMenu`)
  can be re-run after every re-export. It:
  - slices each sheet into frames named `{id}_{frame}`, with Point
    filtering, PPU 1, no compression and no mipmaps;
  - writes one `SpriteSheetIndex` per sheet under
    `Assets/Art/Screens/Index/`;
  - builds `Assets/Art/Screens/MenuArtLibrary.asset`, which the screens
    use to look up sprites by legend id;
  - copies the colorway ramps and the sprite shape ids into
    `DefaultPetAppearanceCatalog`.
- **Fonts:** Pixelify Sans Regular for titles and captions, Silkscreen
  for small labels, both Hinted Raster. Pixelify Sans Bold was dropped
  because its "2" reads as an "8" at caption size.
- **Fur recolor:** the `Game/UI/FurRecolor` shader swaps the 4 cat
  fur-key colors and the 4 dog fur-key colors for the chosen ramps.
  - Each recolored Image gets its own material instance (`FurRecolor`).
  - `PetColorOption` now holds a 4-shade `Ramp` instead of one `Color`.
  - `PetShapeOption` gained an `ArtId` (round, fluffy, slim, chonk;
    floppy, pointy, fluffy, short).

**Canvas (Phase 6b)**
- `PixelCanvas.Configure` sets up every screen canvas:
  - Scale With Screen Size at a 270×585 reference, so one unit is one
    art pixel;
  - match mode **Expand**, so the full native frame always fits and the
    extra space becomes background;
  - reference PPU 1.

  This differs from the 1080×2340 + match-width plan: Expand keeps 16:9
  phones from cropping the 585-tall layout.
- `NativeStage` gives each screen a full-bleed background and a 270×585
  stage centered in the safe area (`SafeAreaFitter`). Layout coordinates
  are the design's native pixels, measured from the top-left
  (`UiPlacement.TopLeft(RectInt)`).
- The Game and BattleMode result screens build their own overlay canvas
  (`OverlayCanvasFactory`), so the HUD canvas and touch zones are still
  untouched. They change with Phase 6.

**Screens**
- **Main menu:**
  - deep space: a static layer plus near stars scrolling at 4 px/s;
  - drifting cosmos: one big galaxy slot, three small slots and a
    shooting star;
  - 15 floating pets in random species, shapes, colorways, sizes and
    poses;
  - the logo and four buttons.
  - The spawn timing (`SpawnSlot`, `CrossingPath`, `FloaterPick`) is
    ported from `menus-scenes.js`.
- **Pet customization:** a two-step aimer/firer flow. See
  `../game-flow/overview.md`. The dusk-town `bg*` layers are no longer
  used.
- **Settings:** one panel with the landing-animation toggle.
- **Victory overlay:** a flat 78 % Abyss dim, the win banner, fireworks,
  confetti and lanterns, the level/score panel, then Continue and Menu.
  There's no pet.
- **Game over:** the board holds for 3 s with input blocked, then the paw
  iris shows the **Retry screen**. It has the napping cat and dog in the
  saved pets' fur, the level/score panel, then Retry and Exit.
- **2-player result:**
  - YOU WIN / YOU LOSE banners (or a DRAW plate) per half for 2.5 s;
  - then a dimmed stats card per player (score, popped, best chain,
    shots), with Rematch in the center bar and Home at each player's
    right.
  - The top half is rotated 180°.
  - The launcher pets in the design wait for Art Phases 4 and 7.
- **Transition:** `SceneTransition` plays the paw-print iris on every
  scene change and on the in-scene swap to and from the Retry screen.

Crown, rain cloud, draw badge, dim tiles, petals, snow and the
customization `stage_*` alternatives are in the sheets but unused for now.

## Working in parallel with debugging and tuning

- **Branch and worktree.** Art code (Phase 3+) goes on its own branch,
  ideally a separate git worktree, so a debug fix on `main` never has to
  wait for half-finished renderer work.
- **Prefabs over scene edits.** `Game.unity`, `BattleMode.unity` and the
  menu scenes merge badly, so keep art in prefabs and components and
  keep scene diffs minimal.
- **Tune with placeholders first.** Art changes how readable the board
  is, so do a quick re-check of tuning decisions (color count, pattern
  readability) once the bubbles are real.
- **Keep the debug toggle** (Phase 3), so bug reports can always be
  reproduced with plain visuals.

## Open questions

- Pixel scaling approach: non-integer point filtering vs. Pixel Perfect
  Camera with crop/pad (Phase 2).
- Whether bubbles get a face/expression (cute but busy) or only the
  accessibility glyph.
- Whether background themes switch per level range or per curated
  pattern level.
- Asset source: whether everything comes from Claude Design, or some is
  hand-touched afterwards in a pixel editor (e.g. Aseprite or Pixelorama)
  for animation frames.
- Whether pet names appear in-game (e.g. on the victory overlay: "Mochi
  cleared the board!") or only on the menus.
- Whether the in-game 16-bit palette borrows any of the screens' dusk
  hues to make the jump between the two directions feel smaller.
