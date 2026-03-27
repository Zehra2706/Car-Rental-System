using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace userConnections.Models
{
    public class UserConnections
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required string Adress { get; set; }
        public required string Number { get; set; }


    }
}