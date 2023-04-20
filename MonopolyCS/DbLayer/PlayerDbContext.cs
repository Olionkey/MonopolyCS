using Dapper;
using Microsoft.Data.Sqlite;
using MonopolyCS.Models;

namespace MonopolyCS.DbLayer
{
    public class PlayerDbContext
    {
        public DbSet<PlayerData> Players { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./game_data/playerData.sqlite");
        }
        
        public void Setup()
        {
            using var connection = new SqliteConnection("Data Source=./game_data/playerData.sqlite");
 
            var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Product';");
            var tableName = table.FirstOrDefault();
            if (!string.IsNullOrEmpty(tableName) && tableName == "Product")
                return;
 
            connection.Execute("Create Table Product (" +
                               "Name VARCHAR(100) NOT NULL," +
                               "Description VARCHAR(1000) NULL);");
        }
    }
}