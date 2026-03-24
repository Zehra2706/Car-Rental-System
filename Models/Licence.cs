using System.ComponentModel.DataAnnotations;

namespace licence.Models 
{
    public class Licence
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }//  (Foreign Key)
        
        public string LicenceNumber { get; set; } 
        public DateTime Date { get; set; } // (Foreign Key)
        public int Score { get; set; }

    }
}