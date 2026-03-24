using System.ComponentModel.DataAnnotations;

namespace payment.Models 
{
    public class Payment
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public double Amount { get; set; }
        public bool IsPaid { get; set; }

    }
}