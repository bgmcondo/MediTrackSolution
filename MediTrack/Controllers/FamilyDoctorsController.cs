using MediTrack.Dtos.FamilyDoctorDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamilyDoctorsController : ControllerBase
    {
        private readonly IFamilyDoctorService _familyDoctorService;

        public FamilyDoctorsController(IFamilyDoctorService familyDoctorService)
        {
            _familyDoctorService = familyDoctorService;
         
        }
        [HttpPut("change/{familyDoctorId:int:min(1)}")]
        public async Task<ActionResult> ChangeFamilyDoctor(int familyDoctorId, ChangeFamilyDoctorDto changeFamilyDoctorDto)
        {
            try
            {
               await _familyDoctorService.ChangeFamilyDoctorAsync(familyDoctorId, changeFamilyDoctorDto.DoctorId);
               return NoContent();
            }

            catch (KeyNotFoundException ex) 
            {
                return NotFound(ex.Message);




            }      
        }

        [HttpGet("doctor/{doctorId:int:min(1)}")]
        public async Task<ActionResult<List<GetByIdFamilyDoctorHistoryDto>>> GetAllByDoctor(int doctorId) {

            try {
                var history=await _familyDoctorService.GetFamilyDoctorHistoryByFamilyDoctorIdAsync(doctorId);
                return Ok(history);

            }

            catch (KeyNotFoundException ex) {

                return NotFound(ex.Message);


            }
        }

        [HttpGet("patient/{patientId:int:min(1)}")]
        public async Task<ActionResult<List<GetByIdFamilyDoctorHistoryDto>>> GetAllByPatient(int patientId)
        {
            try
            {
                var history = await _familyDoctorService.GetFamilyDoctorHistoryByPatientIdAsync(patientId);
                return Ok(history);
            }

            catch (KeyNotFoundException ex)
            {


                return NotFound(ex.Message);

            }
        }




    }
}
