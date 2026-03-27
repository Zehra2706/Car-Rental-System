using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace userConnections.Models
{
    public class UserConnections
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; } //  (Foreign Key)
        [ForeignKey("UserId")]
        // public User? User { get; set; }
        public required string Adress { get; set; }
        public required string Number { get; set; }


    }
}