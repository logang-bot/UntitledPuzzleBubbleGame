# Art Pass — Roadmap

**Status: planned, not started.** This replaces the "Real art pass" bullet
in `../../ROADMAP.md`'s "Later / not yet scoped" section. It runs in
parallel with playtest/tuning work (see "Working in parallel" below).

## Art direction

- **Style:** 16-bit pixel art, evoking 1990s arcade puzzle games in
  general: chunky outlines, limited palettes, bouncy and cheerful
  animation. The inspiration is the genre's overall feel. We don't copy
  the inspiration game's specific characters, logo, UI layout, launcher
  design or backgrounds. Every character and screen is our own design.
- **Characters:** chibi **cats and dogs**, in two roles:
  - a **cat** runs the launcher in solo mode and as Player 1 in Battle
    Mode;
  - a **dog** is Player 2 in Battle Mode.
  - Optionally, other cat and dog breeds can appear as background
    "audience" or level-theme cameos.
- **Tone:** cozy and playful, like a pet-filled household or
  neighborhood. Nothing grim.
- **Accessibility:** each of the 6 bubble colors also carries a distinct
  inner glyph, such as a paw, a fish or a bone, so colorblind players
  can still tell bubbles apart. This matters for a matching game and
  costs little at this pixel size.

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
- **UI:** the HUD bottom bar (score, shots, level), the shot countdown
  (`ShotTimerDisplay`), superpower buttons with charge counts
  (`SuperpowerHud`), the win/lose panels (`LevelResultScreen`), the
  main menu, the settings menu, and the battle result screen.
- **Battle Mode:** Player 2's half is rendered rotated 180°. Every
  sprite must read correctly upside-down, so text is never baked into
  board art.

## Phases

### Art Phase 1 — Style guide and asset generation
- Generate a style guide and the asset set with Claude Design, using
  `claude-design-prompt.md` in this folder.
- Lock down:
  - the master palette;
  - the bubble size;
  - the outline rules;
  - the character model sheets (cat and dog: front, idle, happy, sad,
    panic);
  - the pixel font choice.
- **Deliverable:** PNG sprite sheets plus a palette file, reviewed on a
  real phone screen before anything is wired in.

### Art Phase 2 — Import pipeline
- Folder layout under `Assets/Art/`: `Bubbles/`, `Characters/`,
  `Board/`, `UI/`, `Effects/`, `Backgrounds/`, `Fonts/`.
- Import settings:
  - Sprite (2D and UI);
  - point filtering, no compression, no mipmaps;
  - one shared pixels-per-unit;
  - a Sprite Atlas per group.
- Decide the pixel scaling option (see constraints above).

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
- Win and lose panels, and the next-bubble frame.

### Art Phase 7 — Battle Mode pass
- The dog as Player 2's operator, plus the cat-vs-dog result screen.
- Check every Player 2 asset upside-down, since the whole half is
  rotated.
- An incoming-attack indicator, if the pending-rows meter ever gets a
  display.

### Deferred (wait on other decisions)
- **Main menu art, logo and title.** The game is still untitled.
- **Level map and meta-progression screens.** They wait on the meta
  progression design.
- **Audio.** It's a separate pass.

## Working in parallel with debugging and tuning

- **Branch and worktree.** Art code (Phase 3+) goes on its own branch,
  ideally a separate git worktree, so a debug fix on `main` never has to
  wait for half-finished renderer work.
- **Prefabs over scene edits.** `SampleScene.unity` and
  `BattleMode.unity` merge badly, so keep art in prefabs and components
  and keep scene diffs minimal.
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
