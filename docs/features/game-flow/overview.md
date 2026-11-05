# Game Flow and Pet Customization

**Status: implemented with placeholder UI** (Phase 4 in `../../ROADMAP.md`).
The art for these screens is planned in
[`../art/overview.md`](../art/overview.md), Art Phases 1b and 6b.

## Screen flow

```
                     ┌──────────────► SettingsMenu ──(Back)──┐
                     │                                       │
 MainMenu ──(New Game)──► PetCustomization ──(Start)──► Game │
   ▲   │                     │ (Back)                   │    │
   │   ├──(Continue)─────────┼─────────────────────────►│    │
   │   └──(2 Players)──► BattleMode                     │    │
   │                         │ (Menu)                   │    │
   └─────────────────────────┴──────(Menu)──────────────┴────┘
```

| Scene | Controller | Notes |
|---|---|---|
| `MainMenu.unity` | `Gameplay/MainMenuController` | Placeholder title plus 4 buttons. Continue is disabled until a solo run exists. |
| `PetCustomization.unity` | `Pets/PetCustomizationController` | Name, fur color and shape for the cat (top) and the dog (bottom). |
| `Game.unity` (was `SampleScene`) | `GameStateManager`, `LevelResultScreen` | Solo play. Victory and game over, see below. |
| `BattleMode.unity` | `Battle/BattleMatchController` | Unchanged. |
| `SettingsMenu.unity` | `Gameplay/SettingsMenuController` | Unchanged. |

Scene names live in `Flow/SceneNames`. The build order is MainMenu,
PetCustomization, Game, BattleMode, SettingsMenu.

## New Game vs. Continue

- **New Game** → PetCustomization → **Start**:
  1. both pets are saved (`PetProfileStore.Save`);
  2. `SoloRunSave.BeginNewRun()` marks that a run exists, sets the level
     to 1 and resets `SuperpowerProgress.HighestLevelReached` to 1, so
     superpower unlocks follow the run;
  3. the Game scene loads.

  Start is disabled while either pet's name is blank.
- **Continue** loads the Game scene directly. `GameBoard` has
  `startAtSavedSoloLevel` ticked in `Game.unity` only, so it opens at
  `SoloRunSave.CurrentLevel`. Battle Mode's boards leave it off and keep
  their random level 1–5.
- **Back** on customization returns to the menu without saving.
- Re-entering customization shows the previously saved pets.

`SoloRunRecorder`, which sits on the GameBoard object in `Game.unity`,
writes `SoloRunSave.CurrentLevel` every time a level loads (Continue,
Retry). So the saved level is always the level the player was last on,
including after a game over.

## Victory and game over

These were already in place before this phase. `GameStateManager` raises
`OnLevelWon` when the board is cleared and `OnLevelLost` when the ceiling
reaches the shooter line. `LevelResultScreen` shows one shared panel:

- **Victory:** "Level Complete!", Continue (next level) and Menu.
- **Game over:** "Game Over", Retry (same level, score reset) and Menu.

Splitting these into two distinct presentations is Art Phase 6b.

## Saved data (PlayerPrefs)

| Key | Type | Owner |
|---|---|---|
| `SoloRun.HasRun` | int (0/1) | `Flow/SoloRunSave` |
| `SoloRun.CurrentLevel` | int, default 1 | `Flow/SoloRunSave` |
| `HighestLevelReached` | int, default 1 | `Superpowers/SuperpowerProgress` (reset by New Game) |
| `Pet.Cat.Name` / `Pet.Dog.Name` | string, default "Mochi" / "Biscuit" | `Pets/PetProfileStore` |
| `Pet.{Cat,Dog}.Color` | index into the catalog's colors | `Pets/PetProfileStore` |
| `Pet.{Cat,Dog}.Shape` | index into the catalog's shapes | `Pets/PetProfileStore` |

## Pet appearance data

`ScriptableObjects/DefaultPetAppearanceCatalog.asset`
(`Pets/PetAppearanceCatalog`) holds, per species, a list of
`PetColorOption {Name, Color}` and `PetShapeOption {Name}`:

- **Cat:** colors Ginger, Cream, Silver, Midnight, Cocoa, Lilac; shapes
  Shorthair, Longhair, Slim, Chonky.
- **Dog:** colors Golden, Chocolate, Cream, Charcoal, Rust, Snow; shapes
  Floppy Ears, Pointy Ears, Fluffy, Short Legs.

The profile stores **indices**, and `CyclicIndex.Clamp` keeps a stale
index valid if the catalog shrinks. The art pass adds sprites per shape
and grows `Color` into a 4-shade fur ramp for the recolor material.

## UI construction

The menus build their placeholder uGUI at runtime through the shared
`UI/RuntimeUi` factory (buttons, labels, images and input fields, with
`UiPlacement`/`UiButtonSpec`/`UiInputSpec` describing each element).
Customization is composed from `PetEditorPanel` (one per pet) and
`OptionCycler` (a "< value >" row whose wraparound comes from the
unit-tested `CyclicIndex.Step`). `SettingsMenuController` and
`LevelResultScreen` still use their own builder code. Moving them onto
`RuntimeUi` is a natural step for Art Phase 6b, when every screen gets
skinned.

## Open questions

- Should pet names show in-game (HUD, victory overlay, Battle Mode
  labels)?
- Should Battle Mode use the saved pets for P1 and P2 once there's art
  to show them?
- Should New Game warn before overwriting an existing run?
