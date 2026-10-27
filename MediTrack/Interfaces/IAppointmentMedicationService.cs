using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IAppointmentMedicationService
    {
        Task<AppointmentMedication> PrescribeMedicationAsync(AppointmentMedication appointmentMedication); // Randevu ilaç ekleme işlemi için gerekli metot imzası. //Doctor  tarafından kullanılabilir.>
        Task  RemovePrescribedMedicationAsync(int appointmentMedicationId); // Randevu ilaç silme işlemi için gerekli metot imzası. //Doctor  tarafından kullanılabilir.>

        Task <List<AppointmentMedication>> GetPrescribedMedicationsByAppointmentIdAsync(int appointmentId); // Belirli bir randevuya ait reçete edilen ilaçları getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.>

        Task<List<AppointmentMedication>> GetPrescribedMedicationsByPatientIdAsync(int patientId,int appointmentId); // Belirli bir hastaya ait reçete edilen ilaçları getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.>
    }
}
