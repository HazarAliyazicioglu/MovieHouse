namespace MovieHouse.Models
{
    public class Director
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<Film>? Films { get; set; }
    }
}
