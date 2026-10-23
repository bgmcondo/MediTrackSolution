using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IAppointmentService
    {
        
        Task<Appointment> CreateAppointmentAsync(Appointment appointment); // HastanınAppointment oluşturma işlemi için gerekli metot imzası.
        Task <Appointment> GetAppointmentByIdAsync(int appointmentId); // Randevu ID'sine göre randevu bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        Task<Appointment> GetAllAppointmentsAsync(); // Tüm randevu bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        Task<Appointment> UpdateAppointmentAsync(Appointment appointment); // Randevu bilgilerini güncellemek için gerekli metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.
        
        Task<Appointment> DeleteAppointmentAsync(int appointmentId); // Randevu ID'sine göre randevu silme işlemi için gerekli metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.>
    }
}
