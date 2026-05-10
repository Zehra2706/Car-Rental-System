using System.ComponentModel.DataAnnotations;

namespace car.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string RoleName { get; set; }
    }
}