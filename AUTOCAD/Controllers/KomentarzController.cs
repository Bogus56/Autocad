using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AUTOCAD.Models;
using AUTOCAD.DB;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AUTOCAD.Controllers
{
    [Authorize] // Wymagane logowanie
    public class KomentarzController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KomentarzController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

     
        public async Task<IActionResult> Index(long kursId)
        {
            var komentarze = await _context.Komentarze
                .Include(k => k.User)  // Pobranie danych użytkownika
                .Where(k => k.KursId == kursId)  // Filtrujemy po kursie
                .OrderByDescending(k => k.DataDodania)
                .ToListAsync();

            ViewBag.KursId = kursId;
            return View(komentarze);
        }



    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var komentarz = await _context.Komentarze.FindAsync(id);
            if (komentarz == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Administrator");

            if (user.Id != komentarz.UserId && !isAdmin)
            {
                return Forbid(); // Brak uprawnień
            }

            _context.Komentarze.Remove(komentarz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Kurs", new { id = komentarz.KursId });
        }



  
        [HttpGet]
        public IActionResult Create(long kursId)
        {
            ViewBag.KursId = kursId; 
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(long kursId, [Bind("Tresc")] Komentarz komentarz)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("❌ Błąd: Użytkownik niezalogowany!");
                return Unauthorized(); // Użytkownik musi być zalogowany
            }

           
            komentarz.KursId = kursId;
            komentarz.UserId = user.Id;
            komentarz.DataDodania = DateTime.Now;

            Console.WriteLine($"🔍 DEBUG: KursID={kursId}, UserID={user.Id}, Treść='{komentarz.Tresc}'");

         
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Kurs");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState nadal nie jest poprawny!");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Błąd walidacji: {error.ErrorMessage}");
                }
                ViewBag.KursId = kursId;
                return View(komentarz);
            }

            _context.Komentarze.Add(komentarz);
            await _context.SaveChangesAsync();

            Console.WriteLine($" KOMENTARZ DODANY! KursID: {kursId}, UserID: {user.Id}, Treść: {komentarz.Tresc}");

            return RedirectToAction("Details", "Kurs", new { id = kursId });
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var komentarz = await _context.Komentarze.FindAsync(id);
            if (komentarz == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Administrator");

            if (user.Id != komentarz.UserId && !isAdmin)
            {
                return Forbid(); // Brak uprawnień
            }

            return View(komentarz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KomentarzId,Tresc")] Komentarz komentarz)
        {
            if (id != komentarz.KomentarzId)
            {
                return BadRequest();
            }

            var existingKomentarz = await _context.Komentarze.FindAsync(id);
            if (existingKomentarz == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Administrator");

            if (user.Id != existingKomentarz.UserId && !isAdmin)
            {
                return Forbid();
            }

            existingKomentarz.Tresc = komentarz.Tresc;
            existingKomentarz.DataDodania = DateTime.Now; // Opcjonalnie aktualizacja daty

            _context.Komentarze.Update(existingKomentarz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Kurs", new { id = existingKomentarz.KursId });
        }

    }
}
