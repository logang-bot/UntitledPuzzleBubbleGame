# Battle Mode (Simple Attack Variant) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the first, simplest local split-screen battle mode variant — two players on one device, matches push rows onto the opponent's board instead of an autonomous ceiling timer.

**Architecture:** Two full instances of the existing Phase 1 stack (`GridModel`/`GameBoard`/`ShooterController`/`FiredBubbleController`/`MatchProcessor`) run side by side in one new scene, split top/bottom via `Camera.rect`, Player 2's camera and input zones rotated 180° for face-to-face tabletop play. A small new `Game.Battle` layer (attack economy, per-side outcome tracking, match settlement) wires the two boards together purely through existing public events — no changes to core gameplay logic, one small additive change to `GameBoard` to support a per-instance camera.

**Tech Stack:** Unity 6000.5.1f1, C#, Unity Test Framework (NUnit, EditMode tests), Unity MCP bridge for scene assembly and Play Mode verification.

**Spec:** `docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md`

## Global Constraints

- File length ≤ ~200 lines (imports excluded); function bodies ≤ ~7 lines (+3 spare); ≤ 3 parameters per method/constructor (group extras into a type). Enforced by the `general-code-style` Claude Code plugin — see `docs/architecture/overview.md`'s "Code style".
- No inline comments explaining *what* code does — only non-obvious *why*, matching every existing file in this codebase.
- New code lives under `Assets/Scripts/Battle/` (namespace `Game.Battle`), new tests flat under `Assets/Tests/EditMode/` (matching the existing convention — see `Assets/Tests/EditMode/SuperpowerChargeTrackerTests.cs` for a same-shape precedent).
- Superpowers are **disabled** for this variant — do not wire `SuperpowerController`/`SuperpowerHud` into the battle scene.
- Reuse `Assets/ScriptableObjects/DefaultDifficultyCurve.asset` and `Assets/ScriptableObjects/DefaultPatternLevelCatalog.asset` as-is for both battle boards — no new content-generation config needed (see Task 7's note).
- **No commit steps.** This project's convention is that the user reviews and commits changes themselves — do not run `git commit` at any point in this plan (per the project's own standing instruction). Stage changes are left uncommitted after each task for the user to review.
- Unity MCP bridge tools (`manage_scene`, `manage_gameobject`, `manage_components`, `execute_code`, `run_tests`) are how this project builds and verifies scenes — see `docs/architecture/overview.md`'s "Tooling" section. If the bridge isn't connected when a task runs, fall back to the Unity Editor UI directly for the same steps.

---

## Task 1: `GameBoard` — support a per-instance camera and partial-screen viewport

**Why this is needed (not in the spec, found while reading the real source):** `GameBoard.Awake()` currently does `_camera = Camera.main;` unconditionally, and `FitCameraAndComputeRows` sizes the camera against the full `Screen.height`. With two simultaneous `GameBoard`s (one per player), only one GameObject can practically be `Camera.main`, and each camera only owns half the screen's pixel height (via `Camera.rect`), not the full height. This is a small, purely additive change — defaults preserve today's exact behavior, so solo mode (`SampleScene`) is unaffected.

**Files:**
- Modify: `Assets/Scripts/Grid/GameBoard.cs:15-19` (fields), `:40-47` (`Awake`), `:108-114` (`FitCameraAndComputeRows`)

**Interfaces:**
- Produces: `GameBoard` gains two new optional Inspector fields — `targetCamera` (Camera, defaults to null → falls back to `Camera.main`) and `viewportHeightFraction` (float 0-1, defaults to 1 → identical math to today). No public API changes.

- [ ] **Step 1: Add the two new fields**

In `Assets/Scripts/Grid/GameBoard.cs`, after line 18 (`[SerializeField] private int levelNumber = 1;`):

```csharp
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0f, 1f)] private float viewportHeightFraction = 1f;
```

- [ ] **Step 2: Use `targetCamera` in `Awake`**

Change line 42 from:

```csharp
            _camera = Camera.main;
```

to:

```csharp
            _camera = targetCamera != null ? targetCamera : Camera.main;
```

- [ ] **Step 3: Scale the fit height by the viewport fraction**

Change line 110 (inside `FitCameraAndComputeRows`) from:

```csharp
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height);
```

to:

```csharp
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height * viewportHeightFraction);
```

- [ ] **Step 4: Regression-check the existing EditMode suite**

Run: `.\run-edittests.ps1` (Editor closed) or Window > General > Test Runner (Editor open) — see `docs/architecture/overview.md`'s "Testing" section for which to use.
Expected: All existing tests still pass unchanged — this change touches no pure-logic class, only `GameBoard.Awake`'s camera selection.

- [ ] **Step 5: Play Mode regression check in `SampleScene`**

Via Unity MCP (`manage_scene` to open `SampleScene`, enter Play mode, take a screenshot) or the Editor directly: confirm the board still fills the full screen exactly as before (default `targetCamera` is unset, `viewportHeightFraction` is 1, so behavior must be pixel-identical to before this change).
Expected: No visual change from solo mode's current behavior.

---

## Task 2: `PendingRowsMeter` — accumulates attack value into whole rows

**Files:**
- Create: `Assets/Scripts/Battle/PendingRowsMeter.cs`
- Test: `Assets/Tests/EditMode/PendingRowsMeterTests.cs`

**Interfaces:**
- Produces: `PendingRowsMeter` — `void Add(float amount)`, `int ConsumeWholeRows()` (returns and subtracts the whole-number part of the accumulated total, keeping the fractional remainder).

- [ ] **Step 1: Write the failing tests**

```csharp
using Game.Battle;
using NUnit.Framework;

namespace Game.Tests
{
    public class PendingRowsMeterTests
    {
        [Test]
        public void ConsumeWholeRows_BelowOne_ReturnsZero()
        {
            var meter = new PendingRowsMeter();
            meter.Add(0.6f);

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(0, rows);
        }

        [Test]
        public void ConsumeWholeRows_AtLeastOne_ReturnsWholeCountAndKeepsRemainder()
        {
            var meter = new PendingRowsMeter();
            meter.Add(2.3f);

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(2, rows);
        }

        [Test]
        public void ConsumeWholeRows_AfterConsuming_RemainderCarriesForward()
        {
            var meter = new PendingRowsMeter();
            meter.Add(1.5f);
            meter.ConsumeWholeRows();

            meter.Add(0.6f);
            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(1, rows);
        }

        [Test]
        public void ConsumeWholeRows_CalledTwiceWithoutAdding_SecondCallReturnsZero()
        {
            var meter = new PendingRowsMeter();
            meter.Add(3f);
            meter.ConsumeWholeRows();

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(0, rows);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Test Runner / `run-edittests.ps1`, filtered to `PendingRowsMeterTests`.
Expected: FAIL — `PendingRowsMeter` does not exist yet.

- [ ] **Step 3: Write the implementation**

```csharp
namespace Game.Battle
{
    /// <summary>
    /// Accumulates fractional attack value and releases only whole rows,
    /// since GridModel.PushRowsDown only moves whole rows — the remainder
    /// carries forward rather than being lost. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class PendingRowsMeter
    {
        private float _accumulated;

        public void Add(float amount) => _accumulated += amount;

        public int ConsumeWholeRows()
        {
            var wholeRows = (int)_accumulated;
            _accumulated -= wholeRows;
            return wholeRows;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Test Runner / `run-edittests.ps1`, filtered to `PendingRowsMeterTests`.
Expected: PASS, all 4 tests.

---

## Task 3: `BattleAttackConfig` — pop/drop count to attack value

**Files:**
- Create: `Assets/Scripts/Battle/BattleAttackConfig.cs`
- Test: `Assets/Tests/EditMode/BattleAttackConfigTests.cs`

**Interfaces:**
- Produces: `BattleAttackConfig` (ScriptableObject) — `float AttackForPop(int bubbleCount)`, `float AttackForDrop(int bubbleCount)`.

- [ ] **Step 1: Write the failing tests**

```csharp
using Game.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class BattleAttackConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void AttackForPop_MinimumMatchSize_ReturnsQuadraticValue()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var attack = config.AttackForPop(bubbleCount: 3);

            Assert.That(attack, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void AttackForPop_LargerMatch_ScalesFasterThanLinear()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var smallMatch = config.AttackForPop(bubbleCount: 3);
            var largeMatch = config.AttackForPop(bubbleCount: 6);

            Assert.That(largeMatch, Is.GreaterThan(smallMatch * 2f));
        }

        [Test]
        public void AttackForDrop_FiveBubbles_ReturnsLinearValue()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var attack = config.AttackForDrop(bubbleCount: 5);

            Assert.That(attack, Is.EqualTo(0.75f).Within(Tolerance));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: Test Runner / `run-edittests.ps1`, filtered to `BattleAttackConfigTests`.
Expected: FAIL — `BattleAttackConfig` does not exist yet.

- [ ] **Step 3: Write the implementation**

```csharp
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Match size / cascade-drop count to attack value, deliberately
    /// decoupled from ScoreCalculator's solo-mode formula so the two can be
    /// tuned independently. Pop scales quadratically (rewards bigger
    /// matches disproportionately, mirroring ScoreCalculator's own pop
    /// formula shape); drops scale linearly. Untuned placeholder constants,
    /// same caveat as DifficultyCurveConfig's keyframes — see
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleAttackConfig", menuName = "Game/Battle Attack Config")]
    public sealed class BattleAttackConfig : ScriptableObject
    {
        [SerializeField] private float popAttackPerBubble = 0.05f;
        [SerializeField] private float dropAttackPerBubble = 0.15f;

        public float AttackForPop(int bubbleCount) => popAttackPerBubble * bubbleCount * (bubbleCount - 1);
        public float AttackForDrop(int bubbleCount) => dropAttackPerBubble * bubbleCount;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Test Runner / `run-edittests.ps1`, filtered to `BattleAttackConfigTests`.
Expected: PASS, all 3 tests.

- [ ] **Step 5: Create the default asset**

Via Unity MCP `manage_asset` (or Editor: right-click `Assets/ScriptableObjects/` > Create > Game > Battle Attack Config): create `Assets/ScriptableObjects/DefaultBattleAttackConfig.asset` with default values, matching the existing `DefaultDifficultyCurve.asset`/`DefaultSuperpowerCatalog.asset` convention.

---

## Task 4: `BattleShotClock` — per-side shot timer, no ceiling timer

**Files:**
- Create: `Assets/Scripts/Battle/BattleShotClock.cs`

**Interfaces:**
- Consumes: `Game.Shooter.ShooterController` — `event Action<Vector2, float> OnFireRequested`, `void Fire()`.
- Produces: `BattleShotClock` (MonoBehaviour) — no public API beyond the `enabled` flag (used by `BattleMatchController` to stop it at match end).

No EditMode test — this is a thin thirteen-line wrapper around the already-tested `Game.Gameplay.ShotTimer`; verified via Play Mode in Task 10.

- [ ] **Step 1: Write the implementation**

```csharp
using Game.Gameplay;
using Game.Shooter;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Per-side shot timer only — battle mode has no ceiling-descent timer
    /// (see GameStateManager, whose ceiling/freeze machinery this
    /// deliberately does not reuse). Auto-fires at the current aim on
    /// expiry, same 12s default as solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleShotClock : MonoBehaviour
    {
        [SerializeField] private ShooterController shooterController;
        [SerializeField] private float shotTimeSeconds = 12f;

        private ShotTimer _shotTimer;

        private void Awake() => _shotTimer = new ShotTimer(shotTimeSeconds);

        private void Start() => shooterController.OnFireRequested += HandleFireRequested;

        private void OnDestroy() => shooterController.OnFireRequested -= HandleFireRequested;

        private void Update()
        {
            if (_shotTimer.Tick(Time.deltaTime)) shooterController.Fire();
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees) => _shotTimer.Reset();
    }
}
```

---

## Task 5: `BattleSideOutcome` — per-side win/loss signal

**Files:**
- Create: `Assets/Scripts/Battle/BattleEndReason.cs`
- Create: `Assets/Scripts/Battle/BattleSideOutcome.cs`

**Interfaces:**
- Consumes: `Game.Grid.GameBoard` — `event Action<bool> OnRowPushedDown`, `event Action<IReadOnlyCollection<(int,int)>, BubbleColor> OnBubblesPopped`, `event Action<IReadOnlyCollection<(int,int)>> OnClusterDropped`, `GridModel Grid { get; }` (via `Grid.IsEmpty`).
- Produces: `BattleSideOutcome` (MonoBehaviour) — `event Action<BattleEndReason> OnSideEnded`, `void ResetForRematch()`. Consumed by `BattleMatchController` in Task 7.

No EditMode test — reuses two already-tested signals (`GameBoard.OnRowPushedDown`, `GridModel.IsEmpty`) with only a one-shot guard as new logic; verified via Play Mode in Task 10/11.

- [ ] **Step 1: Write `BattleEndReason`**

```csharp
namespace Game.Battle
{
    public enum BattleEndReason { Cleared, WallReachedLine }
}
```

- [ ] **Step 2: Write `BattleSideOutcome`**

```csharp
using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Reuses solo mode's exact win/loss signals (GameBoard.OnRowPushedDown,
    /// GridModel.IsEmpty — see GameStateManager), scoped to one side and
    /// without the shot/ceiling timer machinery those checks came bundled
    /// with in solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleSideOutcome : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        public event Action<BattleEndReason> OnSideEnded;

        private bool _hasEnded;

        private void Start()
        {
            gameBoard.OnRowPushedDown += HandleRowPushedDown;
            gameBoard.OnBubblesPopped += HandlePopped;
            gameBoard.OnClusterDropped += HandleDropped;
        }

        private void OnDestroy()
        {
            gameBoard.OnRowPushedDown -= HandleRowPushedDown;
            gameBoard.OnBubblesPopped -= HandlePopped;
            gameBoard.OnClusterDropped -= HandleDropped;
        }

        public void ResetForRematch() => _hasEnded = false;

        private void HandleRowPushedDown(bool wasLastRowOccupied)
        {
            if (wasLastRowOccupied) RaiseEnded(BattleEndReason.WallReachedLine);
        }

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color) => CheckCleared();
        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells) => CheckCleared();

        private void CheckCleared()
        {
            if (gameBoard.Grid.IsEmpty) RaiseEnded(BattleEndReason.Cleared);
        }

        private void RaiseEnded(BattleEndReason reason)
        {
            if (_hasEnded) return;
            _hasEnded = true;
            OnSideEnded?.Invoke(reason);
        }
    }
}
```

---

## Task 6: `BattleAttackController` — pops/drops on my board push the opponent's wall

**Files:**
- Create: `Assets/Scripts/Battle/BattleAttackController.cs`

**Interfaces:**
- Consumes: `Game.Grid.GameBoard.OnBubblesPopped`/`OnClusterDropped`/`PushRowDown()`; `BattleAttackConfig.AttackForPop`/`AttackForDrop` (Task 3); `PendingRowsMeter` (Task 2).
- Produces: `BattleAttackController` (MonoBehaviour) — no public API beyond `enabled` (stopped at match end by `BattleMatchController`, Task 7).

No EditMode test — the pure math (`PendingRowsMeter`, `BattleAttackConfig`) is already covered; this class is pure event-wiring glue, verified via Play Mode in Task 10/11.

- [ ] **Step 1: Write the implementation**

```csharp
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Turns my own board's pops/drops into rows pushed onto the opponent's
    /// board — the one new cross-board wiring this feature needs; every
    /// other event stays single-board, same as solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleAttackController : MonoBehaviour
    {
        [SerializeField] private GameBoard ownBoard;
        [SerializeField] private GameBoard opponentBoard;
        [SerializeField] private BattleAttackConfig attackConfig;

        private readonly PendingRowsMeter _meter = new();

        private void Start()
        {
            ownBoard.OnBubblesPopped += HandlePopped;
            ownBoard.OnClusterDropped += HandleDropped;
        }

        private void OnDestroy()
        {
            ownBoard.OnBubblesPopped -= HandlePopped;
            ownBoard.OnClusterDropped -= HandleDropped;
        }

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color) =>
            BankAttack(attackConfig.AttackForPop(cells.Count));

        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells) =>
            BankAttack(attackConfig.AttackForDrop(cells.Count));

        private void BankAttack(float amount)
        {
            _meter.Add(amount);
            var rowsToSend = _meter.ConsumeWholeRows();
            for (var i = 0; i < rowsToSend; i++)
                opponentBoard.PushRowDown();
        }
    }
}
```

---

## Task 7: `BattleMatchController` — settles win/loss/draw and picks shared board content

**Files:**
- Create: `Assets/Scripts/Battle/BattleMatchResult.cs`
- Create: `Assets/Scripts/Battle/BattlePlayerSide.cs`
- Create: `Assets/Scripts/Battle/BattleMatchController.cs`

**Interfaces:**
- Consumes: `BattleSideOutcome.OnSideEnded` (Task 5); `GameBoard.LoadLevel(int)` (existing, reused directly — see note below); `ShooterController`/`BattleShotClock`/`BattleAttackController`'s `enabled` flags.
- Produces: `BattleMatchController` — `event Action<BattleMatchResult> OnMatchEnded`, `void Rematch()`. Consumed by `BattleResultScreen` in Task 8.

**Note on board content (deliberate simplification of the spec's literal wording):** the spec calls for a new `BattleBoardConfig`. Reading the real generators shows this isn't needed — `LevelContentGenerator`/`LevelGenerator`/`PatternLevelGenerator` are already fully deterministic from `levelNumber` alone (`new System.Random(levelNumber)`/`new System.Random(plan.LevelNumber)`). Calling `GameBoard.LoadLevel(n)` with the *same* `n` on both boards already produces byte-identical layouts, reusing the existing `DefaultDifficultyCurve.asset`/`DefaultPatternLevelCatalog.asset` — so "shared seed" and "seed from a curated pattern" both fall out for free by picking one random level number in `[1,5]` per match. No new content-generation code needed.

**Note on execution order:** `GameBoard.Awake()` calls `LoadLevel` once already (with whatever `levelNumber` is left in its Inspector field) before any `Start()` runs. `BattleMatchController` calling `LoadLevel` again in `Start()` (with the real, randomly-picked shared level) must run *after* every other battle component has subscribed to `OnLevelLoaded` in *their* `Start()` — otherwise a listener could miss the real load and show stale content, the same class of bug the Superpowers implementation already hit once with `SuperpowerHud`'s build order (see `docs/ROADMAP.md`'s Phase 2 implementation notes). `[DefaultExecutionOrder(1000)]` guarantees this deterministically rather than relying on Unity's default (undefined) `Start()` ordering.

- [ ] **Step 1: Write `BattleMatchResult`**

```csharp
namespace Game.Battle
{
    public enum BattleMatchResult { Player1Wins, Player2Wins, Draw }
}
```

- [ ] **Step 2: Write `BattlePlayerSide`**

```csharp
using System;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Battle
{
    [Serializable]
    public sealed class BattlePlayerSide
    {
        public GameBoard GameBoard;
        public ShooterController ShooterController;
        public BattleShotClock ShotClock;
        public BattleAttackController AttackController;
        public BattleSideOutcome Outcome;
    }
}
```

- [ ] **Step 3: Write `BattleMatchController`**

```csharp
using System;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Reconciles both sides' outcomes into one authoritative result rather
    /// than letting each side unilaterally declare a winner — needed since,
    /// unlike solo mode, two boards can both end in the same frame (see the
    /// spec's "simultaneous end" rule: a draw). DefaultExecutionOrder
    /// ensures StartMatch's real LoadLevel call runs after every other
    /// battle component has subscribed to OnLevelLoaded. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class BattleMatchController : MonoBehaviour
    {
        [SerializeField] private BattlePlayerSide player1;
        [SerializeField] private BattlePlayerSide player2;
        [SerializeField] private int minLevelNumber = 1;
        [SerializeField] private int maxLevelNumber = 5;

        public event Action<BattleMatchResult> OnMatchEnded;

        private BattleEndReason? _player1EndedThisFrame;
        private BattleEndReason? _player2EndedThisFrame;
        private bool _matchEnded;

        private void Start()
        {
            player1.Outcome.OnSideEnded += HandlePlayer1Ended;
            player2.Outcome.OnSideEnded += HandlePlayer2Ended;
            StartMatch();
        }

        private void OnDestroy()
        {
            player1.Outcome.OnSideEnded -= HandlePlayer1Ended;
            player2.Outcome.OnSideEnded -= HandlePlayer2Ended;
        }

        public void Rematch()
        {
            _matchEnded = false;
            player1.Outcome.ResetForRematch();
            player2.Outcome.ResetForRematch();
            EnableSide(player1);
            EnableSide(player2);
            StartMatch();
        }

        private void LateUpdate()
        {
            if (_matchEnded) return;
            if (_player1EndedThisFrame == null && _player2EndedThisFrame == null) return;
            Settle();
        }

        private void StartMatch()
        {
            _player1EndedThisFrame = null;
            _player2EndedThisFrame = null;
            var levelNumber = UnityEngine.Random.Range(minLevelNumber, maxLevelNumber + 1);
            player1.GameBoard.LoadLevel(levelNumber);
            player2.GameBoard.LoadLevel(levelNumber);
        }

        private void HandlePlayer1Ended(BattleEndReason reason) => _player1EndedThisFrame = reason;
        private void HandlePlayer2Ended(BattleEndReason reason) => _player2EndedThisFrame = reason;

        private void Settle()
        {
            _matchEnded = true;
            DisableSide(player1);
            DisableSide(player2);
            OnMatchEnded?.Invoke(ResolveResult());
        }

        private BattleMatchResult ResolveResult()
        {
            if (_player1EndedThisFrame.HasValue && _player2EndedThisFrame.HasValue) return BattleMatchResult.Draw;
            if (_player1EndedThisFrame.HasValue) return ResultFor(_player1EndedThisFrame.Value, BattleMatchResult.Player1Wins, BattleMatchResult.Player2Wins);
            return ResultFor(_player2EndedThisFrame.Value, BattleMatchResult.Player2Wins, BattleMatchResult.Player1Wins);
        }

        private static BattleMatchResult ResultFor(BattleEndReason reason, BattleMatchResult ifCleared, BattleMatchResult ifWallReached) =>
            reason == BattleEndReason.Cleared ? ifCleared : ifWallReached;

        private static void DisableSide(BattlePlayerSide side)
        {
            side.ShooterController.enabled = false;
            side.ShotClock.enabled = false;
            side.AttackController.enabled = false;
        }

        private static void EnableSide(BattlePlayerSide side)
        {
            side.ShooterController.enabled = true;
            side.ShotClock.enabled = true;
            side.AttackController.enabled = true;
        }
    }
}
```

---

## Task 8: `BattleResultScreen` — win/lose/draw panel with Rematch

**Files:**
- Create: `Assets/Scripts/Battle/BattleResultScreen.cs`

**Interfaces:**
- Consumes: `BattleMatchController.OnMatchEnded`/`Rematch()` (Task 7).
- Produces: none consumed elsewhere — leaf UI component.

No EditMode test — runtime-built UI, same as `LevelResultScreen`'s existing precedent (verified via Play Mode only). Verified in Task 10/11.

- [ ] **Step 1: Write the implementation**

```csharp
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Battle
{
    /// <summary>
    /// Same runtime-built recipe as LevelResultScreen (full-screen dim
    /// panel + message + button), with a third draw state and Rematch
    /// instead of Continue/Retry. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleResultScreen : MonoBehaviour
    {
        [SerializeField] private BattleMatchController matchController;
        [SerializeField] private RectTransform canvasRect;

        private const float ButtonWidth = 200f;
        private const float ButtonHeight = 80f;

        private GameObject _panel;
        private Text _messageText;

        private void Start()
        {
            BuildPanel();
            matchController.OnMatchEnded += HandleMatchEnded;
        }

        private void OnDestroy()
        {
            matchController.OnMatchEnded -= HandleMatchEnded;
        }

        private void HandleMatchEnded(BattleMatchResult result)
        {
            _messageText.text = MessageFor(result);
            _panel.SetActive(true);
        }

        private static string MessageFor(BattleMatchResult result) => result switch
        {
            BattleMatchResult.Player1Wins => "Player 1 Wins!",
            BattleMatchResult.Player2Wins => "Player 2 Wins!",
            _ => "Draw!",
        };

        private void HandleRematchClicked()
        {
            _panel.SetActive(false);
            matchController.Rematch();
        }

        private void HandleMenuClicked()
        {
            _panel.SetActive(false);
            SceneManager.LoadScene("MainMenu");
        }

        private void BuildPanel()
        {
            _panel = new GameObject("BattleResultPanel", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)_panel.transform, canvasRect, Vector2.zero, Vector2.one);
            _panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            _messageText = SpawnMessageText();
            SpawnRematchButton().onClick.AddListener(HandleRematchClicked);
            SpawnMenuButton().onClick.AddListener(HandleMenuClicked);
            _panel.SetActive(false);
        }

        private Button SpawnRematchButton()
        {
            var buttonObj = new GameObject("RematchButton", typeof(RectTransform));
            var rect = (RectTransform)buttonObj.transform;
            rect.SetParent(_panel.transform, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            buttonObj.AddComponent<Image>().color = Color.white;
            SpawnButtonLabel(buttonObj.transform, "Rematch");
            return buttonObj.AddComponent<Button>();
        }

        private Button SpawnMenuButton()
        {
            var buttonObj = new GameObject("MenuButton", typeof(RectTransform));
            var rect = (RectTransform)buttonObj.transform;
            rect.SetParent(_panel.transform, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.22f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight * 0.75f);
            buttonObj.AddComponent<Image>().color = Color.white;
            SpawnButtonLabel(buttonObj.transform, "Menu");
            return buttonObj.AddComponent<Button>();
        }

        private Text SpawnMessageText()
        {
            var textObj = new GameObject("ResultMessage", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)textObj.transform, (RectTransform)_panel.transform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));
            return ConfigureLegacyText(textObj.AddComponent<Text>(), fontSize: 48, color: Color.white);
        }

        private static void SpawnButtonLabel(Transform buttonTransform, string text)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)labelObj.transform, (RectTransform)buttonTransform, Vector2.zero, Vector2.one);
            ConfigureLegacyText(labelObj.AddComponent<Text>(), fontSize: 28, color: Color.black).text = text;
        }

        private static void ConfigureStretchRect(RectTransform rect, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.SetParent(parent, worldPositionStays: false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text ConfigureLegacyText(Text text, int fontSize, Color color)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            return text;
        }
    }
}
```

---

## Task 9: Wire the main menu's "2 Players" button

**Files:**
- Modify: `Assets/Scripts/Gameplay/MainMenuController.cs:38` (button spec), add a new handler method

**Interfaces:**
- Consumes: nothing new — `UnityEngine.SceneManagement.SceneManager.LoadScene` (already imported in this file).

The scene this loads (`"BattleMode"`) doesn't exist until Task 10 — do this task after Task 10, or expect the button to log a scene-not-found error until then.

- [ ] **Step 1: Enable the button and point it at the new scene**

Change line 38 in `Assets/Scripts/Gameplay/MainMenuController.cs` from:

```csharp
            SpawnButton(new MenuButtonSpec { Name = "TwoPlayerButton", Label = "2 Players (Coming Soon)", AnchorY = 0.26f, Interactable = false, OnClick = null });
```

to:

```csharp
            SpawnButton(new MenuButtonSpec { Name = "TwoPlayerButton", Label = "2 Players", AnchorY = 0.26f, Interactable = true, OnClick = HandleBattleClicked });
```

- [ ] **Step 2: Add the handler**

Add directly after `HandleSettingsClicked` (line 42):

```csharp
        private void HandleBattleClicked() => SceneManager.LoadScene("BattleMode");
```

- [ ] **Step 3: Play Mode check**

Via Unity MCP or the Editor: open `MainMenu`, enter Play mode, confirm the "2 Players" button is now enabled and clicking it attempts to load `BattleMode` (will error until Task 10 creates the scene — that error is expected at this point, not a regression).

---

## Task 10: Assemble the `BattleMode` scene

**Files:**
- Create: `Assets/Scenes/BattleMode.unity`

This task is Editor/MCP-driven scene assembly, not code — there is no source file to diff. Follow these steps via the Unity MCP bridge (`manage_scene`, `manage_gameobject`, `manage_components`) or the Editor UI directly.

- [ ] **Step 1: Create the scene**

Create `Assets/Scenes/BattleMode.unity` and add it to Build Settings (matching how `MainMenu`/`SettingsMenu`/`SampleScene` are already registered).

- [ ] **Step 2: Build Player 1's side**

Duplicate `SampleScene`'s full per-player hierarchy (the `GameBoard`, `Shooter` [`ShooterController`], `FiredBubbleController`, `MatchProcessor`, `GridDebugRenderer`, `CeilingRenderer`, `Main Camera`, and the `RotateLeftZone`/`RotateRightZone`/`FireZone` UI zones) into `BattleMode.unity`, parented under a new empty `Player1` root at world position `(0, 0, 0)`. This is Player 1 — leave everything as-is except:
- On the copied `Main Camera`: set `Rect` to `(x: 0, y: 0, w: 1, h: 0.5)` (bottom half of the screen). Leave rotation at identity.
- On the copied `GameBoard`: set the new `Target Camera` field (Task 1) to this camera; set `Viewport Height Fraction` to `0.5`. Assign `DefaultDifficultyCurve.asset` and `DefaultPatternLevelCatalog.asset` to `Difficulty Curve`/`Pattern Catalog` (same assets `SampleScene` uses).
- Remove (or leave disabled — either is fine since solo mode's `GameStateManager`/`ScoreTracker`/`ShotsFiredCounter`/`HudDisplay`/`LevelResultScreen`/`CameraShake` are not part of battle mode): do not carry these over into `BattleMode.unity` at all.
- Add a `BattleShotClock` component (Task 4) to the `Shooter` GameObject (or a new sibling GameObject), with `Shooter Controller` wired to Player 1's `ShooterController`.

- [ ] **Step 3: Build Player 2's side as a rotated mirror**

Duplicate the same set of GameObjects again, parented under a new empty `Player2` root at world position `(1000, 0, 0)` — a large world-space offset so the two boards never visually overlap regardless of framing.
- On Player 2's `Main Camera`: set `Rect` to `(x: 0, y: 0.5, w: 1, h: 0.5)` (top half). Set `Transform.rotation` to `(0, 0, 180)`. Position it at `(1000, 0, -10)` (matching the `Player2` root's X offset, same Z as Player 1's camera).

  *Why this achieves the visual flip with zero code changes:* `GameBoard`'s board-fit math (`PositionBoard`, `BoardBoundsCalculator`, `ShooterOrigin`) only ever reads `camera.transform.position` (a point) and `camera.orthographicSize` (a scalar) — both rotation-invariant — never the camera's rotation/forward/up axes. So rotating only the camera transform 180° changes nothing about where `GameBoard` *thinks* things are; it only flips what direction on screen corresponds to world "up" for that camera's rendered output. The board's world-space ceiling (computed as "above" the camera) ends up rendered at the *bottom* of Player 2's screen half (near the middle divider) and the shooter (computed as "below" the camera) ends up rendered at the *top* (near the device's outer edge) — exactly the layout a player facing the opposite direction needs.
- On Player 2's `GameBoard`: `Target Camera` → Player 2's camera; `Viewport Height Fraction` → `0.5`; same `Difficulty Curve`/`Pattern Catalog` assets as Player 1.
- Add a `BattleShotClock` to Player 2's `ShooterController`, same as Player 1.

- [ ] **Step 4: Mirror Player 2's touch input zones**

Both players' UI zones live under **one shared, full-screen `Canvas`** (Screen Space - Overlay) — not per-camera canvases, since Overlay-mode UI raycasting is purely screen-space and doesn't care about world cameras.
- Player 1's `RotateLeftZone`/`RotateRightZone`/`FireZone` stay exactly as duplicated from `SampleScene` (already anchored near the bottom edge), parented directly under the shared `Canvas`.
- For Player 2: create an empty `Player2Zones` RectTransform under the shared `Canvas`, anchored to the top half (`anchorMin = (0, 0.5)`, `anchorMax = (1, 1)`), with `localEulerAngles = (0, 0, 180)`. Duplicate Player 1's three zone GameObjects *into* `Player2Zones`, keeping their original local anchors/positions unchanged (i.e. don't hand-adjust their numbers — the parent's 180° rotation does the mirroring automatically, both visually and for uGUI's rotation-aware raycasting).
- Wire Player 2's `ShooterController`'s `Rotate Left Zone`/`Rotate Right Zone`/`Fire Zone` fields to these new copies (not Player 1's).

- [ ] **Step 5: Wire the Battle components**

Add a `BattleAttackConfig` reference (`DefaultBattleAttackConfig.asset` from Task 3) and create:
- `BattleAttackController` × 2 (one per side) — `Own Board`/`Opponent Board` cross-wired (Player 1's `Own Board` = Player 1's `GameBoard`, `Opponent Board` = Player 2's; and vice versa), `Attack Config` = the shared asset.
- `BattleSideOutcome` × 2 (one per side) — `Game Board` wired to that side's own `GameBoard`.
- One `BattleMatchController` on a new top-level `BattleMatch` GameObject — `Player1`/`Player2` fields filled in with each side's `GameBoard`/`ShooterController`/`BattleShotClock`/`BattleAttackController`/`BattleSideOutcome`.
- One `BattleResultScreen` on the `BattleMatch` GameObject (or a new UI-focused one) — `Match Controller` wired to the `BattleMatchController` above, `Canvas Rect` wired to the shared `Canvas`.

- [ ] **Step 6: Play Mode verification**

Via Unity MCP `execute_code` + screenshot (or the Editor directly): enter Play mode, confirm both boards render in their correct halves, Player 2's content and touch zones are visibly flipped, and pressing each side's rotate/fire zones controls only that side's shooter.

---

## Task 11: End-to-end match flow verification

No new files — this is a full Play Mode pass exercising everything built above together, following the same `execute_code`-driven verification pattern used for Milestone 10/11 and the Superpowers Task 11 Play Mode check (see `docs/ROADMAP.md`).

- [ ] **Step 1: Verify attacks push the opponent's wall**

Via `execute_code` in Play mode: force a match on Player 1's board (e.g. call `MatchProcessor`'s handling path directly, or place same-colored bubbles and trigger a placement), confirm Player 2's `GridModel.RowsPushed` increments accordingly once enough attack value is banked, and confirm it does **not** happen after every single pop for a match too small to bank a whole row (verifies `PendingRowsMeter`'s remainder-carrying end-to-end, not just in isolation).

- [ ] **Step 2: Verify win by clearing**

Force Player 1's board to empty (e.g. repeatedly pop until `Grid.IsEmpty`); confirm `BattleResultScreen` shows "Player 1 Wins!" and both sides' `ShooterController`/`BattleShotClock`/`BattleAttackController` are disabled.

- [ ] **Step 3: Verify loss by wall reaching the shooter line**

Force enough pushes onto Player 2's board that its last row is occupied when pushed (e.g. repeatedly call `PushRowDown` after filling the last row); confirm the result screen shows "Player 1 Wins!" (Player 2 lost) and the match stops.

- [ ] **Step 4: Verify the draw case**

Via `execute_code`, trigger both sides' end conditions within the same script execution (before the next `LateUpdate`) — confirm the result screen shows "Draw!" rather than either player's win.

- [ ] **Step 5: Verify Rematch**

Click "Rematch"; confirm both boards regenerate with new (but matching) content, both sides' shooting is re-enabled, and the match can be won/lost again.

- [ ] **Step 6: Full regression pass**

Run the full EditMode suite once more (`.\run-edittests.ps1` or Test Runner) and re-open `SampleScene` in Play mode to confirm solo mode is completely unaffected by everything added in this plan.
