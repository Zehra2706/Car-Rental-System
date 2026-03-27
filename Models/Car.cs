using System.ComponentModel.DataAnnotations;
using carFeature.Models;
using price.Models;

namespace car.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public required string Location { get; set; }
        public required string Brand { get; set; }
        public required string Color { get; set; }
        public required string ModelName { get; set; }
        public int ModelYear { get; set; }
        public required string Description { get; set; }
        public bool IsInsured { get; set; }
        public string? ImagePath { get; set; }
        public int UserId { get; set; }
        public virtual ICollection<CarFeature> CarFeatures { get; set; } = new List<CarFeature>();
        public virtual ICollection<Price> Prices { get; set; } = new List<Price>();
    }
}