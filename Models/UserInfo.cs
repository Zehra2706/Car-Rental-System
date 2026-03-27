using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace userInfo.Models
{
    public class UserInfo
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required string Email { get; set; }
        public required string Password { get; set; }

    }
}