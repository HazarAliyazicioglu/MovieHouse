using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHouse.Data;
using MovieHouse.Models;

namespace MovieHouse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int? categoryId = null)
        {
            // Sayfa başına 20 film
            int pageSize = 20;
            int skip = (page - 1) * pageSize;

            // Film sorgusu oluştur
            var filmsQuery = _context.Films
                .Include(f => f.Director)
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .AsQueryable();

            // Kategori filtresi uygula
            if (categoryId.HasValue)
            {
                filmsQuery = filmsQuery.Where(f => f.FilmCategories.Any(fc => fc.CategoryId == categoryId.Value));
            }

            // Toplam film sayısını al
            int totalFilms = await filmsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalFilms / pageSize);

            // Rastgele sıralama için GUID kullan
            var films = await filmsQuery
                .OrderBy(f => Guid.NewGuid()) // Rastgele sıralama
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Kategorileri al
            var categories = await _context.Categories
                .Include(c => c.FilmCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ViewModel oluştur
            var viewModel = new HomeViewModel
            {
                Films = films,
                Categories = categories,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedCategoryId = categoryId,
                TotalFilms = await _context.Films.CountAsync(),
                TotalDirectors = await _context.Directors.CountAsync(),
                TotalActors = await _context.Actors.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                PageSize = pageSize
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string query, int page = 1, int? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            // Sayfa başına 20 film
            int pageSize = 20;
            int skip = (page - 1) * pageSize;

            // Film sorgusu oluştur
            var filmsQuery = _context.Films
                .Include(f => f.Director)
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .AsQueryable();

            // Arama filtresi uygula
            var searchTerm = query.ToLower().Trim();
            filmsQuery = filmsQuery.Where(f =>
                f.Title.ToLower().Contains(searchTerm) ||
                (f.Director != null && f.Director.Name.ToLower().Contains(searchTerm)) ||
                (f.FilmActors != null && f.FilmActors.Any(fa => fa.Actor != null && fa.Actor.Name.ToLower().Contains(searchTerm))) ||
                (f.Genre != null && f.Genre.ToLower().Contains(searchTerm)) ||
                (f.Description != null && f.Description.ToLower().Contains(searchTerm))
            );

            // Kategori filtresi uygula
            if (categoryId.HasValue)
            {
                filmsQuery = filmsQuery.Where(f => f.FilmCategories.Any(fc => fc.CategoryId == categoryId.Value));
            }

            // Toplam film sayısını al
            int totalFilms = await filmsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalFilms / pageSize);

            // Arama sonuçlarını al
            var films = await filmsQuery
                .OrderByDescending(f => f.Rating) // Arama sonuçlarında puan sıralaması
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Kategorileri al
            var categories = await _context.Categories
                .Include(c => c.FilmCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ViewModel oluştur
            var viewModel = new HomeViewModel
            {
                Films = films,
                Categories = categories,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedCategoryId = categoryId,
                SearchQuery = query,
                TotalFilms = await _context.Films.CountAsync(),
                TotalDirectors = await _context.Directors.CountAsync(),
                TotalActors = await _context.Actors.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                PageSize = pageSize,
                IsSearch = true
            };

            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> LiveSearch(string query, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { success = false, message = "En az 2 karakter girin" });
            }

            try
            {
                var searchTerm = query.ToLower().Trim();

                // Film arama sorgusu
                var films = await _context.Films
                    .Include(f => f.Director)
                    .Include(f => f.FilmActors)
                        .ThenInclude(fa => fa.Actor)
                    .Where(f =>
                        f.Title.ToLower().Contains(searchTerm) ||
                        (f.Director != null && f.Director.Name.ToLower().Contains(searchTerm)) ||
                        (f.FilmActors != null && f.FilmActors.Any(fa => fa.Actor != null && fa.Actor.Name.ToLower().Contains(searchTerm))) ||
                        (f.Genre != null && f.Genre.ToLower().Contains(searchTerm))
                    )
                    .OrderByDescending(f => f.Rating)
                    .Take(limit)
                    .Select(f => new
                    {
                        id = f.Id,
                        title = f.Title,
                        year = f.Year,
                        rating = f.Rating,
                        posterUrl = f.PosterUrl,
                        director = f.Director != null ? f.Director.Name : "",
                        genre = f.Genre ?? "",
                        actors = f.FilmActors != null ? f.FilmActors.Take(3).Select(fa => fa.Actor != null ? fa.Actor.Name : "").ToList() : new List<string>()
                    })
                    .ToListAsync();

                return Json(new { success = true, films = films, count = films.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Live search error");
                return Json(new { success = false, message = "Arama sırasında hata oluştu" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class HomeViewModel
    {
        public List<Film> Films { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
        public bool IsSearch { get; set; }
        public int TotalFilms { get; set; }
        public int TotalDirectors { get; set; }
        public int TotalActors { get; set; }
        public int TotalCategories { get; set; }
        public int PageSize { get; set; }
    }
}
