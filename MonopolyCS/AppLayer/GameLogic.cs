using Discord.WebSocket;
using MonopolyCS.Configuration;
using MonopolyCS.DbLayer;
using MonopolyCS.Helpers;
using MonopolyCS.Models;
#pragma warning disable CS8600


namespace MonopolyCS.AppLayer
{
    public class GameLogic
    {
        // It's only purpose is to manage the connection to the database and providng the necessary methods
        private readonly PlayerDbContext _dbContext;
        private List<PropertyCard> _propertyCards;

        public GameLogic(PlayerDbContext dbContext, MonopolyCsConfigMgr cm)
        {
            _dbContext = dbContext;
            _propertyCards = PropertyClassHelper.LoadPropertyCards(cm.EnvironmentVariables.PropertyCards, cm).ToList();
        }

        public async Task CreatePlayer(SocketMessage message, string gameId)
        {

            PlayerData playerData = new()
            {
                UserId = message.Author.Id.ToString(),
                Guild = (message.Channel as SocketGuildChannel)?.Guild.Id
                    .ToString(), // tries to access the guild id from SocketGuildChannel, if it can't then it returns as null
                Properties = new List<PropertyCard>(),
                Balance = 1500,
                ChanceCard = new List<string>(),
                CommunityChest = new List<string>(),
                InJail = false,
                TurnsInJail = 0,
                SnakeEyeCount = 0,
                CurrentPos = 0
            };

            _dbContext.Players.Add(playerData);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SetPlayerToJail(string userId, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);

            if (player == null) return;
            player.InJail = true;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> RemovePlayerFromJail(string userId, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);

            if (player == null) return false;
            player.InJail = false;
            await _dbContext.SaveChangesAsync();
            return true;

        }

        public async Task<bool> AddPlayerProperty(string userId, string gameId, string propertyName)
        {
            PlayerData player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);
            if (player == null) return false;
            
            PropertyCard propertyCard = _propertyCards.FirstOrDefault(p => p.Name == propertyName);
            if (propertyCard == null) return false;
            
            player.Properties.Add(propertyCard);
            player.Properties.Sort((a, b) =>
                a.Pos.CompareTo(b
                    .Pos)); // This may not be added, but would like it be sorted by how it is on the map
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePlayerProperty(string userId, string gameId, string propertyName)
        {
            PlayerData player = _dbContext.Players.FirstOrDefault(p => p.UserId == userId && p.GameId == gameId);

            if (player == null) return false;
            player.Properties.RemoveAll(p => p.Name == propertyName);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // This and findOwner can be probably be turned into one method but too lazy at the moment
        // right now this just finds who ever owns a property if there is someone that does own it
        public async Task<PlayerData> FindPropertyOwner(string gameID, string propertyName)
        {
            return _dbContext.Players
                .FirstOrDefault(p => p.GameId == gameID && p.Properties.Any(property => property.Name == propertyName));
        }

        public async Task<PlayerData> MovePlayer(SocketMessage message, string gameId, int move)
        {
            PlayerData? player =
                _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameID == gameId);

            if (player == null) return player;
            int newPos = player.CurrentPos + move;
            player.CurrentPos = (newPos <= 40 && newPos > 0) ? newPos : (newPos < 0) ? newPos + 40 : newPos - 40;
            await _dbContext.SaveChangesAsync();

            return player;
        }

        //The fuck is this method supposed to do? Is it moving a player to a card location?
        public Task<(PlayerData player, int index)> FindOwner(string gameId, int movePosition)
        {
            foreach (PlayerData player in _dbContext.Players)
            {
                List<PropertyCard> ownedProperties = player.Properties;

                foreach (PropertyCard property in ownedProperties)
                {
                    int propertyIndex = _propertyCards.FindIndex(card => card.Name == property.Name);

                    if (propertyIndex != -1 && _propertyCards[propertyIndex].Pos == movePosition)
                        return Task.FromResult((player, propertyIndex));
                }
            }

            return Task.FromResult<(PlayerData player, int index)>((null, -1));
        }

        public async Task<PlayerData> AddSnakeEyeCount(SocketMessage message, string gameId)
        {
            PlayerData? player =
                _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameId == gameId);

            if (player == null)
                return player ?? new PlayerData(); //TODO: handle this logic better. Shouldn't return new Player()
            player.SnakeEyeCount++;
            // it should never even get above 3 but just in case
            if (player.SnakeEyeCount >= 3)
            {
                player.SnakeEyeCount = 0;
                await SetPlayerToJail(message.Author.Id.ToString(), gameId);
            }

            await _dbContext.SaveChangesAsync();
            return player;
        }

        public Task<int> GetSnakeEyeCount(SocketMessage message, string gameId)
        {
            PlayerData? player =
                _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameId == gameId);
            // if player is null return null, otherwise return count if that is null return 0
            return Task.FromResult(player?.SnakeEyeCount ?? 0);
        }

        public async Task<PlayerData> AddMoney(string playerId, string gameId, int balance)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);

            if (player == null)
                return player ?? new PlayerData(); //TODO: handle this logic better. Shouldn't return new Player()
            player.Balance += balance;
            await _dbContext.SaveChangesAsync();
            return player;
        }

        // TODO: add some logic if you happen to go negative and try to figure out what the best items are to mortage to stay in game and not go bankrupt
        public async Task<(PlayerData player, int newBalance)> RemoveMoney(string playerId, string gameId, int balance)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);

            if (player != null)
            {
                int newBalance = player.Balance - balance;
                if (newBalance < 0)
                {
                    return (player, newBalance);
                }

                player.Balance = newBalance;
                await _dbContext.SaveChangesAsync();
            }

            return (player, 0);
        }

        public Task<int> GetMoney(string playerId, string gameId)
        {
            PlayerData? player = _dbContext.Players.FirstOrDefault(p => p.UserId == playerId && p.GameId == gameId);
            return Task.FromResult(player?.Balance ?? 0);
        }

        // Originally thought of letting the user return specific data regarding their data IE property, mortgaged, buildings. We should just show it all when they ask.
        private Task<PlayerData> GetPlayerInfo(SocketMessage message, string gameId)
        {
            return Task.FromResult(_dbContext.Players.FirstOrDefault(p =>
                p.UserId == message.Author.Id.ToString() && p.GameID == gameId));
        }

        public async Task RestJailTurns(SocketMessage message, string gameId)
        {
            PlayerData? player =
                _dbContext.Players.FirstOrDefault(p => p.UserId == message.Author.Id.ToString() && p.GameID == gameId);
            if (player != null)
            {
                player.TurnsInJail = 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        //TODO: needs logic to handle max building amounts as well as the 'level' building rule (a property cannot have more than +/- 1 house difference)
        /*public async Task<bool> AddBuildings(SocketMessage message, string gameId, string propertyName, int buildingAmount)
        {
            PlayerData playerData = await GetPlayerInfo(message, gameId);
            PropertyCard? propertyCard = _propertyCards.FirstOrDefault(pc => string.Equals(pc.Name, propertyName, StringComparison.CurrentCultureIgnoreCase));

            string colorGroup = propertyCard.Color;
            int houseCost = propertyCard.HouseCost;
            int hotelCost = propertyCard.HotelCost;

            if (!OwnsAllColorGroup(colorGroup, playerData))
                throw new Exception(" You do not own all properties in the color group.");
                
            int buildingCost = houseCost * buildingAmount;  //TODO: needs to be conditional on how many houses are being built

            if ( !(playerData.Balance >= buildingCost) ) 
                throw new Exception (" You do not have enough money");
                    
            while (buildingAmount > 0) {
                int minBuildings = playerData.Properties.Where(p => p.Color == colorGroup).Min(p => p.BuildingAmount);

                List<PropertyCard> updatedProperties = new List<PropertyCard>();

                foreach (PropertyCard property in playerData.Properties)
                {

                    if (property.Color == colorGroup && property.buildingAmount == minBuildings && --buildingAmount > 0)
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
                            }
                        );
                    }
                    else
                    {
                        updatedProperties.Add(proprety);
                    }
                }

                PlayerData.properties = updatedProperties;
                PlayerData.balance = buildingCost;

                await _dbContext.SaveChangesAsync();
            }
        }*/

        private bool OwnsAllColorGroup(string colorGroup, PlayerData player)
        {
            int cardsInColorGroup = _propertyCards.Count(p => p.Color == colorGroup);
            int ownedProps = player.Properties.Count(p => p.Color == colorGroup);

            // Return if the user has the required amount of cards
            return cardsInColorGroup == ownedProps;
        }
    }
}