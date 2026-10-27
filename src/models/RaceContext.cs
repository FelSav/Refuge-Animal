using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class RaceContext : DbContext
    {
        public RaceContext(DbContextOptions<RaceContext> option) : base(option)
        {

        }
        public DbSet<Race> Races { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Race>().ToTable("race");
            modelBuilder.Entity<Race>().HasKey(r => r.race_id);
            modelBuilder.Entity<Race>().Property(r => r.nom);
        }
    }
}

