using System.ComponentModel.DataAnnotations;

namespace user.Models
{
    public class User
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public required string Name { get; set; } // Konum (Foreign Key)

        public required string Surname { get; set; } // Hangi markaya ait? (Foreign Key)
        public enum Role { Customer, Admin }
        public required Role UserRole { get; set; }
        public required DateTime Date { get; set; }

    }
}