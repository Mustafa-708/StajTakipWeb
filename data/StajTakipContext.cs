
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StajTakipWeb.Models;

namespace StajTakipWeb.Data
{
    public class StajTakipContext
        : IdentityDbContext<ApplicationUser>
    {
        public StajTakipContext(
            DbContextOptions<StajTakipContext> options)
            : base(options)
        {
        }

        public DbSet<Ogrenci> Ogrenciler
            => Set<Ogrenci>();

        public DbSet<StajBilgisi> StajBilgileri => Set<StajBilgisi>();
        public DbSet<StajRaporu> StajRaporlari => Set<StajRaporu>();
        public DbSet<Odev> Odevler => Set<Odev>();
        public DbSet<Duyuru> Duyurular => Set<Duyuru>();
        public DbSet<DuyuruOkuma> DuyuruOkumalari => Set<DuyuruOkuma>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<DuyuruOkuma>().HasKey(x => new { x.DuyuruId, x.KullaniciId });
            builder.Entity<DuyuruOkuma>().Property(x => x.KullaniciId).HasMaxLength(450);
            builder.Entity<DuyuruOkuma>().HasOne<Duyuru>().WithMany().HasForeignKey(x => x.DuyuruId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<DuyuruOkuma>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationUser>().Property(x => x.HesapNotu).HasMaxLength(500);
            builder.Entity<Odev>().Ignore(x => x.Durum);
            builder.Entity<Odev>().HasOne(x => x.Ogrenci).WithMany().HasForeignKey(x => x.OgrenciID).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Odev>().Property(x => x.AtayanId).HasMaxLength(450);
            builder.Entity<Odev>().Property(x => x.TeslimMetni).HasMaxLength(10000);
            builder.Entity<Odev>().Property(x => x.TeslimBaglantisi).HasMaxLength(2000);
            builder.Entity<Odev>().Property(x => x.GeriBildirim).HasMaxLength(4000);
            builder.Entity<StajBilgisi>().HasIndex(x => x.OgrenciID).IsUnique();
            builder.Entity<StajBilgisi>().HasOne<Ogrenci>().WithMany()
                .HasForeignKey(x => x.OgrenciID).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<StajRaporu>().HasIndex(x => new { x.OgrenciID, x.Tarih }).IsUnique();
            builder.Entity<StajRaporu>().Property(x => x.Durum).HasMaxLength(30);
            builder.Entity<StajRaporu>().HasOne<Ogrenci>().WithMany()
                .HasForeignKey(x => x.OgrenciID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
