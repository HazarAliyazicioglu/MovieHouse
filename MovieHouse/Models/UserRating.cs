namespace MovieHouse.Models
{
    public class UserRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int FilmId { get; set; }
        public Film? Film { get; set; }
        public int Rating { get; set; } // 1-10 arası puan
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
} 