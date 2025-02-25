using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class QuizViewModel
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
        public List<QuizResultViewModel> Results { get; set; }
        // Dodaj klucz obcy wskazujący na Kurs

        public long KursId { get; set; }
        [ForeignKey("KursId")]
        public Kurs Kurs { get; set; }
        public int TotalQuestions { get; internal set; }
    }
}
