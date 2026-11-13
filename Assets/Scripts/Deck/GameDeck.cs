using Game.Art;
using Game.Gameplay;
using Game.Grid;
using Game.Pets;
using Game.Settings;
using Game.Shooter;
using Game.Superpowers;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The deck under a board (cartoon game guide): the launcher mechanism,
    /// the rotate slabs and pointer that drive the ShooterController, the
    /// timer pips and hurry burst, the loaded and next bubbles, and in solo
    /// the HUD and superpower buttons. The controls are bound in Awake, before
    /// the shooter first reads them.
    /// </summary>
    public class GameDeck : MonoBehaviour
    {
        public MechanismParts Mechanism { get; private set; }
        public DeckSetup Setup { get; private set; }

        [SerializeField] private MenuArtLibrary art;
        [SerializeField] private DeckPlacement placement;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private ShooterController shooter;
        [SerializeField] private FiredBubbleController firedBubbles;
        [SerializeField] private MonoBehaviour shotClock;
        [SerializeField] private ScoreTracker scoreTracker;
        [SerializeField] private ShotsFiredCounter shotsFired;
        [SerializeField] private SuperpowerController superpowers;
        [SerializeField] private PetAppearanceCatalog appearanceCatalog;

        private DeckUi _ui;
        private RectTransform _stage;

        private void Awake()
        {
            Setup = DeckSetup.For(placement, PetProfileStore.LoadAimer(), GameSettings.Handedness);
            _ui = new DeckUi(art);
            _stage = placement == DeckPlacement.Solo ? DeckCanvas.SoloStage() : DeckCanvas.BattleStage(placement == DeckPlacement.BattleTop);
            Mechanism = MechanismBuilder.Build(_ui, _stage, Setup.NewParts());
            var controls = DeckControls.Build(_ui, Mechanism);
            shooter.BindControls(controls.RotateLeft, controls.RotateRight, controls.Fire);
        }

        private void Start()
        {
            var clock = (IShotClock)shotClock;
            var animator = MechanismAnimator.Create(Mechanism, shooter, art);
            PetStage.Create(_ui, Mechanism, PetInputs(animator, clock));
            TimerPipsView.Create(_ui, Mechanism, clock);
            LoadedBubbleView.Create(_ui, Mechanism, firedBubbles);
            if (placement == DeckPlacement.Solo) BuildSoloHud();
            BuildHurryBurst(clock);
        }

        private PetStageInputs PetInputs(MechanismAnimator animator, IShotClock clock) => new PetStageInputs
        {
            Setup = Setup,
            Catalog = appearanceCatalog,
            Shooter = shooter,
            Mechanism = animator,
            Clock = clock,
            Board = gameBoard,
            Game = shotClock as GameStateManager
        };

        private void BuildSoloHud()
        {
            var layer = _ui.Group(_stage, "Hud");
            var right = Setup.Geometry.RightHanded;
            HudView.Create(_ui, layer, right ? HudView.RightHandX : HudView.LeftHandX).Bind(scoreTracker, shotsFired, gameBoard);
            SuperpowerButtonsView.Create(_ui, layer, right ? SuperpowerButtonsView.RightHandX : SuperpowerButtonsView.LeftHandX).Bind(superpowers);
            FreezeOverlayView.Create(_ui, _stage, ((GameStateManager)shotClock, Setup.Geometry.Point(DeckLayout.Pointer) - new Vector2(0f, 40f)));
        }

        private void BuildHurryBurst(IShotClock clock)
        {
            var point = placement == DeckPlacement.Solo ? DeckLayout.SoloHurryBurst : DeckLayout.BattleHurryBurst;
            HurryBurstView.Create(_ui, _ui.Group(_stage, "Hurry"), (point, clock));
        }
    }
}
