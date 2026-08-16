namespace FOG.EscapeTheLava
{
    public readonly struct RoundResult
    {
        public RoundResult(bool won, RoundEndReason reason, int score, int diamondsCollected, int totalDiamonds, float timeRemaining)
        {
            Won = won;
            Reason = reason;
            Score = score;
            DiamondsCollected = diamondsCollected;
            TotalDiamonds = totalDiamonds;
            TimeRemaining = timeRemaining;
        }

        public bool Won { get; }
        public RoundEndReason Reason { get; }
        public int Score { get; }
        public int DiamondsCollected { get; }
        public int TotalDiamonds { get; }
        public float TimeRemaining { get; }
    }
}
