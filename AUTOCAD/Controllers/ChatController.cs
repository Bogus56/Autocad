using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AUTOCAD.DB;
using AUTOCAD.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AUTOCAD.Controllers
{
    [Authorize]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Pobieranie wiadomości dla danego użytkownika
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(string receiverId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var messages = await _context.Chats
                .Where(c => (c.SenderId == currentUser.Id && c.ReceiverId == receiverId) ||
                            (c.SenderId == receiverId && c.ReceiverId == currentUser.Id))
                .OrderBy(c => c.SentAt)
                .Select(c => new
                {
                    isSender = (c.SenderId == currentUser.Id), //  Łatwiejsze sprawdzenie, kto wysłał
                    message = c.Message,
                    sentAt = c.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }






        // Wysyłanie wiadomości
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Chat chatMessage)
        {
            if (chatMessage == null || string.IsNullOrWhiteSpace(chatMessage.Message))
            {
                return BadRequest("Nieprawidłowe dane wiadomości.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            chatMessage.SenderId = currentUser.Id;
            chatMessage.SentAt = DateTime.UtcNow;
            Console.WriteLine($"Zapisuje wiadomość: {chatMessage.Message}");

            _context.Chats.Add(chatMessage);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });

        }



        // Pobieranie użytkowników (oprócz siebie)
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var users = await _context.Users
                .Where(u => u.Id != currentUser.Id)
                .Select(u => new { u.Id, u.Imie, u.Nazwisko })
                .ToListAsync();

            return Json(users);
        }

    }
}
