using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUTOCAD.Models
{
    public class Komentarz
    {
        [Key]
        public int KomentarzId { get; set; }

        [Required]
        public long KursId { get; set; }  // Klucz obcy do kursu

        [Required]
        public string UserId { get; set; } = string.Empty;  // Klucz obcy do użytkownika

        [Required]
        [StringLength(1000, ErrorMessage = "Komentarz może mieć maksymalnie 1000 znaków.")]
        public string Tresc { get; set; }

        public DateTime DataDodania { get; set; } = DateTime.UtcNow;

        // ✅ Relacje
        [ForeignKey("KursId")]
        public virtual Kurs Kurs { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
