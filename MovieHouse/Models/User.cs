namespace MovieHouse.Models
{
    public class User
    {  
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public ICollection<UserList>? UserLists { get; set; }
        public ICollection<UserRating>? UserRatings { get; set; }
    }
}
