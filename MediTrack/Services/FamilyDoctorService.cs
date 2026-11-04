using MediTrack.Data;
using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Entities;
using MediTrack.Enums;
using MediTrack.Helpers;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class FamilyDoctorService : IFamilyDoctorService
    {
        private readonly MediTrackDbContext _context;
        private readonly IAccessService _access;   // sahiplik/yetki kontrolleri

        public FamilyDoctorService(MediTrackDbContext context, IAccessService access)
        {
            _context = context;
            _access = access;
        }

        // Hastanın aile hekimini değiştirir.
        // Yetki: hastanın kendisi veya Admin (Doctor rolü controller'da engelleniyor)
        public async Task ChangeFamilyDoctorAsync(int patientId, int newDoctorId)
        {
            await _access.EnsureCanAccessPatientAsync(patientId);

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId)
                ?? throw new KeyNotFoundException("Hasta bulunamadı");

            // Ucuz kontrol önce: hasta zaten bu doktora kayıtlıysa veritabanına gitmeye gerek yok
            if (patient.CurrentFamilyDoctorId == newDoctorId)
                throw new InvalidOperationException("Hasta zaten bu doktora kayıtlı");

            // Doktor aktif ve aile hekimi mi?
            var isActiveFamilyDoctor = await _context.Doctors.AnyAsync(d =>
                d.Id == newDoctorId &&
                d.Status == DoctorStatus.Active &&
                d.IsFamilyDoctor);
            if (!isActiveFamilyDoctor)
                throw new KeyNotFoundException("Aile hekimi bulunamadı veya yeni hasta kabul edemiyor");

            var now = TimeHelper.NowInTurkey();

            // Açık kalmış geçmiş kaydını kapat (ilk atamada kayıt yoktur, bu adım atlanır)
            var activeHistory = await _context.patientFamilyDoctorHistories
                .FirstOrDefaultAsync(h => h.PatientId == patientId && h.UnassignedAt == null);
            if (activeHistory != null)
                activeHistory.UnassignedAt = now;

            // Yeni geçmiş kaydını aç
            _context.patientFamilyDoctorHistories.Add(new PatientFamilyDoctorHistory
            {
                PatientId = patientId,
                FamilyDoctorId = newDoctorId,
                AssignedAt = now
            });

            // Hastanın güncel doktorunu değiştir
            patient.CurrentFamilyDoctorId = newDoctorId;

            // Hepsini tek seferde veritabanına yaz
            await _context.SaveChangesAsync();
        }

        // Bir doktorun aile hekimliği geçmişi (hangi hastalara ne zaman atandığı).
        // Yetki: doktorun kendisi veya Admin
        public async Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByFamilyDoctorIdAsync(int familyDoctorId)
        {
            await _access.EnsureCanAccessDoctorAsync(familyDoctorId);

            // IsFamilyDoctor kontrolü BİLEREK yok: artık aile hekimi olmayan
            // bir doktorun eski geçmişi de görüntülenebilmeli
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == familyDoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Doktor bulunamadı");

            var histories = await _context.patientFamilyDoctorHistories
                .AsNoTracking()
                .Include(h => h.FamilyDoctor)
                .Where(h => h.FamilyDoctorId == familyDoctorId)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();

            return histories.Select(ToDto).ToList();
        }

        // Bir hastanın aile hekimi geçmişi.
        // Yetki: hastanın kendisi, ilişkili doktor veya Admin (idari veri)
        public async Task<List<GetByIdFamilyDoctorHistoryDto>> GetFamilyDoctorHistoryByPatientIdAsync(int patientId)
        {
            await _access.EnsureCanAccessPatientAsync(patientId);

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                throw new KeyNotFoundException("Hasta bulunamadı");

            var histories = await _context.patientFamilyDoctorHistories
                .AsNoTracking()
                .Include(h => h.FamilyDoctor)
                .Where(h => h.PatientId == patientId)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();

            return histories.Select(ToDto).ToList();
        }

        // Entity -> DTO dönüşümü tek yerde (iki metotta da aynı alanlar kullanılıyor).
        // FamilyDoctor null olamaz: doktorlar silinmiyor (soft delete), Include ile yükleniyor.
        private static GetByIdFamilyDoctorHistoryDto ToDto(PatientFamilyDoctorHistory h)
            => new GetByIdFamilyDoctorHistoryDto
            {
                Id = h.Id,
                PatientId = h.PatientId,
                FamilyDoctorId = h.FamilyDoctorId,
                FamilyDoctorFullName = h.FamilyDoctor.FirstName + " " + h.FamilyDoctor.LastName,
                AssignedAt = h.AssignedAt,
                UnassignedAt = h.UnassignedAt
            };
    }
}