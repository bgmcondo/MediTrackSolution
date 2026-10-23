namespace MediTrack.Entities
{
    public class AppointmentMedication
    {
        public int MedicationId { get; set; }
        public int AppointmentId { get; set; }

        public Medication Medication { get; set; } = null!; 
        public Appointment Appointment{ get; set; } = null!;

        public required string  Dosage { get; set; } //Yarım tablet vb ifade edilebilir o yüzden string daha uygun

        public required string UsageInstructions { get; set; } 
    }
}
