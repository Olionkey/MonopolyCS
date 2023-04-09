namespace PlayerGameData
{
    public class playerGameData
    {
        public List<Property> properties { get; set; }
        public int balance { get; set; }
        public List<string> chanceCard { get; set; }
        public List<string> communityChest { get; set; }
        public bool inJail { get; set; }
        public int turnsInJail { get; set; }
        public int snakeEyeCount { get; set; }
        public int currentPos { get; set; }
    }
}