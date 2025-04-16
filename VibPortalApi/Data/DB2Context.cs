using Microsoft.EntityFrameworkCore;
using VibPortalApi.Models.DB2Models;

namespace VibPortalApi.Data
{
    public class DB2Context : DbContext
    {
        public DB2Context(DbContextOptions<DB2Context> options)
            : base(options)
        {
        }

        // Define your entity sets (tables) for the DB2 database
        public DbSet<EuravibImport> EuravibImport { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map the entity to a specific schema if needed.
            // For example, mapping MyEntity to the DB2ADMIN schema:
            modelBuilder.Entity<EuravibImport>()
                .ToTable("Euravib_Import_Test", "Euravib")
                .HasKey(e => new { e.Id });




            base.OnModelCreating(modelBuilder);
        }
    }
}
