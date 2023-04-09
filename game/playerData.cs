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
        public int baseRent         { get; set; }
        public int oneH             { get; set; }
        public int twoH             { get; set; }
        public int threeH           { get; set; }
        public int fourH            { get; set; }
        public int hotel            { get; set; }      
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
            return propertyCards;
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
                user = message.Author.Id.ToString(),
                guild = (message.Channel as SocketGuildChannel)?.Guild.Id.ToString(), // tries to acces the guild id from SocketGuildChannel, if it can't then it returns as null
                playerGameData = new PlayerGameData {
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
            propertyCards = propertyClassHelper.loadPropertyCards(propertyCardsPath);
        }

        public async Task<bool> setPlayerToJail ( string userID, string gameID ) {

            var player = await _dbContext.Players.FirstOrDefaultAsync( p => p.userID == userID && p.gameID == gameID );

            if (player != null) {
                player.PlayerGameData.inJail = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
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
                    player.playerGameData.properties.Add(propertyCard);
                    player.playerGameData.properties.Sort( (a,b) => a.pos.CompareTo( b.pos ) ); // This may not be added, but would like it be sorted by how it is on the map
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> removePlayerProperty ( string userID, string gameID, string propertyName ) {
            var player = await _dbContext.Players.FirstOrDefaultAsync( p => p.userID == userID && p.gameID);

            if ( player != null) {
                player.playerGameData.properties.RemoveAll( propertyCards[propertyName] );
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        // This and findOwner can be probably be turned into one method but too lazy at the moment
        // right now this just finds who ever owns a property if there is someone that does own it
        public async Task<PlayerData> findPropertyOwner ( string gameID, string propertyName ) {
            return await _dbContext.players
                    .FirstOrDefaultAsync ( p => p.gameID == gameID && p.playerGameData.properties.Any( property => property.name == propertyName));
        }

        public async Task<PlayerData> movePlayer ( SocketMessage message, string gameID, int move) {
            var player = await _db.context.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if ( player != null ) {
                int newPos = player.playerGameData.currentPos + move;
                player.PlayerGameData.currentPos = ( newPos <= 40 && newPos > 0 ) ? newPos : ( newPos < 0 ) ? newPos + 40 : newPos - 40;
                await _dbContext.SaveChangesAsync();
            }

            return player;
        }

        public async Task<(PlayerData player, int index)> findOwner ( string gameID, inst movePosition ) {
            var gamePlayer = await _dbContext.Players 
                .Where( p => p.gameID == gameID )
                .Select( p => p.playerGameData )
                .ToListAsync();
            
            foreach ( var currentPlayer in gamePlayers ) {
                
                var ownProperties = currentPlayer.ownProperties;

                foreach ( var property in ownProperties ) {
                    
                    int propertyIndex = propertyCards.FindIndex( card => card.name == property );

                    if ( propertyIndex != -1 && propertyCards[propertyIndex].pos == movePostion )
                        return ( currentPlayer, propertyIndex );
                }
            }
            return (null, -1);
        }

        public async Task<PlayerData> addSnakeEyeCount ( SocketMessage message, string gameID) {
            var player = await _dbContext.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);
            
            if ( player != null ) {
                
                player.playerGameData.snakeEyeCount ++;
                // it should never even get above 3 but just in case
                if ( player.playerGameData.snakeEyeCount >= 3 ) {

                    player.playerGameData.snakeEyeCount = 0;
                    await setPlayerToJail ( message );
                }
                await _dbContext.SaveChangesAsync();
            }
            return player;
        }

        public async Task<int> getSnakeEyeCount ( SocketMessage message, string gameID ) {
            var player = await _dbContext.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);
            // if player is null return null, otherwise return count if that is null return 0
            return player?.playerGameData.snakeEyeCount ?? 0;
        } 

        public async Task<PlayerData> addMoney ( string playerID, string gameID, int balance ) {
            var player = await _dbContext.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if ( player != null ) {
                player.playerGameData.balance += balance;
                await _dbContext.SaveChangesAsync();
            }
            return player;
        }

        // TODO: add some logic if you happen to go negative and try to figure out what the best items are to mortage to stay in game and not go bankrupt
        public async Task<( PlayerData player, int newBalance)> removeMoney ( string playerID, string gameID, int balance ) {
            var player = await _dbContext.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if ( player != null ) {
                int newBalance = player.playerGameData.balance - balance;
                if ( newBalance < 0 ) {
                    return ( player, newBalance );
                } 

                player.playerGameData.balance = newBalance;
                await _dbContext.SaveChangesAsync();
            }
            return ( player, 0 );
        }

        public async Task<int> getMoney ( string playerID, string gameID ){
            var player = await _dbContext.players.FirstOrDefaultAsync( p => p.user == message.Author.Id.ToString() && p.gameID == gameID);
            return player?.playerGameData.balance ?? 0;
        }

        // Originally thought of letting the user return specific data regarding their data IE property, mortgaged, buildings. We should just show it all when they ask.
        public async Task<List<Property>> getPlayerInfo( SocketMessage message, string gameID ) {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.User == message.Author.Id.ToString() && p.GameID == gameID);
            var playerData = player?.PlayerGameData;
            return playerData?.Properties;
        }

        public async Task restJailTurns ( SocketMessage message, string gameID ) {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.User == message.Author.Id.ToString() && p.GameID == gameID);
            if ( player != null ) {
                player.playerGameData.turnsInJail = 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> addBuildings ( Socketmessage message, string gameID, string propertyName, int amountsOfBuildings ) {
            var player = await getPlayerInfo ( message, gameID );
            var propertyCards = await loadPropertyCards();
            var propertyCard = propertyCards.FirstOrDefault ( pc => pc.name.ToLower() == propertyName.ToLower() );

            string colorGroup = propertyCard.color;
            int houseCout = propertyCard.houseCost;
            int hotelCost = propertyCard.hotelCost;

            if ( ownsAllColorGroup ( colorGroup, playerData.properties ) ) {
                int buildingCost = houseCost * amountsOfBuildings;

                if ( playerData.balance >= buildingsCost ) {
                    while ( amountsOfBuildings > 0 ) {
                        int minBuildings = playerData.properties.Where ( p => p.color == colorGroup ).Min( p => p.buildingAmount );

                        List<Property> updatedProperties = new List<Property>();
                        foreach ( var property in playerData.properties ) {

                            if ( property.color == colorGroup && propety.amountsOfBuildings == minBuildings && --amountsOfBuildings > 0){
                                updatedProperties.Add( new property {
                                    name = property.name,
                                    pos = property.pos,
                                    baseRent = property.baseRent,
                                    oneHouse = property.oneH,
                                    twoHouse = property.twoH,
                                    threeHouse = property.threeH,
                                    fourHouse = property.fourH,
                                    hotel = property.hotel,
                                    houseCost = property.houseCost,
                                    hotelCost = property.hotelCost,
                                    price = property.cost,
                                    mortage = property.mortage,
                                    color = property.color,
                                    amountInGroup = property.amountInGroup,
                                    buildingAmount = property.buildingAmount,
                                    isMortgaged = property.isMortgaged
                                });
                            } else {
                                updatedProperties.Add( proprety );
                            }
                        }

                        playerData.properties = updatedProperties;
                        playerData.balance = buildingCost;

                        await _dbContext.SaveChangesAsync();
                    }
                } else {
                    // TODO: Write logic telling the player how many they could afford
                }
            } else {
                throw new Exception ( " You do not own all properties in the color group." );
            }
        } 
        private bool OwnsAllColorGroup(string color, List<PropertyData> playerProperties)
        {
            int ownedProps = playerProperties.Count(p => p.Color == color);

            // Get the number of cards in the color group from the first property card of that color
            var colorGroupProperty = propertyCards.FirstOrDefault(pc => pc.Color == color);
            if (colorGroupProperty == null)
            {
                throw new Exception($"Invalid color group: {color}");
            }

            // Compare the owned properties count to the amount of properties in the color group
            return ownedProps == colorGroupProperty.Amount;
        }
    }

}


