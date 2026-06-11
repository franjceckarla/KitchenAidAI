using KitchenAidAI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace KitchenAidAI.Data
{
    public class KitchenAidDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public KitchenAidDbContext(DbContextOptions<KitchenAidDbContext> options)
            : base(options)
        {
        }

        // Keep legacy Users table for compatibility during migration
        public new DbSet<User> Users => Set<User>();
        // Identity provides `Users` via IdentityDbContext for `AppUser` (accessible as Set<AppUser>())
        public DbSet<Frizider> Frizideri => Set<Frizider>();
        public DbSet<Namirnica> Namirnice => Set<Namirnica>();
        public DbSet<Kuharica> Kuharice => Set<Kuharica>();
        public DbSet<Recept> Recepti => Set<Recept>();
        public DbSet<KorakRecepta> KoraciRecepta => Set<KorakRecepta>();
        public DbSet<ReceptKuharica> ReceptKuharice => Set<ReceptKuharica>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Datoteka> Datoteke => Set<Datoteka>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.username).HasMaxLength(100);
                entity.Property(e => e.ime).HasMaxLength(120);
                entity.Property(e => e.prezime).HasMaxLength(120);
                entity.Property(e => e.datumRodenja).HasColumnType("date");
                entity.Property(e => e.zemlja).HasMaxLength(120);
                entity.Property(e => e.email).HasMaxLength(200);
                entity.Property(e => e.passwordHash).HasMaxLength(2000);
                entity.Property(e => e.authProvider).HasMaxLength(100);
                entity.Property(e => e.authProviderKey).HasMaxLength(200);

                entity.HasOne(e => e.frizider)
                    .WithOne(e => e.user)
                    .HasForeignKey<Frizider>(e => e.userId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.kuharica)
                    .WithOne(e => e.user)
                    .HasForeignKey<Kuharica>(e => e.userId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.chatPoruke)
                    .WithOne(e => e.user)
                    .HasForeignKey(e => e.userId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.datoteke)
                    .WithOne(e => e.user)
                    .HasForeignKey(e => e.userId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Frizider>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.HasMany(e => e.namirnice)
                    .WithOne(e => e.frizider)
                    .HasForeignKey(e => e.friziderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Namirnica>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.naziv).HasMaxLength(200);
                entity.OwnsOne(e => e.nutritivnaVrijednost);
            });

            modelBuilder.Entity<Kuharica>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.naziv).HasMaxLength(200);
                entity.HasMany(e => e.receptKuharice)
                    .WithOne(e => e.kuharica)
                    .HasForeignKey(e => e.kuharicaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Recept>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.naziv).HasMaxLength(200);
                entity.Property(e => e.opis).HasMaxLength(1000);
                entity.HasMany(e => e.koraci)
                    .WithOne(e => e.recept)
                    .HasForeignKey(e => e.receptId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.receptKuharice)
                    .WithOne(e => e.recept)
                    .HasForeignKey(e => e.receptId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<KorakRecepta>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.opis).HasMaxLength(500);
            });

            modelBuilder.Entity<ReceptKuharica>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.HasIndex(e => new { e.receptId, e.kuharicaId }).IsUnique();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.message).HasMaxLength(2000);
                entity.Property(e => e.response).HasMaxLength(2000);
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.naziv).HasMaxLength(120).IsRequired();
                entity.HasIndex(e => e.naziv).IsUnique();
            });

            modelBuilder.Entity<Datoteka>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.naziv).HasMaxLength(260);
                entity.Property(e => e.opis).HasMaxLength(1000);
                entity.Property(e => e.contentType).HasMaxLength(255);
                entity.Property(e => e.putanja).HasMaxLength(500);
                entity.HasIndex(e => new { e.userId, e.isDeleted, e.kreirano });
            });
        }
    }
}