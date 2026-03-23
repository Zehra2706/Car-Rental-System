using Microsoft.EntityFrameworkCore;
using car.Models; // <<-- BU SATIR ÇOK KRİTİK, EKSİKSE HATA VERİR

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
    }
}