using MediTrack.Dtos.PatientDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [ApiController] //Otomatik model validation (geçersiz istekte 400 döner), parametre kaynağını otomatik bağlama ve standart hata formatı sağlar

    [Route("api/[controller]")]   // temel adres: api/patients
    public class PatientsController : ControllerBase
    {

        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
            
        }
        [HttpGet("{id:int:min(1)}")] //Sadece get isteklerini karşılar
        public async Task<ActionResult<GetByIdPatientDto>> GetById(int id){

            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id); // Servisten hastayı getir
                return Ok(patient); // 200 OK + hasta bilgisi
            }
            catch (KeyNotFoundException ex) // Servis hastayı bulamazsa bu hatayı fırlatır
            {
                return NotFound(new { message = ex.Message }); // 404 + "Hasta bulunamadı"
            }




        }

        [HttpGet] //Sadece get isteklerini karşılar
        public async Task<ActionResult<List<GetByIdPatientDto>>> GetAll() {

         
                var patients = await _patientService.GetAllPatientsAsync();
                return Ok(patients);
       

        }

        [HttpPut("{id:int:min(1)}")]

        public async Task<ActionResult<UpdatePatientDto>> Update(int id,UpdatePatientDto updatePatientDto)
        {
            try
            {
                var patient=await _patientService.UpdatePatientAsync(id,updatePatientDto);
                return Ok(patient);

            }

            catch (KeyNotFoundException ex)
            {
                return NotFound(new {message=ex.Message});



            }


        }


   

        
    }
}
