using MovieHouse.Data;

namespace MovieHouse.Models
{
    public class Film
    {
        public int Id { get; set; }
        public string? MovieId { get; set; } // IMDB ID (tt0043014 gibi)
        public string? Title { get; set; }
        public int Year { get; set; }
        public string? Runtime { get; set; } // "110 min" gibi
        public string? Genre { get; set; } // "Drama, Film-Noir" gibi
        public decimal Rating { get; set; } // IMDB puanı
        public string? Description { get; set; }
        public string? PosterUrl { get; set; } // Film posteri URL'i
        
        // Navigation Properties
        public int DirectorId { get; set; }
        public Director? Director { get; set; }
        public ICollection<FilmActor>? FilmActors { get; set; }
        public ICollection<FilmCategory>? FilmCategories { get; set; }
        public ICollection<UserRating>? UserRatings { get; set; }
        public ICollection<UserListFilm>? UserListFilms { get; set; }
    }
}
