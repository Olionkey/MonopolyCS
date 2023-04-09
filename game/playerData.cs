using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PlayerData
{
    public class PlayerData
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Guild { get; set; }
        public string GameID { get; set; }
        public string PlayerDataJson { get; set; }

        [NotMapped]
        public PlayerGameData PlayerGameData
        {
            get => JsonConvert.DeserializeObject<PlayerGameData>(playerDataJson);
            set => playerDataJson = JsonConvert.SerializeObject(value);
        }
    }
}