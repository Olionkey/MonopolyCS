using PlayerData;

namespace PlayerObContext
{
    public class PlyaerObContext : DbContext
    {
        public DbSet<PlayerData> Players { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilds.UseSqlite("Data Source=./game_data/playerData.sqlite");
        }
    }
}