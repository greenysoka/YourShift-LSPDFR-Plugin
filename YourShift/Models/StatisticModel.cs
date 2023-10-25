using LiteDB;

namespace YourShift.Models
{
    public class StatisticModel
    {
        // By LiteDB generated Id ver every entry.
        [BsonId]
        public int Id { get; set; }

        // How many Shifts the player has made in his career
        public int Shifts { get; set; }

        // What the players rank is, based on his statistics
        public Rank Rank { get; set; }
 
    }

    public enum Rank
    {
        Rooki,
        Officer,
        Sargeant,
        Captain
    }
}
