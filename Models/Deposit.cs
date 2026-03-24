using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;
using user.Models;
namespace deposit.Models
{
    public class Deposit
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int CarId { get; set; } //  (Foreign Key),
        [ForeignKey("CarId")]
        public Car? Car { get; set; } // Bu da o arabanın kendisi (Java'daki nesne referansı)
        public int UserId { get; set; } // (Foreign Key)
        [ForeignKey("UserId")]
        public User? User { get; set; } // Bu da o kullanıcının kendisi (Java'daki nesne referansı)
        public required double Amount { get; set; }
        public required bool IsPaid { get; set; }

    }
}