using System.Linq.Expressions;
using MediTrack.Data;
using MediTrack.Dtos.MedicationDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly MediTrackDbContext _context;
        private readonly IAccessService _access;   // hastanın ilaç geçmişi için yetki kontrolü

        public MedicationService(MediTrackDbContext context, IAccessService access)
        {
            _context = context;
            _access = access;
        }

        // Entity -> DTO dönüşümü tek yerde.
        // Expression: sorgu içinde (Select) kullanılır, EF Core SQL'e çevirir.
        private static readonly Expression<Func<Medication, GetByIdMedicationDto>> ToDto =
            m => new GetByIdMedicationDto
            {
                Id = m.Id,
                Name = m.Name,
                Barcode = m.Barcode,
                Description = m.Description
            };

        // Aynı dönüşümün bellekteki nesneler için derlenmiş hali (Create/Update sonrası).
        // Bir kez derlenir, her çağrıda yeniden derlenmez.
        private static readonly Func<Medication, GetByIdMedicationDto> ToDtoFunc = ToDto.Compile();

        // ---------------- İlaç kataloğu ----------------
        // Bu metotlar hasta verisi içermez, sahiplik kontrolü gerekmez.
        // Yetki controller'daki rollerle sağlanır (ekleme/güncelleme: sadece Admin).

        public async Task<GetByIdMedicationDto> CreateMedicationAsync(CreateMedicationDto createMedicationDto)
        {
            var barcodeExists = await _context.Medications
                .AnyAsync(m => m.Barcode == createMedicationDto.Barcode);
            if (barcodeExists)
                throw new InvalidOperationException("Bu barkodla kayıtlı bir ilaç zaten var");

            var medication = new Medication
            {
                Name = createMedicationDto.Name,
                Barcode = createMedicationDto.Barcode,
                Description = createMedicationDto.Description
            };

            _context.Medications.Add(medication);   // eklenecekler listesine koy (henüz yazılmadı)
            await _context.SaveChangesAsync();      // veritabanına yaz, Id burada oluşur

            return ToDtoFunc(medication);
        }

        public async Task<List<GetByIdMedicationDto>> GetAllMedicationsAsync()
        {
            // Sadece okuma: AsNoTracking ile EF Core takip etmez, daha hızlı.
            // Boşsa [] döner, frontend "Henüz ilaç eklenmemiş" yazar.
            return await _context.Medications
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<GetByIdMedicationDto> GetMedicationByIdAsync(int id)
        {
            return await _context.Medications
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("İlaç bulunamadı");
        }

        public async Task<GetByIdMedicationDto> UpdateMedicationAsync(int id, UpdateMedicationDto updateMedicationDto)
        {
            // Güncelleneceği için AsNoTracking kullanılmıyor
            var medication = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException("İlaç bulunamadı");

            // Aynı barkod başka bir ilaçta var mı? (kendisi hariç)
            var barcodeExists = await _context.Medications
                .AnyAsync(m => m.Barcode == updateMedicationDto.Barcode && m.Id != id);
            if (barcodeExists)
                throw new InvalidOperationException("Aynı barkoda sahip başka bir ilaç var");

            medication.Name = updateMedicationDto.Name;
            medication.Description = updateMedicationDto.Description;
            medication.Barcode = updateMedicationDto.Barcode;

            // Entity takip edildiği için _context.Update gerekmez
            await _context.SaveChangesAsync();

            // Güncellenmiş hali (Id dahil) döndür
            return ToDtoFunc(medication);
        }

        // ---------------- Hastanın ilaç geçmişi ----------------

        // TIBBİ VERİ: Sadece hastanın kendisi ve ilişkili doktor görebilir, Admin göremez.
        // Her reçete satırı ayrı döner: aynı ilaç farklı randevularda yazıldıysa birden fazla satır olur.
        public async Task<List<PatientMedicationHistoryDto>> GetPatientMedicationsAsync(int patientId)
        {
            await _access.EnsureCanAccessPatientMedicalDataAsync(patientId);

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                throw new KeyNotFoundException("Hasta bulunamadı");

            // Select içinde navigation property'lere (Medication, Appointment, Appointment.Doctor)
            // erişildiği için Include gerekmez: EF Core gerekli JOIN'leri kendisi kurar
            // ve sadece seçilen kolonları çeker.
            return await _context.AppointmentMedications
                .AsNoTracking()
                .Where(am => am.Appointment.PatientId == patientId)
                .OrderByDescending(am => am.Appointment.AppointmentDate)   // en yeni reçete en üstte
                .Select(am => new PatientMedicationHistoryDto
                {
                    AppointmentId = am.AppointmentId,
                    MedicationId = am.MedicationId,
                    MedicationName = am.Medication.Name,
                    Barcode = am.Medication.Barcode,
                    Dosage = am.Dosage,
                    UsageInstructions = am.UsageInstructions,
                    PrescribedAt = am.Appointment.AppointmentDate,
                    DoctorId = am.Appointment.DoctorId,
                    DoctorFullName = am.Appointment.Doctor.FirstName + " " + am.Appointment.Doctor.LastName
                })
                .ToListAsync();
        }
    }
}