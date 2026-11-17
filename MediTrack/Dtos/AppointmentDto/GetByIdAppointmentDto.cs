using MediTrack.Entities;
using MediTrack.Enums;

namespace MediTrack.Dtos.AppointmentDto
{
    public class GetByIdAppointmentDto
    {

        public int Id { get; set; }

        public int DoctorId { get; set; }

        public string DoctorFullName { get; set; } = string.Empty;

        public  int PatientId { get; set; }

        public string PatientFullName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        // true: hasta kendisi aldı, false: başkası (ör. Admin) onun adına oluşturdu
        public bool IsCreatedByPatient { get; set; }

        public AppointmentStatus AppointmentStatus { get; set; }





    }
}
