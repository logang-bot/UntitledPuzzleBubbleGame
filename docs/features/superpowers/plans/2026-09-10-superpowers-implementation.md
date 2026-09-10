# Superpowers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the four launch superpower abilities (Freeze, Bomb, Row
Clear, Rainbow) as player-activated, unlock-gated consumables that hook into
the existing `GameBoard` event pipeline with no changes to its public
signatures.

**Architecture:** Pure logic (charge economy, per-ability cell resolution,
hex-radius search) lives in plain C# classes under `Assets/Scripts/Superpowers/`
and gets EditMode unit tests, mirroring the existing `MatchResolver`/
`FloodFill`/`LevelGenerator` split of "pure static logic + thin MonoBehaviour
wiring." Two existing files (`ShotTimer`, `GameStateManager`) get small,
narrowly-scoped additions; `FiredBubbleController` gains an "armed ability"
concept parallel to its existing `_color`/`_nextColor` fields. MonoBehaviour
wiring and UI get verified in Play Mode via the Unity MCP bridge, following
the Milestone 10 HUD precedent.

**Tech Stack:** Unity 6000.5.1f1, C#, Unity Test Framework (NUnit, EditMode),
Unity MCP bridge for Play Mode verification.

**Spec:** `docs/features/superpowers/specs/2026-09-10-superpowers-design.md`
— this plan implements that spec exactly; read it first for the *why* behind
each decision. Two small refinements made while planning (both consistent
with the spec, not contradicting it) are called out inline where they occur:
`SuperpowerController` (not `GameStateManager`) listens for `OnLevelWon` to
update `SuperpowerProgress`, keeping `GameStateManager` free of any
Superpowers-specific coupling; and `SuperpowerHud` lives at
`Assets/Scripts/Superpowers/SuperpowerHud.cs` (namespace `Game.Superpowers`)
rather than the spec's placeholder `Scripts/UI/` path, since this project has
no `UI` folder — HUD-like classes live beside the subsystem they render for
(see `Assets/Scripts/Gameplay/HudDisplay.cs`).

## Global Constraints

- File length ≤ 250 lines (imports not counted). Function bodies ≤ 7 lines
  (≤10 with comments/blank-line slack). ≤ 3 parameters per function — group
  extras into a type. These are enforced per-file below; none of the new
  files in this plan are expected to approach the cap.
- Member order: `[SerializeField]` fields, then public events/properties,
  then private fields, then Unity lifecycle methods (`Awake`/`Start`/
  `OnDestroy`/`Update`, kept together per the existing `GameStateManager`/
  `CameraShake` precedent even where that runs ahead of "public before
  private"), then remaining public methods, then private methods, each
  helper directly below its caller.
- No inline comments narrating code. Namespaces: `Game.Superpowers` for all
  new production files, `Game.Tests` for all new test files (matching every
  existing test file regardless of subsystem).
- Test convention: plain `public class` (no `[TestFixture]`), `[Test]`
  methods named `MethodUnderTest_Scenario_ExpectedOutcome`, classic
  `Assert.AreEqual`/`Assert.IsTrue`/`Assert.IsFalse` and
  `CollectionAssert.*` — no `Assert.That` constraint syntax (none of the
  existing tests use it). Pure logic gets EditMode tests; MonoBehaviour
  wiring gets a Play Mode check via Unity MCP instead of an EditMode test,
  matching the project's established split.
- **Do not run `git commit`.** This project's convention is that committing
  is the user's job. Each task's final step stages the relevant files with
  `git add` and stops there — never commit.

---

## Task 1: `SuperpowerId`, `SuperpowerDefinition`, `SuperpowerChargeTracker`

The core charge-economy logic, kept fully decoupled from Unity so it's
directly unit-testable — mirrors `LevelGenerator` taking a plain
`DifficultyConfig` rather than the `DifficultyCurveConfig` ScriptableObject
itself.

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerId.cs`
- Create: `Assets/Scripts/Superpowers/SuperpowerDefinition.cs`
- Create: `Assets/Scripts/Superpowers/SuperpowerChargeTracker.cs`
- Test: `Assets/Tests/EditMode/SuperpowerChargeTrackerTests.cs`

**Interfaces:**
- Produces: `enum SuperpowerId { Freeze, Bomb, RowClear, Rainbow }`;
  `class SuperpowerDefinition { public SuperpowerId Id; public int
  UnlockLevel; public int ChargesPerLevel; }`; `class
  SuperpowerChargeTracker` with `IEnumerable<SuperpowerId> UnlockedIds`,
  `void ResetForLevel(IReadOnlyList<SuperpowerDefinition> definitions, int
  highestLevelReached)`, `int Remaining(SuperpowerId id)`, `bool
  TryConsume(SuperpowerId id)`. Task 2 (`SuperpowerCatalog`) and Task 6
  (`SuperpowerController`) both depend on these exact names/signatures.

- [ ] **Step 1: Create the enum and definition data class**

`Assets/Scripts/Superpowers/SuperpowerId.cs`:
```csharp
namespace Game.Superpowers
{
    public enum SuperpowerId
    {
        Freeze,
        Bomb,
        RowClear,
        Rainbow
    }
}
```

`Assets/Scripts/Superpowers/SuperpowerDefinition.cs`:
```csharp
using System;

namespace Game.Superpowers
{
    [Serializable]
    public class SuperpowerDefinition
    {
        public SuperpowerId Id;
        public int UnlockLevel;
        public int ChargesPerLevel;
    }
}
```

- [ ] **Step 2: Write the failing tests for `SuperpowerChargeTracker`**

`Assets/Tests/EditMode/SuperpowerChargeTrackerTests.cs`:
```csharp
using System.Collections.Generic;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class SuperpowerChargeTrackerTests
    {
        [Test]
        public void ResetForLevel_HighestLevelBelowUnlock_AbilityNotUnlocked()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Freeze, UnlockLevel = 3, ChargesPerLevel = 1 }
            };

            tracker.ResetForLevel(definitions, highestLevelReached: 2);

            Assert.AreEqual(0, tracker.Remaining(SuperpowerId.Freeze));
            CollectionAssert.DoesNotContain(new List<SuperpowerId>(tracker.UnlockedIds), SuperpowerId.Freeze);
        }

        [Test]
        public void ResetForLevel_HighestLevelAtOrAboveUnlock_GrantsConfiguredCharges()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Bomb, UnlockLevel = 6, ChargesPerLevel = 2 }
            };

            tracker.ResetForLevel(definitions, highestLevelReached: 6);

            Assert.AreEqual(2, tracker.Remaining(SuperpowerId.Bomb));
        }

        [Test]
        public void TryConsume_HasCharge_DecrementsAndReturnsTrue()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Freeze, UnlockLevel = 1, ChargesPerLevel = 1 }
            };
            tracker.ResetForLevel(definitions, highestLevelReached: 1);

            var consumed = tracker.TryConsume(SuperpowerId.Freeze);

            Assert.IsTrue(consumed);
            Assert.AreEqual(0, tracker.Remaining(SuperpowerId.Freeze));
        }

        [Test]
        public void TryConsume_NoChargesRemaining_ReturnsFalse()
        {
            var tracker = new SuperpowerChargeTracker();

            var consumed = tracker.TryConsume(SuperpowerId.Freeze);

            Assert.IsFalse(consumed);
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

If the Unity Editor is closed: run `.\run-edittests.ps1` at the project
root. If it's open, use **Window > General > Test Runner** or the Unity MCP
`run_tests`/`get_test_job` tools.
Expected: FAIL/compile error — `SuperpowerChargeTracker` doesn't exist yet.

- [ ] **Step 4: Implement `SuperpowerChargeTracker`**

`Assets/Scripts/Superpowers/SuperpowerChargeTracker.cs`:
```csharp
using System.Collections.Generic;

namespace Game.Superpowers
{
    public class SuperpowerChargeTracker
    {
        public IEnumerable<SuperpowerId> UnlockedIds => _charges.Keys;

        private readonly Dictionary<SuperpowerId, int> _charges = new();

        public void ResetForLevel(IReadOnlyList<SuperpowerDefinition> definitions, int highestLevelReached)
        {
            _charges.Clear();
            foreach (var definition in definitions)
                if (highestLevelReached >= definition.UnlockLevel)
                    _charges[definition.Id] = definition.ChargesPerLevel;
        }

        public int Remaining(SuperpowerId id) => _charges.TryGetValue(id, out var count) ? count : 0;

        public bool TryConsume(SuperpowerId id)
        {
            if (Remaining(id) <= 0) return false;
            _charges[id]--;
            return true;
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

Same command as Step 3. Expected: PASS, all 4 tests green.

- [ ] **Step 6: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerId.cs Assets/Scripts/Superpowers/SuperpowerDefinition.cs Assets/Scripts/Superpowers/SuperpowerChargeTracker.cs Assets/Tests/EditMode/SuperpowerChargeTrackerTests.cs
```
Do not commit.

---

## Task 2: `SuperpowerCatalog` ScriptableObject + default asset

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerCatalog.cs`
- Create (via Unity Editor/MCP, not a text file edit): `Assets/ScriptableObjects/DefaultSuperpowerCatalog.asset`

**Interfaces:**
- Consumes: `SuperpowerDefinition` (Task 1).
- Produces: `class SuperpowerCatalog : ScriptableObject { public
  IReadOnlyList<SuperpowerDefinition> Definitions { get; } }`. Task 6
  (`SuperpowerController`) reads `catalog.Definitions` and passes it
  straight into `SuperpowerChargeTracker.ResetForLevel`.

No dedicated EditMode test for this file — like `DifficultyCurveConfig`, it
is pure data plumbing with no logic of its own; its wiring is verified in
Task 10's Play Mode pass.

- [ ] **Step 1: Implement `SuperpowerCatalog`**

`Assets/Scripts/Superpowers/SuperpowerCatalog.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Game.Superpowers
{
    [CreateAssetMenu(menuName = "Game/Superpower Catalog")]
    public class SuperpowerCatalog : ScriptableObject
    {
        [SerializeField] private List<SuperpowerDefinition> definitions = new();

        public IReadOnlyList<SuperpowerDefinition> Definitions => definitions;
    }
}
```

- [ ] **Step 2: Confirm it compiles**

Open/refresh the Unity Editor (or `mcp__UnityMCP__refresh_unity`) and check
`mcp__UnityMCP__read_console` for compile errors. Expected: no errors.

- [ ] **Step 3: Create the default asset with the spec's placeholder values**

Via the Unity MCP bridge (`mcp__UnityMCP__manage_asset` to create the
asset, `mcp__UnityMCP__manage_scriptable_object` or direct Inspector edits
to populate the list), create
`Assets/ScriptableObjects/DefaultSuperpowerCatalog.asset` with exactly:

| Id | UnlockLevel | ChargesPerLevel |
|---|---|---|
| Freeze | 3 | 1 |
| Bomb | 6 | 1 |
| RowClear | 9 | 1 |
| Rainbow | 12 | 1 |

- [ ] **Step 4: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerCatalog.cs Assets/Scripts/Superpowers/SuperpowerCatalog.cs.meta Assets/ScriptableObjects/DefaultSuperpowerCatalog.asset Assets/ScriptableObjects/DefaultSuperpowerCatalog.asset.meta
```
Do not commit.

---

## Task 3: `SuperpowerProgress` (persisted unlock tracking)

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerProgress.cs`
- Test: `Assets/Tests/EditMode/SuperpowerProgressTests.cs`

**Interfaces:**
- Produces: `static class SuperpowerProgress { public static int
  HighestLevelReached { get; set; } }`. Task 6 (`SuperpowerController`)
  reads this for `ResetForLevel` and writes it on `OnLevelWon`.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/SuperpowerProgressTests.cs`:
```csharp
using Game.Superpowers;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SuperpowerProgressTests
    {
        private const string Key = "HighestLevelReached";

        [SetUp]
        public void SetUp() => PlayerPrefs.DeleteKey(Key);

        [TearDown]
        public void TearDown() => PlayerPrefs.DeleteKey(Key);

        [Test]
        public void HighestLevelReached_NoValueSaved_DefaultsToOne()
        {
            Assert.AreEqual(1, SuperpowerProgress.HighestLevelReached);
        }

        [Test]
        public void HighestLevelReached_AfterSettingValue_PersistsAndReturnsIt()
        {
            SuperpowerProgress.HighestLevelReached = 7;

            Assert.AreEqual(7, SuperpowerProgress.HighestLevelReached);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Same test-running instructions as Task 1 Step 3.
Expected: FAIL/compile error — `SuperpowerProgress` doesn't exist yet.

- [ ] **Step 3: Implement `SuperpowerProgress`**

`Assets/Scripts/Superpowers/SuperpowerProgress.cs`, following the exact
`GameSettings` (`Assets/Scripts/Settings/GameSettings.cs`) precedent — one
static property per persisted value, backed by an un-prefixed
`private const string` key, saving immediately on write:
```csharp
using UnityEngine;

namespace Game.Superpowers
{
    public static class SuperpowerProgress
    {
        private const string HighestLevelReachedKey = "HighestLevelReached";

        public static int HighestLevelReached
        {
            get => PlayerPrefs.GetInt(HighestLevelReachedKey, 1);
            set
            {
                PlayerPrefs.SetInt(HighestLevelReachedKey, value);
                PlayerPrefs.Save();
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Same command as Step 2. Expected: PASS, both tests green.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerProgress.cs Assets/Tests/EditMode/SuperpowerProgressTests.cs
```
Do not commit.

---

## Task 4: `HexRadius` (pure BFS radius utility)

**Files:**
- Create: `Assets/Scripts/Superpowers/HexRadius.cs`
- Test: `Assets/Tests/EditMode/HexRadiusTests.cs`

**Interfaces:**
- Consumes: `GridModel.GetNeighbors(int row, int col)` (existing, returns
  `List<(int Row, int Col)>`).
- Produces: `static class HexRadius { public static HashSet<(int Row, int
  Col)> CellsWithinRadius(GridModel grid, (int Row, int Col) center, int
  radius) }`. Task 5 (`SuperpowerEffectResolver.ResolveBomb`) depends on
  this exact signature.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/HexRadiusTests.cs`:
```csharp
using System.Collections.Generic;
using Game.Grid;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class HexRadiusTests
    {
        [Test]
        public void CellsWithinRadius_RadiusZero_ReturnsOnlyCenter()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var result = HexRadius.CellsWithinRadius(grid, (2, 2), 0);

            CollectionAssert.AreEquivalent(new List<(int Row, int Col)> { (2, 2) }, result);
        }

        [Test]
        public void CellsWithinRadius_RadiusOne_IncludesAllImmediateNeighbors()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            var expectedNeighbors = grid.GetNeighbors(2, 2);

            var result = HexRadius.CellsWithinRadius(grid, (2, 2), 1);

            foreach (var neighbor in expectedNeighbors)
                CollectionAssert.Contains(result, neighbor);
            CollectionAssert.Contains(result, (2, 2));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL/compile error — `HexRadius` doesn't exist yet.

- [ ] **Step 3: Implement `HexRadius`**

`Assets/Scripts/Superpowers/HexRadius.cs`:
```csharp
using System.Collections.Generic;
using Game.Grid;

namespace Game.Superpowers
{
    public static class HexRadius
    {
        public static HashSet<(int Row, int Col)> CellsWithinRadius(GridModel grid, (int Row, int Col) center, int radius)
        {
            var visited = new HashSet<(int Row, int Col)> { center };
            var frontier = new List<(int Row, int Col)> { center };
            for (var step = 0; step < radius; step++)
                frontier = ExpandFrontier(grid, frontier, visited);
            return visited;
        }

        private static List<(int Row, int Col)> ExpandFrontier(GridModel grid, List<(int Row, int Col)> frontier, HashSet<(int Row, int Col)> visited)
        {
            var next = new List<(int Row, int Col)>();
            foreach (var cell in frontier)
                foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                    if (visited.Add(neighbor))
                        next.Add(neighbor);
            return next;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: PASS, both tests green.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/HexRadius.cs Assets/Tests/EditMode/HexRadiusTests.cs
```
Do not commit.

---

## Task 5: `SuperpowerEffectResolver` (per-ability cell resolution)

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerEffectResolver.cs`
- Test: `Assets/Tests/EditMode/SuperpowerEffectResolverTests.cs`

**Interfaces:**
- Consumes: `HexRadius.CellsWithinRadius` (Task 4);
  `Game.Grid.FloodFill.Run(GridModel, IEnumerable<(int,int)>, Func<(int,int),bool>)`
  and `GridModel.GetNeighbors`/`GetColor`/`IsOccupied`/`Cols` (all existing).
- Produces: `static class SuperpowerEffectResolver` with `ResolveBomb(GridModel
  grid, (int Row, int Col) landingCell, int radius)`, `ResolveRowClear(GridModel
  grid, int row)`, `ResolveRainbow(GridModel grid, (int Row, int Col)
  landingCell)` — all returning `HashSet<(int Row, int Col)>`. Task 8
  (`SuperpowerEffectController`) calls these three directly.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/SuperpowerEffectResolverTests.cs`:
```csharp
using System.Collections.Generic;
using Game.Grid;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class SuperpowerEffectResolverTests
    {
        [Test]
        public void ResolveBomb_RadiusOne_IncludesOccupiedNeighborExcludesFarCellAndCenter()
        {
            var grid = new GridModel(rows: 8, cols: 8);
            var center = (Row: 3, Col: 3);
            var neighbor = grid.GetNeighbors(center.Row, center.Col)[0];
            grid.PlaceBubble(neighbor.Row, neighbor.Col, BubbleColor.Red);
            grid.PlaceBubble(0, 0, BubbleColor.Green);

            var result = SuperpowerEffectResolver.ResolveBomb(grid, center, 1);

            CollectionAssert.Contains(result, neighbor);
            CollectionAssert.DoesNotContain(result, (0, 0));
            CollectionAssert.DoesNotContain(result, center);
        }

        [Test]
        public void ResolveRowClear_OccupiedRow_ReturnsAllOccupiedCellsInRow()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(3, 0, BubbleColor.Red);
            grid.PlaceBubble(3, 2, BubbleColor.Blue);
            grid.PlaceBubble(1, 0, BubbleColor.Green);

            var result = SuperpowerEffectResolver.ResolveRowClear(grid, 3);

            var expected = new List<(int Row, int Col)> { (3, 0), (3, 2) };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void ResolveRainbow_SingleAdjacentGroup_ReturnsThatGroup()
        {
            var grid = new GridModel(rows: 6, cols: 6);
            var landing = (Row: 2, Col: 2);
            var neighbor = grid.GetNeighbors(landing.Row, landing.Col)[0];
            grid.PlaceBubble(neighbor.Row, neighbor.Col, BubbleColor.Red);

            var result = SuperpowerEffectResolver.ResolveRainbow(grid, landing);

            CollectionAssert.AreEquivalent(new List<(int Row, int Col)> { neighbor }, result);
        }

        [Test]
        public void ResolveRainbow_NoOccupiedNeighbors_ReturnsEmptySet()
        {
            var grid = new GridModel(rows: 6, cols: 6);

            var result = SuperpowerEffectResolver.ResolveRainbow(grid, (2, 2));

            CollectionAssert.IsEmpty(result);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL/compile error — `SuperpowerEffectResolver` doesn't exist yet.

- [ ] **Step 3: Implement `SuperpowerEffectResolver`**

`Assets/Scripts/Superpowers/SuperpowerEffectResolver.cs`:
```csharp
using System.Collections.Generic;
using Game.Grid;

namespace Game.Superpowers
{
    public static class SuperpowerEffectResolver
    {
        public static HashSet<(int Row, int Col)> ResolveBomb(GridModel grid, (int Row, int Col) landingCell, int radius)
        {
            var inRange = HexRadius.CellsWithinRadius(grid, landingCell, radius);
            var occupied = new HashSet<(int Row, int Col)>();
            foreach (var cell in inRange)
                if (cell != landingCell && grid.IsOccupied(cell.Row, cell.Col))
                    occupied.Add(cell);
            return occupied;
        }

        public static HashSet<(int Row, int Col)> ResolveRowClear(GridModel grid, int row)
        {
            var occupied = new HashSet<(int Row, int Col)>();
            for (var col = 0; col < grid.Cols; col++)
                if (grid.IsOccupied(row, col))
                    occupied.Add((row, col));
            return occupied;
        }

        public static HashSet<(int Row, int Col)> ResolveRainbow(GridModel grid, (int Row, int Col) landingCell)
        {
            var best = new HashSet<(int Row, int Col)>();
            foreach (var neighbor in OccupiedNeighbors(grid, landingCell))
            {
                var color = grid.GetColor(neighbor.Row, neighbor.Col);
                var group = FloodFill.Run(grid, new[] { neighbor }, c => grid.IsOccupied(c.Row, c.Col) && grid.GetColor(c.Row, c.Col) == color);
                if (group.Count > best.Count) best = group;
            }
            return best;
        }

        private static IEnumerable<(int Row, int Col)> OccupiedNeighbors(GridModel grid, (int Row, int Col) cell)
        {
            foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                if (grid.IsOccupied(neighbor.Row, neighbor.Col))
                    yield return neighbor;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: PASS, all 4 tests green.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerEffectResolver.cs Assets/Tests/EditMode/SuperpowerEffectResolverTests.cs
```
Do not commit.

---

## Task 6: `ShotTimer.Pause`/`Resume` + `GameStateManager.Freeze`

**Files:**
- Modify: `Assets/Scripts/Gameplay/ShotTimer.cs`
- Modify: `Assets/Scripts/Gameplay/GameStateManager.cs`
- Test: `Assets/Tests/EditMode/ShotTimerTests.cs` (extend existing file if
  present; create it with this name if it doesn't already exist)

**Interfaces:**
- Produces: `ShotTimer.Pause()`, `ShotTimer.Resume()`, `ShotTimer.IsPaused`
  (bool); `GameStateManager.Freeze(float duration)` (public). Task 9
  (`SuperpowerController`) calls `gameStateManager.Freeze(...)` directly
  for the `Freeze` ability.

- [ ] **Step 1: Write the failing tests for `ShotTimer` pause/resume**

Add to `Assets/Tests/EditMode/ShotTimerTests.cs` (create it with this
namespace/style if it doesn't exist yet):
```csharp
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class ShotTimerTests
    {
        [Test]
        public void Tick_WhilePaused_DoesNotDecrementTimeRemaining()
        {
            var timer = new ShotTimer(10f);
            timer.Pause();

            timer.Tick(5f);

            Assert.AreEqual(10f, timer.TimeRemaining);
        }

        [Test]
        public void Tick_AfterResume_DecrementsAgain()
        {
            var timer = new ShotTimer(10f);
            timer.Pause();
            timer.Tick(5f);
            timer.Resume();

            timer.Tick(3f);

            Assert.AreEqual(7f, timer.TimeRemaining);
        }
    }
}
```
If `ShotTimerTests.cs` already exists with other tests in it, add these two
`[Test]` methods to the existing class instead of creating a new file.

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL/compile error — `ShotTimer` has no `Pause`/`Resume` yet.

- [ ] **Step 3: Add `Pause`/`Resume`/`IsPaused` to `ShotTimer`**

Read `Assets/Scripts/Gameplay/ShotTimer.cs` first. It currently has
`Duration`, `TimeRemaining`, a constructor, `Tick(float deltaTime)`, and
`Reset()`. Add an `IsPaused` property next to `TimeRemaining`, guard `Tick`
with it, and add the two toggle methods:
```csharp
public bool IsPaused { get; private set; }
```
```csharp
public bool Tick(float deltaTime)
{
    if (IsPaused) return false;
    TimeRemaining -= deltaTime;
    return TimeRemaining <= 0f;
}
```
```csharp
public void Pause() => IsPaused = true;
public void Resume() => IsPaused = false;
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: PASS, including all pre-existing `ShotTimer` tests still green.

- [ ] **Step 5: Add `Freeze` to `GameStateManager`**

Read `Assets/Scripts/Gameplay/GameStateManager.cs` first. Add a new private
field next to the existing `_isGameOver` field:
```csharp
private float _freezeTimeRemaining;
```
At the top of `Update()`, immediately after the existing
`if (_isGameOver) return;` line, add:
```csharp
if (TickFreeze()) return;
```
Add a new private method near the other private tick helpers
(`TickCeiling`, etc.):
```csharp
private bool TickFreeze()
{
    if (_freezeTimeRemaining <= 0f) return false;
    _freezeTimeRemaining -= Time.deltaTime;
    if (_freezeTimeRemaining <= 0f) Unfreeze();
    return true;
}

private void Unfreeze()
{
    _shotTimer.Resume();
    _ceilingTimer.Resume();
}
```
Add a new public method grouped with the existing `RetryLevel()`/
`AdvanceToNextLevel()` public methods near the end of the file:
```csharp
public void Freeze(float duration)
{
    _shotTimer.Pause();
    _ceilingTimer.Pause();
    _freezeTimeRemaining = duration;
}
```

- [ ] **Step 6: Confirm it compiles**

Refresh the Unity Editor (`mcp__UnityMCP__refresh_unity`) and check
`mcp__UnityMCP__read_console` for compile errors. Expected: no errors.

- [ ] **Step 7: Stage the changes**

```bash
git add Assets/Scripts/Gameplay/ShotTimer.cs Assets/Scripts/Gameplay/GameStateManager.cs Assets/Tests/EditMode/ShotTimerTests.cs
```
Do not commit.

---

## Task 7: `FiredBubbleController` armed-ability extension

**Files:**
- Modify: `Assets/Scripts/Shooter/FiredBubbleController.cs`

**Interfaces:**
- Consumes: `SuperpowerId` (Task 1).
- Produces: `public event Action<SuperpowerId, (int Row, int Col)>
  OnSuperpowerLanded;`, `public void ArmSuperpower(SuperpowerId ability)`.
  Task 8 (`SuperpowerEffectController`) subscribes to `OnSuperpowerLanded`;
  Task 9 (`SuperpowerController`) calls `ArmSuperpower`.

No new automated test for this task — `FiredBubbleController` has no
existing EditMode tests (it's pure `MonoBehaviour` wiring around an already
system-tested trajectory/landing path), so this is verified in Task 11's
Play Mode pass, consistent with the project's convention.

- [ ] **Step 1: Read the current file**

Read `Assets/Scripts/Shooter/FiredBubbleController.cs` in full before
editing — the exact current text of `HandleFireRequested` and `Land` is
needed to place the new branches correctly.

- [ ] **Step 2: Add the new event and fields**

Add `using System;` and `using Game.Superpowers;` to the top of the file if
not already present (needed for `Action<>` and `SuperpowerId`).

Add a new public event, placed after the `[SerializeField]` field block and
before any private fields (matching the `GameStateManager` precedent of
declaring public events right after serialized fields):
```csharp
public event Action<SuperpowerId, (int Row, int Col)> OnSuperpowerLanded;
```

Add two new private fields alongside the existing `_color`/`_nextColor`
fields:
```csharp
private SuperpowerId? _armedAbility;
private SuperpowerId? _firedAbility;
```

- [ ] **Step 3: Add the public `ArmSuperpower` method**

Add this public method directly below the new event declaration:
```csharp
public void ArmSuperpower(SuperpowerId ability) => _armedAbility = ability;
```

- [ ] **Step 4: Snapshot the armed ability when a shot is fired**

In `HandleFireRequested(Vector2 origin, float angleDegrees)`, find the
existing line `_color = _nextColor;` and add immediately after it:
```csharp
_firedAbility = _armedAbility;
_armedAbility = null;
```

- [ ] **Step 5: Branch on the fired ability when the bubble lands**

In `Land((int Row, int Col)? landingCell)`, add this branch as the very
first lines of the method body, before the existing landing-cell handling:
```csharp
if (_firedAbility.HasValue && landingCell.HasValue)
{
    OnSuperpowerLanded?.Invoke(_firedAbility.Value, landingCell.Value);
    _firedAbility = null;
    PrepareNextBubble();
    return;
}
```
Leave the rest of the existing method (the normal-color `PlaceBubble` path)
unchanged below this new branch.

- [ ] **Step 6: Confirm it compiles**

Refresh the Unity Editor and check `mcp__UnityMCP__read_console`. Expected:
no errors.

- [ ] **Step 7: Stage the changes**

```bash
git add Assets/Scripts/Shooter/FiredBubbleController.cs
```
Do not commit.

---

## Task 8: `SuperpowerEffectController`

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerEffectController.cs`

**Interfaces:**
- Consumes: `FiredBubbleController.OnSuperpowerLanded` (Task 7);
  `SuperpowerEffectResolver.ResolveBomb`/`ResolveRowClear`/`ResolveRainbow`
  (Task 5); `GameBoard.Grid`, `GameBoard.PopCells(IReadOnlyCollection<(int,int)>, BubbleColor)`
  (existing, unchanged).
- Produces: a `MonoBehaviour` with `[SerializeField] GameBoard gameBoard`,
  `[SerializeField] FiredBubbleController firedBubbleController`,
  `[SerializeField] int bombRadius`. No task after this one depends on it
  directly — it's wired up in Task 11.

- [ ] **Step 1: Implement `SuperpowerEffectController`**

`Assets/Scripts/Superpowers/SuperpowerEffectController.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Superpowers
{
    public class SuperpowerEffectController : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private FiredBubbleController firedBubbleController;
        [SerializeField] private int bombRadius = 2;

        private void Start()
        {
            firedBubbleController.OnSuperpowerLanded += HandleSuperpowerLanded;
        }

        private void OnDestroy()
        {
            firedBubbleController.OnSuperpowerLanded -= HandleSuperpowerLanded;
        }

        private void HandleSuperpowerLanded(SuperpowerId id, (int Row, int Col) cell)
        {
            PopByColorGroup(ResolveAffectedCells(id, cell));
        }

        private HashSet<(int Row, int Col)> ResolveAffectedCells(SuperpowerId id, (int Row, int Col) cell)
        {
            return id switch
            {
                SuperpowerId.Bomb => SuperpowerEffectResolver.ResolveBomb(gameBoard.Grid, cell, bombRadius),
                SuperpowerId.RowClear => SuperpowerEffectResolver.ResolveRowClear(gameBoard.Grid, cell.Row),
                SuperpowerId.Rainbow => SuperpowerEffectResolver.ResolveRainbow(gameBoard.Grid, cell),
                _ => new HashSet<(int Row, int Col)>()
            };
        }

        private void PopByColorGroup(HashSet<(int Row, int Col)> cells)
        {
            foreach (var group in cells.GroupBy(c => gameBoard.Grid.GetColor(c.Row, c.Col)))
                gameBoard.PopCells(group.ToList(), group.Key);
        }
    }
}
```

- [ ] **Step 2: Confirm it compiles**

Refresh the Unity Editor and check `mcp__UnityMCP__read_console`. Expected:
no errors.

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerEffectController.cs
```
Do not commit.

---

## Task 9: `SuperpowerController`

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerController.cs`

**Interfaces:**
- Consumes: `SuperpowerChargeTracker` (Task 1), `SuperpowerCatalog` (Task
  2), `SuperpowerProgress` (Task 3), `GameStateManager.Freeze` (Task 6),
  `FiredBubbleController.ArmSuperpower` (Task 7), `GameBoard.OnLevelLoaded`
  (existing, `Action<int>`), `GameBoard.LevelNumber` (existing, `int`),
  `GameStateManager.OnLevelWon` (existing, `Action`).
- Produces: `public IEnumerable<SuperpowerId> UnlockedAbilities`, `public
  int RemainingCharges(SuperpowerId id)`, `public bool
  TryActivate(SuperpowerId id)`. Task 10 (`SuperpowerHud`) calls all three.

- [ ] **Step 1: Implement `SuperpowerController`**

`Assets/Scripts/Superpowers/SuperpowerController.cs`:
```csharp
using System.Collections.Generic;
using Game.Gameplay;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Superpowers
{
    public class SuperpowerController : MonoBehaviour
    {
        [SerializeField] private SuperpowerCatalog catalog;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private FiredBubbleController firedBubbleController;
        [SerializeField] private float freezeDurationSeconds = 5f;

        public IEnumerable<SuperpowerId> UnlockedAbilities => _tracker.UnlockedIds;

        private readonly SuperpowerChargeTracker _tracker = new();

        private void Start()
        {
            gameBoard.OnLevelLoaded += HandleLevelLoaded;
            gameStateManager.OnLevelWon += HandleLevelWon;
            HandleLevelLoaded(gameBoard.LevelNumber);
        }

        private void OnDestroy()
        {
            gameBoard.OnLevelLoaded -= HandleLevelLoaded;
            gameStateManager.OnLevelWon -= HandleLevelWon;
        }

        public int RemainingCharges(SuperpowerId id) => _tracker.Remaining(id);

        public bool TryActivate(SuperpowerId id)
        {
            if (!_tracker.TryConsume(id)) return false;
            if (id == SuperpowerId.Freeze) gameStateManager.Freeze(freezeDurationSeconds);
            else firedBubbleController.ArmSuperpower(id);
            return true;
        }

        private void HandleLevelLoaded(int levelNumber)
        {
            _tracker.ResetForLevel(catalog.Definitions, SuperpowerProgress.HighestLevelReached);
        }

        private void HandleLevelWon()
        {
            if (gameBoard.LevelNumber > SuperpowerProgress.HighestLevelReached)
                SuperpowerProgress.HighestLevelReached = gameBoard.LevelNumber;
        }
    }
}
```

- [ ] **Step 2: Confirm it compiles**

Refresh the Unity Editor and check `mcp__UnityMCP__read_console`. Expected:
no errors.

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerController.cs
```
Do not commit.

---

## Task 10: `SuperpowerHud`

**Files:**
- Create: `Assets/Scripts/Superpowers/SuperpowerHud.cs`

**Interfaces:**
- Consumes: `SuperpowerController.UnlockedAbilities`/`RemainingCharges`/
  `TryActivate` (Task 9).
- Produces: a `MonoBehaviour` with `[SerializeField] SuperpowerController
  superpowerController`, `[SerializeField] RectTransform anchorRect`,
  `[SerializeField] RectTransform fireZoneRect`.

- [ ] **Step 1: Implement `SuperpowerHud`**

`Assets/Scripts/Superpowers/SuperpowerHud.cs`, following the runtime
"build UI in code, no prefab" pattern from `Assets/Scripts/Gameplay/HudDisplay.cs`
and the `Button` pattern from `Assets/Scripts/Gameplay/LevelResultScreen.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Superpowers
{
    public class SuperpowerHud : MonoBehaviour
    {
        [SerializeField] private SuperpowerController superpowerController;
        [SerializeField] private RectTransform anchorRect;
        [SerializeField] private RectTransform fireZoneRect;

        private const float ButtonSize = 96f;
        private const float ButtonSpacing = 12f;

        private readonly Dictionary<SuperpowerId, Button> _buttons = new();
        private readonly Dictionary<SuperpowerId, Text> _chargeLabels = new();

        private void Start()
        {
            foreach (var ability in superpowerController.UnlockedAbilities)
                SpawnButton(ability);
        }

        private void Update()
        {
            foreach (var entry in _buttons)
                RefreshButton(entry.Key, entry.Value);
        }

        private void SpawnButton(SuperpowerId ability)
        {
            var buttonObj = new GameObject($"SuperpowerButton_{ability}", typeof(RectTransform));
            ConfigureButtonRect((RectTransform)buttonObj.transform, _buttons.Count);
            buttonObj.AddComponent<Image>().color = Color.white;
            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => superpowerController.TryActivate(ability));
            _buttons[ability] = button;
            _chargeLabels[ability] = SpawnChargeLabel(buttonObj.transform);
        }

        private void ConfigureButtonRect(RectTransform rect, int index)
        {
            rect.SetParent(anchorRect.parent, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = new Vector2(index * (ButtonSize + ButtonSpacing), TopOffset());
        }

        private Text SpawnChargeLabel(Transform parent)
        {
            var obj = new GameObject("ChargeLabel", typeof(RectTransform));
            obj.transform.SetParent(parent, worldPositionStays: false);
            var text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            return text;
        }

        private void RefreshButton(SuperpowerId id, Button button)
        {
            var remaining = superpowerController.RemainingCharges(id);
            button.interactable = remaining > 0;
            _chargeLabels[id].text = remaining.ToString();
        }

        private float TopOffset()
        {
            return fireZoneRect.anchoredPosition.y + fireZoneRect.sizeDelta.y + ButtonSpacing;
        }
    }
}
```

- [ ] **Step 2: Confirm it compiles**

Refresh the Unity Editor and check `mcp__UnityMCP__read_console`. Expected:
no errors.

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Superpowers/SuperpowerHud.cs
```
Do not commit.

---

## Task 11: Scene wiring + Play Mode verification

**Files:**
- Modify: the gameplay scene (add/wire `SuperpowerController`,
  `SuperpowerEffectController`, `SuperpowerHud` components) — via Unity
  Editor/MCP, not a text edit.

- [ ] **Step 1: Add and wire the three new components in the gameplay scene**

Via `mcp__UnityMCP__manage_gameobject`/`manage_components`:
- Add a `SuperpowerController` component (to the same GameObject as
  `GameStateManager`, or a new dedicated `Superpowers` GameObject); assign
  `catalog` = `DefaultSuperpowerCatalog.asset` (Task 2), and the
  `gameBoard`/`gameStateManager`/`firedBubbleController` scene references.
- Add a `SuperpowerEffectController` component; assign `gameBoard`/
  `firedBubbleController`.
- Add a `SuperpowerHud` component; assign `superpowerController` and the
  same `anchorRect`/`fireZoneRect` references `HudDisplay`/
  `ShotTimerDisplay` already use in the scene.

- [ ] **Step 2: Verify Freeze via Play Mode**

Using `mcp__UnityMCP__execute_code` while in Play mode: set
`SuperpowerProgress.HighestLevelReached = 3` before the level loads (or
call `gameBoard.LoadLevel(3)`), confirm the Freeze button appears in
`SuperpowerHud` with 1 charge, click it (or call
`superpowerController.TryActivate(SuperpowerId.Freeze)` directly), and
confirm both `GameStateManager`'s shot timer and ceiling timer stop
advancing for the freeze duration, then resume. Take a screenshot of the
HUD showing the charge count drop to 0.

- [ ] **Step 3: Verify Bomb, Row Clear, and Rainbow via Play Mode**

For each ability: set `SuperpowerProgress.HighestLevelReached` to its
unlock level, activate it, aim and fire a shot, and confirm via
`gameBoard.Grid.OccupiedCells()` (diffed before/after, same technique used
to verify the Milestone 7 ceiling-descent fix) that the expected cells were
cleared — a radius around the landing point for Bomb, the whole landing row
for Row Clear, the matched neighbor group for Rainbow. Screenshot each
result.

- [ ] **Step 4: Run the full EditMode suite one more time**

Run `.\run-edittests.ps1` (Editor closed) or the Test Runner (Editor open).
Expected: every test green, including all pre-existing Phase 1 tests — this
confirms nothing in Tasks 6-7's edits to `ShotTimer`/`GameStateManager`/
`FiredBubbleController` broke existing behavior.

- [ ] **Step 5: Stage the scene changes**

```bash
git add -u
git status
```
Review the output, confirm only the expected scene/asset files changed,
and stop. Do not commit — leave everything staged for the user to review
and commit.

## Verification (end-to-end)

- All EditMode tests (Tasks 1, 3, 4, 5, 6, plus the full pre-existing
  suite) pass via `.\run-edittests.ps1` or the Test Runner.
- Play Mode, via Unity MCP: all four abilities are locked at level 1,
  unlock at their configured level, show correct charge counts in
  `SuperpowerHud`, and each produces its documented board effect —
  confirmed by diffing `GridModel.OccupiedCells()` and by screenshot.
- `git status` shows only the files this plan touched, all staged, none
  committed.
