using Microsoft.EntityFrameworkCore;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Chien> Chiens { get; set; }
        public DbSet<Licence> Licences { get; set; }
        public DbSet<LicenceChien> LicenceChiens { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public DbSet<Couleur> Couleur { get; set; }
        public DbSet<Municipalite> Municipalites { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Proprietaires> Proprietaires { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Rue> Rues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================
            // TABLES ET CLÉS PRIMAIRES
            // =====================================

            modelBuilder.Entity<Chien>()
                .ToTable("chien")
                .HasKey(c => c.chien_id);

            modelBuilder.Entity<Licence>()
                .ToTable("licence")
                .HasKey(l => l.licence_id);

            modelBuilder.Entity<LicenceChien>()
                .ToTable("licence_chien")
                .HasKey(lc => new { lc.licence_id, lc.chien_id });

            modelBuilder.Entity<Avis>()
                .ToTable("avis")
                .HasKey(a => a.avis_id);

            modelBuilder.Entity<Municipalite>()
                .ToTable("municipalite")
                .HasKey(m => m.municipalite_id);

            modelBuilder.Entity<Paiement>()
                .ToTable("paiement")
                .HasKey(p => p.paiement_id);

            modelBuilder.Entity<Couleur>()
               .ToTable("couleur")
               .HasKey(co => co.couleur_id);

            modelBuilder.Entity<Proprietaires>()
                .ToTable("proprietaire")
                .HasKey(p => p.proprietaire_id);

            modelBuilder.Entity<Race>()
               .ToTable("race")
               .HasKey(r => r.race_id);

            modelBuilder.Entity<Rue>()
                .ToTable("rue")
                .HasKey(r => r.rue_id);


            // =====================================
            // RELATIONS DE LA TABLE CHIEN
            // =====================================

            modelBuilder.Entity<Chien>()
                .HasOne(c => c.Proprietaire)
                .WithMany()
                .HasForeignKey(c => c.proprietaire_id);

            modelBuilder.Entity<Chien>()
                .HasOne(c => c.Race)
                .WithMany()
                .HasForeignKey(c => c.race_id);

            modelBuilder.Entity<Chien>()
                .HasOne(c => c.Couleur)
                .WithMany()
                .HasForeignKey(c => c.couleur_id);


            // =====================================
            // RELATIONS DE LA TABLE LICENCE
            // =====================================

            modelBuilder.Entity<Licence>()
                .HasOne<Proprietaires>()
                .WithMany()
                .HasForeignKey(l => l.proprietaire_id);

            modelBuilder.Entity<Licence>()
                .HasOne<Municipalite>()
                .WithMany()
                .HasForeignKey(l => l.municipalite_id);


            // =====================================
            // RELATIONS DE LA TABLE LICENCE_CHIEN
            // =====================================

            modelBuilder.Entity<LicenceChien>()
                .HasOne(lc => lc.Chien)
                .WithMany(c => c.LicenceChiens)
                .HasForeignKey(lc => lc.chien_id);

            modelBuilder.Entity<LicenceChien>()
                .HasOne(lc => lc.Licence)
                .WithMany()
                .HasForeignKey(lc => lc.licence_id);


            // =====================================
            // RELATIONS DE LA TABLE RUE
            // =====================================

            modelBuilder.Entity<Rue>()
                .HasOne(r => r.Municipalite)
                .WithMany()
                .HasForeignKey(r => r.municipalite_id);


            // =====================================
            // RELATIONS DE LA TABLE PROPRIETAIRE
            // =====================================

            modelBuilder.Entity<Proprietaires>()
                .HasOne<Rue>()
                .WithMany()
                .HasForeignKey(p => p.rue_id);

            // =====================================
            // RELATIONS DE LA TABLE PAIEMENT
            // =====================================

            modelBuilder.Entity<Paiement>()
                .HasOne<Proprietaires>()
                .WithMany()
                .HasForeignKey(p => p.proprietaire_id);

            modelBuilder.Entity<Paiement>()
                .HasOne<Municipalite>()
                .WithMany()
                .HasForeignKey(p => p.municipalite_id);

        }
    }
}