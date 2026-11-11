# Art Pass — Roadmap

**Status: screens done (vector cartoon UI with pixel-art pets), in-game
art not started.** The pre-game screens were first built in pixel art
(Art Phases 1b, 2 and 6b). Every pre-game screen, result overlay and the
transition has since been rebuilt in a **vector cartoon style with
pixel-art pets** (see "Screens: as built" below).

Phases 1, 3–7 (in-game art) are still planned. This replaces the "Real
art pass" bullet in
`../../ROADMAP.md`'s "Later / not yet scoped" section. It runs in
parallel with playtest/tuning work (see "Working in parallel" below).

## Art direction

The game uses **two related art directions**. They share the same original
cat and dog cast, drawn in pixel art in both, but serve different jobs.

### A. In-game: 16-bit arcade (board, bubbles, HUD, launcher)

- **Style:** 16-bit pixel art, evoking 1990s arcade puzzle games in
  general: chunky outlines, limited palettes, bouncy and cheerful
  animation. The inspiration is the genre's overall feel. We don't copy
  the inspiration game's specific characters, logo, UI layout, launcher
  design or backgrounds. Every character and screen is our own design.
- **Why:** the board must stay readable at a glance during fast play, so
  flat colors, hard outlines and a small palette win over atmosphere.
- **Prompt:** [`claude-design-prompt.md`](claude-design-prompt.md).

### B. Pre-game screens and result overlays: vector cartoon UI, pixel-art pets

- **Covers:** the main menu, pet customization, settings, the
  victory and game-over overlays, the 2-player result overlay, and
  screen transitions.
- **Current style** (Claude Design, `Cartoon Menu Guide.dc.html`):
  - smooth vector cartoon art: chunky ink outlines (#1a1424, the same
    ink as the pixel art), glossy 3-stop gradient fills with a soft
    highlight;
  - a cozy night palette: night grounds, warm gold and coral for the
    forward action, violet for everything else, soft blue for sad
    states;
  - Lilita One for titles, buttons and numbers; Baloo 2 for labels,
    toggles and the name field.
- **The pets stay pixel art.** The portraits, the space floaters and the
  Retry nap are the existing pixel sheets. They are drawn at whole
  screen-pixel scales, so they match the in-game 16-bit pets. Around
  them the cartoon art steps down to thinner lines and flatter fills.
- **Why the change:** on a phone, the pixel screens felt choppy and
  their runtime fonts never sat on the art's pixel grid. In the cartoon
  version the engine animates everything at 60 fps, and text and color
  are designed as part of the style. Prompt:
  [`claude-design-prompt-menus-cartoon.md`](claude-design-prompt-menus-cartoon.md).
- **Previous style (superseded):** the atmospheric indie pixel-art
  direction below. It is kept for reference, since the pixel pets still
  come from it.
- **Previous style details:** inspired by modern indie pixel games such
  as *Celeste*:
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
- **Prompt (pixel version):**
  [`claude-design-prompt-menus.md`](claude-design-prompt-menus.md).

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
- **Layout grid: 360×780 pt** (9:19.5). The cartoon sheets are exported
  at 3×, so a full screen is 1080×2340 px.
  - Other aspect ratios extend the background sideways or vertically.
    Backdrops bleed 32 pt beyond the grid (424×844 pt placed at
    (−32, −32)), and the UI anchors to the safe area.
- **Safe area:** nothing interactive in the top 32 pt or the bottom
  22 pt (notches and gesture bars).
- Buttons, panels, plates, fields and toggles are **9-slice**, so the
  same art serves every size.
  - A 9-slice rect must be at least as large as its borders. Smaller
    rects squash the rounded corners.
- **Motion is code, not frames.** Drift, twinkle, spin, pulse, pops and
  drops come from the guide's motion spec. Only the fireworks are baked
  (10 frames at 15 fps).
- The pixel pets keep their own sheets. The guide gives their scale in
  device pixels per art pixel on the 3× reference:
  - portrait 5;
  - floater and nap 4;
  - swatch fill 5;
  - shape silhouette 6.

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

*This is the original plan. It was built first in pixel art and is now
being redone in the cartoon style; see "Screens: as built" for what was
actually built.*

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

The screens follow the **latest Claude Design source** (the "Mobile
bubble-shooter art system" project). Where it differs from the plan
above, it wins.
- **Cartoon UI:** `Cartoon Menu Guide.dc.html`, with `cartoon-art.js` and
  `cartoon-engine.js`.
- **Pixel pets:** `Menu Art Guide.dc.html`, with `menus.js` and
  `menus-scenes.js`.

### Rework status (vector cartoon + pixel pets)

| Screen | State |
|---|---|
| Main menu | ✅ Rebuilt on the cartoon guide |
| Pet customization (both steps) | ✅ Rebuilt on the cartoon guide |
| Paw-print transition | ✅ Rebuilt (stencil iris, eased) |
| Settings | ✅ Rebuilt on the cartoon guide |
| Victory, knock-out → Retry | ✅ Rebuilt on the cartoon guide |
| 2-player banners and stats | ✅ Rebuilt on the cartoon guide |

The old pixel UI has been removed:
- the sheets `buttons`, `panels`, `space`, `cosmos`, `fx`, `transition`
  and `logo`, with their indices and `MANIFEST.json`;
- the Pixelify Sans and Silkscreen fonts;
- `MenuPalette`, `CelebrationParticles`, `LoopingLayer`,
  `TransitionIrisBuilder`, `DottedLine` and `LabelLine`.

The importer now reads 13 sheets (280 sprite ids) with no duplicate ids.

### Bundle and import

- **Source files:**
  - cartoon sheets: Claude Design's "Download Unity bundle" (3× PNGs plus
    legend JSONs) in `Assets/Art/Cartoon/Sheets/`;
  - the spec files (`tokens.json`, `motion.json`, `layout.json`,
    `fonts.txt`, `MANIFEST.json`) in `Assets/Art/Cartoon/Spec/`. Code
    transcribes them; they're kept so a re-export can be diffed.
  - The pixel pet sheets (`chars_cat`, `chars_dog`, `floaters`, `retry`,
    `picker`) and `colorways.json` stay in `Assets/Art/Screens/`.
- **Tools ▸ Art ▸ Import Menu Sheets** (`Editor/Art/MenuArtImportMenu`)
  can be re-run after every re-export. It imports both folders, each
  with its own `SheetImportProfile`:
  - `Pixel`: Point filtering, PPU 1, uncompressed, into
    `Assets/Art/Screens/Index/`;
  - `Cartoon`: Bilinear filtering, PPU 3, CompressedHQ, into
    `Assets/Art/Cartoon/Index/`.

  Both profiles skip mipmaps. Sprites are sliced into frames named
  `{id}_{frame}`, with the legend's pivot and 9-slice border (in 3× px).
  The importer also:
  - builds `Assets/Art/Screens/MenuArtLibrary.asset` (pixel sheets
    first, cartoon second);
  - creates the TMP fonts (`FontAssetWriter`);
  - creates the shared materials (`UIFurRecolor`, `UIIrisHole`,
    `UIIrisFill`, `UIIrisRim` in `Assets/Art/Shaders/`);
  - copies the colorway ramps and shape art ids into
    `DefaultPetAppearanceCatalog`.
- **Fonts:**
  - Lilita One (display) and Baloo 2 SemiBold, Bold and ExtraBold (UI),
    all SIL OFL. The TTFs and `*-OFL.txt` are in `Assets/Art/Fonts/`.
  - The SDF font assets (`* SDF.asset`) are sampled at 72 pt with 20 px
    padding on a 1024×2048 atlas, pre-filled with printable ASCII. They
    stay dynamic, so typed pet names can add glyphs.
  - TMP Essential Resources live in `Assets/TextMesh Pro/`.
- **Fur recolor:** unchanged. The `Game/UI/FurRecolor` shader swaps the
  pixel pets' 4 fur-key colors for the chosen 4-shade ramp, with one
  material instance per recolored Image.

### Canvas and placement

- `MenuCanvas.Configure` sets up every screen canvas:
  - Scale With Screen Size at a **360×780** reference with match mode
    Expand, so one canvas unit is one pt and the whole grid always fits;
  - reference PPU 1. Cartoon sprites at PPU 3 come out one unit per pt,
    and so do their 9-slice borders.
  - Pixel-perfect is **off**, so drift stays smooth.
- `NativeStage` gives each screen a full-bleed background and a 360×780
  stage centered in the safe area (`SafeAreaFitter`). Layout rects are
  the guide's pt from the top-left (`UiPlacement.TopLeft(RectInt)`), as
  listed in `layout.json`.
- Placement helpers:
  - `UiPlacement.SmoothPosition` places moving things without rounding;
  - `UiPlacement.PivotAt` and `MenuUiFactory.PivotSprite` put a sprite's
    legend pivot on a point;
  - `UiPlacement.MovePivot` re-pivots a rect in place, so it scales or
    rotates about its center.
- `MenuUiFactory.Cover` stretches a static backdrop over the whole
  screen, bleed included (`AspectRatioFitter`, Envelope Parent).
- **Pixel pets** use `MenuUiFactory.PixelSprite` with a `PixelPlacement`
  (pivot point, plus the guide's device px per art px). Its `PixelSnap`
  component:
  - sizes the sprite so one art pixel covers a whole number of screen
    pixels (`PixelArtScale.UnitsPerArtPixel`, rounded per device, at
    least 1);
  - snaps the pivot onto the screen-pixel grid every LateUpdate.
- The Game and BattleMode result screens build their own overlay canvas
  (`OverlayCanvasFactory`), so the HUD canvas and touch zones are
  untouched. They change with Phase 6.

### Text

- `TextStyles` holds the guide's 14 text tokens:
  - Title, TitleSad, TitlePlate;
  - ButtonPrimary, ButtonSecondary, ButtonDisabled;
  - Label, SmallLabel;
  - Number, StatValue;
  - SegmentOn, SegmentOff;
  - NameText, NamePlaceholder.

  Each `TextStyle` sets the font role, the size in pt, a solid or
  top-to-bottom gradient fill, the outline, the drop shadow and the
  letter spacing. It's built fluently (`WithFill`, `Aligned`, …).
- `TextStyleApplier` puts a style on a `TextMeshProUGUI`:
  - the face uses the plain font material, with the gradient as a TMP
    vertex gradient;
  - letter spacing becomes `characterSpacing` (pt / size × 100).
- **Outlines and shadows are a second layer.** TMP draws an outline per
  letter, so a thick outline paints over the previous letter's face (a T
  loses half its crossbar). `TextOutlineLayer` puts a copy of the text
  just behind the face:
  - the copy is dilated out to the outline's edge, in the outline color
    (`TextStyleMaterials`, one material per font and style);
  - the shadow is that copy's underlay, offset down;
  - the layer follows the face's text, rect, enabled state and
    lifetime.

  This matches the guide, which strokes the whole word before filling
  it. The SDF unit conversions are documented on
  `TextStyleMaterials.SdfScale`.
- Colors used at runtime are in `MenuColors` (from `tokens.json`).

### Motion

- `Scripts/Motion/` is pure and tested:
  - `Ease` has the guide's 7 curves (linear, inQuad, outQuad, inOutSine,
    inCubic, outCubic, outBack);
  - `KeyTrack`, `SineTrack` and `SpinTrack` feed a `MotionClip` with
    channels x, y and rot (added) and sx, sy and alpha (multiplied),
    plus an optional delay and duration;
  - `MenuMotions` transcribes `motion.json`: logo bob, button release,
    banner drop, panel pop, swatch pick, galaxy glow and spin, bubble
    wobble, star twinkle.
- `MotionPlayer` plays a clip on a RectTransform from `Update()`, on
  unscaled time, relative to its rest pose. It only touches the channels
  the clip has, and `Play()` re-triggers it.
- Everything runs at 60 fps. `FrameRateInitializer` also sets the target
  before the first scene loads, so the menus no longer start at the
  mobile default of 30 fps.

### UI kit

- **Buttons** (`MenuButtonBuilder`, `SkinnedButton`, `ButtonSkin`):
  - kinds Primary, Secondary, Disabled, Back, Home, Rematch, ArrowLeft
    and ArrowRight map to `btn_*`, `icon_*` and `arrow_*` sprites, with
    sprite-swap pressed states;
  - the caption sits 1.5 pt from the top and 7.5 pt from the bottom of
    the face, and drops 4 pt while pressed;
  - on release the button springs 0.96 → 1;
  - `SetInteractable(false)` shows the disabled face and caption style.
- **`SegmentedToggle`:** a `toggle_track` with `seg_on` / `seg_off`
  segments inset 4 pt. The selected caption is raised 3 pt.
- **Name field** (`NameFieldBuilder`, `NameFieldSkin`):
  - a `TMP_InputField` on `field_empty` / `_focused` / `_filled`;
  - text inset 18 pt, an ink caret blinking once a second;
  - the customization screen shows names in capitals.
- **Other widgets:**
  - `MenuUiFactory.Plate` (title plate plus caption);
  - `Tiled` (the dotted divider);
  - `LabelIn` / `Label`.

### Screens

- **Main menu** (`MainMenuController`, `Menu/`):
  - `SpaceBackdrop`: the deep space covers the screen.
  - `StarField`: two seamless star layers scrolling left at 6 pt/s
    (wrapping at 424 pt), plus 14 twinkling sparkles.
  - `CosmosSpawner`, from `CosmosSprites.Slots`:
    - a hero slot alternating the hero and rose spirals in y 200–420;
    - three small slots with planets, the teal spiral and the comet;
    - galaxies spin at 3°/s over a pulsing glow;
    - the comet flies sideways, mirrored to trail its tail;
    - `ShootingStarView`: every 6 s, a 0.7 s streak at 420 pt/s, 20–35°
      below horizontal.
  - `FloaterSpawner`: 12 slots, each with a pre-warmed `FloaterSchedule`
    (see below).
  - The logo lockup has its pivot at (180, 52) and bobs (`LogoBob`).
  - Four 228×52 buttons sit at y 520/580/640/700. Continue is disabled
    without a save.
- **Floating pets:**
  - **Speed:** constant per size (S 9, M 15, L 23 pt/s), small to large
    drawn back to front for depth. This is a deliberate change from the
    guide, where every crossing took its slot's fixed time.
  - **Crossings:** horizontal ones stay in y 250–500; each is followed
    by a 2–8 s rest.
  - **Seeding:** every flight is seeded from (slot, flight number), and
    each slot starts at a random point of its first flight, so the sky
    is busy on the first frame.
  - **Look:** a pixel floater at 4 device px per art px, recolored, in
    90° turns (none for the tumble pose) and mirrored. It bobs ±3 pt
    over 3 s; 20 % sit inside a wobbling cartoon bubble.
- **Pet customization** (`PetCustomizationController`, `PetPanelView`):
  - `DuskBackdrop`: the dusk sky with its moon and hills, plus the
    `StarField`;
  - the back icon, title, step and hint (`CustomizationHeader`);
  - the CAT/DOG toggle at (54, 108, 252, 44);
  - a 336×400 pt panel at (12, 186), holding:
    - the cushion (cat) or mat (dog), with the pixel portrait's feet at
      (180, 370), 5 device px per art px (`PortraitView`);
    - the name field at (80, 396, 200, 48);
    - FUR and SHAPE labels;
    - `SwatchRowView`: 40 pt swatches from (92, 458), the pixel fill
      under a cartoon ring;
    - `ShapePickerView`: arrows and 40×46 slots with cartoon silhouettes
      from (92, 514).
  - Picks pop (`SwatchPick`) and make the pet hop.
  - Next/Start at (46, 664, 268, 56).
- **Settings** (`SettingsMenuController`):
  - the deep space covering the screen and the `StarField`, under the
    `settingsBackdrop` dim (62 %);
  - the back icon;
  - a 308×132 pt panel at (26, 300) under a 180×44 SETTINGS title plate
    at (90, 280);
  - one row: LANDING (label) and ANIMATION (small label), the
    BOUNCE/SQUASH toggle at (164, 342, 152, 44), and a dotted divider at
    (46, 404, 268, 6).
  - Further rows stack at a 52 pt pitch.
- **Shared result pieces** (`Results/`):
  - `BannerView` (from a `BannerSpec`): a banner or plate with its
    caption, dropping in with `BannerDrop` each time it is shown;
  - `ResultPanelView`: the 268×148 pt level/score panel at (46, 372),
    with LEVEL nn (label), SCORE (small label) and the score (number),
    popping in with `PanelPop` (0.25 s after the banner);
  - `ResultButtons`: the 240×56 pt primary/secondary pair at (60, 560)
    and (60, 628), popping in with the panel.
- **Particles** (`Results/`, each its own MonoBehaviour on unscaled
  time, using the pure `ParticleMotion` helpers):
  - `ConfettiField`: pieces fall at 50–90 pt/s and wrap, sway ±12 pt
    over 1.6 s, spin 180–420°/s, and flip their x scale over 0.6 s;
  - `FireworkShow`: a burst every 0.7 s in x 40–320 and y 70–270,
    scaled 0.7–1.1, cycling gold, coral and teal, each playing its 10
    frames once at 15 fps;
  - `LanternField`: 5 lanterns rising at 18–26 pt/s and re-entering
    below, swaying ±6 pt and rocking ±4°, over a glow pulsing 0.8 ± 0.2.
- **Victory overlay** (`VictoryOverlay`):
  - the board under the victoryDim (72 %);
  - lanterns and fireworks, then the LEVEL COMPLETE win banner at
    (10, 132);
  - the panel and Continue/Menu;
  - 28 confetti pieces over everything.
- **Retry screen** (`RetryScreen`):
  - the `DuskBackdrop`;
  - KNOCKED OUT (title_sad) and THE PETS NEED A NAP;
  - the cartoon `stage_nap` blanket with its pivot at (177, 329), under
    the pixel `retry_nap` at (20, 156), 4 device px per art px,
    recolored as the saved pets;
  - the panel and Retry/Exit.
- **2-player result** (`Battle/`):
  - `BattleHalfFrame` makes two 360×390 pt halves; the top one is
    rotated 180°.
  - `BattleResultHalfView` shows, per half, one of:
    - YOU WIN (`banner_win`, title) under 16 confetti pieces;
    - YOU LOSE (`banner_lose`, title_sad, soft blue);
    - the DRAW plate (`draw_plate`, title_plate).

    Each drops in.
  - `BattleStatsView`:
    - the statsDim (78 %);
    - a `BattleStatsPanel` per half: the `stats_card`, a WIN / LOSE /
      DRAW title plate, and rows every 42 pt (label left, stat_value
      right) with dotted dividers;
    - the night center strip at y 374, edged in ink with a panelRim
      line. It runs full-bleed past the grid, so wider screens are
      covered too.
    - Rematch sits on the strip, and there's a Home button at each
      player's right (the top one rotated).
- **Transition** (`Flow/SceneTransition`, `PawIris`, `TransitionTimeline`):
  - It draws three stencil layers:
    1. `paw_mask` writes stencil bit 128 where its alpha passes 0.5
       (`Game/UI/StencilWrite`);
    2. the dark fill draws outside that hole (`Game/UI/StencilOutside`);
    3. the same paw at 1.06× in gold draws outside it too, as the rim.
  - The paw's pivot (its main pad) sits on the screen center.
  - Timing: the hole closes from 24× to 0 in 0.45 s (inCubic), holds
    0.1 s while the scene swaps, then opens in 0.45 s (outCubic).
  - The time step is capped at 1/30 s, so a scene-load hitch can't skip
    the opening.

Every cartoon sprite in the bundle is in use. In the kept pixel `picker`
sheet, only the `swatch_<species>_<color>` fills are used; its old swatch
frames and shape tiles are leftovers.

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

- In-game pixel scaling: non-integer point filtering vs. Pixel Perfect
  Camera with crop/pad (Phase 2). The screens already answer this for
  their pixel pets: they round to whole screen pixels per device
  (`PixelSnap`).
- The text outline and shadow factors (`TextStyleMaterials`) are derived
  from TMP's shader math. Check them on a real phone against the guide's
  text-token cards.
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
