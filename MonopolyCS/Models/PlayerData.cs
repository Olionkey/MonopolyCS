using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

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

        [JsonIgnore]
        public PlayerGameData PlayerGameData
        {
            get => JsonConvert.DeserializeObject<PlayerGameData>(PlayerJsonData);
            set => PlayerGameData = JsonConvert.SerializeObject(value);
        }
    }
}
