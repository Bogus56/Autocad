using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class QuestionViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        // ✅ Add Foreign Key to QuizViewModel
        public int QuizViewModelId { get; set; }  // Foreign key
        [ForeignKey("QuizViewModelId")]
        public QuizViewModel Quiz { get; set; }  // Navigation property

        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }
}
