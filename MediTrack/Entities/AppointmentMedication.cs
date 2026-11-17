using System.ComponentModel.DataAnnotations;

namespace MediTrack.Entities
{
    public class AppointmentMedication
    {
        public int MedicationId { get; set; }
        public int AppointmentId { get; set; }

        public Medication Medication { get; set; } = null!; 
        public Appointment Appointment{ get; set; } = null!;

        [MaxLength(100)]
        public required string  Dosage { get; set; } //Yarım tablet vb ifade edilebilir o yüzden string daha uygun

        [MaxLength(500)]
        public required string UsageInstructions { get; set; } 
    }
}
