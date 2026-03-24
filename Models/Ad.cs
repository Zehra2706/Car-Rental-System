using System.ComponentModel.DataAnnotations;

namespace ad.Models 
{
    public class Ad
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public int CarId { get; set; } // (Foreign Key)
        public int UserId { get; set; } //  (Foreign Key)
        public string ConnectionNumber { get; set; }
        public bool IsAvailable { get; set; }

    }
}