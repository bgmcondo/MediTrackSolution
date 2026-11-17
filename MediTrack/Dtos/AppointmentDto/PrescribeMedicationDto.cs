using System.ComponentModel.DataAnnotations;

namespace MediTrack.Dtos.AppointmentDto
{
    public class PrescribeMedicationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ilaç seçin.")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Doz zorunludur.")]
        [StringLength(100, ErrorMessage = "Doz en fazla 100 karakter olabilir.")]
        public string Dosage { get; set; } = null!;

        [Required(ErrorMessage = "Kullanım talimatı zorunludur.")]
        [StringLength(500, ErrorMessage = "Kullanım talimatı en fazla 500 karakter olabilir.")]
        public string UsageInstructions { get; set; } = null!;
    }
}
