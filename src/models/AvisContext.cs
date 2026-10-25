using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class AvisContext : DbContext
    {
        public AvisContext(DbContextOptions<AvisContext> option) : base(option)
        {

        }
        public DbSet<Avis> Avis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Avis>().ToTable("avis");
            modelBuilder.Entity<Avis>().HasKey(a => a.avis_id);
            modelBuilder.Entity<Avis>().Property(a => a.code);
            modelBuilder.Entity<Avis>().Property(a => a.titre);
            modelBuilder.Entity<Avis>().Property(a => a.sous_titre);
            modelBuilder.Entity<Avis>().Property(a => a.text);
            modelBuilder.Entity<Avis>().Property(a => a.lieu);

        }
    }
}