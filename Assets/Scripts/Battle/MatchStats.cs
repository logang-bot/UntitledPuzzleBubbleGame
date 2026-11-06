using Game.Gameplay;

namespace Game.Battle
{
    /// <summary>
    /// One player's match stats for the 2-player result panel. Popped counts
    /// every bubble cleared (popped or dropped); best chain is the most
    /// bubbles cleared by a single shot; score uses the solo ScoreCalculator.
    /// </summary>
    public class MatchStats
    {
        public int Score { get; private set; }
        public int Popped { get; private set; }
        public int BestChain { get; private set; }
        public int Shots { get; private set; }

        private int _currentChain;

        public void RecordShot()
        {
            Shots++;
            _currentChain = 0;
        }

        public void RecordPopped(int bubbleCount)
        {
            Score += ScoreCalculator.PointsForPop(bubbleCount);
            AddCleared(bubbleCount);
        }

        public void RecordDropped(int bubbleCount)
        {
            Score += ScoreCalculator.PointsForDrop(bubbleCount);
            AddCleared(bubbleCount);
        }

        public void Reset()
        {
            Score = 0;
            Popped = 0;
            BestChain = 0;
            Shots = 0;
            _currentChain = 0;
        }

        private void AddCleared(int bubbleCount)
        {
            Popped += bubbleCount;
            _currentChain += bubbleCount;
            if (_currentChain > BestChain) BestChain = _currentChain;
        }
    }
}
