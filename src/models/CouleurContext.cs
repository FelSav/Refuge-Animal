using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class CouleurContext : DbContext
    {
        public CouleurContext(DbContextOptions<CouleurContext> option) : base(option)
        {

        }
        public DbSet<Couleur> Couleur { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Couleur>().ToTable("couleur");
            modelBuilder.Entity<Couleur>().HasKey(c => c.couleur_id);
            modelBuilder.Entity<Couleur>().Property(c => c.nom);
        }
    }
}