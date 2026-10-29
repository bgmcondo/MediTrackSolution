using MediTrack.Data;
using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using MediTrack.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class FamilyDoctorService : IFamilyDoctorService
    {
        private readonly MediTrackDbContext _context;

        public FamilyDoctorService(MediTrackDbContext context)
        {
            _context = context;
             
        }
        public async Task ChangeFamilyDoctorAsync(int patientId, int newDoctorId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(e => e.Id == patientId);
            
            if (patient == null)
                throw new KeyNotFoundException("Hasta bulunamadı");

            // Bu id'ye sahip doktor var mı onu kontrol et
            var doctorExists = await _context.Doctors.AnyAsync(d=>d.Id==newDoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Doktor bulunamadı");
            if (patient.CurrentFamilyDoctorId == newDoctorId)
                throw new InvalidOperationException("Hasta zaten bu doktora kayıtlı");
         // Aktif geçmiş kaydını kapat
          var activeHistory=await _context.patientFamilyDoctorHistories.FirstOrDefaultAsync(h=>h.PatientId==patientId&&h.UnassignedAt==null);
           if (activeHistory != null) // Eğer aktif geçmiş kaydı null değilse kapat
                activeHistory.UnassignedAt = DateTime.UtcNow;
            // Yeni geçmiş kaydını aç 
            _context.patientFamilyDoctorHistories.Add(new PatientFamilyDoctorHistory
            {
                PatientId = patientId,
                FamilyDoctorId= newDoctorId,
                AssignedAt=TimeHelper.NowInTurkey()
            }
                
                
               
                );

            //Hastanın güncel doktorunu değiştir
            patient.CurrentFamilyDoctorId= newDoctorId;

            //Hepsini veritabanına yaz
            await _context.SaveChangesAsync();
        }
        public async  Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByFamilyDoctorIdAsync(int familyDoctorId)
        {
           // Doktor mevcut mu kontrol ediyoruz
           var doctorExists=await _context.Doctors.AnyAsync(d=>d.Id ==familyDoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Doktor mevcut değil");
           var familyDoctorHistoriesEntity=await _context.patientFamilyDoctorHistories.Include(p=>p.FamilyDoctor).Where(p=>p.FamilyDoctorId==familyDoctorId).ToListAsync();
           var familyDoctorHistoryList=new List<GetByIdFamilyDoctorHistoryDto>();

            foreach (var familyDoctorHistory in familyDoctorHistoriesEntity)
                familyDoctorHistoryList.Add(new GetByIdFamilyDoctorHistoryDto
                {
                    Id = familyDoctorHistory.Id,
                    PatientId = familyDoctorHistory.PatientId,
                    FamilyDoctorId = familyDoctorHistory.FamilyDoctorId,
                    FamilyDoctorFullName = familyDoctorHistory.FamilyDoctor.FirstName + " " + familyDoctorHistory.FamilyDoctor.LastName,
                    AssignedAt = familyDoctorHistory.AssignedAt,
                    UnassignedAt = familyDoctorHistory.UnassignedAt,










                });


           return familyDoctorHistoryList;
                    
        }

        public async Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByPatientIdAsync(int patientId)
        {
            var IsPatientExists = await _context.Patients.FirstOrDefaultAsync(e=>e.Id==patientId);

            // Hasta mevcut mu kontrol
            if (IsPatientExists == null)
                throw new KeyNotFoundException("Hasta mevcut değil");
             
            var familyDoctorsEntity=await  _context.patientFamilyDoctorHistories.Include(p=>p.FamilyDoctor).Where(p=>p.PatientId==patientId).ToListAsync();
             
            var familyDoctorsList=new List<GetByIdFamilyDoctorHistoryDto>();

            foreach (var familyDoctor in familyDoctorsEntity)
                familyDoctorsList.Add(new GetByIdFamilyDoctorHistoryDto
                {
                    Id = familyDoctor.Id,
                    PatientId=familyDoctor.PatientId,
                    FamilyDoctorId = familyDoctor.FamilyDoctorId,
                    // TODO: FamilyDoctor null olabilir (FamilyDoctorId int? ve doktor silinince SetNull olabilir).
                    // Ya null kontrolü ekle: FamilyDoctor != null ? ... : null
                    // Ya da FamilyDoctorId'yi int yapıp DeleteBehavior.Restrict kullan.
                    FamilyDoctorFullName = familyDoctor.FamilyDoctor.FirstName + " " + familyDoctor.FamilyDoctor.LastName,
                    AssignedAt=familyDoctor.AssignedAt,
                    UnassignedAt=familyDoctor.UnassignedAt,




                });

            return familyDoctorsList;

        }
    }
}
