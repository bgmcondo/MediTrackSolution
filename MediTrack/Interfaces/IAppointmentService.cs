using MediTrack.Dtos.AppointmentDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IAppointmentService
    {
        
        Task<CreateAppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto); // HastanınAppointment oluşturma işlemi için gerekli metot imzası.
        Task <GetByIdAppointmentDto> GetAppointmentByIdAsync(int appointmentId); // Randevu ID'sine göre randevu bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.
        Task <List<GetByIdAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId); // Belirli bir hastanın randevularını getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        Task <List<GetByIdAppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId); // Belirli bir doktorun randevularını getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.>
        Task<List<GetByIdAppointmentDto>> GetAllAppointmentsAsync(); // Tüm randevu bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        Task<UpdateAppointmentDto> UpdateAppointmentAsync(Appointment appointment); // Randevu bilgilerini güncellemek için gerekli metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        
        Task<Appointment> CancelAppointmentAsync(int appointmentId); // Randevu ID'sine göre randevu silme işlemi için gerekli metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.>
    }
}
