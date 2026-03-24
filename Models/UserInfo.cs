using System.ComponentModel.DataAnnotations;

namespace userInfo.Models 
{
    public class UserInfo
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; } // (Foreign Key)
        
        public string Email { get; set; } 
        public string Password { get; set; }


    }
}