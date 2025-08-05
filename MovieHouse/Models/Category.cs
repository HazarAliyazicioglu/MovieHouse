namespace MovieHouse.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<FilmCategory>? FilmCategories { get; set; }
    }
} 