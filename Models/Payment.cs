using System.ComponentModel.DataAnnotations;
using car.Models;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace payment.Models
{
    public class Payment
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
        public required double Amount { get; set; }
        public required bool IsPaid { get; set; }

    }
}