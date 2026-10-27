using MediTrack.Dtos.DoctorDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IDoctorService
    {
        Task<List<GetByIdDoctorDto>> GetFamilyDoctorsAsync();  //Tüm aile hekimlerini listeleyen bir metod. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin tüm aile hekimlerini listelemek için kullanılabilir. Bu metod, veritabanındaki tüm doktorları alır ve sadece aile hekimlerini filtreler.

        Task<GetByIdDoctorDto?> GetFamilyDoctorByIdAsync(int doctorId); //Belirli bir aile hekiminin detaylarını döner. Asenkron çalışır ve bir doktor nesnesi döner. Admin,hasta,doktor farketmeksizin belirli bir aile hekiminin detaylarını almak için kullanılabilir. Bu metod, veritabanındaki doktorları alır ve sadece belirli bir aile hekimini filtreler.>
        Task<GetByIdDoctorDto?> GetDoctorByIdAsync(int doctorId); // Bu metod, belirli bir doktorun ID'sine göre doktor bilgilerini almak için kullanılacak.Admin,Doctor ve Patient rollerine sahip kullanıcılar bu metoda erişebilecek.
        Task<List<GetByIdDoctorDto>> GetAllDoctorsAsync(); // Tüm doktorların bilgilerini getiren metot imzası. // Admin, Doctor ve Patient tarafından kullanılabilir.



        Task<bool> DeleteDoctorAsync(int doctorId); // Belirli bir doktoru silmek için kullanılacak. // Sadece Admin tarafından kullanılabilir.

        Task<UpdateDoctorDto> UpdateDoctorAsync(int doctorId, UpdateDoctorDto updateDoctorDto); // Belirli bir doktorun bilgilerini güncellemek için kullanılacak. // Sadece Admin tarafından kullanılabilir.


    }
}
