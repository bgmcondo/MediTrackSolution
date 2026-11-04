using System.Security.Claims;
using MediTrack.Data;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    // AccessService: "Bu isteği yapan kullanıcı, bu işlemi yapmaya yetkili mi?" sorusunu cevaplar.
    // Tüm sahiplik/yetki kuralları tek yerde durur; diğer servisler metotlarının başında
    // buradaki Ensure... metotlarını çağırır. Yetki yoksa UnauthorizedAccessException fırlatılır,
    // controller bunu yakalayıp 403 döner.
    public class AccessService : IAccessService
    {
        // Rol adları sabit: yazım hatası riskini ortadan kaldırır
        private const string AdminRole = "Admin";
        private const string DoctorRole = "Doctor";
        private const string PatientRole = "Patient";

        private readonly MediTrackDbContext _context;

        // Servis içinden şu anki HTTP isteğine (ve isteği yapan kullanıcıya) erişmeyi sağlar.
        // Controller'daki User özelliği servislerde olmadığı için bu nesne kullanılır.
        // Program.cs'te builder.Services.AddHttpContextAccessor(); satırı olmalı.
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessService(MediTrackDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // İDARİ VERİLER (randevu listesi, aile hekimi geçmişi):
        // Hastanın kendisi, ilişkili doktor ve Admin
        public async Task EnsureCanAccessPatientAsync(int patientId)
        {
            var user = GetCurrentUser();

            if (user.IsInRole(AdminRole))
                return;

            if (await IsPatientSelfOrRelatedDoctorAsync(user, patientId))
                return;

            throw Denied("Bu hastanın verilerine erişim yetkiniz yok");
        }

        // TIBBİ VERİLER (reçete/ilaç geçmişi, tanı, doktor notları):
        // SADECE hastanın kendisi ve ilişkili doktor. Admin kontrolü BİLEREK yok:
        // Admin hastayı tedavi etmez, tıbbi veriye ihtiyacı yoktur (KVKK: amaçla sınırlılık).
        public async Task EnsureCanAccessPatientMedicalDataAsync(int patientId)
        {
            var user = GetCurrentUser();

            if (await IsPatientSelfOrRelatedDoctorAsync(user, patientId))
                return;

            throw Denied("Bu hastanın tıbbi verilerine erişim yetkiniz yok");
        }

        // DOKTORUN VERİLERİ (ör. randevu listesi): doktorun kendisi ve Admin.
        // doctorId: listesi İSTENEN doktor. İsteği YAPAN kişi token'dan (user) öğrenilir.
        public async Task EnsureCanAccessDoctorAsync(int doctorId)
        {
            var user = GetCurrentUser();

            if (user.IsInRole(AdminRole))
                return;

            if (user.IsInRole(DoctorRole))
            {
                var userId = GetUserId(user);

                var isOwnRecord = await _context.Doctors
                    .AnyAsync(d => d.Id == doctorId && d.ApplicationUserId == userId);

                if (isOwnRecord)
                    return;
            }

            throw Denied("Bu doktorun verilerine erişim yetkiniz yok");
        }

        // RANDEVU İPTALİ: randevunun sahibi olan hasta ve Admin. Doktor iptal edemez.
        public async Task EnsureCanCancelAppointmentAsync(int appointmentId)
        {
            var user = GetCurrentUser();

            if (user.IsInRole(AdminRole))
                return;

            if (await IsAppointmentOwnerAsync(user, appointmentId))
                return;

            throw Denied("Bu randevuyu iptal etme yetkiniz yok");
        }

        // RANDEVU GÜNCELLEME: randevunun sahibi olan hasta ve Admin. Doktor güncelleyemez.
        public async Task EnsureCanUpdateAppointmentAsync(int appointmentId)
        {
            var user = GetCurrentUser();

            if (user.IsInRole(AdminRole)) // DEĞİŞTİ: Admin artık güncelleyebilir
                return;

            if (await IsAppointmentOwnerAsync(user, appointmentId))
                return;

            throw Denied("Bu randevuyu güncelleme yetkiniz yok");
        }

        // ---------------- Yardımcı metotlar ----------------

        // ClaimsPrincipal: İsteği yapan kullanıcı. Token'daki claim'ler
        // (NameIdentifier, Email, Role) burada durur. HttpContext, HTTP isteği dışında
        // (arka plan görevi, test) null olabilir.
        private ClaimsPrincipal GetCurrentUser()
            => _httpContextAccessor.HttpContext?.User
               ?? throw Denied("Oturum bulunamadı");

        // Token'daki ApplicationUser.Id (AuthService'te NameIdentifier olarak eklenmişti)
        private static string GetUserId(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw Denied("Kullanıcı bilgisi bulunamadı");

        private static UnauthorizedAccessException Denied(string message)
            => new UnauthorizedAccessException(message);

        // Giriş yapan kişi bu hastanın KENDİSİ mi, ya da hastayla İLİŞKİSİ OLAN bir doktor mu?
        private async Task<bool> IsPatientSelfOrRelatedDoctorAsync(ClaimsPrincipal user, int patientId)
        {
            var userId = GetUserId(user);

            if (user.IsInRole(PatientRole))
            {
                var isOwnRecord = await _context.Patients
                    .AnyAsync(p => p.Id == patientId && p.ApplicationUserId == userId);

                if (isOwnRecord)
                    return true;
            }

            if (user.IsInRole(DoctorRole))
            {
                var doctorId = await GetDoctorIdAsync(userId);

                if (doctorId != null && await DoctorHasRelationWithPatientAsync(doctorId.Value, patientId))
                    return true;
            }

            return false;
        }

        // Giriş yapan kişi bu randevunun sahibi olan hasta mı?
        private async Task<bool> IsAppointmentOwnerAsync(ClaimsPrincipal user, int appointmentId)
        {
            if (!user.IsInRole(PatientRole))
                return false;

            var userId = GetUserId(user);

            return await _context.Appointments
                .AnyAsync(a => a.Id == appointmentId && a.Patient.ApplicationUserId == userId);
        }

        // Token'daki kullanıcı id'sinden Doctor tablosundaki id'yi bulur.
        // (int?): doktor bulunamazsa 0 yerine null dönsün diye.
        private async Task<int?> GetDoctorIdAsync(string userId)
            => await _context.Doctors
                .Where(d => d.ApplicationUserId == userId)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

        // Doktorun bu hastayla tedavi ilişkisi var mı?
        // 1) Hastanın güncel aile hekimi ise, 2) hastanın bu doktordan randevusu varsa.
        private async Task<bool> DoctorHasRelationWithPatientAsync(int doctorId, int patientId)
        {
            var isFamilyDoctor = await _context.Patients
                .AnyAsync(p => p.Id == patientId && p.CurrentFamilyDoctorId == doctorId);

            if (isFamilyDoctor)
                return true;

            return await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == patientId);
        }
    }
}