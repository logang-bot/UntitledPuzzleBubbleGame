using Game.Art;
using Game.Flow;
using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// 2-player results on their own pixel-art overlay canvas. When the match
    /// ends, each half shows its YOU WIN / YOU LOSE banner (or DRAW) for
    /// 2.5 s, then the dimmed stats screen with Rematch and Home. Player 2's
    /// half is on top, rotated 180°. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleResultScreen : MonoBehaviour
    {
        [SerializeField] private BattleMatchController matchController;
        [SerializeField] private MenuArtLibrary art;

        private const int OverlaySortingOrder = 10;
        private const float ResultMomentSeconds = 2.5f;

        private MatchStatsTracker _player1Stats;
        private MatchStatsTracker _player2Stats;
        private GameObject _momentRoot;
        private BattleResultHalfView _topMoment;
        private BattleResultHalfView _bottomMoment;
        private BattleStatsView _statsView;
        private BattleMatchResult _result;
        private float _momentRemaining = -1f;

        private void Start()
        {
            SceneTransition.Install(art);
            _player1Stats = new MatchStatsTracker(matchController.Player1);
            _player2Stats = new MatchStatsTracker(matchController.Player2);
            BuildScreens(new MenuUiFactory(art), OverlayCanvasFactory.Create("BattleResultCanvas", OverlaySortingOrder));
            matchController.OnMatchEnded += HandleMatchEnded;
        }

        private void OnDestroy()
        {
            matchController.OnMatchEnded -= HandleMatchEnded;
            _player1Stats?.Dispose();
            _player2Stats?.Dispose();
        }

        private void BuildScreens(MenuUiFactory ui, RectTransform canvas)
        {
            var momentRoot = RuntimeUi.CreateRect(canvas, "ResultMoment", UiPlacement.Fill());
            var stage = NativeStage.Create(momentRoot).Stage;
            _topMoment = new BattleResultHalfView(ui, BattleHalfFrame.Create(stage, isTop: true));
            _bottomMoment = new BattleResultHalfView(ui, BattleHalfFrame.Create(stage, isTop: false));
            _momentRoot = momentRoot.gameObject;
            _momentRoot.SetActive(false);
            _statsView = new BattleStatsView(ui, canvas, (HandleRematch, () => SceneTransition.LoadScene(SceneNames.MainMenu)));
        }

        private void HandleMatchEnded(BattleMatchResult result)
        {
            _result = result;
            _topMoment.Show(OutcomeFor(result, BattleMatchResult.Player2Wins));
            _bottomMoment.Show(OutcomeFor(result, BattleMatchResult.Player1Wins));
            _momentRoot.SetActive(true);
            _momentRemaining = ResultMomentSeconds;
        }

        private void Update()
        {
            if (_momentRemaining < 0f) return;
            _momentRemaining -= Time.unscaledDeltaTime;
            if (_momentRemaining < 0f) ShowStats();
        }

        private void ShowStats()
        {
            _momentRoot.SetActive(false);
            _statsView.Show(
                (OutcomeFor(_result, BattleMatchResult.Player2Wins), _player2Stats.Stats),
                (OutcomeFor(_result, BattleMatchResult.Player1Wins), _player1Stats.Stats));
        }

        private void HandleRematch()
        {
            _statsView.Hide();
            _player1Stats.Stats.Reset();
            _player2Stats.Stats.Reset();
            matchController.Rematch();
        }

        private static BattleHalfOutcome OutcomeFor(BattleMatchResult result, BattleMatchResult winForThisHalf)
        {
            if (result == BattleMatchResult.Draw) return BattleHalfOutcome.Draw;
            return result == winForThisHalf ? BattleHalfOutcome.Win : BattleHalfOutcome.Lose;
        }
    }
}
