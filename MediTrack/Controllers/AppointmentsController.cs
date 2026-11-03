using MediTrack.Dtos.AppointmentDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {

        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;

        }

        // İŞ BÖLÜMÜ:
        // - Servis: İş kurallarını kontrol eder (hasta var mı, doktor var mı, saat dolu mu).
        //   Bir sorun bulursa exception fırlatır. HTTP'den (404, 409 vb.) haberi yoktur.
        // - Controller: Servisin fırlattığı exception'ı yakalar ve istemciye
        //   uygun HTTP cevabına çevirir.
        //
        // Controller'da catch OLMAZSA: Exception yakalanmaz, ASP.NET Core bunu
        // beklenmeyen hata sayar ve istemci "Hasta bulunamadı" yerine
        // 500 Internal Server Error alır.
        //
        // Akış:
        //   Servis: hasta yok → throw KeyNotFoundException("Hasta bulunamadı")
        //   Controller catch varsa → 404 { "message": "Hasta bulunamadı" }
        //   Controller catch yoksa → 500 Internal Server Error
        //
        // NOT: Bu catch bloklarını her controller'da tekrar yazmamak için
        // global exception handler (IExceptionHandler) kullanılabilir.
        // O zaman bu try-catch'ler silinebilir.
        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<GetByIdAppointmentDto>> Create(CreateAppointmentDto createAppointmentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;   // TOKEN DAN USER İD BİLGİSİNİ ALIYORUZ 
            var isAdmin = User.IsInRole("Admin"); // KİŞİ ADMIN ROLÜNDE Mİ DEĞİL Mİ BUNU KONTROL EDİYORUZ
            if (!isAdmin && !await _appointmentService.IsPatientOwnedByUserAsync(createAppointmentDto.PatientId, userId))
                return Forbid(); // 403 Forbidden: Admin değilse ve kendi randevusunu oluşturuyorsa izin ver, yoksa yasakla.)
            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);
                // 201 Created: Yeni kayıt oluşturuldu.
                // Response header'ına yeni kaydın adresini (Location: api/appointments/{id}) ekler.
                return CreatedAtAction(
                nameof(GetById),          // hangi action'a yönlendirilecek
                new { id = appointment.Id },  // o action'ın route parametreleri   // Anonim tip (yaygın kullanım)
                 appointment);                 // yanıt gövdesi
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); //404

            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });     // 400  //istek verisi hatalı geçmişteki randevu gibi
            }
        }




        [HttpGet]

        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<List<GetByIdAppointmentDto>>> GetAll()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); //404
            }

        }
        [HttpGet("patient/{patientId:int:min(1)}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<List<GetByIdAppointmentDto>>> GetAllByPatient(int patientId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;   // TOKEN DAN USER İD BİLGİSİNİ ALIYORUZ
            //Admin ve doctor her hastanın randevusunu görebilir. Patient sadece kendi randevusunu görebilir.
            if (!User.IsInRole("Admin") && !User.IsInRole("Doctor") && !await _appointmentService.IsPatientOwnedByUserAsync(patientId, userId))
                return Forbid(); // 403 Forbidden: Admin veya doctor değilse ve bu hasta kendi değilse)
            try
            {
                var appointmentsPatient = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
                return Ok(appointmentsPatient);
            }

            catch (KeyNotFoundException ex)
            {

                return NotFound(new { message = ex.Message });

            }


        }

        [HttpGet("doctor/{doctorId:int:min(1)}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<List<GetByIdAppointmentDto>>> GetAllByDoctor(int doctorId)
        {
            try
            {
                var appointmentsDoctor = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
                return Ok(appointmentsDoctor);
            }


            catch (KeyNotFoundException ex)
            {

                return NotFound(new { message = ex.Message });

            }


        }

        [HttpGet("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<GetByIdAppointmentDto>> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;   // TOKEN DAN USER İD BİLGİSİNİ ALIYORUZ
            //Admin ve doctor her hastanın randevusunu görebilir. Patient sadece kendi randevusunu görebilir.
            if (!User.IsInRole("Admin") && !User.IsInRole("Doctor") && !await _appointmentService.IsAppointmentOwnedByUserAsync(id, userId))
                return Forbid(); // 403 Forbidden: Admin veya doctor değilse ve kendi randevusunu görüyorsa izin ver, yoksa yasakla.)
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                return Ok(appointment);
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });


            }

        }


        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<UpdateAppointmentDto>> Update(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            // Giriş yapan kullanıcının id'sini token'dan alıp kontrol et. 
            // Login de token'a nameIdentifier  olarak kullanıcı id'si eklenmişti.
            //[Authorize] olduğu için 

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Kullanıcı admin mi?(Token'da role claim'e bakar)
            var isAdmin = User.IsInRole("Admin");

            //Sahiplik kontrolü:
            //Admin her randevuyu değiştirebilir.
            //Hasta sadece kendi randevusunu değiştirebilir.
            //    Randevu başkasına aitse (veya hiç yoksa) 403 Forbidden döner.
            //    Randevu yoksa da 403 dönmesi bilinçli: hasta, başkalarının randevu Id'lerinin
            //    var olup olmadığını bu yoldan öğrenemesin.
            if (!isAdmin && !await _appointmentService.IsAppointmentOwnedByUserAsync(id, userId))
                return Forbid();
            try
            {
                var appointment = await _appointmentService.UpdateAppointmentAsync(id, updateAppointmentDto);


                return appointment;
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); //404
            }

            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });    // 400

            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }



        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;   // TOKEN DAN USER İD BİLGİSİNİ ALIYORUZ

            var isAdmin = User.IsInRole("Admin"); // KİŞİ ADMIN ROLÜNDE Mİ DEĞİL Mİ BUNU KONTROL EDİYORUZ

            //Sahiplik kontrolü:
            if (!isAdmin && !await _appointmentService.IsAppointmentOwnedByUserAsync(id, userId))
                return Forbid();

            try
            {
                await _appointmentService.CancelAppointmentAsync(id);

                return NoContent();

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });


            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); //isteğin kaynağın mevcut durumuyla çelişmesi.

            }




        }
    }
}
