using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace licence.Models
{
    public class Licence
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public required string LicenceNumber { get; set; }
        public DateTime Date { get; set; }
        public int Score { get; set; }

    }
}