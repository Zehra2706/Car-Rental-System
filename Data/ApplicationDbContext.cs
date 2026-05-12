using Microsoft.EntityFrameworkCore;
using car.Models;
using carFeature.Models;
using licence.Models;
using price.Models;
using user.Models;
using userConnections.Models;
using userInfo.Models;
using rental.Models;
using System;

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
        public DbSet<car.Models.Role> Roles { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<UserConnections> UserConnections { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Licence> Licences { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<MailLog> MailLogs { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cascade hatasını engellemek için (Kritik kısım)
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Car)
                .WithMany(c => c.Rentals)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}