using System.ComponentModel.DataAnnotations;

namespace MovieHouse.Models
{
    public class UserList
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public ICollection<UserListFilm>? UserListFilms { get; set; }
    }
} 