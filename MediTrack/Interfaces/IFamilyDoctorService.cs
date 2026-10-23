using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IFamilyDoctorService
    {
        Task<List<Doctor>> GetFamilyDoctorsAsync();  //Tüm aile hekimlerini listeleyen bir metod. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin tüm aile hekimlerini listelemek için kullanılabilir. Bu metod, veritabanındaki tüm doktorları alır ve sadece aile hekimlerini filtreler.
        Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByPatientIdAsync(int patientId); //Belirli bir hastanın aile hekimliği geçmişini döner. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin belirli bir hastanın aile hekimliği geçmişini almak için kullanılabilir. Bu metod, veritabanındaki aile hekimliği geçmişlerini alır ve sadece belirli bir hastayı filtreler.

        Task<Doctor> GetFamilyDoctorByIdAsync(int doctorId); //Belirli bir aile hekiminin detaylarını döner. Asenkron çalışır ve bir doktor nesnesi döner. Admin,hasta,doktor farketmeksizin belirli bir aile hekiminin detaylarını almak için kullanılabilir. Bu metod, veritabanındaki doktorları alır ve sadece belirli bir aile hekimini filtreler.>

        Task <List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByDoctorIdAsync(int doctorId); //Belirli bir doktorun aile hekimliği geçmişini döner. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin belirli bir doktorun aile hekimliği geçmişini almak için kullanılabilir. Bu metod, veritabanındaki aile hekimliği geçmişlerini alır ve sadece belirli bir doktoru filtreler.

      

    }
}
