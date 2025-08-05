using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHouse.Data;
using MovieHouse.Models;

namespace MovieHouse.Controllers
{
    public class RatingController : Controller
    {
        private readonly AppDbContext _context;

        public RatingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RateFilm(int filmId, int rating)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor" });
            }

            if (rating < 1 || rating > 10)
            {
                return Json(new { success = false, message = "Puan 1-10 arasında olmalıdır" });
            }

            try
            {
                // Mevcut puanı kontrol et
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(ur => ur.UserId == userIdInt && ur.FilmId == filmId);

                if (existingRating != null)
                {
                    // Puanı güncelle
                    existingRating.Rating = rating;
                    existingRating.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Yeni puan ekle
                    var newRating = new UserRating
                    {
                        UserId = userIdInt,
                        FilmId = filmId,
                        Rating = rating,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserRatings.Add(newRating);
                }

                await _context.SaveChangesAsync();

                // Otomatik olarak "İzlediklerim" listesine ekle
                await AddToWatchedList(userIdInt, filmId);

                return Json(new { success = true, message = "Film başarıyla puanlandı ve izlediklerim listesine eklendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRating(int filmId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, rating = 0 });
            }

            var rating = await _context.UserRatings
                .Where(ur => ur.UserId == userIdInt && ur.FilmId == filmId)
                .Select(ur => ur.Rating)
                .FirstOrDefaultAsync();

            return Json(new { success = true, rating = rating });
        }

        private async Task AddToWatchedList(int userId, int filmId)
        {
            // "İzlediklerim" listesini bul veya oluştur
            var watchedList = await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.Name == "İzlediklerim");

            if (watchedList == null)
            {
                watchedList = new UserList
                {
                    UserId = userId,
                    Name = "İzlediklerim",
                    Description = "İzlediğim filmler",
                    CreatedAt = DateTime.Now
                };
                _context.UserLists.Add(watchedList);
                await _context.SaveChangesAsync();
            }

            // Film zaten listede mi kontrol et
            var existingFilm = await _context.UserListFilms
                .FirstOrDefaultAsync(ulf => ulf.UserListId == watchedList.Id && ulf.FilmId == filmId);

            if (existingFilm == null)
            {
                // Filmi listeye ekle
                var userListFilm = new UserListFilm
                {
                    UserListId = watchedList.Id,
                    FilmId = filmId,
                    AddedAt = DateTime.Now
                };
                _context.UserListFilms.Add(userListFilm);
                await _context.SaveChangesAsync();
            }

            // "İzleyeceklerim" listesinden çıkar (eğer varsa)
            var watchlist = await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.Name == "İzleyeceklerim");

            if (watchlist != null)
            {
                var watchlistFilm = await _context.UserListFilms
                    .FirstOrDefaultAsync(ulf => ulf.UserListId == watchlist.Id && ulf.FilmId == filmId);

                if (watchlistFilm != null)
                {
                    _context.UserListFilms.Remove(watchlistFilm);
                    await _context.SaveChangesAsync();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToWatchlist(int filmId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor" });
            }

            try
            {
                // "İzleyeceklerim" listesini bul veya oluştur
                var watchlist = await _context.UserLists
                    .FirstOrDefaultAsync(ul => ul.UserId == userIdInt && ul.Name == "İzleyeceklerim");

                if (watchlist == null)
                {
                    watchlist = new UserList
                    {
                        UserId = userIdInt,
                        Name = "İzleyeceklerim",
                        Description = "İzlemek istediğim filmler",
                        CreatedAt = DateTime.Now
                    };
                    _context.UserLists.Add(watchlist);
                    await _context.SaveChangesAsync();
                }

                // Film zaten listede mi kontrol et
                var existingFilm = await _context.UserListFilms
                    .FirstOrDefaultAsync(ulf => ulf.UserListId == watchlist.Id && ulf.FilmId == filmId);

                if (existingFilm == null)
                {
                    // Filmi listeye ekle
                    var userListFilm = new UserListFilm
                    {
                        UserListId = watchlist.Id,
                        FilmId = filmId,
                        AddedAt = DateTime.Now
                    };
                    _context.UserListFilms.Add(userListFilm);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Film izleyeceklerim listesine eklendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 