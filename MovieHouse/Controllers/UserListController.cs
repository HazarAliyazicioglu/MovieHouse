using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHouse.Data;
using MovieHouse.Models;

namespace MovieHouse.Controllers
{
    public class UserListController : Controller
    {
        private readonly AppDbContext _context;

        public UserListController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UserList/GetUserLists
        public async Task<IActionResult> GetUserLists()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userLists = await _context.UserLists
                .Include(ul => ul.UserListFilms)
                .Where(ul => ul.UserId == int.Parse(userId))
                .OrderByDescending(ul => ul.UpdatedAt)
                .ToListAsync();

            return PartialView("_UserLists", userLists);
        }

        // GET: UserList/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: UserList/Create
        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(name))
            {
                ViewBag.Error = "Liste adı gereklidir.";
                return View();
            }

            var userList = new UserList
            {
                Name = name,
                Description = description,
                UserId = int.Parse(userId),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.UserLists.Add(userList);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Account");
        }

        // GET: UserList/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userList = await _context.UserLists
                .Include(ul => ul.UserListFilms)
                    .ThenInclude(ulf => ulf.Film)
                        .ThenInclude(f => f.Director)
                .Include(ul => ul.UserListFilms)
                    .ThenInclude(ulf => ulf.Film)
                        .ThenInclude(f => f.FilmActors)
                            .ThenInclude(fa => fa.Actor)
                .FirstOrDefaultAsync(ul => ul.Id == id && ul.UserId == int.Parse(userId));

            if (userList == null)
            {
                return NotFound();
            }

            return View(userList);
        }

        // POST: UserList/AddFilm
        [HttpPost]
        public async Task<IActionResult> AddFilm(int listId, int filmId, string note)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Liste kullanıcıya ait mi kontrol et
            var userList = await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.Id == listId && ul.UserId == int.Parse(userId));

            if (userList == null)
            {
                return NotFound();
            }

            // Film zaten listede var mı kontrol et
            var existingFilm = await _context.UserListFilms
                .FirstOrDefaultAsync(ulf => ulf.UserListId == listId && ulf.FilmId == filmId);

            if (existingFilm != null)
            {
                return Json(new { success = false, message = "Film zaten listede bulunuyor." });
            }

            var userListFilm = new UserListFilm
            {
                UserListId = listId,
                FilmId = filmId,
                Note = note,
                AddedAt = DateTime.Now
            };

            _context.UserListFilms.Add(userListFilm);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Film listeye eklendi." });
        }

        // POST: UserList/RemoveFilm
        [HttpPost]
        public async Task<IActionResult> RemoveFilm(int listId, int filmId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Liste kullanıcıya ait mi kontrol et
            var userList = await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.Id == listId && ul.UserId == int.Parse(userId));

            if (userList == null)
            {
                return NotFound();
            }

            var userListFilm = await _context.UserListFilms
                .FirstOrDefaultAsync(ulf => ulf.UserListId == listId && ulf.FilmId == filmId);

            if (userListFilm == null)
            {
                return NotFound();
            }

            _context.UserListFilms.Remove(userListFilm);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Film listeden çıkarıldı." });
        }

        // POST: UserList/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userList = await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.Id == id && ul.UserId == int.Parse(userId));

            if (userList == null)
            {
                return NotFound();
            }

            _context.UserLists.Remove(userList);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Account");
        }
    }
} 