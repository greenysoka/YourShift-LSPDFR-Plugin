namespace YourShift.Model
{
    public class Shift
    {
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
