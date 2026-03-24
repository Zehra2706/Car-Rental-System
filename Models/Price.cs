using System.ComponentModel.DataAnnotations;

namespace price.Models 
{
    public class Price
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int AracId { get; set; } // Konum (Foreign Key)

        public double daily { get; set; } // Hangi markaya ait? (Foreign Key)

        public double weekly { get; set; } // Hangi markaya ait? (Foreign Key)

        public double monthly { get; set; } // Hangi markaya ait? (Foreign Key)


    }
}