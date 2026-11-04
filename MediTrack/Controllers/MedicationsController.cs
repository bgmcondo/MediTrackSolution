using MediTrack.Dtos.MedicationDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    // [ApiController]: Otomatik model doğrulama (geçersiz istekte 400), parametre kaynağını
    // otomatik bağlama ve standart hata formatı sağlar.
    [ApiController]
    [Route("api/[controller]")]   // temel adres: api/medications
    [Authorize]                   // giriş yapmamış kimse erişemez (401)
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationsController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        // ---------------- İlaç kataloğu ----------------

        // POST api/medications
        // Kataloğa yeni ilaç ekleme: sadece Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Create(CreateMedicationDto createMedicationDto) => Execute(async () =>
        {
            var medication = await _medicationService.CreateMedicationAsync(createMedicationDto);
            return CreatedAtAction(nameof(GetById), new { id = medication.Id }, medication); // 201
        });

        // GET api/medications
        // Katalog listesi: giriş yapmış herkes (doktor reçete yazarken ilaç seçer)
        [HttpGet]
        public Task<IActionResult> GetAll() => Execute(async () =>
            Ok(await _medicationService.GetAllMedicationsAsync()));

        // GET api/medications/5
        [HttpGet("{id:int:min(1)}")]
        public Task<IActionResult> GetById(int id) => Execute(async () =>
            Ok(await _medicationService.GetMedicationByIdAsync(id)));

        // PUT api/medications/5
        // Katalogdaki ilacı güncelleme: sadece Admin
        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Update(int id, UpdateMedicationDto updateMedicationDto) => Execute(async () =>
            Ok(await _medicationService.UpdateMedicationAsync(id, updateMedicationDto)));

        // ---------------- Hastanın ilaç geçmişi ----------------

        // GET api/medications/patient/12
        // TIBBİ VERİ: hastanın kendisi ve ilişkili doktor. Admin bilerek yok.
        // Rol kontrolü burada, "hasta kendisi mi / doktor ilişkili mi" kontrolü serviste (AccessService).
        [HttpGet("patient/{patientId:int:min(1)}")]
        [Authorize(Roles = "Doctor,Patient")]
        public Task<IActionResult> GetAllByPatientId(int patientId) => Execute(async () =>
            Ok(await _medicationService.GetPatientMedicationsAsync(patientId)));

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
            catch (KeyNotFoundException ex)           // kayıt bulunamadı
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)      // aynı barkod zaten var
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