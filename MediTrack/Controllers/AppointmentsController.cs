using MediTrack.Dtos.AppointmentDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // giriş yapmamış kimse erişemez (401)
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // İŞ BÖLÜMÜ:
        // - Controller ([Authorize]): Bu endpoint'i hangi ROL çağırabilir? (sadece token'a bakar)
        // - Servis + AccessService: Bu kişi bu KAYDA erişebilir mi? (hasta kendisi mi,
        //   doktor ilişkili mi?) ve iş kuralları (hasta var mı, saat dolu mu).
        //   Bir sorun bulursa exception fırlatır. HTTP'den (404, 409 vb.) haberi yoktur.
        // - Execute: Servisin fırlattığı exception'ı yakalar ve uygun HTTP cevabına çevirir.
        //
        // Akış:
        //   Servis: yetki yok    → throw UnauthorizedAccessException → 403
        //   Servis: hasta yok    → throw KeyNotFoundException        → 404
        //   Servis: saat dolu    → throw InvalidOperationException   → 409
        //   Servis: geçmiş tarih → throw ArgumentException           → 400
        //   Yakalanmayan exception → 500 Internal Server Error

        // POST api/appointments
        // Admin: herhangi bir hasta için, Patient: sadece kendisi için
        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        public Task<IActionResult> Create(CreateAppointmentDto createAppointmentDto) => Execute(async () =>
        {
            var appointment = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);

            // 201 Created: Response header'ına yeni kaydın adresini (Location: api/appointments/{id}) ekler
            return CreatedAtAction(
                nameof(GetById),               // hangi action'a yönlendirilecek
                new { id = appointment.Id },   // o action'ın route parametreleri
                appointment);                  // yanıt gövdesi
        });

        // GET api/appointments
        // Tüm randevular: sadece Admin (serviste sahiplik kontrolü yok, koruma bu rolde)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> GetAll() => Execute(async () =>
            Ok(await _appointmentService.GetAllAppointmentsAsync()));

        // GET api/appointments/patient/12
        // Hastanın kendisi, ilişkili doktor (aile hekimi veya randevusu olan), Admin
        [HttpGet("patient/{patientId:int:min(1)}")]
        public Task<IActionResult> GetAllByPatient(int patientId) => Execute(async () =>
            Ok(await _appointmentService.GetAppointmentsByPatientIdAsync(patientId)));

        // GET api/appointments/doctor/7
        // Doktorun kendisi, Admin
        [HttpGet("doctor/{doctorId:int:min(1)}")]
        [Authorize(Roles = "Admin,Doctor")]
        public Task<IActionResult> GetAllByDoctor(int doctorId) => Execute(async () =>
            Ok(await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId)));

        // GET api/appointments/5
        // Randevunun hastası, ilişkili doktor, Admin
        [HttpGet("{id:int:min(1)}")]
        public Task<IActionResult> GetById(int id) => Execute(async () =>
            Ok(await _appointmentService.GetAppointmentByIdAsync(id)));

        // PUT api/appointments/5
        // Randevunun sahibi olan hasta, Admin
        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public Task<IActionResult> Update(int id, UpdateAppointmentDto updateAppointmentDto) => Execute(async () =>
            Ok(await _appointmentService.UpdateAppointmentAsync(id, updateAppointmentDto)));

        // DELETE api/appointments/5
        // Randevunun sahibi olan hasta, Admin.
        // Kayıt silinmez, durumu Cancelled yapılır (soft cancel).
        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public Task<IActionResult> Cancel(int id) => Execute(async () =>
        {
            await _appointmentService.CancelAppointmentAsync(id);
            return NoContent(); // 204
        });

        // Tüm endpoint'lerde aynı try/catch bloğunu tekrar yazmamak için ortak yardımcı.
        // NOT: Global exception handler (IExceptionHandler) kullanılırsa bu metot da silinebilir.
        private async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)   // yetki yok (AccessService)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)           // kayıt bulunamadı
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)      // çakışma, zaten iptal edilmiş vb.
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)              // geçersiz veri (geçmiş tarih vb.)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}