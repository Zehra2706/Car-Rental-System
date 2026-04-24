namespace car.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public int UserId { get; set; }

        public int Rating { get; set; } // 1-5 arası puan
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Car Car { get; set; }

    }
}