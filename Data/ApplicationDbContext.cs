using Microsoft.EntityFrameworkCore;
using car.Models;
using carFeature.Models;
using price.Models;
using licence.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;


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
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<UserConnections> UserConnections { get; set; }
        public DbSet<Licence> Licences { get; set; }

        public DbSet<rental.Models.Rental> Rentals { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İlişkiler
            modelBuilder.Entity<CarFeature>()
                .HasOne(cf => cf.Car)
                .WithMany(c => c.CarFeatures)
                .HasForeignKey(cf => cf.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Price>()
                .HasOne(p => p.Car)
                .WithMany(c => c.Prices)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade);

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