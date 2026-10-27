using MediTrack.Entities;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Dtos.AuthDto
{
    public class RegisterPatientDto
    {

        //Identity Kullanıcı bilgileri 

        [Required] //Email alanı boş bırakılamaz.
        [EmailAddress] //Email formatında olmalıdır.
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = null!;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        //Patient bilgileri
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[1-9][0-9]{10}$",
        ErrorMessage = "TC kimlik numarası 11 haneli olmalıdır.")]
        public string NationalId { get; set; } = null!;



    }
}
