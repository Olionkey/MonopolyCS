namespace MonopolyCS
{
    public class ChanceCard
    {
        private List<int> _deck = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private int index = 0;

        public static void ShuffleDeck()
        {
            //Shuffle the deck to randomize cards
        }

        /*public static void NextCard()
        {
            switch (index)
            {
                case 0:
                    AdvanceToBoardwalk();
                    break;
                case 1:
                    //something...
                    break;
                default:
                    throw new Error("Something bad happened :(");
                    break;
            }
            index++;
        }

        public static void AdvanceToBoardwalk(List<players> players, int activePlayerID)
        {
            players.where(p => p.turn == true).MovePlayer("boardwalk");
        }

        public static void ChairmanOfTheBoard(List<players> players)
        {
            foreach (PlayerData.where(players => players.turn != true))
            {
                //players.turn == true pays other players
            }
        }*/
    }
}