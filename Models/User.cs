using System.ComponentModel.DataAnnotations;
using userInfo.Models;
using userConnections.Models;
using licence.Models;
namespace user.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }

        public required string Surname { get; set; }
        public enum Role { Customer, Admin }
        public required Role UserRole { get; set; }
        public required DateTime Date { get; set; }

        public UserInfo? UserInfo { get; set; }
        public UserConnections? UserConnections { get; set; }

        public Licence? Licence { get; set; }




    }
}