using Newtonsoft.Json;

namespace MonopolyCS.Models
{
    public class PlayerData
    {
        public string GameID { get; set; }
        public int ID { get; set; }
        public string UserId { get; set; }
        public string Guild { get; set; }
        public string GameId { get; set; }
        public string PlayerJsonData { get; set; }
        public PlayerGameData PlayerGameData { get; set; }
    }
}
