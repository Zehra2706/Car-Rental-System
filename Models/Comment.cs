using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using car.Models;
using user.Models;
using rental.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace comment.Models
{
    public class Comment
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")] // 3. "Bu sayı User tablosundaki anahtardır" diyoruz.
        public User? User { get; set; } // Bu da o kullanıcının kendisi

        public int CarId { get; set; }
        [ForeignKey("CarId")] // 2. "Yukarıdaki CarId, Car tablosuna gider" diyoruz.
        public Car? Car { get; set; } // Bu da o arabanın kendisi
        public int RentId { get; set; }
        [ForeignKey("RentId")] // 4. "Bu sayı Rent tablosundaki anahtardır" diyoruz.
        public Rental? Rental { get; set; } // Bu da o kiralamanın kendisi
        public required string Content { get; set; }
        public required int Score { get; set; }

    }
}