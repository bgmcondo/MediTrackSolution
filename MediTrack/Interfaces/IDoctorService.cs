using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IDoctorService
    {
        Task<Doctor> GetDoctorByIdAsync(int doctorId); // Bu metod, belirli bir doktorun ID'sine göre doktor bilgilerini almak için kullanılacak.Admin,Doctor ve Patient rollerine sahip kullanıcılar bu metoda erişebilecek.
        Task<Doctor> GetAllDoctorsAsync(); // Tüm doktorların bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.

        Task <Doctor> CreateDoctorAsync(Doctor doctor); // Yeni bir doktor oluşturmak için kullanılacak. // Sadece Admin tarafından kullanılabilir.

        Task DeleteDoctorAsync(int doctorId); // Belirli bir doktoru silmek için kullanılacak. // Sadece Admin tarafından kullanılabilir.

        Task<Doctor> UpdateDoctorAsync(int doctorId, Doctor doctor); // Belirli bir doktorun bilgilerini güncellemek için kullanılacak. // Sadece Admin tarafından kullanılabilir.


    }
}
