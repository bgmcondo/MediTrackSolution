using MediTrack.Data;
using MediTrack.Dtos.DoctorDto;
using MediTrack.Entities;
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
            var doctors = await _context.Doctors.AsNoTracking().Select(
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
                     Title=d.Title
                 }
                ).ToListAsync();
           
            return doctors;
        }

        public async Task<GetByIdDoctorDto?> GetDoctorByIdAsync(int doctorId)
        {
            var doctor=await _context.Doctors.FirstOrDefaultAsync(e=>e.Id==doctorId);
            if (doctor == null)
                return null;

            return new GetByIdDoctorDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                IsFamilyDoctor = doctor.IsFamilyDoctor,
                Branch = doctor.Branch,
                Title = doctor.Title


            };
        }

        public async Task<GetByIdDoctorDto?> GetFamilyDoctorByIdAsync(int doctorId)
        {
            var familyDoctor=await _context.Doctors.FirstOrDefaultAsync(e=> e.Id == doctorId&&e.IsFamilyDoctor);
            if (familyDoctor == null)
                return null;
            return new GetByIdDoctorDto{
               Id=familyDoctor.Id,
               FirstName=familyDoctor.FirstName,
               LastName=familyDoctor.LastName,
               Branch=familyDoctor.Branch,
               Title=familyDoctor.Title,
               IsFamilyDoctor=familyDoctor.IsFamilyDoctor,


            };
        }

        public async  Task<List<GetByIdDoctorDto>> GetFamilyDoctorsAsync()
        {
            var familyDoctors = await _context.Doctors.Where(e=>e.IsFamilyDoctor).ToListAsync();
            var result= new List<GetByIdDoctorDto>();
            foreach (var doctor in familyDoctors)
                result.Add(new GetByIdDoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Branch = doctor.Branch,
                    Title = doctor.Title,
                    IsFamilyDoctor = doctor.IsFamilyDoctor
                });

         return result;
        }

        public async Task<UpdateDoctorDto> UpdateDoctorAsync(int doctorId,UpdateDoctorDto updateDoctorDto)
        {
            var targetDoctor =await  _context.Doctors.FirstOrDefaultAsync(e => e.Id==doctorId);
            if (targetDoctor == null)
                return null;

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

    public async Task<bool> DeleteDoctorAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(e => e.Id == doctorId);
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            _context.Doctors.Remove(doctor);

            await _context.SaveChangesAsync();

          return true;
        }
    }
}
