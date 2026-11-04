using MediTrack.Dtos.MedicationDto;

namespace MediTrack.Interfaces
{
    public interface IMedicationService
    {
        // İlaç kataloğu (hasta verisi içermez)
        Task<GetByIdMedicationDto> CreateMedicationAsync(CreateMedicationDto createMedicationDto);
        Task<List<GetByIdMedicationDto>> GetAllMedicationsAsync();
        Task<GetByIdMedicationDto> GetMedicationByIdAsync(int id);
        Task<GetByIdMedicationDto> UpdateMedicationAsync(int id, UpdateMedicationDto updateMedicationDto);

        // Hastanın ilaç geçmişi (TIBBİ VERİ: hastanın kendisi ve ilişkili doktor; Admin göremez)
        Task<List<PatientMedicationHistoryDto>> GetPatientMedicationsAsync(int patientId);
    }
}