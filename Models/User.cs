using car.Models;
using licence.Models;
using System.ComponentModel.DataAnnotations;
using userConnections.Models;
using userInfo.Models;

namespace user.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required DateTime Date { get; set; }

        // HATALI KISIM BURAYDI - SADECE BU SATIRI BIRAK:

        public int? RoleId { get; set; }
        public virtual car.Models.Role? UserRole { get; set; }

        public UserInfo? UserInfo { get; set; }
        public UserConnections? UserConnections { get; set; }
        public Licence? Licence { get; set; }

        [Required]
        [StringLength(11)]
        public string TC { get; set; } = "00000000000";
        public virtual ICollection<rental.Models.Rental> Rentals { get; set; } = new List<rental.Models.Rental>();
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public string? EmailVerificationCode { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}