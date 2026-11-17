using System.ComponentModel.DataAnnotations;

namespace MediTrack.Dtos.DoctorDto
{
    public class UpdateDoctorDto
    {
        // Id'yi almamıza gerek yok çünkü id  URL üzerinden alınacak. Bu yüzden sadece güncellenmesi gereken alanları alıyoruz.

        [Required(ErrorMessage = "Branş zorunludur.")]
        [StringLength(100, ErrorMessage = "Branş en fazla 100 karakter olabilir.")]
        public string Branch { get; set; } = null!;

        [Required(ErrorMessage = "Unvan zorunludur.")]
        [StringLength(50, ErrorMessage = "Unvan en fazla 50 karakter olabilir.")]
        public string Title { get; set; } = null!;
        public bool IsFamilyDoctor { get; set; }
    }
}
