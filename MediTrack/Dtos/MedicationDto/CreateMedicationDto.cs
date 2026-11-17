using System.ComponentModel.DataAnnotations;

namespace MediTrack.Dtos.MedicationDto
{
    public class CreateMedicationDto
    {
        [Required(ErrorMessage = "İlaç adı zorunludur.")]
        [StringLength(200, ErrorMessage = "İlaç adı en fazla 200 karakter olabilir.")]
        public required string Name { get; set; } //required null da olsa bir değer atanmasını zorunlu kılar 

        [Required(ErrorMessage = "Barkod zorunludur.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Barkod 13 haneli bir sayı olmalıdır.")]
        public required string Barcode { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public required string Description { get; set; }
    }
}
