using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class QuizResultViewModel
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int QuizId { get; set; }

        public int Score { get; set; }

        // Dodatkowe pola, jeśli są potrzebne

        // Relacje do innych encji
        public ApplicationUser User { get; set; }

        [ForeignKey("QuizId")]
        public QuizViewModel Quiz { get; set; }

    }
}

