namespace MonopolyCS.Models
{
    public class PlayerGameData
    {
        public List<PropertyCard> Properties { get; set; }
        public int Balance { get; set; }
        public List<string> ChanceCard { get; set; }
        public List<string> CommunityChest { get; set; }
        public bool InJail { get; set; }
        public int TurnsInJail { get; set; }
        public int SnakeEyeCount { get; set; }
        public int CurrentPos { get; set; }
    }
}