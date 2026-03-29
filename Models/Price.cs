using System.ComponentModel.DataAnnotations;
using car.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace price.Models
{
    public class Price
    {
        [Key]
        public int Id { get; set; }
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }


        public required double daily { get; set; }

        public required double weekly { get; set; }

        public required double monthly { get; set; }


    }
}