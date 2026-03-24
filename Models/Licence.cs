using System.ComponentModel.DataAnnotations;
using user.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace licence.Models
{
    public class Licence
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }//  (Foreign Key)
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public required string LicenceNumber { get; set; }
        public required DateTime Date { get; set; } // (Foreign Key)
        public required int Score { get; set; }

    }
}