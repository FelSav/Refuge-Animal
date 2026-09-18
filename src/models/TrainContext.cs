using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class TrainContext : DbContext
    {
        public TrainContext(DbContextOptions<TrainContext> option) : base(option)
        {

        }
        public DbSet<Train> Trains { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Train>().ToTable("train");
            modelBuilder.Entity<Train>().HasKey(t => t.id);
            modelBuilder.Entity<Train>().Property(t => t.model);
            modelBuilder.Entity<Train>().Property(t => t.couleur);
            modelBuilder.Entity<Train>().Property(t => t.enstock);
        }
    }
}

