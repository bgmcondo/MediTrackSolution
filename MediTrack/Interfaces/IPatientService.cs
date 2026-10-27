using MediTrack.Entities;

namespace MediTrack.Interfaces
{
    public interface IPatientService
    {
        public Task<Patient> GetPatientByIdAsync(int patientId);
        public Task<List<Patient>> GetAllPatientsAsync();

        public Task <Patient> CreatePatientAsync(Patient patient);

        public Task <Patient> UpdatePatientAsync(int patientId, Patient patient);


        public Task<Patient> DeletePatientAsync( int patientId);

        public Task<Medication> GetPatientMedicationsAsync(int patientId);


    }
}
