
using MediTrack.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Data
{
    // IdentityDbContext<ApplicationUser>:
    // ASP.NET Core Identity tablolarını ve kendi Entity'lerimizi
    // aynı DbContext üzerinden yönetmemizi sağlar.
    public class MediTrackDbContext : IdentityDbContext<ApplicationUser>
    {
        // Dependency Injection ile DbContext ayarlarını alıyoruz.
        // base(options), bu ayarları üst sınıfa gönderir.
        public MediTrackDbContext(
            DbContextOptions<MediTrackDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }      // bu satır yoksa eklemen lazım
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Medication> Medications { get; set; }

        public DbSet<AppointmentMedication> AppointmentMedications { get; set; }

        public DbSet<PatientFamilyDoctorHistory> patientFamilyDoctorHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity'nin kullanıcı, rol ve diğer tablo
            // yapılandırmalarını korumak için çağrılır.
            base.OnModelCreating(modelBuilder);

            // ============================================
            // 1. PATIENT
            // ============================================

            modelBuilder.Entity<Patient>(entity =>
            {
                // Patient -> Doctor
                //
                // Bir hastanın bir aktif aile hekimi vardır.
                // Bir doktorun birden fazla aktif hastası olabilir.
                //
                // Principal: Doctor
                // Dependent: Patient
                // Foreign Key: CurrentFamilyDoctorId
                //
                // Restrict:
                // Doktorun aktif hastaları varsa doktor silinemez.
                // Hasta silinirse doktor otomatik silinmez.

                entity.HasOne(p => p.CurrentFamilyDoctor)
                    .WithMany()
                    .HasForeignKey(p => p.CurrentFamilyDoctorId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                // Aynı TC kimlik numarasıyla ikinci bir
                // hasta kaydı oluşturulmasını engeller.
                entity.HasIndex(p => p.NationalId)
                    .IsUnique();

                // Patient -> ApplicationUser
                //
                // One-to-One ilişki.
                // Her hasta bir Identity kullanıcı hesabına bağlıdır.
                // Bir kullanıcı hesabı en fazla bir hasta
                // kaydıyla ilişkilendirilebilir.
                //
                // Principal: ApplicationUser
                // Dependent: Patient
                // Foreign Key: ApplicationUserId
                //
                // Restrict:
                // Bağlı hasta kaydı varsa kullanıcı silinemez.
                // Hasta silinirse kullanıcı otomatik silinmez.

                entity.HasOne(p => p.ApplicationUser)
                    .WithOne()
                    .HasForeignKey<Patient>(p => p.ApplicationUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // 2. DOCTOR
            // ============================================

            modelBuilder.Entity<Doctor>(entity =>
            {
                // Doctor -> ApplicationUser
                //
                // One-to-One ilişki.
                // Her doktorun bir Identity hesabı vardır.
                // Bir Identity hesabı en fazla bir doktor
                // kaydıyla ilişkilendirilebilir.
                //
                // Principal: ApplicationUser
                // Dependent: Doctor
                // Foreign Key: ApplicationUserId
                //
                // Restrict:
                // Doktor kaydı bağlıysa kullanıcı silinemez.
                // Doktor silinirse kullanıcı otomatik silinmez.

                entity.HasOne(d => d.ApplicationUser)
                    .WithOne()
                    .HasForeignKey<Doctor>(d => d.ApplicationUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // 3. APPOINTMENT
            // ============================================

            modelBuilder.Entity<Appointment>(entity =>
            {
                // Appointment -> Patient
                //
                // Bir hastanın birden fazla randevusu olabilir.
                // Her randevu yalnızca bir hastaya aittir.
                //
                // Principal: Patient
                // Dependent: Appointment
                // Foreign Key: PatientId
                //
                // Restrict:
                // Hastanın randevuları varsa hasta silinemez.
                // Randevu silinirse hasta silinmez.
                //
                // Geçmiş randevuları korumak için
                // Cascade yerine Restrict tercih edildi.

                entity.HasOne(a => a.Patient)
                    .WithMany()
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Appointment -> Doctor
                //
                // Bir doktorun birden fazla randevusu olabilir.
                // Her randevu yalnızca bir doktora aittir.
                //
                // Principal: Doctor
                // Dependent: Appointment
                // Foreign Key: DoctorId
                //
                // Restrict:
                // Doktorun randevuları varsa doktor silinemez.
                // Randevu silinirse doktor silinmez.

                entity.HasOne(a => a.Doctor)
                    .WithMany()
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // 4. MEDICATION
            // ============================================

            modelBuilder.Entity<Medication>(entity =>
            {
                // Aynı barkodla birden fazla ilaç
                // kaydı oluşturulmasını engeller.

                entity.HasIndex(m => m.Barcode)
                    .IsUnique();
            });

            // ============================================
            // 5. PATIENT FAMILY DOCTOR HISTORY
            // ============================================

            modelBuilder.Entity<PatientFamilyDoctorHistory>(entity =>
            {
                // History -> Patient
                //
                // Bir hastanın birden fazla aile hekimi
                // geçmiş kaydı olabilir.
                // Her geçmiş kaydı yalnızca bir hastaya aittir.
                //
                // Principal: Patient
                // Dependent: PatientFamilyDoctorHistory
                // Foreign Key: PatientId
                //
                // Restrict:
                // Hastanın geçmiş kayıtları varsa hasta silinemez.
                // Geçmiş kaydı silinirse hasta silinmez.

                entity.HasOne(h => h.Patient)
                    .WithMany()
                    .HasForeignKey(h => h.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // History -> Doctor
                //
                // Bir doktor birden fazla hastanın
                // aile hekimliği geçmişinde bulunabilir.
                // Her geçmiş kaydı tek bir doktora aittir.
                //
                // Principal: Doctor
                // Dependent: PatientFamilyDoctorHistory
                // Foreign Key: FamilyDoctorId
                //
                // Restrict:
                // Doktorun geçmiş kayıtları varsa doktor silinemez.
                // Geçmiş kaydı silinirse doktor silinmez.

                entity.HasOne(h => h.FamilyDoctor)
                    .WithMany()
                    .HasForeignKey(h => h.FamilyDoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // 6. APPOINTMENT MEDICATION
            // ============================================

            modelBuilder.Entity<AppointmentMedication>(entity =>
            {
                // Composite Primary Key
                //
                // AppointmentId ve MedicationId birlikte
                // Primary Key olarak kullanılır.
                //
                // Örneğin:
                // AppointmentId = 10, MedicationId = 5
                // kombinasyonu ikinci kez eklenemez.
                //
                // new { } anonim tip oluşturur.
                // Burada yeni veritabanı kaydı oluşturmaz.

                entity.HasKey(am => new
                {
                    am.AppointmentId,
                    am.MedicationId
                });

                // AppointmentMedication -> Appointment
                //
                // Bir randevuya birden fazla ilaç eklenebilir.
                // Her ilaç ilişki kaydı bir randevuya aittir.
                //
                // Principal: Appointment
                // Dependent: AppointmentMedication
                // Foreign Key: AppointmentId
                //
                // Restrict:
                // Randevuya bağlı ilaç kayıtları varsa
                // randevu silinemez.
                // İlaç ilişki kaydı silinirse randevu silinmez.

                entity.HasOne(am => am.Appointment)
                    .WithMany()
                    .HasForeignKey(am => am.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // AppointmentMedication -> Medication
                //
                // Bir ilaç birden fazla randevuda kullanılabilir.
                // Her ilaç ilişki kaydı tek bir ilaca bağlıdır.
                //
                // Principal: Medication
                // Dependent: AppointmentMedication
                // Foreign Key: MedicationId
                //
                // Restrict:
                // İlaç randevularda kullanılmışsa
                // katalogdaki ilaç kaydı silinemez.
                //
                // AppointmentMedication kaydı silinirse
                // Medication kaydı silinmez.

                entity.HasOne(am => am.Medication)
                    .WithMany()
                    .HasForeignKey(am => am.MedicationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
