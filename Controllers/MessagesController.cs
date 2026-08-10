using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Data;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    public class MessagesController : Controller
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        // Benutzer auswählen
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.users
                .Where(u => u.Id != userId.Value)
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }

        // Chat mit einem bestimmten Benutzer
        public async Task<IActionResult> Chat(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var otherUser = await _context.users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (otherUser == null)
            {
                return NotFound();
            }

            var messages = await _context.messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m =>
                    (m.SenderId == userId.Value &&
                     m.ReceiverId == id)
                    ||
                    (m.SenderId == id &&
                     m.ReceiverId == userId.Value))
                .OrderBy(m => m.Date)
                .ToListAsync();

            ViewBag.OtherUser = otherUser;

            return View(messages);
        }

        // Nachricht senden
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
            int receiverId,
            string text)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return RedirectToAction(
                    nameof(Chat),
                    new { id = receiverId });
            }

            var receiver = await _context.users
                .FindAsync(receiverId);

            if (receiver == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                SenderId = userId.Value,
                ReceiverId = receiverId,
                Text = text,
                Date = DateTime.Now
            };

            _context.messages.Add(message);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Chat),
                new { id = receiverId });
        }
    }
}