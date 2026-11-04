using System.ComponentModel.DataAnnotations;

namespace MediTrack.Dtos.AuthDto
{
    public class RegisterDoctorDto
    {
        //İdentity hesabı için gerekli alanlar
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!; //Email alanı boş olamaz ve geçerli bir email formatında olmalıdır.Ayrıca null! operatörü ile null olamayacağı belirtilmiştir.

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]   //Password alanı boş olamaz ve en az 8 karakter uzunluğunda olmalıdır. ErrorMessage ile kullanıcıya gösterilecek hata mesajı belirtilmiştir.
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")] //ConfirmPassword alanı boş olamaz ve Password alanı ile aynı değere sahip olmalıdır. ErrorMessage ile kullanıcıya gösterilecek hata mesajı belirtilmiştir.
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        public string Branch{ get; set; } = null!;


        public string Title { get; set; } = null!; // Unvan (ör. Uzm. Dr., Doç. Dr.). Doctor entity'sinde zorunlu alan.

        public bool IsFamilyDoctor { get; set; } // Aile hekimi mi? Gönderilmezse false olur.
    }
}
