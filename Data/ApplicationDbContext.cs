using car.Models;
using carFeature.Models;
using licence.Models;
using Microsoft.EntityFrameworkCore;
using price.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
// Diğer usingler...

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

        // DİKKAT: Burada 'Roles' yerine sınıf adın olan 'Role' kullanmalısın
        public DbSet<car.Models.Role> Roles { get; set; }

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

            // --- İLİŞKİ TANIMLARI ---
            // (Buradaki Car ve Rental ilişkilerini olduğu gibi bırakabilirsin)

            // --- TOHUM (SEED) VERİLERİ ---

            // 1. Önce Rolü Oluştur
            modelBuilder.Entity<car.Models.Role>().HasData(
                new car.Models.Role { Id = 1, RoleName = "User", UserId = 1 }
            );

            // 2. Kullanıcıyı Oluştur (UserRole eklemeden!)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "deneme",
                    Surname = "deneme",
                    Date = new DateTime(2024, 6, 25)
                }
            );

            // NOT: Aşağıdaki hatalı "TOHUM VERİLERİ" kısmını SİLDİM. 
            // Çünkü 'new car.Models.Role' şeklinde iç içe veri eklenemez.

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