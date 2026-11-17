using MediTrack.Data;
using MediTrack.Dtos.DoctorDto;
using MediTrack.Entities;
using MediTrack.Enums;
using MediTrack.Helpers;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


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
            // Neden yeni bir DTO nesnesi oluşturuyoruz?
            // Elimizdeki Doctor (entity), döndürmek istediğimiz GetByIdDoctorDto. İkisi FARKLI sınıflar.
            // Alan adları aynı olsa bile C# birini diğerine kendiliğinden çeviremez.
            //
            // Bu yüzden her doktor için boş bir DTO oluşturup (new GetByIdDoctorDto)
            // içine sadece dışarı gitmesi gereken alanları tek tek kopyalıyoruz.
            var doctors = await _context.Doctors.AsNoTracking().Where(d=>d.Status != DoctorStatus.Deleted).Select(ToDtoExpression ).ToListAsync();
           
            return doctors;
        }

        public async Task<GetByIdDoctorDto?> GetDoctorByIdAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Where(e => e.Id == doctorId && e.Status != DoctorStatus.Deleted)
                .Select(ToDtoExpression)
                .FirstOrDefaultAsync();

            if (doctor == null)
                throw new KeyNotFoundException($"Id'si {doctorId} olan doktor bulunamadı");

            return doctor;
        }

        public async Task<GetByIdDoctorDto?> GetFamilyDoctorByIdAsync(int doctorId)
        {
            var familyDoctor=await _context.Doctors.AsNoTracking().Where(e=> e.Id == doctorId&&e.IsFamilyDoctor&&e.Status!=DoctorStatus.Deleted).Select(ToDtoExpression).FirstOrDefaultAsync();
            if (familyDoctor == null)
                throw new KeyNotFoundException("Bu Id'ye sahip aile hekimi bulunmamaktadır");
            return familyDoctor;
        }

        public async Task<List<GetByIdDoctorDto>> GetFamilyDoctorsAsync()
        {
            return await _context.Doctors
                .AsNoTracking()
                .Where(e => e.IsFamilyDoctor && e.Status!=DoctorStatus.Deleted)
                .Select(ToDtoExpression)
                .ToListAsync();
        }

        public async Task<GetByIdDoctorDto> UpdateDoctorAsync(int doctorId,UpdateDoctorDto updateDoctorDto)
        {
            var targetDoctor =await  _context.Doctors.FindAsync(doctorId);
            if (targetDoctor == null||targetDoctor.Status==DoctorStatus.Deleted)
                throw new KeyNotFoundException($"Id'si {doctorId} olan doktor bulunamadı");

            if (targetDoctor.IsFamilyDoctor && !updateDoctorDto.IsFamilyDoctor
               && await _context.Patients.AnyAsync(p => p.CurrentFamilyDoctorId == doctorId))
                throw new InvalidOperationException("Bu doktorun aile hekimi olduğu hastalar var; önce hastaları başka bir hekime aktarın.");

            targetDoctor.Title= updateDoctorDto.Title;
            targetDoctor.IsFamilyDoctor=updateDoctorDto.IsFamilyDoctor;
            targetDoctor.Branch=updateDoctorDto.Branch;
            await _context.SaveChangesAsync();
            return ToDto(targetDoctor);

        
        }



    public async Task<DoctorStatus> DeleteDoctorAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync( doctorId);
            if (doctor == null||doctor.Status==DoctorStatus.Deleted)
                throw new KeyNotFoundException($"Id'si {doctorId} olan doktor bulunamadı");
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


        // Doctor → GetByIdDoctorDto dönüşümü TEK yerde tanımlı.
        // Expression olduğu için EF Core bunu SQL'e çevirebilir (sorgularda .Select içinde kullanılır).
        private static readonly Expression<Func<Doctor, GetByIdDoctorDto>> ToDtoExpression = d => new GetByIdDoctorDto
        {
            Id = d.Id,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Branch = d.Branch,
            Title = d.Title,
            IsFamilyDoctor = d.IsFamilyDoctor,
            Status = d.Status
        };

        // Aynı dönüşümün bellekteki tek bir nesne için derlenmiş hali (ör. FindAsync ile gelen doktor)
        private static readonly Func<Doctor, GetByIdDoctorDto> ToDto = ToDtoExpression.Compile();
    }
}
