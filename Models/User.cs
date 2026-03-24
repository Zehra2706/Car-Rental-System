using System.ComponentModel.DataAnnotations;

namespace user.Models 
{
    public class User
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public string Name { get; set; } // Konum (Foreign Key)
        
        public string Surname { get; set; } // Hangi markaya ait? (Foreign Key)
        public enum Role { Customer, Admin }
        public Role UserRole { get; set; }
        public DateTime Date { get; set; }

    }
}