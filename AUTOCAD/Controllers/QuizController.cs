using AUTOCAD.DB;
using AUTOCAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Geom;
using Org.BouncyCastle.Utilities;
using static iText.IO.Util.IntHashtable;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Org.BouncyCastle.Utilities.Test.FixedSecureRandom;
using System.Runtime.ConstrainedExecution;


namespace AUTOCAD.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context; // Your database context
        private readonly UserManager<ApplicationUser> _userManager;  // Dodaj pole do zarządzania użytkownikami

        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var quiz = await _context.QuizViewModel.ToListAsync();
            return View(quiz);

        }


        /*   [HttpGet]
           public async Task<IActionResult> QuizDetails(int id)
           {
               var quiz = await _context.QuizViewModel.Include(q => q.Questions).ThenInclude(q => q.Answers).FirstOrDefaultAsync(q => q.Id == id);

               if (quiz == null)
               {
                   return View("BrakQuizu");
               }

               return View(quiz);
           }*/

        [HttpGet]
        public async Task<IActionResult> QuizDetails(int id)
        {
            var quiz = await _context.QuizViewModel
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return View("BrakQuizu");
            }

            return View(quiz);
        }

        //zapis wyniku
        /* [HttpPost]
         public async Task<IActionResult> SubmitQuiz(QuizResultViewModel quizResult)
         {
             try
             {
                 // Sprawdź, czy użytkownik jest zalogowany
                 ApplicationUser currentUser = await _userManager.GetUserAsync(User);

                 if (currentUser == null)
                 {
                     Console.WriteLine("Unauthorized user attempted to submit a quiz.");
                     return RedirectToAction("Error");
                 }

                 // Przypisz UserId do quizResult
                 quizResult.UserId = currentUser.Id;

                 // Zapisz wynik do bazy danych
                 _context.QuizResultViewModel.Add(quizResult);
                 await _context.SaveChangesAsync();

                 Console.WriteLine($"Quiz result added for User ID {currentUser.Id}, Quiz ID {quizResult.QuizId}, Score {quizResult.Score}");

                 // Przekierowanie do szczegółów quizu
                 return RedirectToAction("QuizDetails", new { id = quizResult.QuizId });
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Exception while submitting quiz: {ex.Message}");
                 return View("Error");
             }
         }*/

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizResultViewModel quizResult)
        {
            try
            {
                // Sprawdź, czy QuizId istnieje w bazie
                var quizExists = await _context.QuizViewModel.AnyAsync(q => q.Id == quizResult.QuizId);
                if (!quizExists)
                {
                    Console.WriteLine($"Błąd: QuizId {quizResult.QuizId} nie istnieje w bazie danych.");
                    return BadRequest("Niepoprawny QuizId.");
                }

                // Pobierz użytkownika
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    Console.WriteLine("Nieautoryzowany użytkownik próbował przesłać quiz.");
                    return Unauthorized();
                }

                // Ustaw UserId i zapisz wynik w bazie
                quizResult.UserId = currentUser.Id;
                _context.QuizResultViewModel.Add(quizResult);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Quiz wynik zapisany dla UserId: {currentUser.Id}, QuizId: {quizResult.QuizId}, Wynik: {quizResult.Score}");
                return Ok("Wynik zapisany pomyślnie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu wyniku: {ex.Message}");
                return StatusCode(500, "Wystąpił błąd podczas zapisu wyniku.");
            }
        }

        //wyswietlenie wyniku
        [HttpGet]
        public async Task<IActionResult> GetUserStatistics()
        {
            // Pobierz aktualnie zalogowanego użytkownika
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Użytkownik niezalogowany.");
            }

            // Oblicz łączny wynik użytkownika
            int totalScore = await _context.QuizResultViewModel
                .Where(result => result.UserId == currentUser.Id)
                .SumAsync(result => result.Score);

            // Oblicz liczbę quizów rozwiązanych przez użytkownika
            int totalQuizzes = await _context.QuizResultViewModel
                .Where(result => result.UserId == currentUser.Id)
                .CountAsync();

            // Oblicz średni wynik
            double averageScore = totalQuizzes > 0 ? (double)totalScore / totalQuizzes : 0;

            // Zwróć dane w formacie JSON
            return Json(new
            {
                totalScore,
                totalQuizzes,
                averageScore
            });
        }


      
        [HttpGet]
        public async Task<IActionResult> GenerateCertificate(int quizId)
        {
            // Pobierz aktualnego użytkownika
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Użytkownik niezalogowany.");
            }

            // Pobierz wszystkie Id quizów
            var allQuizIds = await _context.QuizViewModel
                .Select(q => q.Id)
                .ToListAsync();

            // Pobierz quizy ukończone przez użytkownika
            var userCompletedQuizIds = await _context.QuizResultViewModel
                .Where(r => r.UserId == currentUser.Id)
                .Select(r => r.QuizId)
                .Distinct()
                .ToListAsync();

            // Sprawdź, czy użytkownik ukończył wszystkie quizy
            var missingQuizIds = allQuizIds.Except(userCompletedQuizIds).ToList();
            bool hasCompletedAllQuizzes = !missingQuizIds.Any();

            if (!hasCompletedAllQuizzes)
            {
                return NotFound("Nie ukończono wszystkich quizów.");
            }

            // Nazwa pliku PDF
            string fileName = $"Certyfikat_{currentUser.UserName}_Quiz_{quizId}.pdf";

            using (MemoryStream ms = new MemoryStream())
            {
                // Utwórz dokument PDF
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                string fontPath = System.IO.Path.Combine("wwwroot", "fonts", "arial.ttf");

                if (!System.IO.File.Exists(fontPath))
                {
                    return NotFound("Nie znaleziono pliku czcionki: " + fontPath);
                }

                PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                document.SetMargins(50, 50, 50, 50);

                string imagePath = System.IO.Path.Combine("wwwroot", "photos", "tlo_cert.png");

                if (!System.IO.File.Exists(imagePath))
                {
                    return NotFound("Nie znaleziono pliku tła: " + imagePath);
                }

                ImageData backgroundImageData = ImageDataFactory.Create(imagePath);
                Image backgroundImage = new Image(backgroundImageData);

                PdfDocument pdfDocument = document.GetPdfDocument();
                iText.Kernel.Geom.Rectangle pageSize = pdfDocument.GetDefaultPageSize();

               
                backgroundImage.ScaleToFit(pageSize.GetWidth(), pageSize.GetHeight());


                backgroundImage.SetFixedPosition(0, 0);

                backgroundImage.SetOpacity(0.3f);

                pdf.AddNewPage();
               
                document.Add(backgroundImage);

              
                document.Add(new Paragraph("\nCERTYFIKAT")
                    .SetFont(font).SetFontSize(72).SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("Kurs AUTOCAD")
                    .SetFont(font).SetFontSize(26).SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph("\n\nPotwierdzamy, udział:")
                    .SetFont(font).SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph($"{currentUser.Imie} {currentUser.Nazwisko}")
                    .SetFont(font).SetFontSize(18).SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph("\n\nDziękujemy za udział!")
                    .SetFont(font).SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                
                document.Add(new Paragraph("\n\n\n\n\n\n\n"));

                document.Add(new Paragraph($"\n\nData wygenerowania: {DateTime.Now:dd-MM-yyyy}")
                    .SetFont(font).SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                // 📌 Zamknięcie dokumentu
                document.Close();

                return File(ms.ToArray(), "application/pdf", fileName);
            }
        }



        //ukrywanie przycisku
        [HttpGet]
        public async Task<IActionResult> CheckIfAllQuizzesCompleted()
        {
            // Pobierz aktualnie zalogowanego użytkownika
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Użytkownik niezalogowany.");
            }

            // Pobierz wszystkie Id z tabeli QuizViewModel
            var allQuizIds = await _context.QuizViewModel
                .Select(q => q.Id)
                .ToListAsync();

            // Pobierz wszystkie QuizId z tabeli QuizResultViewModel dla zalogowanego użytkownika
            var userCompletedQuizIds = await _context.QuizResultViewModel
                .Where(r => r.UserId == currentUser.Id)
                .Select(r => r.QuizId)
                .Distinct()
                .ToListAsync();

            // Porównanie: sprawdzamy, czy wszystkie Id z QuizViewModel istnieją w QuizResultViewModel
            var missingQuizIds = allQuizIds.Except(userCompletedQuizIds).ToList();
            bool hasCompletedAllQuizzes = !missingQuizIds.Any();

            // Loguj wyniki dla debugowania
            Console.WriteLine("All Quiz IDs: " + string.Join(", ", allQuizIds));
            Console.WriteLine("User Completed Quiz IDs: " + string.Join(", ", userCompletedQuizIds));
            Console.WriteLine("Missing Quiz IDs: " + string.Join(", ", missingQuizIds));
            Console.WriteLine("Has Completed All Quizzes: " + hasCompletedAllQuizzes);

            // Zwróć wynik jako JSON
            return Json(new
            {
                allCompleted = hasCompletedAllQuizzes,
                allQuizIds = allQuizIds,
                userCompletedQuizIds = userCompletedQuizIds,
                missingQuizIds = missingQuizIds
            });
        }


        //**********************************************************************************
        public IActionResult Create(int kursId)
        {
            var viewModel = new QuizViewModel { KursId = kursId };

            return View(viewModel);
        }



        // POST: Handle the quiz creation form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizViewModel viewModel, int kursId)
        {
            /* if (ModelState.IsValid)
             {*/

            var quiz = new QuizViewModel

            {
                Title = viewModel.Title,
                KursId = kursId,
                Questions = viewModel.Questions.Select(q => new QuestionViewModel
                {
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new AnswerViewModel
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            _context.QuizViewModel.Add(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home"); // Redirect to the quiz list or confirmation page
            /* }*/

        }


        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(long id)
        {

            var quiz = await _context.QuizViewModel
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            _context.QuestionViewModel.RemoveRange(quiz.Questions);
            _context.QuizViewModel.Remove(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        /// edycja

        /* [Authorize(Roles = "Administrator")]
         [HttpGet]
         public async Task<IActionResult> EditQuiz(int id)
         {
             var quiz = await _context.QuizViewModel
                 .Include(q => q.Questions)
                     .ThenInclude(q => q.Answers)
                 .FirstOrDefaultAsync(q => q.Id == id);

             if (quiz == null)
             {
                 return NotFound();
             }

             var editViewModel = new QuizViewModel
             {
                 Id = quiz.Id,
                 Title = quiz.Title,
                 KursId = quiz.KursId,
                 Questions = quiz.Questions.Select(q => new QuestionViewModel
                 {
                     Id = q.Id,
                     Text = q.Text,
                     Answers = q.Answers.Select(a => new AnswerViewModel
                     {
                         Id = a.Id,
                         Text = a.Text,
                         IsCorrect = a.IsCorrect
                     }).ToList()
                 }).ToList()
             };

             return View(editViewModel);
         }

         [Authorize(Roles = "Administrator")]
         [HttpPost]
         public async Task<IActionResult> EditQuiz(QuizViewModel model)
         {
             var quiz = await _context.QuizViewModel
                 .Include(q => q.Questions)
                     .ThenInclude(q => q.Answers)
                 .FirstOrDefaultAsync(q => q.Id == model.Id);

             if (quiz == null)
             {
                 return NotFound();
             }

             // Aktualizacja tytułu i kursu
             quiz.Title = model.Title;
             quiz.KursId = model.KursId;

             // Aktualizacja pytań i odpowiedzi
             foreach (var modelQuestion in model.Questions)
             {
                 var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == modelQuestion.Id);

                 if (existingQuestion != null)
                 {
                     existingQuestion.Text = modelQuestion.Text;
                     _context.Entry(existingQuestion).State = EntityState.Modified;

                     foreach (var modelAnswer in modelQuestion.Answers)
                     {
                         var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == modelAnswer.Id);

                         if (existingAnswer != null)
                         {
                             existingAnswer.Text = modelAnswer.Text;
                             existingAnswer.IsCorrect = modelAnswer.IsCorrect;
                             _context.Entry(existingAnswer).State = EntityState.Modified;
                         }
                         else
                         {
                             var newAnswer = new AnswerViewModel
                             {
                                 Text = modelAnswer.Text,
                                 IsCorrect = modelAnswer.IsCorrect
                             };

                             existingQuestion.Answers.Add(newAnswer);
                             _context.Entry(newAnswer).State = EntityState.Added;
                         }
                     }
                 }
                 else
                 {
                     var newQuestion = new QuestionViewModel
                     {
                         Text = modelQuestion.Text,
                         Answers = modelQuestion.Answers.Select(a => new AnswerViewModel
                         {
                             Text = a.Text,
                             IsCorrect = a.IsCorrect
                         }).ToList()
                     };

                     quiz.Questions.Add(newQuestion);
                     _context.Entry(newQuestion).State = EntityState.Added;
                 }
             }

             await _context.SaveChangesAsync();
             return RedirectToAction("QuizDetails", new { id = model.Id });
         }

         public async Task<IActionResult> QuizDetails()
         {
             var quizy = await _context.QuizViewModel.ToListAsync();
             return View(quizy);
         }*/

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> EditQuiz(int id)
        {
            var quiz = await _context.QuizViewModel
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var editViewModel = new QuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                KursId = quiz.KursId,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new AnswerViewModel
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return View(editViewModel);
        }


        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> EditQuiz(QuizViewModel model)
        {
            var quiz = await _context.QuizViewModel
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            // ✅ Update quiz title and course ID
            quiz.Title = model.Title;
            quiz.KursId = model.KursId;

            // ✅ Update existing questions and add new ones
            foreach (var modelQuestion in model.Questions)
            {
                var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == modelQuestion.Id);

                if (existingQuestion != null)
                {
                    // ✅ Update existing question
                    existingQuestion.Text = modelQuestion.Text;
                    _context.Entry(existingQuestion).State = EntityState.Modified;

                    foreach (var modelAnswer in modelQuestion.Answers)
                    {
                        var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == modelAnswer.Id);

                        if (existingAnswer != null)
                        {
                            // ✅ Only update existing answers that have a valid ID
                            if (existingAnswer.Id > 0)
                            {
                                existingAnswer.Text = modelAnswer.Text;
                                existingAnswer.IsCorrect = modelAnswer.IsCorrect;
                                _context.Entry(existingAnswer).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            // ✅ Ensure new answers are added properly (not modified)
                            var newAnswer = new AnswerViewModel
                            {
                                Text = modelAnswer.Text,
                                IsCorrect = modelAnswer.IsCorrect,
                                QuestionId = existingQuestion.Id  // ✅ Correct Foreign Key Assignment
                            };

                            _context.AnswerViewModel.Add(newAnswer);
                        }
                    }
                }
                else
                {
                    // ✅ Add a new question with its answers
                    var newQuestion = new QuestionViewModel
                    {
                        Text = modelQuestion.Text,
                        Answers = modelQuestion.Answers.Select(a => new AnswerViewModel
                        {
                            Text = a.Text,
                            IsCorrect = a.IsCorrect
                        }).ToList()
                    };

                    _context.QuestionViewModel.Add(newQuestion);
                    quiz.Questions.Add(newQuestion);
                }
            }

            // ✅ Save changes to database
            await _context.SaveChangesAsync();
            return RedirectToAction("QuizDetails", new { id = model.Id });
        }



    }
}

    

