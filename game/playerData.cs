using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace playerData
{
    public class playerData
    {
        public int ID { get; set; }
        public string userID { get; set; }
        public string guild { get; set; }
        public string gameID { get; set; }
        public string playerDataJson { get; set; }

        [NotMapped]
        public playerGameData playerGameData
        {
            get => JsonConvert.DeserializeObject<playerGameData>(playerDataJson);
            set => playerDataJson = JsonConvert.SerializeObject(value);
        }
    }
}


