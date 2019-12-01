using Microsoft.EntityFrameworkCore;

namespace Scrabble.Data.Models
{
    public class ScrabbleContext : DbContext
    {
        public ScrabbleContext(DbContextOptions<ScrabbleContext> contextOptions) : base(contextOptions)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GamePlayer>()
                .HasKey(t => new { t.GameId, t.PlayerId });

            modelBuilder.Entity<PlayerRound>()
                .HasKey(t => new { t.RoundId, t.PlayerId });
        }
    }
}
