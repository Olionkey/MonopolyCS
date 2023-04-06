using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace playerData {
    public class PlyaerObContext : DbContext {
        public DbSet<PlayerData> Players { get; set; }
        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder ) {
            optionsBuilds.UseSqlite( "Data Source=./game_data/playerData.sqlite");
        }
    }

    public class playerData {
        public int ID                   { get; set; }
        public string userID            { get; set; }
        public string guild             { get; set; }
        public string gameID            { get; set; }
        public string playerDataJson    { get; set; }

        [NotMapped]
        public playerGameData playerGameData {
            get => JsonConvert.DeserializeObject<playerGameData>( playerDataJson );
            set => playerDataJson = JsonConvert.SerializeObject( value );
        }
    }

    public class playerGameData {
        public List<Property> properties    { get; set; }
        public int balance                  { get; set; }
        public List<string> chanceCard      { get; set; }
        public List<string> communityChest  { get; set; }
        public bool inJail                  { get; set; }
        public int turnsInJail              { get; set; }
        public int snakeEyeCount            { get; set; }
        public int currentPos               { get; set; }
    }

    public class propertyCard {
        public string name          { get; set; }
        public int pos              { get; set; }
        public int rent             { get; set; }
        public int houseCost        { get; set; }
        public int hotelCost        { get; set; }
        public int price            { get; set; }
        public int mortage          { get; set; }
        public string color         { get; set; }
        public int amountInGroup    { get; set; }
        public int buildingAmount   { get; set; }
        public bool isMortgaged     { get; set; }    
    }

    public static class propertyClassHelper {
        public static Dictionary<string, PropertyCard> LoadPropertyCards(string filepath) 
        {
            var jsonContent = File.ReadAllText("./game_data/propertyCards.json");
            var propertyCards = JsonConvert.DeserializeObject<Dictionary<string, PropetyCard>>(jsonContent);
            return propertyCards
        }
    }

    
    public class GameLogic {
        // It's only purpose is to manage the connection to the database and providng the necessary methods
        private readonly playerObContext _dbContext;
        private Dictionary<string, PropertyCard> propertyCards;

        public gameLogic ( playerObContext dbContext ) {
            _dbContext = dbContext;
            loadPropertyCards();
        }

        public async Task createPlayer ( SocketMessage message, string gameID ) {

            var PlayerData = new PlayerData {
                User: message.Author.Id.ToString(),
                Guild: (message.Channel as SocketGuildChannel)?.Guild.Id.ToString(), // tries to acces the guild id from SocketGuildChannel, if it can't then it returns as null
                PlayerGameData = new PlayerGameData {
                    properties      = new List<property>(),
                    balance         = 1500,
                    chacneCard      = new List<string>(),
                    communityChest  = new List<string>(),
                    inJail          = false,
                    TurnsInJail     = 0,
                    snakeEyeCount   = 0,
                    currentPos      = 0
                }
            };

            _dbContext.Players.Add( playerData );
            await _dbContext.SaveChangesAsync();
        }

        public void loadPropertyCards(){
            string propertyCardsPath = "./game_data/propertyCards.json"; 
            propertyCards = propertyClassHelper.loadPropertyCards(propertyCardsPath)
        }

        public async Task<bool> setPlayerToJail ( string userID, string gameID ) {

            var player = await _dbContext.Players.FirstOrDefaultAsync( p => p.userID == userID && p.gameID == gameID );

            if (player != null) {
                player.PlayerGameData.inJail = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;;
        }

        public async Task<bool> removePlayerFromJail ( string userID, string gameID ) {
            var player = await _dbContext.Players.FirstOrDefaultAsync( p => p.userID == userID && p.gameID == gameID );

            if (player != null) {
                player.PlayerGameData.inJail = false;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> addPlayerProperty ( string userID, string gameID, string propertyName ) {
            var propertyOwner = await findPropertyOwner( gameID, propertyName);
            if (propertyOwner != null)
                return false;
            
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.userID == userId && p.gameId == gameId);
            if ( player != null) {

                var propertyCard = propertyCards[propertyName];

                if ( propertyCard != null ) {
                    player.playerGaneData.properties.Add(propertyCard);
                    player.playerGameData.properties.Sort( (a,b) => a.pos.CompareTo( b.pos ) ); // This may not be added, but would like it be sorted by how it is on the map
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> removePlayerProperty ( string userID, string gameID, string propertyName ) {
            var player = await _dbContext.Players.FirstOrDefaultAsync( p => p.userID == userID && p.gameID)

            if ( player != null) {
                player.playerGameData.properties.RemoveAll( propertyCards[propertyName] );
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<PlayerData> findPropertyOwner ( string gameID, string propertyName ) {
            return await _dbContext.players
                    .FirstOrDefaultAsync ( p => p.gameID == gameID && p.playerGameData.properties.Any( property => property.name == propertyName))
        }










    }


}