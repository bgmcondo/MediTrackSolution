using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IFamilyDoctorService
    {
       
        Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByPatientIdAsync(int patientId); //Belirli bir hastanın aile hekimliği geçmişini döner. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin belirli bir hastanın aile hekimliği geçmişini almak için kullanılabilir. Bu metod, veritabanındaki aile hekimliği geçmişlerini alır ve sadece belirli bir hastayı filtreler.

        Task <List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByFamilyDoctorIdAsync(int familyDoctorId); //Belirli bir doktorun aile hekimliği geçmişini döner. Asenkron çalışır ve bir liste döner. Admin,hasta,doktor farketmeksizin belirli bir doktorun aile hekimliği geçmişini almak için kullanılabilir. Bu metod, veritabanındaki aile hekimliği geçmişlerini alır ve sadece belirli bir doktoru filtreler.

        Task ChangeFamilyDoctorAsync( int patientId, int newDoctorId); //Kişinin aile hekimini değiştirmesini sağlar.


    }
}
