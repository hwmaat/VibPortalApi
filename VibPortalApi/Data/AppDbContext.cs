using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using VibPortalApi.Models.B2B;
using VibPortalApi.Models.Vib;

namespace VibPortalApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<VibImport> VibImport { get; set; } = null!;
        public DbSet<B2BSupplierOc> B2BSupplierOcs { get; set; } = null!;
        public DbSet<B2BSupplierOcLine> B2BSupplierOcLines { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<VibImport>()
                .ToTable("VibImport")
                .Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);


        }
    }
}
