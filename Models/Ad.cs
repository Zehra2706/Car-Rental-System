using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using car.Models; // 1. BU ŞART (Foreign Key için)
using user.Models;
namespace ad.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int CarId { get; set; } // Bu sadece sayı (Veritabanındaki sütun)

        [ForeignKey("CarId")] // 2. "Yukarıdaki CarId, Car tablosuna gider" diyoruz.
        public Car? Car { get; set; } // Bu da o arabanın kendisi (Java'daki nesne referansı)

        public int UserId { get; set; } // Bu sadece sayı

        [ForeignKey("UserId")] // 3. "Bu sayı User tablosundaki anahtardır" diyoruz.
        public User? User { get; set; } // Bu da o kullanıcının kendisi

        public required string ConnectionNumber { get; set; }
        public bool IsAvailable { get; set; }
    }
}