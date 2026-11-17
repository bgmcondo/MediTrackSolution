using MediTrack.Dtos.AppointmentDto;
using MediTrack.Dtos.MedicationDto;

namespace MediTrack.Interfaces
{
    public interface IAppointmentService
    {
        // Admin: herhangi bir hasta için, Patient: sadece kendisi için
        Task<GetByIdAppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);

        // Hastanın kendisi, ilişkili doktor, Admin
        Task<GetByIdAppointmentDto> GetAppointmentByIdAsync(int appointmentId);

        // Hastanın kendisi, ilişkili doktor, Admin
        Task<List<GetByIdAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId);

        // Doktorun kendisi, Admin
        Task<List<GetByIdAppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId);

        // Sadece Admin (controller'da rol ile korunur)
        Task<List<GetByIdAppointmentDto>> GetAllAppointmentsAsync();

        // randevunun sahibi olan hasta ve Admin
        Task<GetByIdAppointmentDto> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto);

        // Randevuya reçete yazar. Yetki: sadece randevunun doktoru.
        Task<PatientMedicationHistoryDto> PrescribeMedicationAsync(int appointmentId, PrescribeMedicationDto prescribeMedicationDto);

        // Randevunun sahibi olan hasta, Admin
        Task CancelAppointmentAsync(int appointmentId);
    }
}