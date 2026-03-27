using Microsoft.EntityFrameworkCore;
using car.Models;
using ad.Models;
using carFeature.Models;
using comment.Models;
using deposit.Models;
using licence.Models;
using payment.Models;
using price.Models;
using userInfo.Models;
using userConnections.Models;
using user.Models;
using rental.Models;


namespace car.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Licence> Licences { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Price> Prices { get; set; }

        public DbSet<Rental> Rentals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserConnections> UserConnections { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "deneme",
                Surname = "deneme",
                UserRole = User.Role.Customer,
                Date = new DateTime(2024, 6, 25)
            }
    );
            modelBuilder.Entity<UserInfo>().HasData(
                new UserInfo
                {
                    Id = 1,
                    UserId = 1,
                    Email = "deneme@deneme.com",
                    Password = "deneme123"
                }
            );
            modelBuilder.Entity<UserConnections>().HasData(
                new UserConnections
                {
                    Id = 1,
                    UserId = 1,
                    Number = "1234567890",
                    Adress = "Deneme Adres"
                }
            );
            modelBuilder.Entity<Licence>().HasData(
                new Licence
                {
                    Id = 1,
                    UserId = 1,
                    LicenceNumber = "L123456789",
                    Date = new DateTime(2024, 6, 25),
                    Score = 100
                }
            );

            // modelBuilder.Entity<UserInfo>().HasData(
            //     new UserInfo { Id = 1, UserId = 1, Email = "deneme@deneme.com", Password = "deneme123" }
            // );

            // modelBuilder.Entity<UserConnections>().HasData(
            //     new UserConnections { Id = 1, UserId = 1, Number = "1234567890", Adress = "Deneme Adres" }
            // );
        }
    }
}