# Game Flow and Pet Customization

**Status: implemented** (Phase 4 in `../../ROADMAP.md`). Every screen
uses the vector cartoon art with pixel-art pets.

See "Screens: as built" in [`../art/overview.md`](../art/overview.md).

## Screen flow

```
                     ┌──────────────► SettingsMenu ──(Back)──┐
                     │                                       │
 MainMenu ──(New Game)──► PetCustomization ──(Start)──► Game │
   ▲   │                  (Aimer → Firer)               │    │
   │   │                     │ (Back on step 1)         │    │
   │   ├──(Continue)─────────┼─────────────────────────►│    │
   │   └──(2 Players)──► BattleMode                     │    │
   │                         │ (Home)                   │    │
   └─────────────────────────┴──────(Menu / Exit)───────┴────┘
```

Every scene change plays the paw-print iris (`Flow/SceneTransition`):
1. a paw-shaped hole with a gold rim shrinks onto the screen center in
   0.45 s;
2. the scene swaps while the screen is dark, during a 0.1 s hold;
3. the hole grows back in 0.45 s.

Input is blocked throughout. The first screen that starts
installs it (`SceneTransition.Install`) and it persists across loads.
A scene opened directly in the editor without a `MenuArtLibrary` falls
back to instant swaps.

| Scene | Controller | Notes |
|---|---|---|
| `MainMenu.unity` | `Gameplay/MainMenuController` | Space backdrop with drifting cosmos and pets, logo, 4 buttons. Continue is disabled until a solo run exists. |
| `PetCustomization.unity` | `Pets/PetCustomizationController` | Dusk sky with stars. Two steps: the aimer, then the firer. |
| `Game.unity` (was `SampleScene`) | `GameStateManager`, `LevelResultScreen` | Solo play. Victory and game over, see below. |
| `BattleMode.unity` | `Battle/BattleMatchController`, `BattleResultScreen` | 2-player result banners, then stats. |
| `SettingsMenu.unity` | `Gameplay/SettingsMenuController` | Dimmed space backdrop, one panel with two toggle rows: landing animation and thumb side (which side of the deck the rotate slabs sit on). |

Scene names live in `Flow/SceneNames`. The build order is MainMenu,
PetCustomization, Game, BattleMode, SettingsMenu.

## New Game vs. Continue

- **New Game** → PetCustomization:
  - **Step 1, "The aimer":** the CAT/DOG toggle picks which pet aims, and
    the card below edits that pet's name, fur and shape. **Next** moves
    on.
  - **Step 2, "The firer":** the card edits the other pet. The toggle
    still names the pet shown, so flipping it here swaps the roles.
    **Start**:
    1. saves both pets and the aimer (`PetProfileStore.Save`,
       `SaveAimer`);
    2. calls `SoloRunSave.BeginNewRun()`, which marks that a run exists,
       sets the level to 1 and resets
       `SuperpowerProgress.HighestLevelReached` to 1, so superpower
       unlocks follow the run;
    3. loads the Game scene.
  - Next and Start are disabled while the edited pet's name is blank.
  - **Back** on step 2 returns to step 1 and keeps the edits. Back on
    step 1 returns to the menu without saving.
  - Picking a fur, a shape or the role makes the portrait hop (its happy
    animation). The picked swatch or shape slot also pops.
  - The name field shows the name in capitals, as in the design. The
    name is saved as typed.
  - Re-entering customization shows the previously saved pets and aimer.
- **Continue** loads the Game scene directly. `GameBoard` has
  `startAtSavedSoloLevel` ticked in `Game.unity` only, so it opens at
  `SoloRunSave.CurrentLevel`. Battle Mode's boards leave it off and keep
  their random level 1–5.

The flow logic lives in the pure `Pets/CustomizationFlow`, tested by
`CustomizationFlowTests`.

`SoloRunRecorder`, which sits on the GameBoard object in `Game.unity`,
writes `SoloRunSave.CurrentLevel` every time a level loads (Continue,
Retry). So the saved level is always the level the player was last on,
including after a game over.

## Victory and game over

`GameStateManager` raises `OnLevelWon` when the board is cleared and
`OnLevelLost` when the ceiling reaches the shooter line.
`LevelResultScreen` shows the results on its own overlay canvas:

- **Victory:** the board under a 72 % dim, with:
  - fireworks, confetti and rising lanterns;
  - a "LEVEL COMPLETE" banner that drops in, then the level/score panel
    pops in;
  - **Continue** (next level) and **Menu**, which pop in with the panel.
- **Game over:**
  1. The knocked-out board stays visible for 3 s with input blocked.
  2. The paw iris shows the **Retry screen**:
     - the dusk sky and "KNOCKED OUT";
     - the pixel cat and dog napping on a cartoon blanket, in the saved
       pets' colors;
     - the level/score panel.
  3. **Retry** irises back to the same level with the score reset.
     **Exit** goes to the menu.

## 2-player result

`BattleResultScreen`, also on its own overlay canvas:

1. Each half shows a banner that drops in: YOU WIN (gold, with
   confetti), YOU LOSE (soft blue), or the DRAW plate. It stays for
   2.5 s. Player 2's half is on top, rotated 180°.
2. Then the board dims and each player gets a stats card:
   - score, bubbles popped, best chain (most bubbles cleared by one
     shot) and shots;
   - **Rematch** in the center bar;
   - **Home** at each player's right.

`Battle/MatchStats` and `MatchStatsTracker` collect the stats from each
side's board and shooter events. Rematch resets them.

## Saved data (PlayerPrefs)

| Key | Type | Owner |
|---|---|---|
| `SoloRun.HasRun` | int (0/1) | `Flow/SoloRunSave` |
| `SoloRun.CurrentLevel` | int, default 1 | `Flow/SoloRunSave` |
| `HighestLevelReached` | int, default 1 | `Superpowers/SuperpowerProgress` (reset by New Game) |
| `Pet.Cat.Name` / `Pet.Dog.Name` | string, default "Mochi" / "Biscuit" | `Pets/PetProfileStore` |
| `Pet.{Cat,Dog}.Color` | index into the catalog's colors | `Pets/PetProfileStore` |
| `Pet.{Cat,Dog}.Shape` | index into the catalog's shapes | `Pets/PetProfileStore` |
| `Pet.Aimer` | int (`PetSpecies`), default Cat; picks the in-game launcher (crank and lever when the dog aims, wheel and button when the cat aims) | `Pets/PetProfileStore` |
| `LandingAnimationStyle` | int (`LandingAnimationStyle`), default bounce | `Settings/GameSettings` |
| `Handedness` | int (`Handedness`), default Right | `Settings/GameSettings` |

## Pet appearance data

`ScriptableObjects/DefaultPetAppearanceCatalog.asset`
(`Pets/PetAppearanceCatalog`) holds, per species, a list of
`PetColorOption {Name, Ramp}` and `PetShapeOption {Name, ArtId}`:

- **Cat:**
  - colors Ginger, Cream, Silver, Midnight, Cocoa, Lilac;
  - shapes Shorthair, Longhair, Slim, Chonky (art ids round, fluffy,
    slim, chonk).
- **Dog:**
  - colors Golden, Chocolate, Cream, Charcoal, Rust, Snow;
  - shapes Floppy Ears, Pointy Ears, Fluffy, Short Legs (art ids floppy,
    pointy, fluffy, short).

`Ramp` is the 4-shade fur ramp from Claude Design's `colorways.json`,
written by Tools ▸ Art ▸ Import Art Sheets. The art id picks the
sprites, for example `cat_round_idle` and `shape_dog_short`.

The profile stores **indices**, and `CyclicIndex.Clamp` keeps a stale
index valid if the catalog shrinks.

## UI construction

The screens build their uGUI at runtime from the Claude Design sheets,
on a 360×780 pt canvas (`UI/MenuCanvas`, `UI/NativeStage`):

- `UI/MenuUiFactory` builds:
  - cartoon sprites, 9-slices, tiled sprites and full-screen backdrops;
  - pixel-art sprites kept on whole screen pixels (`PixelSprite`);
  - TMP labels in the guide's text styles (`UI/Text/TextStyles`);
  - skinned buttons, plates and the name field.
- `UI/SegmentedToggle`: the CAT/DOG and BOUNCE/SQUASH toggles.
- `Motion/MotionPlayer` plays the guide's motion spec (`MenuMotions`) on
  any rect.
- Each screen's views:
  - main menu: `SpaceBackdrop`, `StarField`, `CosmosSpawner`,
    `FloaterSpawner`;
  - customization: `DuskBackdrop`, `CustomizationHeader`,
    `PetPanelView`, `SwatchRowView`, `ShapePickerView`, `PortraitView`;
  - results: `VictoryOverlay`, `RetryScreen`, `ResultPanelView`,
    `ResultButtons`, `BannerView`, and the particles `ConfettiField`,
    `FireworkShow` and `LanternField`;
  - 2-player: `BattleResultHalfView`, `BattleStatsView`,
    `BattleStatsPanel`;
  - transition: `PawIris`.

`UI/RuntimeUi` remains the unskinned primitive layer underneath.
Each controller has a serialized `MenuArtLibrary`; that is the only scene
wiring. The details are in `../art/overview.md`, "Screens: as built".

## Open questions

- Should pet names show in-game (HUD, victory overlay, Battle Mode
  labels)?
- Should Battle Mode use the saved pets for P1 and P2 once there's art
  to show them?
- Should New Game warn before overwriting an existing run?
- The design's 2-player draw has two options for the launcher pets
  (puzzled or dozing). Pick one when the launcher art lands.
