# Curated Levels 1-6 + Level-Transition Animation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement curated bubble-pattern generation for levels 1-5 (hex
blobs / stripe patterns / mixes), an `AnimationCurve`-based difficulty
progression, and a staggered build-in animation for the level-transition
board swap.

**Architecture:** New pure, static pattern-placement classes
(`HexBlobPlacer`, `StripePlacer`, `PatternLevelGenerator`) alongside the
existing `LevelGenerator`, routed by level number from `GameBoard`. A new
`PatternLevelCatalog` `ScriptableObject` holds the per-level recipes and
shared tunables. `DifficultyCurveConfig`'s four ramp knobs become
`AnimationCurve`s. `GridDebugRenderer` gets a second, animated rebuild path
for level loads, using a new self-contained `BubbleSpawnAnimator`
component per bubble (same `Update()`-driven elapsed-time style as the
existing `FallingBubble`/`FiredBubbleController` settle motion).

**Tech Stack:** Unity 6000.5.1f1, C#, Unity Test Framework (NUnit) EditMode
tests, Unity MCP bridge for scene/asset work and Play-mode verification.

**Spec:** `docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md`

## Global Constraints

- Files stay under ~200 lines; function bodies under ~7 lines where
  practical; group >3 parameters into a small type (see
  `PatternPlacementContext` below) — per
  `docs/architecture/overview.md`'s "Code style" section and the
  `general-code-style` plugin.
- No tweening library — hand-rolled `Update()`-driven elapsed-time easing,
  matching `BubbleSettleMotion`/`FallingBubble`/`FiredBubbleController`.
- Determinism: every generator is seeded by `levelNumber`
  (`new System.Random(levelNumber)`), matching `LevelGenerator`'s existing
  convention.
- Pure logic classes (`HexBlobPlacer`, `StripePlacer`,
  `PatternLevelGenerator`) take plain data (`DifficultyConfig`,
  `LevelPatternPlan`, `PatternGenerationSettings`), never a `ScriptableObject`
  directly — matches `LevelGenerator`/`SuperpowerChargeTracker`'s existing
  precedent, and keeps this logic EditMode-testable without a scene.
- **Do not run `git commit` at any point in this plan.** Stage changes
  (`git add`) at the end of each task and stop — committing is the user's
  own step in this project (standing preference, not a plan omission).
- Run EditMode tests via **Window > General > Test Runner** or the
  `run_tests`/`get_test_job` Unity MCP tools if the Editor is already open
  (it is, per this project's Unity MCP bridge); otherwise
  `.\run-edittests.ps1` from the project root. See
  `docs/architecture/overview.md`'s "Testing" section — do not run both at
  once.

---

## Task 1: Pattern data types + HexBlobPlacer

**Files:**
- Create: `Assets/Scripts/Grid/PatternType.cs`
- Create: `Assets/Scripts/Grid/LevelPatternPlan.cs`
- Create: `Assets/Scripts/Grid/PatternGenerationSettings.cs`
- Create: `Assets/Scripts/Grid/PatternPlacementContext.cs`
- Create: `Assets/Scripts/Grid/HexBlobPlacer.cs`
- Test: `Assets/Tests/EditMode/HexBlobPlacerTests.cs`

**Interfaces:**
- Produces: `PatternType { HexBlob, VerticalStripe, HorizontalStripe }`;
  `LevelPatternPlan { int LevelNumber; PatternType[] Regions; }`;
  `PatternGenerationSettings { int HexBlobRadius; int HexBlobGapCells; int StripeWidth; int RegionGapRows; }`
  (all plain get/set properties);
  `PatternPlacementContext { GridModel Grid; PatternGenerationSettings Settings; int ColorCount; System.Random Rng; }`;
  `HexBlobPlacer.Place(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)`.
- Consumes: `Game.Superpowers.HexRadius.CellsWithinRadius(GridModel, (int Row, int Col), int)`
  (`Assets/Scripts/Superpowers/HexRadius.cs`), `GridModel.PlaceBubble`/`IsOccupied`/`GetNeighbors`/`Cols`
  (`Assets/Scripts/Grid/GridModel.cs`), `BubbleColorPalette.AllColors`
  (`Assets/Scripts/Grid/BubbleColorPalette.cs`).

- [ ] **Step 1: Write the data types**

`Assets/Scripts/Grid/PatternType.cs`:
```csharp
namespace Game.Grid
{
    public enum PatternType
    {
        HexBlob,
        VerticalStripe,
        HorizontalStripe,
    }
}
```

`Assets/Scripts/Grid/LevelPatternPlan.cs`:
```csharp
using System;

namespace Game.Grid
{
    [Serializable]
    public sealed class LevelPatternPlan
    {
        public int LevelNumber;
        public PatternType[] Regions;
    }
}
```

`Assets/Scripts/Grid/PatternGenerationSettings.cs`:
```csharp
namespace Game.Grid
{
    public sealed class PatternGenerationSettings
    {
        public int HexBlobRadius { get; set; }
        public int HexBlobGapCells { get; set; }
        public int StripeWidth { get; set; }
        public int RegionGapRows { get; set; }
    }
}
```

`Assets/Scripts/Grid/PatternPlacementContext.cs`:
```csharp
namespace Game.Grid
{
    public sealed class PatternPlacementContext
    {
        public GridModel Grid { get; set; }
        public PatternGenerationSettings Settings { get; set; }
        public int ColorCount { get; set; }
        public System.Random Rng { get; set; }
    }
}
```

- [ ] **Step 2: Write the failing test for HexBlobPlacer**

`Assets/Tests/EditMode/HexBlobPlacerTests.cs`:
```csharp
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class HexBlobPlacerTests
    {
        [Test]
        public void Place_OnlyFillsCellsWithinRowBand()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 4, seed: 1);

            HexBlobPlacer.Place(context, (2, 7));

            foreach (var cell in grid.OccupiedCells())
                Assert.That(cell.Row, Is.InRange(2, 6));
        }

        [Test]
        public void Place_NeverPutsDifferentlyColoredCellsAdjacent()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 6, seed: 2);

            HexBlobPlacer.Place(context, (0, 10));

            foreach (var cell in grid.OccupiedCells())
                foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                    if (grid.IsOccupied(neighbor.Row, neighbor.Col))
                        Assert.That(grid.GetColor(neighbor.Row, neighbor.Col), Is.EqualTo(grid.GetColor(cell.Row, cell.Col)));
        }

        [Test]
        public void Place_OnEmptyFullGridBand_ProducesAtLeastOneBlob()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 3, seed: 3);

            HexBlobPlacer.Place(context, (0, 10));

            Assert.That(grid.IsEmpty, Is.False);
        }

        [Test]
        public void Place_SameSeedTwice_ProducesIdenticalGrids()
        {
            var gridA = new GridModel(rows: 10, cols: 8);
            var gridB = new GridModel(rows: 10, cols: 8);

            HexBlobPlacer.Place(NewContext(gridA, colorCount: 4, seed: 5), (0, 10));
            HexBlobPlacer.Place(NewContext(gridB, colorCount: 4, seed: 5), (0, 10));

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        private static PatternPlacementContext NewContext(GridModel grid, int colorCount, int seed) => new PatternPlacementContext
        {
            Grid = grid,
            Settings = new PatternGenerationSettings { HexBlobRadius = 2, HexBlobGapCells = 1, StripeWidth = 3, RegionGapRows = 1 },
            ColorCount = colorCount,
            Rng = new System.Random(seed),
        };
    }
}
```

Note on `Place_OnEmptyFullGridBand_ProducesAtLeastOneBlob`: the band equals
the full grid, so on an empty grid the very first shuffled candidate center
can never be rejected (no occupied/reserved cells yet, and `HexRadius`
already clips to grid bounds) — this assertion holds for any RNG seed, not
just `seed: 3`.

- [ ] **Step 3: Run the test to verify it fails**

Run via Test Runner (or `run_tests` MCP tool). Expected: compile error /
FAIL — `HexBlobPlacer` doesn't exist yet.

- [ ] **Step 4: Implement HexBlobPlacer**

`Assets/Scripts/Grid/HexBlobPlacer.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using Game.Superpowers;

namespace Game.Grid
{
    /// <summary>
    /// Fills a row band with separated, single-color hexagonal blobs. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class HexBlobPlacer
    {
        public static void Place(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            var reserved = new HashSet<(int Row, int Col)>();
            foreach (var center in ShuffledCenters(context, rowBand))
                if (IsValidCenter(context, rowBand, reserved, center))
                    PlaceBlob(context, reserved, center);
        }

        private static List<(int Row, int Col)> ShuffledCenters(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            var cells = new List<(int Row, int Col)>();
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    cells.Add((row, col));
            return Shuffle(cells, context.Rng);
        }

        private static List<T> Shuffle<T>(List<T> items, System.Random rng)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
            return items;
        }

        private static bool IsValidCenter(PatternPlacementContext context, (int Start, int EndExclusive) rowBand, HashSet<(int Row, int Col)> reserved, (int Row, int Col) center)
        {
            if (reserved.Contains(center)) return false;
            var blobCells = HexRadius.CellsWithinRadius(context.Grid, center, context.Settings.HexBlobRadius);
            return blobCells.All(cell => cell.Row >= rowBand.Start && cell.Row < rowBand.EndExclusive)
                && blobCells.All(cell => !context.Grid.IsOccupied(cell.Row, cell.Col))
                && blobCells.All(cell => !reserved.Contains(cell));
        }

        private static void PlaceBlob(PatternPlacementContext context, HashSet<(int Row, int Col)> reserved, (int Row, int Col) center)
        {
            var color = BubbleColorPalette.AllColors[context.Rng.Next(context.ColorCount)];
            foreach (var cell in HexRadius.CellsWithinRadius(context.Grid, center, context.Settings.HexBlobRadius))
                context.Grid.PlaceBubble(cell.Row, cell.Col, color);

            var reservedZone = HexRadius.CellsWithinRadius(context.Grid, center, context.Settings.HexBlobRadius + context.Settings.HexBlobGapCells);
            reserved.UnionWith(reservedZone);
        }
    }
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Expected: all 4 `HexBlobPlacerTests` PASS, and the full existing EditMode
suite still passes (no regressions from the new files).

- [ ] **Step 6: Stage the changes**

```bash
git add Assets/Scripts/Grid/PatternType.cs Assets/Scripts/Grid/LevelPatternPlan.cs Assets/Scripts/Grid/PatternGenerationSettings.cs Assets/Scripts/Grid/PatternPlacementContext.cs Assets/Scripts/Grid/HexBlobPlacer.cs Assets/Tests/EditMode/HexBlobPlacerTests.cs
```
Do not commit — leave staged for the user.

---

## Task 2: StripePlacer

**Files:**
- Create: `Assets/Scripts/Grid/StripePlacer.cs`
- Test: `Assets/Tests/EditMode/StripePlacerTests.cs`

**Interfaces:**
- Consumes: `PatternPlacementContext`, `GridModel.PlaceBubble`/`IsOccupied`/`Cols`
  (Task 1 / `Assets/Scripts/Grid/GridModel.cs`), `BubbleColorPalette.AllColors`.
- Produces: `StripePlacer.PlaceVertical(PatternPlacementContext, (int Start, int EndExclusive))`,
  `StripePlacer.PlaceHorizontal(PatternPlacementContext, (int Start, int EndExclusive))`.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/StripePlacerTests.cs`:
```csharp
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class StripePlacerTests
    {
        [Test]
        public void PlaceVertical_CyclesColorsEveryStripeWidthColumns()
        {
            var grid = new GridModel(rows: 3, cols: 9);

            StripePlacer.PlaceVertical(NewContext(grid, colorCount: 3), (0, 3));

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.GetColor(0, col), Is.EqualTo(BubbleColorPalette.AllColors[(col / 3) % 3]));
        }

        [Test]
        public void PlaceHorizontal_CyclesColorsEveryStripeWidthRows()
        {
            var grid = new GridModel(rows: 9, cols: 3);

            StripePlacer.PlaceHorizontal(NewContext(grid, colorCount: 3), (0, 9));

            for (var row = 0; row < grid.Rows; row++)
                Assert.That(grid.GetColor(row, 0), Is.EqualTo(BubbleColorPalette.AllColors[(row / 3) % 3]));
        }

        [Test]
        public void PlaceVertical_FillsOnlyCellsWithinBand()
        {
            var grid = new GridModel(rows: 5, cols: 6);

            StripePlacer.PlaceVertical(NewContext(grid, colorCount: 2), (1, 4));

            for (var row = 1; row < 4; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True);

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.IsOccupied(0, col), Is.False);
        }

        private static PatternPlacementContext NewContext(GridModel grid, int colorCount) => new PatternPlacementContext
        {
            Grid = grid,
            Settings = new PatternGenerationSettings { HexBlobRadius = 2, HexBlobGapCells = 1, StripeWidth = 3, RegionGapRows = 1 },
            ColorCount = colorCount,
            Rng = new System.Random(1),
        };
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Expected: compile error — `StripePlacer` doesn't exist yet.

- [ ] **Step 3: Implement StripePlacer**

`Assets/Scripts/Grid/StripePlacer.cs`:
```csharp
namespace Game.Grid
{
    /// <summary>
    /// Fills a row band solid with repeating same-width color stripes along
    /// the given axis. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class StripePlacer
    {
        public static void PlaceVertical(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    context.Grid.PlaceBubble(row, col, ColorForIndex(context, col));
        }

        public static void PlaceHorizontal(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    context.Grid.PlaceBubble(row, col, ColorForIndex(context, row - rowBand.Start));
        }

        private static BubbleColor ColorForIndex(PatternPlacementContext context, int index)
        {
            var stripeIndex = index / context.Settings.StripeWidth;
            return BubbleColorPalette.AllColors[stripeIndex % context.ColorCount];
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Expected: all 3 `StripePlacerTests` PASS.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Grid/StripePlacer.cs Assets/Tests/EditMode/StripePlacerTests.cs
```

---

## Task 3: PatternLevelGenerator

**Files:**
- Create: `Assets/Scripts/Grid/PatternLevelGenerator.cs`
- Test: `Assets/Tests/EditMode/PatternLevelGeneratorTests.cs`

**Interfaces:**
- Consumes: `HexBlobPlacer.Place`, `StripePlacer.PlaceVertical`/`PlaceHorizontal` (Tasks 1-2);
  `MatchResolver.FindFloatingCells(GridModel)` (`Assets/Scripts/Grid/MatchResolver.cs`);
  `DifficultyConfig { ColorCount, HeadroomRows }` (`Assets/Scripts/Grid/DifficultyConfig.cs`, unchanged).
- Produces: `PatternLevelGenerator.Generate(GridModel grid, LevelPatternPlan plan, DifficultyConfig difficulty, PatternGenerationSettings settings) -> GridModel`.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/PatternLevelGeneratorTests.cs`:
```csharp
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class PatternLevelGeneratorTests
    {
        [Test]
        public void Generate_HexBlobPlan_NeverLeavesFloatingCells()
        {
            var grid = new GridModel(rows: 12, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 1, Regions = new[] { PatternType.HexBlob } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 3 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            CollectionAssert.IsEmpty(MatchResolver.FindFloatingCells(grid));
        }

        [Test]
        public void Generate_MixedPlan_SplitsIntoOneBandPerRegionWithGapBetween()
        {
            var grid = new GridModel(rows: 18, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 4, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 0 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.IsOccupied(8, col), Is.False, "gap row between bands should stay empty");

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.IsOccupied(16, col), Is.True, "stripe band's last row should be solid");
        }

        [Test]
        public void Generate_HeadroomRows_LeavesBottomRowsEmpty()
        {
            var grid = new GridModel(rows: 12, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 3, Regions = new[] { PatternType.HorizontalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 3, HeadroomRows = 4 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            for (var row = 0; row < 8; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True, $"cell ({row},{col}) should be occupied");

            for (var row = 8; row < 12; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.False, $"cell ({row},{col}) should be empty");
        }

        [Test]
        public void Generate_SameLevelNumberTwice_ProducesIdenticalGrids()
        {
            var plan = new LevelPatternPlan { LevelNumber = 5, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe, PatternType.HorizontalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 5, HeadroomRows = 2 };
            var gridA = new GridModel(rows: 18, cols: 8);
            var gridB = new GridModel(rows: 18, cols: 8);

            PatternLevelGenerator.Generate(gridA, plan, difficulty, Settings());
            PatternLevelGenerator.Generate(gridB, plan, difficulty, Settings());

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        [Test]
        public void Generate_HeadroomExceedsPlayfieldRows_ClampsToEmptyGridWithoutThrowing()
        {
            var grid = new GridModel(rows: 6, cols: 6);
            var plan = new LevelPatternPlan { LevelNumber = 2, Regions = new[] { PatternType.VerticalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 3, HeadroomRows = 40 };

            Assert.DoesNotThrow(() => PatternLevelGenerator.Generate(grid, plan, difficulty, Settings()));
            Assert.That(grid.IsEmpty, Is.True);
        }

        private static PatternGenerationSettings Settings() => new PatternGenerationSettings
        {
            HexBlobRadius = 2,
            HexBlobGapCells = 1,
            StripeWidth = 3,
            RegionGapRows = 1,
        };
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Expected: compile error — `PatternLevelGenerator` doesn't exist yet.

- [ ] **Step 3: Implement PatternLevelGenerator**

`Assets/Scripts/Grid/PatternLevelGenerator.cs`:
```csharp
using System.Collections.Generic;

namespace Game.Grid
{
    /// <summary>
    /// Produces a populated GridModel from a curated LevelPatternPlan
    /// (levels 1-5). See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class PatternLevelGenerator
    {
        public static GridModel Generate(GridModel grid, LevelPatternPlan plan, DifficultyConfig difficulty, PatternGenerationSettings settings)
        {
            var context = new PatternPlacementContext
            {
                Grid = grid,
                Settings = settings,
                ColorCount = difficulty.ColorCount,
                Rng = new System.Random(plan.LevelNumber),
            };

            foreach (var band in Bands(grid, plan, difficulty, settings))
                PlaceRegion(context, band.RowRange, band.Type);

            ClearFloatingCells(grid);
            return grid;
        }

        private static void PlaceRegion(PatternPlacementContext context, (int Start, int EndExclusive) rowRange, PatternType type)
        {
            switch (type)
            {
                case PatternType.HexBlob: HexBlobPlacer.Place(context, rowRange); break;
                case PatternType.VerticalStripe: StripePlacer.PlaceVertical(context, rowRange); break;
                case PatternType.HorizontalStripe: StripePlacer.PlaceHorizontal(context, rowRange); break;
            }
        }

        private static IEnumerable<(PatternType Type, (int Start, int EndExclusive) RowRange)> Bands(GridModel grid, LevelPatternPlan plan, DifficultyConfig difficulty, PatternGenerationSettings settings)
        {
            var fillableRows = grid.Rows - difficulty.HeadroomRows;
            var regionCount = plan.Regions.Length;
            var totalGapRows = settings.RegionGapRows * (regionCount - 1);
            var bandHeight = (fillableRows - totalGapRows) / regionCount;
            var row = 0;
            foreach (var region in plan.Regions)
            {
                yield return (region, (row, row + bandHeight));
                row += bandHeight + settings.RegionGapRows;
            }
        }

        private static void ClearFloatingCells(GridModel grid)
        {
            foreach (var cell in MatchResolver.FindFloatingCells(grid))
                grid.ClearCell(cell.Row, cell.Col);
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Expected: all 5 `PatternLevelGeneratorTests` PASS.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Grid/PatternLevelGenerator.cs Assets/Tests/EditMode/PatternLevelGeneratorTests.cs
```

---

## Task 4: PatternLevelCatalog ScriptableObject

**Files:**
- Create: `Assets/Scripts/Grid/PatternLevelCatalog.cs`

**Interfaces:**
- Consumes: `LevelPatternPlan`, `PatternGenerationSettings` (Task 1).
- Produces: `PatternLevelCatalog.TryGetPlan(int levelNumber, out LevelPatternPlan plan) -> bool`,
  `PatternLevelCatalog.Settings -> PatternGenerationSettings`.

No dedicated EditMode test for this class — it's `ScriptableObject` glue
around already-tested logic, matching the existing `SuperpowerCatalog`
precedent (also untested directly; its data is exercised through the
plain classes that consume it). Verified in Task 9/10 via the actual
asset + Play Mode.

- [ ] **Step 1: Implement PatternLevelCatalog**

`Assets/Scripts/Grid/PatternLevelCatalog.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Per-level curated pattern recipes (levels 1-5) plus the shared
    /// pattern-generation tunables. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "PatternLevelCatalog", menuName = "Game/Pattern Level Catalog")]
    public sealed class PatternLevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelPatternPlan> plans = new();
        [SerializeField, Min(1)] private int hexBlobRadius = 2;
        [SerializeField, Min(0)] private int hexBlobGapCells = 1;
        [SerializeField, Min(1)] private int stripeWidth = 3;
        [SerializeField, Min(0)] private int regionGapRows = 1;

        public PatternGenerationSettings Settings => new PatternGenerationSettings
        {
            HexBlobRadius = hexBlobRadius,
            HexBlobGapCells = hexBlobGapCells,
            StripeWidth = stripeWidth,
            RegionGapRows = regionGapRows,
        };

        public bool TryGetPlan(int levelNumber, out LevelPatternPlan plan)
        {
            plan = plans.FirstOrDefault(p => p.LevelNumber == levelNumber);
            return plan != null;
        }
    }
}
```

- [ ] **Step 2: Run the full EditMode suite to confirm no regressions/compile errors**

Expected: every existing test still PASSes (this task adds no new tests,
only a new compiling type).

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Grid/PatternLevelCatalog.cs
```

---

## Task 5: DifficultyCurveConfig — AnimationCurve refactor

**Files:**
- Modify: `Assets/Scripts/Grid/DifficultyCurveConfig.cs`
- Modify: `Assets/Tests/EditMode/DifficultyCurveConfigTests.cs`

**Interfaces:**
- Produces (unchanged shape): `DifficultyCurveConfig.ForLevel(int levelNumber) -> DifficultyConfig`
  (`DifficultyConfig` itself, `Assets/Scripts/Grid/DifficultyConfig.cs`, is untouched).
- This replaces the existing `level1Density`/`level1HeadroomRows` override
  fields entirely — folded into each curve's first keyframe. Every other
  caller of `ForLevel` (`GameBoard.LoadLevel`) is unaffected since the
  return type and meaning don't change.

- [ ] **Step 1: Rewrite the test file for the new curve-based defaults**

`Assets/Tests/EditMode/DifficultyCurveConfigTests.cs` (replaces entire
existing content):
```csharp
using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DifficultyCurveConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ForLevel_Level1_ReturnsFirstKeyframeValues()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1);

            Assert.That(config.ColorCount, Is.EqualTo(3));
            Assert.That(config.Density, Is.EqualTo(0.35f).Within(Tolerance));
            Assert.That(config.HeadroomRows, Is.EqualTo(9));
            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_Level2_MatchesDensityCurveKeyframe()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(2);

            Assert.That(config.Density, Is.EqualTo(0.55f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_Level6_MatchesHigherDensityKeyframeForNonPatternFallback()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(6);

            Assert.That(config.Density, Is.EqualTo(0.8f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_LastColorCountKeyframe_ReachesMaxColorCount()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(30);

            Assert.That(config.ColorCount, Is.EqualTo(6));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMaxColorCount()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.ColorCount, Is.EqualTo(6));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMinHeadroomRows()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.HeadroomRows, Is.EqualTo(3));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMinCeilingInterval()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(8f).Within(Tolerance));
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Expected: FAIL against the current start/max/rate implementation (e.g.
`ForLevel_Level2_MatchesDensityCurveKeyframe` expects `0.55`, current code
produces `0.57`).

- [ ] **Step 3: Rewrite DifficultyCurveConfig**

`Assets/Scripts/Grid/DifficultyCurveConfig.cs` (replaces entire existing
content):
```csharp
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Per-level difficulty curve, resolved via ForLevel. Each knob is an
    /// Inspector-editable AnimationCurve keyed by level number — deliberately
    /// rough placeholder keyframes, see
    /// docs/features/core-gameplay/level-generation.md and
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyCurveConfig", menuName = "Game/Difficulty Curve Config")]
    public sealed class DifficultyCurveConfig : ScriptableObject
    {
        [SerializeField] private AnimationCurve colorCountCurve = AnimationCurve.Linear(1, 3, 30, 6);
        [SerializeField] private AnimationCurve densityCurve = DefaultDensityCurve();
        [SerializeField] private AnimationCurve headroomRowsCurve = AnimationCurve.Linear(1, 9, 10, 3);
        [SerializeField] private AnimationCurve ceilingIntervalCurve = AnimationCurve.Linear(1, 20, 20, 8);

        public DifficultyConfig ForLevel(int levelNumber) => new DifficultyConfig
        {
            ColorCount = Mathf.RoundToInt(colorCountCurve.Evaluate(levelNumber)),
            Density = densityCurve.Evaluate(levelNumber),
            HeadroomRows = Mathf.RoundToInt(headroomRowsCurve.Evaluate(levelNumber)),
            CeilingDropIntervalSeconds = ceilingIntervalCurve.Evaluate(levelNumber),
        };

        private static AnimationCurve DefaultDensityCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(1, 0.35f);
            curve.AddKey(2, 0.55f);
            curve.AddKey(6, 0.8f);
            curve.AddKey(15, 0.85f);
            return curve;
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Expected: all 7 `DifficultyCurveConfigTests` PASS, and `LevelGeneratorTests`
(which construct `DifficultyConfig` directly, not through the curve) still
PASS unchanged.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Grid/DifficultyCurveConfig.cs Assets/Tests/EditMode/DifficultyCurveConfigTests.cs
```

Note: the existing `Assets/ScriptableObjects/DefaultDifficultyCurve.asset`
has serialized values for the old `startColorCount`/`level1Density`/etc.
fields. Unity silently drops serialized data for fields that no longer
exist and falls back to each new field's C# default (the `AnimationCurve`
field initializers above) — no manual `.asset` file editing is needed. The
user can then hand-tune the curve shape in the Inspector afterward, per
the spec.

---

## Task 6: BubbleSpawnMotion + BubbleSpawnAnimator

**Files:**
- Create: `Assets/Scripts/Grid/BubbleSpawnMotion.cs`
- Create: `Assets/Scripts/Grid/BubbleSpawnAnimator.cs`
- Test: `Assets/Tests/EditMode/BubbleSpawnMotionTests.cs`

**Interfaces:**
- Produces: `BubbleSpawnMotion.ScaleForProgress(float t) -> float`;
  `BubbleSpawnAnimator.Configure(float delaySeconds)` (MonoBehaviour, added
  via `gameObject.AddComponent<BubbleSpawnAnimator>()`).
- Follows `FallingBubble`'s (`Assets/Scripts/Grid/FallingBubble.cs`)
  self-contained per-GameObject `Update()`-driven pattern, and
  `BubbleSettleMotion`'s (`Assets/Scripts/Shooter/BubbleSettleMotion.cs`)
  pure-easing-function shape.

- [ ] **Step 1: Write the failing tests**

`Assets/Tests/EditMode/BubbleSpawnMotionTests.cs`:
```csharp
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class BubbleSpawnMotionTests
    {
        [Test]
        public void ScaleForProgress_AtStart_IsZero()
        {
            Assert.That(BubbleSpawnMotion.ScaleForProgress(0f), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ScaleForProgress_AtEnd_IsOne()
        {
            Assert.That(BubbleSpawnMotion.ScaleForProgress(1f), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ScaleForProgress_IsMonotonicallyIncreasing()
        {
            var previous = BubbleSpawnMotion.ScaleForProgress(0f);
            for (var i = 1; i <= 10; i++)
            {
                var current = BubbleSpawnMotion.ScaleForProgress(i / 10f);
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                previous = current;
            }
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Expected: compile error — `BubbleSpawnMotion` doesn't exist yet.

- [ ] **Step 3: Implement BubbleSpawnMotion and BubbleSpawnAnimator**

`Assets/Scripts/Grid/BubbleSpawnMotion.cs`:
```csharp
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Pure scale-in easing for the level-load build-in animation
    /// (ease-out-cubic). See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class BubbleSpawnMotion
    {
        public static float ScaleForProgress(float t)
        {
            var m = 1f - Mathf.Clamp01(t);
            return 1f - m * m * m;
        }
    }
}
```

`Assets/Scripts/Grid/BubbleSpawnAnimator.cs`:
```csharp
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Attached to a freshly spawned bubble on level load so it scales in
    /// instead of appearing instantly, delayed by its row so the board
    /// fills in top-to-bottom. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public class BubbleSpawnAnimator : MonoBehaviour
    {
        private const float ScaleInDurationSeconds = 0.25f;

        private float _delayRemaining;
        private float _elapsed;

        public void Configure(float delaySeconds)
        {
            _delayRemaining = delaySeconds;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (_delayRemaining > 0f) { _delayRemaining -= Time.deltaTime; return; }
            AdvanceScaleIn();
        }

        private void AdvanceScaleIn()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / ScaleInDurationSeconds);
            var scale = BubbleSpawnMotion.ScaleForProgress(t);
            transform.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f) Destroy(this);
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Expected: all 3 `BubbleSpawnMotionTests` PASS. `BubbleSpawnAnimator` itself
is a `MonoBehaviour` and isn't unit tested directly (same as
`FallingBubble`) — verified in Task 8/10.

- [ ] **Step 5: Stage the changes**

```bash
git add Assets/Scripts/Grid/BubbleSpawnMotion.cs Assets/Scripts/Grid/BubbleSpawnAnimator.cs Assets/Tests/EditMode/BubbleSpawnMotionTests.cs
```

---

## Task 7: Wire GameBoard to route between PatternLevelGenerator and LevelGenerator

**Files:**
- Modify: `Assets/Scripts/Grid/GameBoard.cs`

**Interfaces:**
- Consumes: `PatternLevelCatalog.TryGetPlan`/`Settings` (Task 4),
  `PatternLevelGenerator.Generate` (Task 3), existing
  `LevelGenerator.Generate` (`Assets/Scripts/Grid/LevelGenerator.cs`,
  unmodified).
- `GameBoard`'s public surface (`LoadLevel`, events, `CurrentDifficulty`,
  etc.) is otherwise unchanged — only a new `[SerializeField] patternCatalog`
  field is added and `LoadLevel`'s grid-construction line is extracted into
  a small private helper.

Not unit-testable directly (`GameBoard` depends on `Camera.main` and scene
context, consistent with there being no existing `GameBoardTests.cs`) —
verified via Play Mode in Task 10.

- [ ] **Step 1: Add the patternCatalog field and GenerateGrid helper**

In `Assets/Scripts/Grid/GameBoard.cs`, add the field next to the existing
`difficultyCurve` field:
```csharp
[SerializeField] private DifficultyCurveConfig difficultyCurve;
[SerializeField] private PatternLevelCatalog patternCatalog;
```

Replace the body of `LoadLevel`:
```csharp
public void LoadLevel(int newLevelNumber)
{
    levelNumber = newLevelNumber;
    CurrentDifficulty = difficultyCurve.ForLevel(levelNumber);
    Grid = GenerateGrid();
    RecomputeBounds();
    OnLevelLoaded?.Invoke(levelNumber);
}

private GridModel GenerateGrid()
{
    var grid = new GridModel(_rows, cols, cellWidth);
    return patternCatalog != null && patternCatalog.TryGetPlan(levelNumber, out var plan)
        ? PatternLevelGenerator.Generate(grid, plan, CurrentDifficulty, patternCatalog.Settings)
        : LevelGenerator.Generate(grid, levelNumber, CurrentDifficulty);
}
```

- [ ] **Step 2: Run the full EditMode suite to confirm no regressions/compile errors**

Expected: every existing test still PASSes.

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Grid/GameBoard.cs
```

---

## Task 8: Split GridDebugRenderer's rebuild into instant vs. animated paths

**Files:**
- Modify: `Assets/Scripts/Grid/GridDebugRenderer.cs`

**Interfaces:**
- Consumes: `BubbleSpawnAnimator.Configure(float)` (Task 6).
- `OnRowPushedDown` keeps the instant rebuild; `OnLevelLoaded` gets the new
  animated one. No public API change (`GridDebugRenderer` has none besides
  the `gameBoard` field it already reads).

Not unit-testable directly (same reasoning as `GameBoard`) — verified via
Play Mode in Task 10.

- [ ] **Step 1: Split RebuildAll and add the animated path**

In `Assets/Scripts/Grid/GridDebugRenderer.cs`, replace:
```csharp
private void OnRowPushedDown(bool wasLastRowOccupied)
{
    RebuildAll();
}

private void OnLevelLoaded(int levelNumber)
{
    RebuildAll();
}

private void RebuildAll()
{
    foreach (var bubble in _bubbles.Values)
        Destroy(bubble);
    _bubbles.Clear();

    foreach (var cell in gameBoard.Grid.OccupiedCells())
        SpawnBubble(cell);
}

private void SpawnBubble((int Row, int Col) cell)
{
    var bubble = new GameObject($"Bubble_{cell.Row}_{cell.Col}");
    bubble.transform.SetParent(gameBoard.transform);
    bubble.transform.localPosition = gameBoard.Grid.GetWorldPosition(cell.Row, cell.Col);
    var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = _sprite;
    spriteRenderer.color = BubbleColorPalette.ToColor(gameBoard.Grid.GetColor(cell.Row, cell.Col));
    _bubbles[cell] = bubble;
}
```

with:
```csharp
private const float PerRowSpawnDelaySeconds = 0.05f;

private void OnRowPushedDown(bool wasLastRowOccupied)
{
    RebuildAllInstant();
}

private void OnLevelLoaded(int levelNumber)
{
    RebuildAllAnimated();
}

private void RebuildAllInstant()
{
    ClearAllBubbles();
    foreach (var cell in gameBoard.Grid.OccupiedCells())
        SpawnBubble(cell);
}

private void RebuildAllAnimated()
{
    ClearAllBubbles();
    foreach (var cell in gameBoard.Grid.OccupiedCells())
        SpawnAnimatedBubble(cell);
}

private void ClearAllBubbles()
{
    foreach (var bubble in _bubbles.Values)
        Destroy(bubble);
    _bubbles.Clear();
}

private void SpawnAnimatedBubble((int Row, int Col) cell)
{
    var bubble = SpawnBubble(cell);
    var animator = bubble.AddComponent<BubbleSpawnAnimator>();
    animator.Configure(cell.Row * PerRowSpawnDelaySeconds);
}

private GameObject SpawnBubble((int Row, int Col) cell)
{
    var bubble = new GameObject($"Bubble_{cell.Row}_{cell.Col}");
    bubble.transform.SetParent(gameBoard.transform);
    bubble.transform.localPosition = gameBoard.Grid.GetWorldPosition(cell.Row, cell.Col);
    var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = _sprite;
    spriteRenderer.color = BubbleColorPalette.ToColor(gameBoard.Grid.GetColor(cell.Row, cell.Col));
    _bubbles[cell] = bubble;
    return bubble;
}
```

Note: `SpawnBubble`'s return type changes from `void` to `GameObject`; its
only other caller, `OnBubblePlaced`, already ignores the return value
(`SpawnBubble((row, col));`), so no other change is needed there.

- [ ] **Step 2: Run the full EditMode suite to confirm no regressions/compile errors**

Expected: every existing test still PASSes.

- [ ] **Step 3: Stage the changes**

```bash
git add Assets/Scripts/Grid/GridDebugRenderer.cs
```

---

## Task 9: Create the DefaultPatternLevelCatalog asset and wire it into the scene

**Files:**
- Create (Unity asset, not hand-written): `Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset`
- Modify (Unity scene, not hand-written): `Assets/Scenes/SampleScene.unity`

This is Editor/asset work, done through the Unity MCP bridge's
`execute_code` tool (see `docs/architecture/overview.md`'s "Tooling"
section) rather than hand-editing YAML.

- [ ] **Step 1: Create and populate the catalog asset**

Run via the Unity MCP `execute_code` tool (Editor must be open on this
project):
```csharp
using UnityEditor;
using UnityEngine;
using Game.Grid;

var catalog = ScriptableObject.CreateInstance<PatternLevelCatalog>();
var so = new SerializedObject(catalog);
var plansProp = so.FindProperty("plans");
plansProp.arraySize = 5;

void SetPlan(int index, int levelNumber, PatternType[] regions)
{
    var element = plansProp.GetArrayElementAtIndex(index);
    element.FindPropertyRelative("LevelNumber").intValue = levelNumber;
    var regionsProp = element.FindPropertyRelative("Regions");
    regionsProp.arraySize = regions.Length;
    for (var i = 0; i < regions.Length; i++)
        regionsProp.GetArrayElementAtIndex(i).enumValueIndex = (int)regions[i];
}

SetPlan(0, 1, new[] { PatternType.HexBlob });
SetPlan(1, 2, new[] { PatternType.VerticalStripe });
SetPlan(2, 3, new[] { PatternType.HorizontalStripe });
SetPlan(3, 4, new[] { PatternType.HexBlob, PatternType.VerticalStripe });
SetPlan(4, 5, new[] { PatternType.HexBlob, PatternType.VerticalStripe, PatternType.HorizontalStripe });

so.ApplyModifiedProperties();
AssetDatabase.CreateAsset(catalog, "Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset");
AssetDatabase.SaveAssets();
```

- [ ] **Step 2: Wire the asset into GameBoard in SampleScene**

Run via `execute_code` (with `SampleScene.unity` open, the same scene that
already has `GameBoard.difficultyCurve` wired to `DefaultDifficultyCurve.asset`):
```csharp
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Grid;

var gameBoard = Object.FindObjectOfType<GameBoard>();
var so = new SerializedObject(gameBoard);
var catalog = AssetDatabase.LoadAssetAtPath<PatternLevelCatalog>("Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset");
so.FindProperty("patternCatalog").objectReferenceValue = catalog;
so.ApplyModifiedProperties();
EditorSceneManager.MarkSceneDirty(gameBoard.gameObject.scene);
EditorSceneManager.SaveScene(gameBoard.gameObject.scene);
```

- [ ] **Step 3: Verify via read_console**

Use the Unity MCP `read_console` tool and confirm no errors/exceptions
were logged by either snippet.

- [ ] **Step 4: Stage the new/modified asset files**

```bash
git add Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset.meta Assets/Scenes/SampleScene.unity
```

---

## Task 10: Play-Mode end-to-end verification

**Files:** none (verification only).

- [ ] **Step 1: Enter Play mode via Unity MCP**

Use `manage_editor` to enter Play mode on `SampleScene`.

- [ ] **Step 2: Screenshot level 1 and confirm the hex-blob pattern**

Use the Unity MCP screenshot capability. Confirm: several separated,
single-color hexagonal clusters, each visibly a different color, filling
roughly 3/4 of the board height, with a staggered top-to-bottom pop-in
having just played on scene start (`OnLevelLoaded` fires from
`GameBoard.Awake`).

- [ ] **Step 3: Drive through levels 2-6 and screenshot each**

Use `execute_code` to call `GameStateManager.AdvanceToNextLevel()` (or the
equivalent reachable via the win-screen `Continue` button in a real
playthrough) for each level, screenshotting after each transition.
Confirm: level 2 solid 3-column vertical stripes; level 3 solid 3-row
horizontal stripes; level 4 two horizontal bands (hex blobs above,
vertical stripes below); level 5 three horizontal bands (hex, vertical,
horizontal); level 6 plain random fill at a visibly higher density than
levels 1-5 (no separated blobs/stripes) — confirming the no-catalog-entry
fallback to `LevelGenerator` works.

- [ ] **Step 4: Confirm the build-in animation plays on each transition**

Watch (via sequential screenshots a few frames apart right after a
transition) that bubbles scale in row-by-row rather than appearing
instantly, and that firing/popping bubbles mid-level (not a level
transition) still behaves as before — unaffected, since only
`OnLevelLoaded`'s path changed.

- [ ] **Step 5: Check the console for errors**

Use `read_console`. Confirm no exceptions, especially none related to the
pattern levels' intentionally pre-formed large same-color groups (there
should be none — nothing auto-pops on load, per the spec's "pre-formed
match groups" decision).

- [ ] **Step 6: Exit Play mode**

Use `manage_editor` to exit Play mode without saving any Play-mode-only
scene changes.

This task produces no file changes to stage — if step 2-5 reveal a bug,
fix it in the relevant task's file(s) above, re-run that task's EditMode
tests, and repeat this task.

---

## Self-Review Notes

- **Spec coverage:** every decision in the spec (level scope, recipes,
  hex sizing, stripe width, region layout, pre-pop exception, transition
  split) maps to a task above — Tasks 1-3 (patterns), Task 5 (curve),
  Tasks 6/8 (transition), Task 9 (asset/wiring), Task 10 (verification).
  Level 6 needed no dedicated task per the spec's "no new code" decision —
  it's exercised only in Task 10's verification.
- **Type consistency:** `PatternPlacementContext`/`PatternGenerationSettings`/
  `LevelPatternPlan`/`PatternType` signatures are identical across Tasks
  1-3, 7-9 (same property names throughout).
- **Placeholder scan:** no TBD/TODO; every step has real code or a
  concrete MCP-tool snippet.
- Deviations from the original spec wording, both consistent with
  existing codebase precedent: (1) dropped the standalone
  `LevelContentGenerator` routing class in favor of a small private
  `GameBoard.GenerateGrid()` helper, since the routing is a single
  ternary and the project keeps `ScriptableObject` glue inside the
  consuming `MonoBehaviour` (see `SuperpowerController` vs.
  `SuperpowerCatalog`/`SuperpowerChargeTracker`). (2) `PatternLevelGenerator`
  takes a plain `PatternGenerationSettings`, not `PatternLevelCatalog`
  directly, matching `LevelGenerator`/`SuperpowerChargeTracker`'s existing
  "pure logic takes plain data" convention — keeps it EditMode-testable
  without a scene.
