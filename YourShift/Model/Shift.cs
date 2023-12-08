using System;

namespace YourShift.Model
{
    public class Shift
    {
        public int Count { get; set; }

        public DateTime LastShift { get; set; }

        public int Shifts { get; set; }

        public Rank Rank { get; set; }
    }

    public enum Rank
    {
        Rookie,
        Officer,
        Sergeant,
        Captian
    }
}
