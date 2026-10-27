using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IMedicationService
    {

        public Task<Medication> CreateMedicationAsync(Medication medication); //Bu metot yeni bir ilaç oluşturur ve veritabanına kaydeder. // Sadece Doktor tarafından kullanılabilir.
        public Task<Medication?> GetMedicationByIdAsync(int id); //Bu metot belrtilen id'ye sahip ilacı veritabanından getirir. //  Doctor  tarafından kullanılabilir.

        public Task<Medication> UpdateMedicationAsync(int id, Medication medication); //Bu metot belirtilen id'ye sahip ilacın bilgilerini günceller. //  Doctor  tarafından kullanılabilir.



        public Task<List<Medication>> GetAllMedicationsAsync(); //Bu metot veritabanındaki tüm ilaçları getirir. //  Doctor  tarafından kullanılabilir.
    }
}
