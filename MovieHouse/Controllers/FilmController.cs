using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHouse.Data;
using MovieHouse.Models;

namespace MovieHouse.Controllers
{
    public class FilmController : Controller
    {
        private readonly AppDbContext _context;

        public FilmController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var film = await _context.Films
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .Include(f => f.Director)
                .Include(f => f.UserRatings)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (film == null)
            {
                return NotFound();
            }

            // Kullanıcının bu film için verdiği puanı al
            var userId = HttpContext.Session.GetString("UserId");
            var userRating = 0;
            
            if (!string.IsNullOrEmpty(userId))
            {
                var rating = await _context.UserRatings
                    .FirstOrDefaultAsync(ur => ur.UserId == int.Parse(userId) && ur.FilmId == id);
                
                if (rating != null)
                {
                    userRating = rating.Rating;
                }
            }

            // Film detayları için view model oluştur
            var viewModel = new FilmDetailsViewModel
            {
                Film = film,
                UserRating = userRating,
                IsLoggedIn = !string.IsNullOrEmpty(userId)
            };

            return View(viewModel);
        }
    }

    public class FilmDetailsViewModel
    {
        public Film Film { get; set; }
        public int UserRating { get; set; }
        public bool IsLoggedIn { get; set; }
    }
} 