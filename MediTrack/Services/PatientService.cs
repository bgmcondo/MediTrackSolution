using MediTrack.Data;
using MediTrack.Dtos.PatientDto;
using MediTrack.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class PatientService : IPatientService
    {
        private readonly MediTrackDbContext _context;
        public PatientService(MediTrackDbContext context) { 
          _context = context;
        
        }
        public  async Task<List<GetByIdPatientDto>> GetAllPatientsAsync()
        {
            var patientEntityList=await _context.Patients.AsNoTracking().Include(p=>p.CurrentFamilyDoctor).OrderBy(e=>e.FirstName).ThenBy(e=>e.LastName).ToListAsync();
            var  patientList=new List<GetByIdPatientDto>();

            foreach (var patient in patientEntityList)
                patientList.Add(new GetByIdPatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    NationalId = patient.NationalId,
                    CurrentFamilyDoctorId = patient.CurrentFamilyDoctorId,
                    CurrentFamilyDoctorName = patient.CurrentFamilyDoctor != null ? patient.CurrentFamilyDoctor.FirstName + " " + patient.CurrentFamilyDoctor.LastName : null   
                
                });
           
            return patientList;
            
         
        }

        public  async Task<GetByIdPatientDto> GetPatientByIdAsync(int patientId)
        {
            var patientEntity = await _context.Patients.AsNoTracking().Include(e=>e.CurrentFamilyDoctor).FirstOrDefaultAsync(e=>e.Id==patientId);
            if (patientEntity == null)
                throw new KeyNotFoundException("Hasta bulunamadı");
            return new GetByIdPatientDto
            {
                Id = patientEntity.Id,
                FirstName = patientEntity.FirstName,
                LastName = patientEntity.LastName,
                NationalId = patientEntity.NationalId,
                CurrentFamilyDoctorId = patientEntity.CurrentFamilyDoctorId,
                CurrentFamilyDoctorName = patientEntity.CurrentFamilyDoctor != null ? patientEntity.CurrentFamilyDoctor.FirstName + " " + patientEntity.CurrentFamilyDoctor.LastName : null





            };
        }

        public async Task<UpdatePatientDto> UpdatePatientAsync(int patientId, UpdatePatientDto updatePatientDto)
        {
            var patientEntity = await _context.Patients.FirstOrDefaultAsync(e => e.Id ==patientId);
            if(patientEntity == null)
            {
                throw new KeyNotFoundException("Hasta bulunamadı");
            }
            patientEntity.FirstName= updatePatientDto.FirstName;
            patientEntity.LastName= updatePatientDto.LastName;

        
            await _context.SaveChangesAsync();

            return new UpdatePatientDto
            {
                FirstName = patientEntity.FirstName,
                LastName = patientEntity.LastName,


            }
                
                ;
        }


    }
}
