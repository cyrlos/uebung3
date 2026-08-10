using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Data;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    public class PostsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostsController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Posts
        public async Task<IActionResult> Index()
        {
            var posts = await _context.posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return View(posts);
        }

        // GET: /Posts/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var post = await _context.posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var existingLike = await _context.likes
                .FirstOrDefaultAsync(l =>
                    l.PostId == id &&
                    l.UserId == userId.Value);

            if (existingLike == null)
            {
                var like = new Like
                {
                    PostId = id,
                    UserId = userId.Value
                };

                _context.likes.Add(like);
            }
            else
            {
                _context.likes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Post post,
            IFormFile? image)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(post.Text))
            {
                ModelState.AddModelError(
                    "Text",
                    "Der Beitrag darf nicht leer sein.");
            }

            if (ModelState.IsValid)
            {
                post.UserId = userId.Value;
                post.Date = DateTime.Now;

                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        _environment.WebRootPath,
                        "uploads");

                    Directory.CreateDirectory(uploadsFolder);

                    var fileName =
                        Guid.NewGuid().ToString()
                        + Path.GetExtension(image.FileName);

                    var filePath = Path.Combine(
                        uploadsFolder,
                        fileName);

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    post.ImagePath = "/uploads/" + fileName;
                }

                _context.posts.Add(post);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }
    }
}