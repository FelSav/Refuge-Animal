using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class PaiementContext : DbContext
    {
        public PaiementContext(DbContextOptions<PaiementContext> option) : base(option)
        {

        }
        public DbSet<Paiement> Paiements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Paiement>().ToTable("paiement");
            modelBuilder.Entity<Paiement>().HasKey(p => p.paiement_id);
            modelBuilder.Entity<Paiement>().Property(p => p.proprietaire_id);
            modelBuilder.Entity<Paiement>().Property(p => p.municipalite_id);
            modelBuilder.Entity<Paiement>().Property(p => p.date_paiement);
            modelBuilder.Entity<Paiement>().Property(p => p.montant);
            modelBuilder.Entity<Paiement>().Property(p => p.mode_paiement);
            modelBuilder.Entity<Paiement>().Property(p => p.precision);
            modelBuilder.Entity<Paiement>().Property(p => p.paye_municipalité);
            modelBuilder.Entity<Paiement>().Property(p => p.commentaire);
            
        }
    }
}

