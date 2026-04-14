using System.ComponentModel.DataAnnotations.Schema;
using car.Models;
using user.Models;
using System.ComponentModel.DataAnnotations;
namespace rental.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }
        public required DateTime Date { get; set; } // Başlangıç
        public required DateTime ReturnDate { get; set; } // Planlanan Bitiş

        public DateTime? RealReturnDate { get; set; }

        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public double Forecast { get; set; } // Toplam Tutar
        public int Deposit { get; set; }
        public bool IsReturned { get; set; } = false;
        public string Status { get; set; } = "OnayBekliyor";
    }
}