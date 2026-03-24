using System.ComponentModel.DataAnnotations;

namespace comment.Models 
{
    public class Comment
    {
        [Key] // Bu birincil anahtar (Primary Key)
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public int RentId { get; set; }
        public string Content { get; set; } 
        public int Score { get; set; } 

    }
}