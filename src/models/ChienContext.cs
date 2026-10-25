using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class ChienContext : DbContext
    {
        public ChienContext(DbContextOptions<ChienContext> option) : base(option)
        {

        }
        public DbSet<Chien> Chiens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chien>().ToTable("chien");
            modelBuilder.Entity<Chien>().HasKey(c => c.chien_id);
            modelBuilder.Entity<Chien>().Property(c => c.chien_id);
            modelBuilder.Entity<Chien>().Property(c => c.proprietaire_id);
            modelBuilder.Entity<Chien>().Property(c => c.nom);
            modelBuilder.Entity<Chien>().Property(c => c.race_id);
            modelBuilder.Entity<Chien>().Property(c => c.couleur_id);
            modelBuilder.Entity<Chien>().Property(c => c.sexe);
            modelBuilder.Entity<Chien>().Property(c => c.date_naissance);
            modelBuilder.Entity<Chien>().Property(c => c.poids_kg);
            modelBuilder.Entity<Chien>().Property(c => c.sterilise);
            modelBuilder.Entity<Chien>().Property(c => c.photo_path);
            modelBuilder.Entity<Chien>().Property(c => c.commentaire);
            modelBuilder.Entity<Chien>().Property(c => c.actif);
            modelBuilder.Entity<Chien>().Property(c => c.raison_inactivite);
            modelBuilder.Entity<Chien>().Property(c => c.date_inactivite);
            modelBuilder.Entity<Chien>().Property(c => c.created_at);
            modelBuilder.Entity<Chien>().Property(c => c.updated_at);


        }
    }
}