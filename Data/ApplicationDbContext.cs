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
using userConnection.Models;
using user.Models;
using rental.Models; // <<-- BU SATIR ÇOK KRİTİK, EKSİKSE HATA VERİR

namespace car.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablolar buraya:
      
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
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }

    }
}