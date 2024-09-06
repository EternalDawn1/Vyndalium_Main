namespace GeneralGame
{
    public class LeaderboardData
    {
        public List<LeaderboardEntry> Entries { get; private set; } = new List<LeaderboardEntry>();

       
        

    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string DisplayName { get; set; }
        public int Value { get; set; }
    }
}