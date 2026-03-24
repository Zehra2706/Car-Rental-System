using System.ComponentModel.DataAnnotations;

namespace car.Models
{
    public class Car
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public required string Location { get; set; } // Konum (Foreign Key)
        public required string Brand { get; set; } // Hangi markaya ait? (Foreign Key)
        public required string Color { get; set; } // Hangi renkte? (Foreign Key)
        public required string ModelName { get; set; }
        public int ModelYear { get; set; }
        public required string Description { get; set; }
        public bool IsInsured { get; set; }

    }
}