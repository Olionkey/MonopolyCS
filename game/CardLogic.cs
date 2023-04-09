using ChanceCards;
using CommunityCards;

namespace CardLogic 
{
    public class CardDeck
    {
        private List<ChanceCards> chance;
        private List<CommunityCards> community;

        public CardDeck()
        {
            LoadCards("./game_data/chanceCards.json", "chance");
            ShuffleDeck(chance);

            LoadCards("./game_data/communityCards.json", "community");
            ShuffleDeck(community);
        }
        
        private void LoadCards( string filePath, string option ) 
        {
            string jsonData = filePath.readAllText(filePath);
            (option == "chance") ? chance = JsonConvert.DeserializeObject<List<ChanceCards>>(jsonData) : community = JsonConvert.DeserializeObject<List<CommunityCards>>(jsonData);      
        }

        public static void ShuffleDeck (List<ICard> deck) 
        {
            Random rng = new Random();
            int n = deck.Count;
            while ( n > 1 ) 
            {
                n--;
                int k = rng.Next( n + 1 );
                ICard value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
        }

        public static void ChanceCard (SocketMessage message, string gameID) 
        {
            
        }

    }
}