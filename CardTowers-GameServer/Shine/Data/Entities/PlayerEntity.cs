namespace CardTowers_GameServer.Shine.Data.Entities
{
    public class PlayerEntity : IEntity
    {
        public Guid Id { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public int TotalExperience { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalDraws { get; set; }
        public int EloRating { get; set; }
        public double MMR { get; set; }
        public DateTime? LastPlayedAt { get; set; }
    }
}

