using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class RueContext : DbContext
    {
        public RueContext(DbContextOptions<RueContext> option) : base(option)
        {
        }
        public DbSet<Rue> Rues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rue>().ToTable("rue");
            modelBuilder.Entity<Rue>().HasKey(r => r.rue_id);
            modelBuilder.Entity<Rue>().Property(r => r.municipalite_id);
            modelBuilder.Entity<Rue>().Property(r => r.nom);
        }
    }
}