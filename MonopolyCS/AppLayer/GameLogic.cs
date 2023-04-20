using Discord.WebSocket;
using MonopolyCS.DbLayer;
using MonopolyCS.Helpers;
using MonopolyCS.Models;

namespace MonopolyCS.AppLayer
{
    public class GameLogic
    {
        // It's only purpose is to manage the connection to the database and providng the necessary methods
        private readonly PlayerDbContext _dbContext;
        private List<PropertyCard> _propertyCards;

        public GameLogic(PlayerDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadPropertyCards();
        }

        public async Task CreatePlayer(SocketMessage message, string gameID)
        {

            PlayerData playerData = new () {
                UserId = message.Author.Id.ToString(),
                Guild = (message.Channel as SocketGuildChannel)?.Guild.Id.ToString(), // tries to acces the guild id from SocketGuildChannel, if it can't then it returns as null
                PlayerGameData = new PlayerGameData
                {
                    Properties = new List<PropertyCard>(),
                    Balance = 1500,
                    ChanceCard = new List<string>(),
                    CommunityChest = new List<string>(),
                    InJail = false,
                    TurnsInJail = 0,
                    SnakeEyeCount = 0,
                    CurrentPos = 0
                }
            };

            _dbContext.Players.Add(playerData);
            await _dbContext.SaveChangesAsync();
        }

        public void LoadPropertyCards()
        {
            string propertyCardsPath = "./game_data/propertyCards.json";
            var PropertyCards = PropertyClassHelper.LoadPropertyCards(propertyCardsPath);
        }

        public async Task<bool> SetPlayerToJail(string userID, string gameID)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.userID == userID && p.gameID == gameID);

            if (player != null)
            {
                player.PlayerGameData.inJail = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false; ;
        }

        public async Task<bool> RemovePlayerFromJail(string userID, string gameID)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.userID == userID && p.gameID == gameID);

            if (player != null)
            {
                player.PlayerGameData.inJail = false;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AddPlayerProperty(string userId, string gameId, string propertyName)
        {
            PlayerData propertyOwner = await FindPropertyOwner(gameId, propertyName);
            if (propertyOwner != null)
                return false;

            PlayerData player = await _dbContext.Players.FirstOrDefaultAsync(p => p.UserId == userId && p.GameId == gameId);
            if (player != null)
            {
                PropertyCard propertyCard = _propertyCards.Where(p => p.Name == propertyName).FirstOrDefault();

                if (propertyCard != null)
                {
                    player.PlayerGameData.Properties.Add(propertyCard);
                    player.PlayerGameData.Properties.Sort((a, b) => a.Pos.CompareTo(b.Pos)); // This may not be added, but would like it be sorted by how it is on the map
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> RemovePlayerProperty(string userId, string gameId, string propertyName)
        {
            PlayerData player = await _dbContext.Players.FirstOrDefaultAsync(p => p.userID == userId && p.gameID);

            if (player != null)
            {
                player.PlayerGameData.Properties.RemoveAll(p => p.Name == propertyName);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        // This and findOwner can be probably be turned into one method but too lazy at the moment
        // right now this just finds who ever owns a property if there is someone that does own it
        public async Task<PlayerData> FindPropertyOwner(string gameID, string propertyName)
        {
            return await _dbContext.Players
                .FirstOrDefaultAsync(p => p.gameID == gameID && p.playerGameData.properties.Any(property => property.name == propertyName));
        }

        public async Task<PlayerData> MovePlayer(SocketMessage message, string gameID, int move)
        {
            var player = await _db.context.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if (player != null)
            {
                int newPos = player.playerGameData.currentPos + move;
                player.PlayerGameData.currentPos = (newPos <= 40 && newPos > 0) ? newPos : (newPos < 0) ? newPos + 40 : newPos - 40;
                await _dbContext.SaveChangesAsync();
            }

            return player;
        }

        //The fuck is this method supposed to do? Is it moving a player to a card location?
        public async Task<(PlayerData player, int index)> FindOwner(string gameID, int movePosition)
        {
            List<PlayerData> gamePlayers = await _dbContext.Players
                .Where(p => p.gameID == gameID)
                .Select(p => p.playerGameData)
                .ToListAsync();

            foreach (PlayerData player in gamePlayers)
            {
                List<PropertyCard> ownedProperties = player.PlayerGameData.Properties;

                foreach (PropertyCard property in ownedProperties)
                {
                    int propertyPosition = propertyCards.FindIndex(card => card.name == property);

                    if (propertyIndex != -1 && propertyCards[propertyIndex].pos == movePostion)
                        return (player, propertyIndex);
                }
            }
            return (null, -1);
        }

        public async Task<PlayerData> AddSnakeEyeCount(SocketMessage message, string gameID)
        {
            var player = await _dbContext.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if (player != null)
            {
                player.playerGameData.snakeEyeCount++;
                // it should never even get above 3 but just in case
                if (player.playerGameData.snakeEyeCount >= 3)
                {
                    player.playerGameData.snakeEyeCount = 0;
                    await setPlayerToJail(message);
                }
                await _dbContext.SaveChangesAsync();
            }
            return player;
        }

        public async Task<int> GetSnakeEyeCount(SocketMessage message, string gameID)
        {
            var player = await _dbContext.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);
            // if player is null return null, otherwise return count if that is null return 0
            return player?.playerGameData.snakeEyeCount ?? 0;
        }

        public async Task<PlayerData> AddMoney(string playerID, string gameID, int balance)
        {
            var player = await _dbContext.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if (player != null)
            {
                player.playerGameData.balance += balance;
                await _dbContext.SaveChangesAsync();
            }
            return player;
        }

        // TODO: add some logic if you happen to go negative and try to figure out what the best items are to mortage to stay in game and not go bankrupt
        public async Task<(PlayerData player, int newBalance)> RemoveMoney(string playerID, string gameID, int balance)
        {
            var player = await _dbContext.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);

            if (player != null)
            {
                int newBalance = player.playerGameData.balance - balance;
                if (newBalance < 0)
                {
                    return (player, newBalance);
                }

                player.playerGameData.balance = newBalance;
                await _dbContext.SaveChangesAsync();
            }
            return (player, 0);
        }

        public async Task<int> GetMoney(string playerID, string gameID)
        {
            var player = await _dbContext.players.FirstOrDefaultAsync(p => p.user == message.Author.Id.ToString() && p.gameID == gameID);
            return player?.playerGameData.balance ?? 0;
        }

        // Originally thought of letting the user return specific data regarding their data IE property, mortgaged, buildings. We should just show it all when they ask.
        public async Task<PlayerData, properties> GetPlayerInfo(Socketmessage message, string gameID)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.User == message.Author.Id.ToString() && p.GameID == gameID);
            var playerData = player?.PlayerGameData;
            return playerData?.Properties;
        }

        public async Task RestJailTurns(SocketMessage message, string gameID)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.User == message.Author.Id.ToString() && p.GameID == gameID);
            if (player != null)
            {
                player.playerGameData.turnsInJail = 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> AddBuildings(Socketmessage message, string gameID, string propertyName, int amountsOfBuildings)
        {
            var player = await getPlayerInfo(message, gameID);
            var propertyCards = await loadPropertyCards();
            var propertyCard = propertyCards.FirstOrDefault(pc => pc.name.ToLower() == propertyName.ToLower());

            string colorGroup = propertyCard.color;
            int houseCout = propertyCard.houseCost;
            int hotelCost = propertyCard.hotelCost;

            if (!ownsAllColorGroup(colorGroup, playerData.properties ))
                throw new Exception(" You do not own all properties in the color group.");
                
            int buildingCost = houseCost * amountsOfBuildings;

            if ( !(playerData.balance >= buildingsCost) ) 
                throw new Exception (" You do not have enough moneyyyyy");
                    
            while (amountsOfBuildings > 0) {
                int minBuildings = playerData.properties.Where(p => p.color == colorGroup).Min(p => p.buildingAmount);

                List<Property> updatedProperties = new List<Property>();

                foreach (var property in playerData.properties)
                {

                    if (property.color == colorGroup && propety.amountsOfBuildings == minBuildings && --amountsOfBuildings > 0)
                    {
                         updatedProperties.Add(new property
                        {
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
                    }
                    else
                    {
                        updatedProperties.Add(proprety);
                    }
                }

                playerData.properties = updatedProperties;
                 playerData.balance = buildingCost;

                await _dbContext.SaveChangesAsync();
            }
                
            }
        }

        private bool OwnsAllColorGroup(string colorGroup, List<Property> properties)
        {
            int cardsInColorGroup;
            int ownedProps = playerProperties.Count(p => p.Color == color);

            // Return if the user has the required amount of cards
            return cardsInColorGroup == ownedProps;
        }
}