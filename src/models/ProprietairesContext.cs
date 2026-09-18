using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class ProprietairesContext : DbContext
    {
        public ProprietairesContext(DbContextOptions<ProprietairesContext> option) : base(option)
        {

        }
        public DbSet<Proprietaires> Proprietaires { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proprietaires>().HasKey(p => p.Id);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Nom);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Prenom);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Num_Civique);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Appartement);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Rue_Id);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Municipalite_Id);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Code_Postal);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Tel);
            modelBuilder.Entity<Proprietaires>().Property(p => p.courriel);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Date_Naissance);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Est_Chenil);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Commentaire);
            modelBuilder.Entity<Proprietaires>().Property(p => p.actif);
            modelBuilder.Entity<Proprietaires>().Property(p => p.version);
            modelBuilder.Entity<Proprietaires>().Property(p => p.Creation);

        }
    }
}
