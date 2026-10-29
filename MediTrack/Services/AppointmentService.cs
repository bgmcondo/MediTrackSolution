using MediTrack.Data;
using MediTrack.Dtos.AppointmentDto;
using MediTrack.Entities;
using MediTrack.Helpers;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Channels;
using System.Xml.Schema;
// Microsoft.AspNetCore.Identity burada kullanılmıyorsa silebilirsin.

namespace MediTrack.Services
{
    public class AppointmentService : IAppointmentService
    {
        // 0) ÖNCE: DbContext'i constructor üzerinden al (Dependency Injection).
        //    - private readonly AppDbContext _context; şeklinde bir alan tanımla.
        //    - Constructor parametresi olarak al ve bu alana ata.
        //    - Program.cs'de builder.Services.AddScoped<IAppointmentService, AppointmentService>(); kayıtlı olduğundan emin ol.

        // NOT: Tüm metotların başına "async" ekle, içeride EF Core'un async metotlarını "await" ile kullan.
        private readonly MediTrackDbContext _context;
        public AppointmentService(MediTrackDbContext  context)
        {
            _context = context;
             
        }

        public async Task CancelAppointmentAsync(int appointmentId)
        {
            // 1) _context.Appointments içinden appointmentId'ye göre randevuyu bul (FindAsync veya FirstOrDefaultAsync).
            var appointment= await _context.Appointments.FirstOrDefaultAsync(e=>e.Id==appointmentId);
            // 2) Bulunamadıysa exception fırlat (ör. KeyNotFoundException veya kendi NotFoundException'ın).
            if(appointment==null)
                throw new KeyNotFoundException("Randevu bulunamadı");
            // 3) Randevu zaten iptal edilmişse (Status == Cancelled) exception fırlat.
            if(appointment.AppointmentStatus==AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Bu randevu zaten iptal edildi");
            // 4) Randevu tarihi geçmişse iptale izin verme, exception fırlat.
            if (appointment.AppointmentDate <= TimeHelper.NowInTurkey())
                throw new InvalidOperationException("Geçmiş bir randevu iptal edilemez");
            // 5) Randevuyu SİLME; Status alanını Cancelled olarak güncelle (soft cancel).
              appointment.AppointmentStatus = AppointmentStatus.Cancelled;
            // 6) SaveChangesAsync ile kaydet.
                await _context.SaveChangesAsync();
        }

        public async Task<CreateAppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
        {
            // 1) DTO'daki PatientId ile hasta var mı kontrol et; yoksa exception fırlat.
            var isPatient = await _context.Appointments.AnyAsync(e => e.PatientId == createAppointmentDto.PatientId);
            if (!isPatient)
                throw new InvalidOperationException("Belirtilen hasta bulunamadı");
            // 2) DTO'daki DoctorId ile doktor var mı kontrol et; yoksa exception fırlat.
            var isDoctor = await _context.Appointments.AnyAsync(e => e.DoctorId == createAppointmentDto.DoctorId);
            if (!isDoctor)
                throw new InvalidOperationException("Belirtilen doktor bulunamadı");
            // 3) Randevu tarihi geçmişte mi kontrol et; geçmişteyse exception fırlat.
            var appointmentTime = createAppointmentDto.AppointmentDate;
            if (appointmentTime <= TimeHelper.NowInTurkey())
                throw new InvalidOperationException("Randevu tarihi geçmiştir");
            // 4) Aynı doktorun aynı tarih/saatte aktif (iptal edilmemiş) başka randevusu var mı kontrol et (AnyAsync);
            //    varsa çakışma olduğu için exception fırlat.
            var anyAppointmentDoctor = await _context.Doctors.Include(d => d.Appointments).AnyAsync(d => d.Id == createAppointmentDto.DoctorId && d.Appointments.Any(a => a.AppointmentDate == createAppointmentDto.AppointmentDate));
            if (anyAppointmentDoctor)
                throw new InvalidOperationException("Doktorun bu saatte başka randevusu var.");
            // 5) (İsteğe bağlı) Aynı hastanın aynı saatte başka randevusu var mı kontrol et.
            var anyAppointmentPatient = await _context.Patients.Include(d => d.Appointments).AnyAsync(p => p.Id == createAppointmentDto.PatientId && p.Appointments.Any(a => a.AppointmentDate == createAppointmentDto.AppointmentDate));
            if (anyAppointmentPatient)
                throw new InvalidOperationException("Bu saatte başka bir randevunuz var");
            // 6) DTO'dan yeni bir Appointment entity'si oluştur (alanları tek tek eşle),
            //    Status'u başlangıç değeri olarak ayarla (ör. Scheduled / Pending).
            var appointment = new Appointment
            {
                AppointmentDate = createAppointmentDto.AppointmentDate,
                DoctorId = createAppointmentDto.DoctorId,
                PatientId = createAppointmentDto.PatientId,
                AppointmentStatus = AppointmentStatus.Scheduled




            };
            // 7) _context.Appointments.AddAsync ile ekle, SaveChangesAsync ile kaydet.
            //Add ne yapar? Sadece EF Core’un bellekteki takip listesine(Change Tracker) “bu nesne yeni, eklenecek” diye işaret koyar.
            //Veritabanına gitmez, //ağ / disk işlemi yoktur, anında biter.Beklenecek bir şey olmadığı için asenkron olmasına gerek yok.
            _context.Appointments.Add(appointment);

            await _context.SaveChangesAsync();

            // 8) Sonucu DTO olarak dön.
            //    (Öneri: Dönüş tipini GetByIdAppointmentDto yaparsan oluşan Id'yi de dönebilirsin.)

            return new CreateAppointmentDto
            {
                AppointmentDate = appointment.AppointmentDate,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,

            };


            }
        public async Task<List<GetByIdAppointmentDto>> GetAllAppointmentsAsync()
        {
            // 1) _context.Appointments üzerinden sorgu başlat.
              var appointmentsEntity=await _context.Appointments.OrderByDescending(a=>a.AppointmentDate).Include(a=>a.Doctor).Include(a=>a.Patient).AsNoTracking().ToListAsync();
              var appointmentList = new List<GetByIdAppointmentDto>();
            foreach (var appointment in appointmentsEntity)
                appointmentList.Add(new GetByIdAppointmentDto
                {
                    Id = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    DoctorFullName = appointment.Doctor.FirstName + " " + appointment.Doctor.LastName,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentStatus = appointment.AppointmentStatus,
                    PatientFullName=appointment.Patient.FirstName+ " "+appointment.Patient.LastName



                });


            // 2) Sadece okuma yapılacağı için AsNoTracking() kullan.
            // 3) DTO'da doktor/hasta adı gibi bilgiler varsa Select içinde ilgili navigation property'lerden al
            //    (Select kullanırsan ayrıca Include'a gerek kalmaz).
            // 4) İstersen tarihe göre sırala (OrderBy / OrderByDescending).
            // 5) Select ile her entity'yi GetByIdAppointmentDto'ya dönüştür.
            // 6) ToListAsync ile listeyi döndür.
            return appointmentList;
        }

        public async Task<GetByIdAppointmentDto> GetAppointmentByIdAsync(int appointmentId)
        {
            // 1) _context.Appointments içinde Id == appointmentId olan kaydı ara.
            var appointment=await _context.Appointments.Include(e=>e.Doctor).Include(e=>e.Patient).AsNoTracking().FirstOrDefaultAsync(a=>a.Id == appointmentId);
            // 2) AsNoTracking() kullan.
            // 3) Select ile GetByIdAppointmentDto'ya dönüştür.
            // 4) FirstOrDefaultAsync ile tek kayıt al.
            // 5) Sonuç null ise exception fırlat (NotFound).
            if (appointment == null)
                throw new KeyNotFoundException("Randevu bulunmamaktadır");

            return new GetByIdAppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DoctorFullName = appointment.Doctor.FirstName + " " + appointment.Doctor.LastName,
                PatientFullName = appointment.Patient.FirstName + " " + appointment.Patient.LastName,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentStatus= appointment.AppointmentStatus,

            };

            // 6) DTO'yu döndür.
        }

        public async Task<List<GetByIdAppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            // 1) (İsteğe bağlı) Böyle bir doktor var mı kontrol et; yoksa exception fırlat.
            var doctorExists=await _context.Doctors.AsNoTracking().AnyAsync(d=>d.Id == doctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Bu Id'ye sahip doktor bulunmamaktadır");
            // 2) _context.Appointments içinde DoctorId == doctorId olanları filtrele (Where).
            var appointmentsDoctorEntity = await _context.Appointments.Include(a=>a.Doctor).Include(a=>a.Patient).Where(a=>a.DoctorId == doctorId).OrderByDescending(a=>a.AppointmentDate).AsNoTracking().ToListAsync();

            // 3) AsNoTracking() kullan.
            // 4) Tarihe göre sırala.
         
            // 5) Select ile DTO'ya dönüştür, ToListAsync ile döndür.
            var appointmentsDoctor=new List<GetByIdAppointmentDto>();
            foreach (var appointment in appointmentsDoctorEntity)
                appointmentsDoctor.Add(
                    new GetByIdAppointmentDto
                    {
                        Id=appointment.Id,
                        PatientId=appointment.PatientId,
                        DoctorId=appointment.DoctorId,
                        DoctorFullName=appointment.Doctor.FirstName+" "+appointment.Doctor.LastName,
                        PatientFullName=appointment.Patient.FirstName+" "+appointment.Patient.LastName,
                        AppointmentDate=appointment.AppointmentDate,
                        AppointmentStatus=appointment.AppointmentStatus

                    }
                    
                    
                    
                    );
             return appointmentsDoctor;

            //    (Kayıt yoksa boş liste dönmesi normaldir, exception fırlatma.)
          
        }

        public async Task<List<GetByIdAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            // 1) (İsteğe bağlı) Böyle bir hasta var mı kontrol et; yoksa exception fırlat.
            var patientExists = await _context.Patients.AnyAsync(p=>p.Id== patientId);
            if (!patientExists)
                throw new KeyNotFoundException("Hasta bulunamamıştır");
            // 2) _context.Appointments içinde PatientId == patientId olanları filtrele (Where).
            var appointmentsPatientEntity=await _context.Appointments.Include(a=>a.Doctor).Include(a=>a.Patient).Where(a=>a.PatientId== patientId).OrderByDescending(a=>a.AppointmentDate).AsNoTracking().ToListAsync();
            var appointmentsPatient=new List<GetByIdAppointmentDto>();

            foreach (var appointment in appointmentsPatientEntity)
                appointmentsPatient.Add(new GetByIdAppointmentDto
                {
                    Id= appointment.Id,
                    DoctorId=appointment.DoctorId,
                    DoctorFullName=appointment.Doctor.FirstName+" "+appointment.Doctor.LastName,
                    PatientId=appointment.PatientId,
                    PatientFullName=appointment.Patient.FirstName+" "+appointment.Patient.LastName,
                    AppointmentDate=appointment.AppointmentDate,
                    AppointmentStatus=appointment.AppointmentStatus,


                });

            return appointmentsPatient;
            // 3) AsNoTracking() kullan.
            // 4) Tarihe göre sırala.
            // 5) Select ile DTO'ya dönüştür, ToListAsync ile döndür.
            //    (Kayıt yoksa boş liste dönmesi normaldir, exception fırlatma.)
         
        }

        public async Task<UpdateAppointmentDto> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDto updateAppointmentDto)
        {
            // 1) appointmentId ile randevuyu bul (AsNoTracking KULLANMA, çünkü güncelleyeceksin).
             var appointment=await _context.Appointments.FirstOrDefaultAsync(a=>a.Id==appointmentId);
            // 2) Bulunamadıysa exception fırlat.
            if (appointment == null)
                throw new KeyNotFoundException("Randevu bulunamamıştır");
            // 3) Randevu iptal edilmişse güncellemeye izin verme, exception fırlat.
            if (appointment.AppointmentStatus == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("İptal edilen randevu güncellenemez.");
            // 4) Yeni tarih geçmişteyse exception fırlat
            if (updateAppointmentDto.AppointmentDate <= TimeHelper.NowInTurkey())
                throw new InvalidOperationException("Randevu zamanı geçmiştir");
            //Doktor var mı kontrol 
            var doctorExists = await _context.Doctors.AnyAsync(e => e.Id == updateAppointmentDto.DoctorId);
            if (!doctorExists)
                throw new KeyNotFoundException("Doktor bulunamadı");
            // 5) Tarih veya doktor değişiyorsa çakışma kontrolü yap:
            var anyAppointmentDoctor = await _context.Appointments.AnyAsync(a => a.DoctorId== updateAppointmentDto.DoctorId &&a.AppointmentDate==updateAppointmentDto.AppointmentDate&&a.Id!=appointmentId&&a.AppointmentStatus!=AppointmentStatus.Cancelled);
            if (anyAppointmentDoctor)
                throw new InvalidOperationException("Bu saatte doktorun başka randevusu var");
            
            var anyAppoinmentPatient = await _context.Appointments.AnyAsync(a=>a.PatientId==updateAppointmentDto.PatientId&&a.AppointmentDate==updateAppointmentDto.AppointmentDate&&a.Id!=appointmentId&&a.AppointmentStatus!=AppointmentStatus.Cancelled);
            //    aynı doktorun o saatte başka aktif randevusu var mı (kendi Id'sini hariç tutarak: a.Id != appointmentId).
            if (anyAppoinmentPatient)
                throw new InvalidOperationException("Bu saatte doktorun başka randevusu var");
            // 6) DTO'daki değerleri entity'nin alanlarına ata.
            appointment.DoctorId=updateAppointmentDto.DoctorId;
            appointment.AppointmentDate=updateAppointmentDto.AppointmentDate;

            await _context.SaveChangesAsync();

            return new UpdateAppointmentDto
            {
                DoctorId=updateAppointmentDto.DoctorId,
                AppointmentDate=updateAppointmentDto.AppointmentDate,


            };
           

            // 7) SaveChangesAsync ile kaydet (entity zaten takip edildiği için _context.Update çağırmana gerek yok).
            // 8) Sonucu DTO olarak dön.
            //    (Öneri: Dönüş tipini GetByIdAppointmentDto yaparsan randevunun güncel halini dönebilirsin.)

        }
    }
}