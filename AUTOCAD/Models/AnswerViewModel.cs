using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class AnswerViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public QuestionViewModel Question { get; set; }
    }
}
