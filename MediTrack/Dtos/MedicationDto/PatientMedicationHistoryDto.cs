namespace MediTrack.Dtos.MedicationDto
{
    // Hastanın ilaç geçmişindeki tek bir reçete satırı:
    // hangi ilaç, hangi dozda, nasıl kullanılacak, ne zaman, hangi doktor tarafından yazıldı
    public class PatientMedicationHistoryDto
    {
        // Reçetenin yazıldığı randevu
        public int AppointmentId { get; set; }

        // İlacın kendisi (katalogdan)
        public int MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        // Bu reçeteye özel bilgiler
        public string Dosage { get; set; } = string.Empty;              // ör. "Yarım tablet"
        public string UsageInstructions { get; set; } = string.Empty;   // ör. "Günde 2 kez, yemekten sonra"
        public DateTime PrescribedAt { get; set; }                      // randevunun tarihi

        // Reçeteyi yazan doktor
        public int DoctorId { get; set; }
        public string DoctorFullName { get; set; } = string.Empty;
    }
}