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

        public async Task<bool> SetPlayerToJail(string userId, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);

            if (player != null)
            {
                player.PlayerGameData.InJail = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false; ;
        }

        public async Task<bool> RemovePlayerFromJail(string userID, string gameID)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == userID && p.GameId == gameID);

            if (player != null)
            {
                player.PlayerGameData.InJail = false;
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

            PlayerData player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);
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
            PlayerData player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameID);

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
            return _dbContext.Players
                .FirstOrDefault(p => p.GameId == gameID && p.PlayerGameData.Properties.Any(property => property.Name == propertyName));
        }

        public async Task<PlayerData> MovePlayer(SocketMessage message, string gameId, int move)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameID == gameId);

            if (player == null) return player;
            int newPos = player.PlayerGameData.CurrentPos + move;
            player.PlayerGameData.CurrentPos = (newPos <= 40 && newPos > 0) ? newPos : (newPos < 0) ? newPos + 40 : newPos - 40;
            await _dbContext.SaveChangesAsync();

            return player;
        }

        //The fuck is this method supposed to do? Is it moving a player to a card location?
        public async Task<(PlayerData player, int index)> FindOwner(string gameId, int movePosition)
        {
            List<PlayerGameData> gamePlayers = _dbContext.Players
                .Where(p => p.GameID == gameId)
                .Select(p => p.PlayerGameData)
                .ToList();

            foreach (PlayerGameData player in gamePlayers)
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

        public async Task<PlayerData> AddSnakeEyeCount(SocketMessage message, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameId == gameId);

            if (player == null) return player ?? new PlayerData(); //TODO: handle this logic better. Shouldn't return new Player()
            player.PlayerGameData.SnakeEyeCount++;
            // it should never even get above 3 but just in case
            if (player.PlayerGameData.SnakeEyeCount >= 3)
            {
                player.PlayerGameData.SnakeEyeCount = 0;
                await SetPlayerToJail(message.Author.Id.ToString(), gameId);
            }
            await _dbContext.SaveChangesAsync();
            return player;
        }

        public async Task<int> GetSnakeEyeCount(SocketMessage message, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameId == gameId);
            // if player is null return null, otherwise return count if that is null return 0
            return player?.PlayerGameData.SnakeEyeCount ?? 0;
        }

        public async Task<PlayerData> AddMoney(string playerId, string gameId, int balance)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);

            if (player == null) return player ?? new PlayerData(); //TODO: handle this logic better. Shouldn't return new Player()
            player.PlayerGameData.Balance += balance;
            await _dbContext.SaveChangesAsync();
            return player;
        }

        // TODO: add some logic if you happen to go negative and try to figure out what the best items are to mortage to stay in game and not go bankrupt
        public async Task<(PlayerData player, int newBalance)> RemoveMoney(string playerId, string gameId, int balance)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);

            if (player != null)
            {
                int newBalance = player.PlayerGameData.Balance - balance;
                if (newBalance < 0)
                {
                    return (player, newBalance);
                }

                player.PlayerGameData.Balance = newBalance;
                await _dbContext.SaveChangesAsync();
            }
            return (player, 0);
        }

        public async Task<int> GetMoney(string playerId, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);
            return player?.PlayerGameData.Balance ?? 0;
        }

        // Originally thought of letting the user return specific data regarding their data IE property, mortgaged, buildings. We should just show it all when they ask.
        public async Task<List<PropertyCard>> GetPlayerInfo(SocketMessage message, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameID == gameId);
            PlayerGameData? playerData = player?.PlayerGameData;
            return playerData?.Properties ?? new List<PropertyCard>(); //TODO: handle this return more betterer
        }

        public async Task RestJailTurns(SocketMessage message, string gameId)
        {
            var player = _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameID == gameId);
            if (player != null)
            {
                player.PlayerGameData.TurnsInJail = 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> AddBuildings(SocketMessage message, string gameId, string propertyName, int buildingAmount)
        {
            var player = await GetPlayerInfo(message, gameId);
            var propertyCards = LoadPropertyCards();
            var propertyCard = propertyCards.FirstOrDefault(pc => pc.name.ToLower() == propertyName.ToLower());

            string colorGroup = propertyCard.color;
            int houseCout = propertyCard.houseCost;
            int hotelCost = propertyCard.hotelCost;

            if (!ownsAllColorGroup(colorGroup, playerData.properties ))
                throw new Exception(" You do not own all properties in the color group.");
                
            int buildingCost = houseCost * buildingAmount;

            if ( !(playerData.balance >= buildingsCost) ) 
                throw new Exception (" You do not have enough moneyyyyy");
                    
            while (buildingAmount > 0) {
                int minBuildings = playerData.properties.Where(p => p.color == colorGroup).Min(p => p.buildingAmount);

                List<Property> updatedProperties = new List<Property>();

                foreach (var property in playerData.properties)
                {

                    if (property.color == colorGroup && propety.amountsOfBuildings == minBuildings && --buildingAmount > 0)
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