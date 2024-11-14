using Microsoft.EntityFrameworkCore;
using API_XML_XSLT.Models;

namespace API_XML_XSLT.Models
{
    public class TootajaDbContext : DbContext
    {
        public TootajaDbContext(DbContextOptions<TootajaDbContext> options) : base(options)
        {
        }
        public DbSet<Tootaja> Tootajad { get; set; }
        public DbSet<Igapaeva_andmed> IgapaevaAndmed { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tootaja>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nimi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Perenimi).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Salasyna).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Telefoni_number).HasMaxLength(20);
                entity.Property(e => e.Amet).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Igapaeva_andmed>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Tootaja_Id)
                      .WithMany()
                      .HasForeignKey("Tootaja_Id")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
