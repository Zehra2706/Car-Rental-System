using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace price.Models
{
    public class Price
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int AracId { get; set; } // (Foreign Key)
        [ForeignKey("AracId")]
        public Car? Car { get; set; }


        public required double daily { get; set; } // Hangi markaya ait? (Foreign Key)

        public required double weekly { get; set; } // Hangi markaya ait? (Foreign Key)

        public required double monthly { get; set; } // Hangi markaya ait? (Foreign Key)


    }
}