using Microsoft.EntityFrameworkCore;

namespace MonopolyCS.DbLayer
{
    public class PlayerDbContext : DbContext
    {
        public DbSet<PlayerData> Players { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./game_data/playerData.sqlite");
        }
    }
}