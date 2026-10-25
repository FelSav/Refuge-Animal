using Microsoft.EntityFrameworkCore;
namespace ApiMuniChien_V1_Proprietaire.models

{
    public class LicenceChienContext : DbContext
    {
        public LicenceChienContext(DbContextOptions<LicenceChienContext> option) : base(option)
        {
            
        }

        public DbSet<LicenceChien> LicenceChiens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LicenceChien>().ToTable("licence_chien", "public");
            modelBuilder.Entity<LicenceChien>().HasKey(lc => new { lc.licence_id, lc.chien_id });
            modelBuilder.Entity<LicenceChien>().Property(lc => lc.licence_id);
            modelBuilder.Entity<LicenceChien>().Property(lc => lc.chien_id);
           
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default){
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}