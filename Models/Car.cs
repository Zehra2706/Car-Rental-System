using System.ComponentModel.DataAnnotations;

namespace car.Models 
{
    public class Car
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        
        public int BrandId { get; set; } // Hangi markaya ait? (Foreign Key)
        public int ColorId { get; set; } // Hangi renkte? (Foreign Key)
        
        public string ModelName { get; set; }
        public int ModelYear { get; set; }
        public decimal DailyPrice { get; set; } // Günlük kira ücreti
        public string Description { get; set; }
    }
}