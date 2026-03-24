using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace userInfo.Models
{
    public class UserInfo
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; } // (Foreign Key)
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }


    }
}