using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class Kurs
    {
        public long KursId { get; set; }
        [Required]
        [StringLength(100)]
        public string Nazwa { get; set; }
        [StringLength(2500)]
        public string Opis { get; set; }
        public string Photo { get; set; }
        /*[NotMapped]
        public IFormFile PhotoFile { get; set; }
        public string MovieID { get; set; }
*/
        [NotMapped]
        public IFormFile? PhotoFile { get; set; }

        public string? MovieID { get; set; }
        // public bool IsCompleted { get; set; }

        public ICollection<QuizViewModel> Quiz { get; set; }

        public ICollection<Komentarz> Komentarze { get; set; } = new List<Komentarz>();

    }


}
