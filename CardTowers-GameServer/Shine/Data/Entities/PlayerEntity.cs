using System.ComponentModel.DataAnnotations;

namespace CardTowers_GameServer.Shine.Data.Entities
{
    public class PlayerEntity : IEntity
    {
        public Guid id { get; set; }
        public string account_id { get; set; }
        public string display_name { get; set; }
        public int total_experience { get; set; }
        public int total_wins { get; set; }
        public int total_losses { get; set; }
        public int total_draws { get; set; }
        public int elo_rating { get; set; }
        public double mmr { get; set; }
        public DateTime? last_played_at { get; set; }
    }
}

