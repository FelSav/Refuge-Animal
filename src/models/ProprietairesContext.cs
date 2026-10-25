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
            modelBuilder.Entity<Proprietaires>().ToTable("proprietaire");
            modelBuilder.Entity<Proprietaires>().HasKey(p => p.proprietaire_id);
            modelBuilder.Entity<Proprietaires>().Property(p => p.numero_dossier);
            modelBuilder.Entity<Proprietaires>().Property(p => p.nom);
            modelBuilder.Entity<Proprietaires>().Property(p => p.prenom);
            modelBuilder.Entity<Proprietaires>().Property(p => p.numero_civique);
            modelBuilder.Entity<Proprietaires>().Property(p => p.appartement);
            modelBuilder.Entity<Proprietaires>().Property(p => p.rue_id);
            modelBuilder.Entity<Proprietaires>().Property(p => p.code_postal);
            modelBuilder.Entity<Proprietaires>().Property(p => p.telephone);
            modelBuilder.Entity<Proprietaires>().Property(p => p.courriel);
            modelBuilder.Entity<Proprietaires>().Property(p => p.date_naissance);
            modelBuilder.Entity<Proprietaires>().Property(p => p.est_chenil);
            modelBuilder.Entity<Proprietaires>().Property(p => p.commentaire);
            modelBuilder.Entity<Proprietaires>().Property(p => p.actif);
            modelBuilder.Entity<Proprietaires>().Property(p => p.created_at);
            modelBuilder.Entity<Proprietaires>().Property(p => p.updated_at);
            

        }
    }
}
