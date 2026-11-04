using MediTrack.Dtos.DoctorDto;
using MediTrack.Enums;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // giriş yapmış herkes (Admin, Doctor, Patient) okuyabilir
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        private const string PendingDeletionMessage =
            "Doktorun bitmemiş randevuları veya kayıtlı hastaları var. Yeni randevu alınamayacak şekilde işaretlendi; bunlar bitince tekrar silebilirsiniz.";

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET api/doctors
        // Doktor listesi: randevu alırken doktor seçmek için herkes görebilir
        [HttpGet]
        public Task<IActionResult> GetAllDoctors() => Execute(async () =>
            Ok(await _doctorService.GetAllDoctorsAsync()));

        // GET api/doctors/7
        [HttpGet("{id:int:min(1)}")]
        public Task<IActionResult> GetDoctorById(int id) => Execute(async () =>
            Ok(await _doctorService.GetDoctorByIdAsync(id)));

        // GET api/doctors/family-doctors
        // Aile hekimi listesi: hasta aile hekimi seçerken görür. Boşsa [] döner.
        [HttpGet("family-doctors")]
        public Task<IActionResult> GetAllFamilyDoctors() => Execute(async () =>
            Ok(await _doctorService.GetFamilyDoctorsAsync()));

        // GET api/doctors/family-doctors/7
        [HttpGet("family-doctors/{id:int:min(1)}")]
        public Task<IActionResult> GetFamilyDoctorById(int id) => Execute(async () =>
            Ok(await _doctorService.GetFamilyDoctorByIdAsync(id)));

        // PUT api/doctors/7
        // Doktor bilgisi güncelleme: sadece Admin
        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Update(int id, UpdateDoctorDto updateDoctorDto) => Execute(async () =>
            Ok(await _doctorService.UpdateDoctorAsync(id, updateDoctorDto)));

        // DELETE api/doctors/7
        // Doktor silme (soft delete): sadece Admin
        // - Bitmemiş randevusu veya kayıtlı hastası varsa: PendingDeletion olarak işaretlenir -> 409
        // - Yoksa: Deleted olarak işaretlenir -> 204
        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Delete(int id) => Execute(async () =>
        {
            var status = await _doctorService.DeleteDoctorAsync(id);

            if (status == DoctorStatus.PendingDeletion)
                return Conflict(new { status = status.ToString(), message = PendingDeletionMessage }); // 409

            return NoContent(); // 204
        });

        // Tüm endpoint'lerde aynı try/catch bloğunu tekrar yazmamak için ortak yardımcı
        private async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)   // yetki yok
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)           // kayıt bulunamadı
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)      // mevcut durumla çakışma
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)              // geçersiz veri
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}