using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;
using user.Models;
namespace rental.Models
{
    public class Rental
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public required DateTime ReturnDate { get; set; }
        public required DateTime Date { get; set; }
        public required DateTime RealReturnDate { get; set; }
        public int CarId { get; set; } // (Foreign Key)
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
        public int UserId { get; set; } // (Foreign Key)
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public required double Forecast { get; set; }
        public required int Deposit { get; set; }
        public required bool IsReturned { get; set; }

    }
}