using MediTrack.Dtos.AppointmentDto;

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

        // Randevunun sahibi olan hasta, Admin
        Task CancelAppointmentAsync(int appointmentId);
    }
}