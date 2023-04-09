namespace PropertyClassHelper
{
    public static class propertyClassHelper
    {
        public static Dictionary<string, PropertyCard> LoadPropertyCards(string filepath)
        {
            var jsonContent = File.ReadAllText("./game_data/propertyCards.json");
            var propertyCards = JsonConvert.DeserializeObject<Dictionary<string, PropetyCard>>(jsonContent);
            return propertyCards;
        }
    }
}