using car.Models;
using carFeature.Models;
using licence.Models;
using Microsoft.EntityFrameworkCore;
using price.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using car.Models;
using static user.Models.User;

namespace car.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<UserConnections> UserConnections { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Licence> Licences { get; set; }

        public DbSet<rental.Models.Rental> Rentals { get; set; }
        public DbSet<MailLog> MailLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CAR İLİŞKİLERİ ---
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Rentals)
                .WithOne(r => r.Car)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Car>()
                .HasMany(c => c.CarFeatures)
                .WithOne(f => f.Car)
                .HasForeignKey(f => f.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Car>()
                .HasMany(c => c.Prices)
                .WithOne(p => p.Car)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Car>()
                .HasMany(c => c.Reviews)
                .WithOne(p => p.Car)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- RENTAL VE USER ARASINDAKİ DÖNGÜYÜ (CASCADE PATH) KIRAN YENİ KURAL ---
            modelBuilder.Entity<rental.Models.Rental>()
                .HasOne(r => r.User)
                .WithMany() // Eğer User sınıfı içinde kiralama listesi varsa r => r.User.Rentals yazabilirsin, yoksa böyle boş kalabilir.
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Döngüsel silme hatasını çözen sihirli satır 👈

            // --- TOHUM (SEED) VERİLERİ ---
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "deneme", Surname = "deneme", UserRole = User.Role.Customer, Date = new DateTime(2024, 6, 25) }
            );

            modelBuilder.Entity<UserInfo>().HasData(
                new UserInfo { Id = 1, UserId = 1, Email = "deneme@deneme.com", Password = "deneme123" }
            );

            modelBuilder.Entity<UserConnections>().HasData(
                new UserConnections { Id = 1, UserId = 1, Number = "1234567890", Adress = "Deneme Adres" }
            );

            modelBuilder.Entity<Licence>().HasData(
                new Licence { Id = 1, UserId = 1, LicenceNumber = "L123456789", Date = new DateTime(2024, 6, 25), Score = 100 }
            );
        }
    }
}