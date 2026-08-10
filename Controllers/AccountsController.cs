using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Data;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (await _context.users
                .AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError(
                    "Username",
                    "Dieser Benutzername ist bereits vergeben.");
            }

            if (ModelState.IsValid)
            {
                _context.users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Benutzername oder Passwort ist falsch.");

                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Delete
        public async Task<IActionResult> Delete()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            _context.users.Remove(user);

            await _context.SaveChangesAsync();

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Register));
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var existingUser = await _context.users
                .FindAsync(userId.Value);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Prüfen, ob der neue Username bereits verwendet wird
            var usernameExists = await _context.users
                .AnyAsync(u =>
                    u.Username == user.Username &&
                    u.Id != userId.Value);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    "Username",
                    "Dieser Benutzername ist bereits vergeben.");
            }

            if (ModelState.IsValid)
            {
                existingUser.Username = user.Username;
                existingUser.Password = user.Password;

                await _context.SaveChangesAsync();

                HttpContext.Session.SetString(
                    "Username",
                    existingUser.Username);

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
    }
}