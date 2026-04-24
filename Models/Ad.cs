using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using car.Models;
using user.Models;
namespace ad.Models
{
    public class Ad
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public required string ConnectionNumber { get; set; }
        public bool IsAvailable { get; set; }
    }
}