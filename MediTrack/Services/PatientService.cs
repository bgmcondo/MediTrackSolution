using System.Linq.Expressions;
using MediTrack.Data;
using MediTrack.Dtos.PatientDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class PatientService : IPatientService
    {
        private readonly MediTrackDbContext _context;
        private readonly IAccessService _access;   // sahiplik/yetki kontrolleri

        public PatientService(MediTrackDbContext context, IAccessService access)
        {
            _context = context;
            _access = access;
        }

        // Entity -> DTO dönüşümü tek yerde. Select içinde kullanıldığı için SQL'e çevrilir,
        // Include gerekmez. Hastanın aile hekimi olmayabilir, bu yüzden null kontrolü var.
        private static readonly Expression<Func<Patient, GetByIdPatientDto>> ToDto =
            p => new GetByIdPatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                NationalId = p.NationalId,
                CurrentFamilyDoctorId = p.CurrentFamilyDoctorId,
                CurrentFamilyDoctorName = p.CurrentFamilyDoctor != null
                    ? p.CurrentFamilyDoctor.FirstName + " " + p.CurrentFamilyDoctor.LastName
                    : null
            };

        // Tüm hastalar (TC kimlik numaraları dahil).
        // Sahiplik kontrolü yapılamaz (belirli bir hastaya ait değil);
        // koruma controller'daki [Authorize(Roles = "Admin")] ile sağlanır.
        public async Task<List<GetByIdPatientDto>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .Select(ToDto)
                .ToListAsync();
        }

        // Yetki: hastanın kendisi, ilişkili doktor veya Admin
        public async Task<GetByIdPatientDto> GetPatientByIdAsync(int patientId)
        {
            await _access.EnsureCanAccessPatientAsync(patientId);

            return await _context.Patients
                .AsNoTracking()
                .Where(p => p.Id == patientId)
                .Select(ToDto)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Hasta bulunamadı");
        }

        // Yetki: hastanın kendisi veya Admin (Doctor rolü controller'da engelleniyor)
        public async Task<GetByIdPatientDto> UpdatePatientAsync(int patientId, UpdatePatientDto updatePatientDto)
        {
            await _access.EnsureCanAccessPatientAsync(patientId);

            // Güncelleneceği için AsNoTracking kullanılmıyor
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId)
                ?? throw new KeyNotFoundException("Hasta bulunamadı");

            patient.FirstName = updatePatientDto.FirstName;
            patient.LastName = updatePatientDto.LastName;

            // Entity takip edildiği için _context.Update gerekmez
            await _context.SaveChangesAsync();

            // Güncellenmiş hali (aile hekimi adı dahil) döndür
            return await _context.Patients
                .AsNoTracking()
                .Where(p => p.Id == patientId)
                .Select(ToDto)
                .FirstAsync();
        }
    }
}