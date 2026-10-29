namespace MediTrack.Entities
{

    public enum AppointmentStatus
    {
        Scheduled = 0,  // Randevu alındı, aktif
        Cancelled = 1,  // İptal edildi
        Completed = 2   // Randevu gerçekleşti
    }
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;  //Bu değer şu anda null olabilir ama derleyici, null uyarısı verme. Sorumluluk bende.

        public int DoctorId { get; set; }

        public Doctor Doctor { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        public bool IsFamilyDoctorAppointment { get; set; }

        public AppointmentStatus AppointmentStatus { get; set; } 


        public ICollection<AppointmentMedication> Appointments { get; set; }= new List<AppointmentMedication>();

    }


     
}