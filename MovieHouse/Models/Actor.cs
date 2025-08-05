namespace MovieHouse.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<FilmActor>? FilmActors { get; set; }
    }
}
