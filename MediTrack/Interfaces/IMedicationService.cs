using MediTrack.Dtos.MedicationDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IMedicationService
    {

        public Task<CreateMedicationDto> CreateMedicationAsync(CreateMedicationDto createMedicationDto); //Bu metot yeni bir ilaç oluşturur ve veritabanına kaydeder. // Sadece Doktor tarafından kullanılabilir.
        public Task<GetByIdMedicationDto?> GetMedicationByIdAsync(int id); //Bu metot belrtilen id'ye sahip ilacı veritabanından getirir. //  Doctor  tarafından kullanılabilir.

        public Task<UpdateMedicationDto> UpdateMedicationAsync(int id,UpdateMedicationDto updateMedicationDto); //Bu metot belirtilen id'ye sahip ilacın bilgilerini günceller. //  Doctor  tarafından kullanılabilir.

        public Task <List<GetByIdMedicationDto>> GetPatientMedicationsAsync(int patientId);

        public Task<List<GetByIdMedicationDto>> GetAllMedicationsAsync(); //Bu metot veritabanındaki tüm ilaçları getirir. //  Doctor  tarafından kullanılabilir.
    }
}
