using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using carFeature.Models;
using price.Models;
using rental.Models;
using user.Models;

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

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<CarFeature> CarFeatures { get; set; } = new List<CarFeature>();
        public ICollection<Price> Prices { get; set; } = new List<Price>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public string Plate { get; set; }
        
    }
}