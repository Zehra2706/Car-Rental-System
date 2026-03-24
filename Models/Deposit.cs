using System.ComponentModel.DataAnnotations;

namespace deposit.Models 
{
    public class Deposit
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int CarId{ get; set; } //  (Foreign Key)

        public int UserId { get; set; } // (Foreign Key)

        public double Amount { get; set; }
        public bool IsPaid { get; set; }

    }
}