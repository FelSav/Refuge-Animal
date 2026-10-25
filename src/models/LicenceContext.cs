using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models

{
    public class LicenceContext : DbContext
    {
        public LicenceContext(DbContextOptions<LicenceContext> option) : base(option)
        {
            
        }

        public DbSet<Licence> Licences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Licence>().ToTable("licence");
            modelBuilder.Entity<Licence>().HasKey(l => l.licence_id);
            modelBuilder.Entity<Licence>().Property(l => l.proprietaire_id);
            modelBuilder.Entity<Licence>().Property(l => l.municipalite_id);
            modelBuilder.Entity<Licence>().Property(l => l.numero);
            modelBuilder.Entity<Licence>().Property(l => l.annee);
            modelBuilder.Entity<Licence>().Property(l => l.date_emission);
            modelBuilder.Entity<Licence>().Property(l => l.date_expiration);
            modelBuilder.Entity<Licence>().Property(l => l.nom_proprietaire);
            modelBuilder.Entity<Licence>().Property(l => l.prenom_proprietaire);
            modelBuilder.Entity<Licence>().Property(l => l.numero_civique);
            modelBuilder.Entity<Licence>().Property(l => l.appartement);
            modelBuilder.Entity<Licence>().Property(l => l.rue);
            modelBuilder.Entity<Licence>().Property(l => l.nom_municipalite);
            modelBuilder.Entity<Licence>().Property(l => l.code_postal);
            modelBuilder.Entity<Licence>().Property(l => l.telephone);
            modelBuilder.Entity<Licence>().Property(l => l.courriel);

        }
    }
}