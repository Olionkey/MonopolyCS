namespace PropertyClassHelper
{
    public static class PropertyClassHelper
    {
        public static List<PropertyCard> LoadPropertyCards(string filepath)
        {
            string jsonContent = File.ReadAllText("./game_data/propertyCards.json");
            PropertyCard propertyCards = JsonConvert.DeserializeObject<List<PropertyCard>(jsonContent);
            return propertyCards;
        }
    }
}