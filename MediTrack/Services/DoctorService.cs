using MediTrack.Data;
using MediTrack.Dtos.DoctorDto;
using MediTrack.Entities;
using MediTrack.Enums;
using MediTrack.Helpers;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MediTrack.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly MediTrackDbContext _context;
   
        public DoctorService(MediTrackDbContext context) {
         
            _context = context;
        
        }


    

        public async Task<List<GetByIdDoctorDto>> GetAllDoctorsAsync()
        {
            var doctors = await _context.Doctors.AsNoTracking().Where(d=>d.Status != DoctorStatus.Deleted).Select(
                 // Neden yeni bir DTO nesnesi oluşturuyoruz?
                 // Elimizdeki Doctor (entity), döndürmek istediğimiz GetByIdDoctorDto. İkisi FARKLI sınıflar.
                 // Alan adları aynı olsa bile C# birini diğerine kendiliğinden çeviremez.
                 //
                 // Bu yüzden her doktor için boş bir DTO oluşturup (new GetByIdDoctorDto)
                 // içine sadece dışarı gitmesi gereken alanları tek tek kopyalıyoruz.
                 d => new GetByIdDoctorDto
                 {
                     Id = d.Id,
                     FirstName = d.FirstName,
                     LastName = d.LastName,
                     Branch = d.Branch,
                     IsFamilyDoctor = d.IsFamilyDoctor,
                     Title = d.Title,
                     Status = d.Status
                 }
             
                ).ToListAsync();
           
            return doctors;
        }

        public async Task<GetByIdDoctorDto?> GetDoctorByIdAsync(int doctorId)
        {
            var doctor=await _context.Doctors.FirstOrDefaultAsync(e=>e.Id==doctorId&&e.Status!=DoctorStatus.Deleted);
            if (doctor == null)
                throw new Exception("Bu Id'ye sahip doktor bulunmamaktadır");

            return new GetByIdDoctorDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                IsFamilyDoctor = doctor.IsFamilyDoctor,
                Branch = doctor.Branch,
                Title = doctor.Title,
                Status = doctor.Status


            };
        }

        public async Task<GetByIdDoctorDto?> GetFamilyDoctorByIdAsync(int doctorId)
        {
            var familyDoctor=await _context.Doctors.FirstOrDefaultAsync(e=> e.Id == doctorId&&e.IsFamilyDoctor&&e.Status!=DoctorStatus.Deleted);
            if (familyDoctor == null)
                throw new KeyNotFoundException("Bu Id'ye sahip aile hekimi bulunmamaktadır");
            return new GetByIdDoctorDto{
               Id=familyDoctor.Id,
               FirstName=familyDoctor.FirstName,
               LastName=familyDoctor.LastName,
               Branch=familyDoctor.Branch,
               Title=familyDoctor.Title,
               IsFamilyDoctor=familyDoctor.IsFamilyDoctor,
               Status=familyDoctor.Status


            };
        }

        public async  Task<List<GetByIdDoctorDto>> GetFamilyDoctorsAsync()
        {
            var familyDoctors = await _context.Doctors.Where(e=>e.IsFamilyDoctor&&e.Status!=DoctorStatus.Deleted).ToListAsync();
            var result= new List<GetByIdDoctorDto>();
            foreach (var doctor in familyDoctors)
                result.Add(new GetByIdDoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Branch = doctor.Branch,
                    Title = doctor.Title,
                    IsFamilyDoctor = doctor.IsFamilyDoctor,
                    Status = doctor.Status
                });

         return result;
        }

        public async Task<UpdateDoctorDto> UpdateDoctorAsync(int doctorId,UpdateDoctorDto updateDoctorDto)
        {
            var targetDoctor =await  _context.Doctors.FindAsync(doctorId);
            if (targetDoctor == null||targetDoctor.Status==DoctorStatus.Deleted)
                throw new KeyNotFoundException($"{doctorId} id'li doktor bulunamadı");

            targetDoctor.Title= updateDoctorDto.Title;
            targetDoctor.IsFamilyDoctor=updateDoctorDto.IsFamilyDoctor;
            targetDoctor.Branch=updateDoctorDto.Branch;
            await _context.SaveChangesAsync();
            return new UpdateDoctorDto
            {
                Title = updateDoctorDto.Title,
                IsFamilyDoctor=updateDoctorDto.IsFamilyDoctor,
                Branch=updateDoctorDto.Branch

            };

        
        }



    public async Task<DoctorStatus> DeleteDoctorAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync( doctorId);
            if (doctor == null||doctor.Status==DoctorStatus.Deleted)
                throw new KeyNotFoundException($"{doctorId}'sine sahip doktor bulunamadı");
            var hasUpcomingAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == doctorId&&a.AppointmentDate > TimeHelper.NowInTurkey()&&a.AppointmentStatus!=AppointmentStatus.Cancelled);

            var hasAssignedPatients = await _context.Patients
             .AnyAsync(p => p.CurrentFamilyDoctorId == doctorId);

            // Eğer doktorun randevuları varsa, silme işlemi yerine durumu "PendingDeletion" olarak güncelle ki yeni randevular alınamasın. Bu, mevcut randevuların etkilenmemesini sağlar.
            if (hasUpcomingAppointments||hasAssignedPatients)
           
            {
                doctor.Status = DoctorStatus.PendingDeletion;
                await _context.SaveChangesAsync(); // Doktor tablosundan Status alanını güncellemek için SaveChangesAsync çağrılır.
                return DoctorStatus.PendingDeletion;
            }


          



            doctor.Status = DoctorStatus.Deleted;
            // ← buradaki SaveChanges silindi

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == doctor.ApplicationUserId);
            if (user != null)
            {
                user.LockoutEnabled = true; // Hesabı kilitle
                user.LockoutEnd = DateTimeOffset.MaxValue; // Hesabı sonsuza kadar kilitle
                                                           // ← buradaki SaveChanges silindi çünkü return olmadığı için gerek yok.

                // return'e gerek yok zaten, doktor her türlü olsa da olmasa da silindi sayılır.O yüzden veritabanına burada kaydetmeye gerek  duymadık
               }

            await  _context.SaveChangesAsync(); // Doktor tablosundan Status alanını güncellemek için SaveChangesAsync çağrılır.    
            return DoctorStatus.Deleted;
        }
    }
}
