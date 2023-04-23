using MonopolyCS.Configuration;
using MonopolyCS.Models;
using Newtonsoft.Json;

namespace MonopolyCS.Helpers
{
    public static class PropertyClassHelper
    {
        public static IEnumerable<PropertyCard> LoadPropertyCards(string filepath, MonopolyCsConfigMgr cm)
        {
            string jsonContent = File.ReadAllText(cm.EnvironmentVariables.PropertyCards);
            List<PropertyCard> propertyCards = JsonConvert.DeserializeObject<List<PropertyCard>>(jsonContent);
            return propertyCards;
        }
    }
}