using MonopolyCS.Models;
using Newtonsoft.Json;

namespace MonopolyCS.Helpers
{
    public static class PropertyClassHelper
    {
        public static List<PropertyCard> LoadPropertyCards(string filepath)
        {
            string jsonContent = File.ReadAllText("./game_data/propertyCards.json");
            List<PropertyCard> propertyCards = JsonConvert.DeserializeObject<List<PropertyCard>>(jsonContent);
            return propertyCards;
        }
    }
}