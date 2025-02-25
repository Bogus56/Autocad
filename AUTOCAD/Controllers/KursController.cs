using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using AUTOCAD.Models;
using AUTOCAD.DB;
using Microsoft.AspNetCore.Authorization;

namespace AUTOCAD.Controllers
{
    public class KursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KursController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult CreateKurs()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateKurs(Kurs kurs)
        {
            if (kurs.PhotoFile != null && kurs.PhotoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "photos");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, kurs.PhotoFile.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await kurs.PhotoFile.CopyToAsync(fileStream);
                }

                kurs.Photo = kurs.PhotoFile.FileName;
            }

            _context.Add(kurs);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        /* [Authorize(Roles = "Administrator")]
         [HttpPost]
         public async Task<IActionResult> DeleteKurs(long id)
         {
             var kurs = await _context.kurs.FindAsync(id);

             if (kurs == null)
             {
                 return NotFound();
             }

             _context.kurs.Remove(kurs);
             await _context.SaveChangesAsync();

             return RedirectToAction("Index");
         }*/
        //delete
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKurs(long id)
        {
            var kurs = await _context.kurs
                .Include(k => k.Quiz) // Pobieramy powiązane quizy
                    .ThenInclude(q => q.Questions) // Pobieramy powiązane pytania
                        .ThenInclude(q => q.Answers) // Pobieramy powiązane odpowiedzi
                .FirstOrDefaultAsync(k => k.KursId == id);

            if (kurs == null)
            {
                return NotFound();
            }

            // Usunięcie wszystkich powiązanych quizów, pytań i odpowiedzi
            foreach (var quiz in kurs.Quiz)
            {
                foreach (var question in quiz.Questions)
                {
                    _context.AnswerViewModel.RemoveRange(question.Answers); // Usuń odpowiedzi
                }
                _context.QuestionViewModel.RemoveRange(quiz.Questions); // Usuń pytania
            }
            _context.QuizViewModel.RemoveRange(kurs.Quiz); // Usuń quizy

            _context.kurs.Remove(kurs); // Usuń kurs

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Nie można usunąć kursu, ponieważ zawiera powiązane dane.");
            }

            return RedirectToAction("Index");
        }


        //edit
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> EditKurs(long id)
        {
            var kurs = await _context.kurs.FindAsync(id);

            if (kurs == null)
            {
                return NotFound();
            }

            var editViewModel = new Kurs
            {
                KursId = kurs.KursId,
                Nazwa = kurs.Nazwa,
                Opis = kurs.Opis,
                MovieID = kurs.MovieID
            };

            return View(editViewModel);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> EditKurs(Kurs model)
        {
            var kurs = await _context.kurs.FindAsync(model.KursId);

            if (kurs == null)
            {
                return NotFound();
            }

            kurs.Nazwa = model.Nazwa;
            kurs.Opis = model.Opis;
            kurs.MovieID = model.MovieID;

            _context.Update(kurs);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var kursy = await _context.kurs.ToListAsync();
            return View(kursy);
        }

        /*  [HttpGet]
          public async Task<IActionResult> Details(long id)
          {
              var kurs = await _context.kurs
                  .Include(k => k.Quiz) // Ensure related quizzes are loaded
                  .FirstOrDefaultAsync(k => k.KursId == id);

              if (kurs == null)
              {
                  return NotFound();
              }

              return View(kurs);
          }*/
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var kurs = await _context.kurs
              .Include(k => k.Quiz)
                .Include(k => k.Komentarze) // Pobranie komentarzy
                .ThenInclude(c => c.User)  // Pobranie danych użytkowników
                .FirstOrDefaultAsync(m => m.KursId == id);

            if (kurs == null)
            {
                return NotFound();
            }

            return View(kurs);
        }

    }

}


