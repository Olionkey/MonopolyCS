using System.Data.SQLite;
using System.Linq;
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
            if (File.Exists("MonopolyDb.sqlite")) return;
            SQLiteConnection.CreateFile("MonopolyDb.sqlite");
            using SqliteConnection connection = new ("Data Source=MonopolyDb.sqlite;Version=3;");
            
        }
    }
}