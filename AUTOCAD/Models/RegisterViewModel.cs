using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
namespace AUTOCAD.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Hasło musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Hasło musi zawierać co najmniej jedną wielką literę, jedną małą literę, jedną cyfrę i jeden znak specjalny.")]
        [Display(Name = "Password")]
        public string Password { get; set; }

    
    
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Imię musi mieć od 3 do 50 znaków.")]
        [RegularExpression(@"^[A-ZŁŚĆŹŻŃÓĄĘ][a-złśćźżńóąę]+$", ErrorMessage = "Imię musi zaczynać się od wielkiej litery i zawierać tylko litery.")]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Nazwisko musi mieć od 3 do 50 znaków.")]
        [RegularExpression(@"^[A-ZŁŚĆŹŻŃÓĄĘ][a-złśćźżńóąę]+(-[A-ZŁŚĆŹŻŃÓĄĘ][a-złśćźżńóąę]+)?$", ErrorMessage = "Nazwisko musi zaczynać się od wielkiej litery i zawierać tylko litery. Może zawierać podwójne nazwisko oddzielone myślnikiem.")]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }
    

    }
}
