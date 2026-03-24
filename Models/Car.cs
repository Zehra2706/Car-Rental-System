using System.ComponentModel.DataAnnotations;

namespace car.Models 
{
    public class Car
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public string Location { get; set; } // Konum (Foreign Key)
        
        public string Brand { get; set; } // Hangi markaya ait? (Foreign Key)
        public int Color { get; set; } // Hangi renkte? (Foreign Key)
        public string ModelName { get; set; }
        public int ModelYear { get; set; }
        public string Description { get; set; }
        public bool IsInsured { get; set; }

    }
}