

using MediTrack.Data;
using MediTrack.Dtos.AppointmentDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class AppointmentService : IAppointmentService
    {
// private → Yalnızca sınıf içerisinden erişilebilir.
//  readonly → _context alanına constructor dışında yeniden atama yapılamaz.
//MediTrackDbContext → Veritabanı işlemlerini gerçekleştiren sınıf.
        private readonly MediTrackDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; //Kullanıcı oluşturma, şifre doğrulama ve rol atama işlemlerini yönetir.
        private readonly RoleManager<IdentityRole> _roleManager;//Kullanıcı rolleri ile ilgili işlemleri yönetir.

        public AppointmentService(MediTrackDbContext context)
        {
            _context = context;

        }

        public Task<Appointment> CancelAppointmentAsync(int appointmentId)
        {
            throw new NotImplementedException();
        }

        public async  Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            if (appointment == null) {
                throw new ArgumentNullException(nameof(appointment));
                //1.Randevu tarihi geçmişte olamaz
                if (appointment.AppointmentDate<DateTime.Now)
                {
                    throw new ArgumentOutOfRangeException(
                         nameof(appointment.AppointmentDate),
                          "Appointment date must be in the future.");

                }

        //2.Doktor var mı
        var doctorExists=await _context.Doctors.AnyAsync();
                if (!doctorExists)
                {

                    throw new Exception("");

                }


                if (appointment.AppointmentDate < DateTime.Now)
                {
                    throw new Exception("");
                }




        //3.Hasta var mı
        var patient=await _context.Patients.AnyAsync();
        if(!patient)
                {
                    throw new Exception( "");
                }

        //4.Doktorun o saatte  başka randevusu var mı
        var doctorAppointment=_context.Doctors.AnyAsync(e=>)
        //5.kaydet

          


           
        }

        public Task<Appointment> GetAllAppointmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            throw new NotImplementedException();
        }

        public Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            throw new NotImplementedException();
        }
    }
}
