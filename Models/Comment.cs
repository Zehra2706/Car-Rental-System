using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using car.Models;
using user.Models;
using rental.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace comment.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
        public int RentId { get; set; }
        [ForeignKey("RentId")]
        public Rental? Rental { get; set; }
        public required string Content { get; set; }
        public required int Score { get; set; }

    }
}