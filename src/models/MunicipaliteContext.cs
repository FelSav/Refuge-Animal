using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models
{
    public class MunicipaliteContext : DbContext
    {
        public MunicipaliteContext(DbContextOptions<MunicipaliteContext> option) : base(option)
        {

        }
        public DbSet<Municipalite> Municipalites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Municipalite>().ToTable("municipalite");
            modelBuilder.Entity<Municipalite>().HasKey(m => m.municipalite_id);
            modelBuilder.Entity<Municipalite>().Property(m => m.nom);
        }
    }
}

