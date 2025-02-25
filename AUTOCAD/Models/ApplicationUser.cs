using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AUTOCAD.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków.")]
        public string Nazwisko { get; set; }


        public ICollection<Komentarz> Komentarze { get; set; } = new List<Komentarz>();
    }
}
