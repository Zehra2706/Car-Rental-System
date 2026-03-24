using System.ComponentModel.DataAnnotations;

namespace userConnection.Models 
{
    public class UserConnection
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; } //  (Foreign Key)
        
        public string Adress { get; set; } 
        public string Number { get; set; } 


    }
}