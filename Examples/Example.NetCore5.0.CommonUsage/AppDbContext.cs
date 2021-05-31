using Microsoft.EntityFrameworkCore;
using Example.NetCore5._0.CommonUsage.Models;

namespace Example.NetCore5._0.CommonUsage
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Vendor> Vendors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            modelBuilder.Entity<Product>().HasOne<Vendor>().WithMany().HasForeignKey(x => x.VendorId);
        }
    }
}