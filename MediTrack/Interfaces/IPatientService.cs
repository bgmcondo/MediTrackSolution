using MediTrack.Dtos.PatientDto;

namespace MediTrack.Interfaces
{
    public interface IPatientService
    {
        // Sadece Admin (controller'da rol ile korunur)
        Task<List<GetByIdPatientDto>> GetAllPatientsAsync();

        // Hastanın kendisi, ilişkili doktor, Admin
        Task<GetByIdPatientDto> GetPatientByIdAsync(int patientId);

        // Hastanın kendisi, Admin
        Task<GetByIdPatientDto> UpdatePatientAsync(int patientId, UpdatePatientDto updatePatientDto);
    }
}


