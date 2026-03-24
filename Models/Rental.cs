using System.ComponentModel.DataAnnotations;

namespace rental.Models 
{
    public class Rental
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public DateTime ReturnDate { get; set; } 
        public DateTime Date { get; set; } 
        public DateTime RealReturnDate { get; set; } 
        public int CarId { get; set; } // (Foreign Key)
        public int UserId { get; set; } // (Foreign Key)
        public double Forecast { get; set; }
        public int Deposit { get; set; }
        public bool IsReturned { get; set; }

    }
}