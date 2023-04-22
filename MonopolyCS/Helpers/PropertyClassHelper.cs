using MonopolyCS.Models;
using Newtonsoft.Json;

namespace MonopolyCS.Helpers
{
    public static class PropertyClassHelper
    {
        public static IEnumerable<PropertyCard> LoadPropertyCards(string filepath)
        {
            string jsonContent = File.ReadAllText(@"MonopolyCs/Data/propertyCards.json");
            List<PropertyCard> propertyCards = JsonConvert.DeserializeObject<List<PropertyCard>>(jsonContent);
            return propertyCards;
        }
    }
}