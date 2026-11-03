using MediTrack.Dtos.MedicationDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace MediTrack.Controllers
{
    
    [ApiController] //Otomatik model validation (geçersiz istekte 400 döner), parametre kaynağını otomatik bağlama ve standart hata formatı sağlar

    [Route("api/[controller]")]   // temel adres: api/patients
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationsController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }




        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<GetByIdMedicationDto>> GetById(int id)
        {
            try
            {
                var medication = await _medicationService.GetMedicationByIdAsync(id);
                return Ok(medication); // 200 + ilaç bilgisi

            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);

            }

        }


        [HttpGet]
        public async Task<ActionResult<List<GetByIdMedicationDto>>> GetAll()
        {
         
                var medication = await _medicationService.GetAllMedicationsAsync();
                return Ok(medication);
     
        }

        [HttpPut("{id:int:min(1)}")]
        public async Task<ActionResult<UpdateMedicationDto>> Update(int id, UpdateMedicationDto updateMedicationDto)
        {

            try
            {

                var medication = await _medicationService.UpdateMedicationAsync(id, updateMedicationDto);
                return Ok(medication);
            }

            catch (KeyNotFoundException ex)
            {

                return NotFound(ex.Message);
            }

            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("patient/{patientId:int:min(1)}")]   
        public async Task<ActionResult<List<GetByIdMedicationDto>>> GetAllByPatientId(int patientId)
        {
            try
            {
                var patientMedications = await _medicationService.GetPatientMedicationsAsync(patientId);
                return Ok(patientMedications);
            }

            catch (KeyNotFoundException ex)
            {

                return NotFound(ex.Message);

            }


        }

        [HttpPost]
        public async Task<ActionResult<GetByIdMedicationDto>>Create(CreateMedicationDto createMedicationDto){
            try
            {
                var medication = await _medicationService.CreateMedicationAsync(createMedicationDto);
                return CreatedAtAction(nameof(GetById), new { id = medication.Id }, medication);
            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);



            }


            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); //barkod zaten var


            }

    }


  

         


}
}