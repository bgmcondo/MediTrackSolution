using MediTrack.Dtos.PatientDto;
using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IPatientService
    {
        public Task<GetByIdPatientDto> GetPatientByIdAsync(int patientId);
        public Task<List<GetByIdPatientDto>> GetAllPatientsAsync();

        public Task <UpdatePatientDto> UpdatePatientAsync(int patientId, UpdatePatientDto updatePatientDto);


    }
}
