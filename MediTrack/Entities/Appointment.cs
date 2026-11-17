using MediTrack.Enums;

namespace MediTrack.Entities
{


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

        // Randevuyu oluşturan kullanıcı (AspNetUsers.Id). Hasta kendisi de olabilir, Admin de.
        public string CreatedByUserId { get; set; } = null!;   // kim (kesin kimlik)


        public ICollection<AppointmentMedication> Appointments { get; set; }= new List<AppointmentMedication>();

    }


     
}