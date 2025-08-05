namespace MovieHouse.Models
{
    public class UserListFilm
    {
        public int Id { get; set; }
        
        public int UserListId { get; set; }
        public UserList? UserList { get; set; }
        
        public int FilmId { get; set; }
        public Film? Film { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        public string? Note { get; set; } // Kullanıcının film hakkında notu
    }
} 