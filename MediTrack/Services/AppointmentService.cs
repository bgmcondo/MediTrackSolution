using System.Linq.Expressions;
using MediTrack.Data;
using MediTrack.Dtos.AppointmentDto;
using MediTrack.Entities;
using MediTrack.Enums;
using MediTrack.Helpers;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly MediTrackDbContext _context;
        private readonly IAccessService _access;   // sahiplik/yetki kontrolleri

        public AppointmentService(MediTrackDbContext context, IAccessService access)
        {
            _context = context;
            _access = access;
        }

        // Entity -> DTO dönüşümü tek yerde. Select içinde kullanıldığı için EF Core bunu
        // SQL'e çevirir: sadece gereken kolonlar çekilir, Include'a gerek kalmaz.
        private static readonly Expression<Func<Appointment, GetByIdAppointmentDto>> ToDto =
            a => new GetByIdAppointmentDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorFullName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                PatientId = a.PatientId,
                PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
                AppointmentDate = a.AppointmentDate,
                AppointmentStatus = a.AppointmentStatus
            };

        // Admin: herhangi bir hasta için, Patient: sadece kendisi için
        public async Task<GetByIdAppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
        {
            await _access.EnsureCanAccessPatientAsync(createAppointmentDto.PatientId);

            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == createAppointmentDto.PatientId);
            if (!patientExists)
                throw new KeyNotFoundException("Belirtilen hasta bulunamadı");

            var isDoctorActive = await _context.Doctors
                .AnyAsync(d => d.Id == createAppointmentDto.DoctorId && d.Status == DoctorStatus.Active);
            if (!isDoctorActive)
                throw new KeyNotFoundException("Belirtilen doktor bulunamadı veya randevu kabul etmiyor");

            if (createAppointmentDto.AppointmentDate <= TimeHelper.NowInTurkey())
                throw new ArgumentException("Randevu tarihi geçmiştir");

            // Doktorun o saatte aktif başka randevusu var mı?
            var doctorHasConflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == createAppointmentDto.DoctorId &&
                a.AppointmentDate == createAppointmentDto.AppointmentDate &&
                a.AppointmentStatus != AppointmentStatus.Cancelled);
            if (doctorHasConflict)
                throw new InvalidOperationException("Doktorun bu saatte başka randevusu var");

            // Hastanın o saatte aktif başka randevusu var mı?
            var patientHasConflict = await _context.Appointments.AnyAsync(a =>
                a.PatientId == createAppointmentDto.PatientId &&
                a.AppointmentDate == createAppointmentDto.AppointmentDate &&
                a.AppointmentStatus != AppointmentStatus.Cancelled);
            if (patientHasConflict)
                throw new InvalidOperationException("Hastanın bu saatte başka randevusu var"); // DEĞİŞTİ

            var appointment = new Appointment
            {
                AppointmentDate = createAppointmentDto.AppointmentDate,
                DoctorId = createAppointmentDto.DoctorId,
                PatientId = createAppointmentDto.PatientId,
                AppointmentStatus = AppointmentStatus.Scheduled
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Doktor ve hasta adlarıyla birlikte dön
            return await GetDtoAsync(appointment.Id);
        }

        // Sadece Admin (controller'da [Authorize(Roles = "Admin")] ile korunur)
        public async Task<List<GetByIdAppointmentDto>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .AsNoTracking()
                .OrderByDescending(a => a.AppointmentDate)
                .Select(ToDto)
                .ToListAsync();
        }

        // Hastanın kendisi, ilişkili doktor, Admin
        public async Task<GetByIdAppointmentDto> GetAppointmentByIdAsync(int appointmentId)
        {
            // Yetki kontrolü için önce randevunun hangi hastaya ait olduğunu bul
            var patientId = await _context.Appointments
                .Where(a => a.Id == appointmentId)
                .Select(a => (int?)a.PatientId)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Randevu bulunamadı");

            await _access.EnsureCanAccessPatientAsync(patientId);

            return await GetDtoAsync(appointmentId);
        }

        // Doktorun kendisi, Admin
        public async Task<List<GetByIdAppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            await _access.EnsureCanAccessDoctorAsync(doctorId);

            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Bu Id'ye sahip doktor bulunmamaktadır");

            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(ToDto)
                .ToListAsync();
        }

        // Hastanın kendisi, ilişkili doktor, Admin
        public async Task<List<GetByIdAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            await _access.EnsureCanAccessPatientAsync(patientId);

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                throw new KeyNotFoundException("Hasta bulunamadı");

            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(ToDto)
                .ToListAsync();
        }

        // Randevunun sahibi olan hasta ve Admin
        public async Task<GetByIdAppointmentDto> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto)
        {
            await _access.EnsureCanUpdateAppointmentAsync(appointmentId);

            // Güncelleneceği için AsNoTracking kullanılmıyor
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId)
                ?? throw new KeyNotFoundException("Randevu bulunamadı");

            if (appointment.AppointmentStatus == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("İptal edilen randevu güncellenemez");

            if (updateAppointmentDto.AppointmentDate <= TimeHelper.NowInTurkey())
                throw new ArgumentException("Randevu tarihi geçmiştir");

            var isDoctorActive = await _context.Doctors
                .AnyAsync(d => d.Id == updateAppointmentDto.DoctorId && d.Status == DoctorStatus.Active);
            if (!isDoctorActive)
                throw new KeyNotFoundException("Doktor bulunamadı veya randevu kabul etmiyor");

            // Çakışma kontrolleri (randevunun kendisi hariç)
            var doctorHasConflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == updateAppointmentDto.DoctorId &&
                a.AppointmentDate == updateAppointmentDto.AppointmentDate &&
                a.Id != appointmentId &&
                a.AppointmentStatus != AppointmentStatus.Cancelled);
            if (doctorHasConflict)
                throw new InvalidOperationException("Doktorun bu saatte başka randevusu var");

            // PatientId güncellenebilen bir alan olmadığı için DTO'dan değil veritabanından alınıyor
            var patientHasConflict = await _context.Appointments.AnyAsync(a =>
                a.PatientId == appointment.PatientId &&
                a.AppointmentDate == updateAppointmentDto.AppointmentDate &&
                a.Id != appointmentId &&
                a.AppointmentStatus != AppointmentStatus.Cancelled);
            if (patientHasConflict)
                throw new InvalidOperationException("Hastanın bu saatte başka randevusu var"); // DEĞİŞTİ

            appointment.DoctorId = updateAppointmentDto.DoctorId;
            appointment.AppointmentDate = updateAppointmentDto.AppointmentDate;

            // Entity takip edildiği için _context.Update çağırmaya gerek yok
            await _context.SaveChangesAsync();

            // Randevunun güncel halini dön
            return await GetDtoAsync(appointmentId);
        }

        // Randevunun sahibi olan hasta, Admin
        public async Task CancelAppointmentAsync(int appointmentId)
        {
            await _access.EnsureCanCancelAppointmentAsync(appointmentId);

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId)
                ?? throw new KeyNotFoundException("Randevu bulunamadı");

            if (appointment.AppointmentStatus == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Bu randevu zaten iptal edildi");

            if (appointment.AppointmentDate <= TimeHelper.NowInTurkey())
                throw new InvalidOperationException("Geçmiş bir randevu iptal edilemez");

            // Silme yok, sadece durum değişiyor (soft cancel)
            appointment.AppointmentStatus = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        // Tek bir randevuyu doktor/hasta adlarıyla birlikte DTO olarak getirir.
        // Create ve Update sonrası güncel hali dönmek için kullanılır.
        private async Task<GetByIdAppointmentDto> GetDtoAsync(int appointmentId)
            => await _context.Appointments
                .AsNoTracking()
                .Where(a => a.Id == appointmentId)
                .Select(ToDto)
                .FirstAsync();
    }
}