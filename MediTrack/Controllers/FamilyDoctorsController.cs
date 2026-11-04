using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // giriş yapmamış kimse erişemez (401)
    public class FamilyDoctorsController : ControllerBase
    {
        private readonly IFamilyDoctorService _familyDoctorService;

        public FamilyDoctorsController(IFamilyDoctorService familyDoctorService)
        {
            _familyDoctorService = familyDoctorService;
        }

        // PUT api/familydoctors/change/12   body: { "doctorId": 5 }
        // 12 numaralı hastanın aile hekimini 5 numaralı doktor yap.
        // Hastanın kendisi veya Admin (hasta sadece kendi doktorunu değiştirebilir: AccessService)
        [HttpPut("change/{patientId:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public Task<IActionResult> ChangeFamilyDoctor(int patientId, ChangeFamilyDoctorDto changeFamilyDoctorDto) => Execute(async () =>
        {
            await _familyDoctorService.ChangeFamilyDoctorAsync(patientId, changeFamilyDoctorDto.DoctorId);
            return NoContent(); // 204
        });

        // GET api/familydoctors/doctor/7
        // Doktorun aile hekimliği geçmişi: doktorun kendisi veya Admin
        [HttpGet("doctor/{doctorId:int:min(1)}")]
        [Authorize(Roles = "Admin,Doctor")]
        public Task<IActionResult> GetAllByDoctor(int doctorId) => Execute(async () =>
            Ok(await _familyDoctorService.GetFamilyDoctorHistoryByFamilyDoctorIdAsync(doctorId)));

        // GET api/familydoctors/patient/12
        // Hastanın aile hekimi geçmişi: hastanın kendisi, ilişkili doktor veya Admin
        [HttpGet("patient/{patientId:int:min(1)}")]
        public Task<IActionResult> GetAllByPatient(int patientId) => Execute(async () =>
            Ok(await _familyDoctorService.GetFamilyDoctorHistoryByPatientIdAsync(patientId)));

        // Tüm endpoint'lerde aynı try/catch bloğunu tekrar yazmamak için ortak yardımcı
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
            catch (KeyNotFoundException ex)           // hasta veya doktor bulunamadı
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)      // hasta zaten bu doktora kayıtlı
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