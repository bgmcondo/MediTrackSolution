using MediTrack.Dtos.PatientDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    // [ApiController]: Otomatik model doğrulama (geçersiz istekte 400), parametre kaynağını
    // otomatik bağlama ve standart hata formatı sağlar.
    [ApiController]
    [Route("api/[controller]")]   // temel adres: api/patients
    [Authorize]                   // giriş yapmamış kimse erişemez (401)
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET api/patients
        // Tüm hastalar (TC kimlik numaraları dahil): SADECE Admin.
        // Serviste sahiplik kontrolü yapılamaz (belirli bir hastaya ait değil), tek koruma bu rol.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> GetAll() => Execute(async () =>
            Ok(await _patientService.GetAllPatientsAsync()));

        // GET api/patients/12
        // Hastanın kendisi, ilişkili doktor veya Admin (kontrol serviste: AccessService)
        [HttpGet("{id:int:min(1)}")]
        public Task<IActionResult> GetById(int id) => Execute(async () =>
            Ok(await _patientService.GetPatientByIdAsync(id)));

        // PUT api/patients/12
        // Hastanın kendisi veya Admin. Doctor rolü burada engelleniyor,
        // "hasta sadece kendi bilgisini değiştirebilir" kontrolü serviste.
        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin,Patient")]
        public Task<IActionResult> Update(int id, UpdatePatientDto updatePatientDto) => Execute(async () =>
            Ok(await _patientService.UpdatePatientAsync(id, updatePatientDto)));

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
            catch (KeyNotFoundException ex)           // hasta bulunamadı
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