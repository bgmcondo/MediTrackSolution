using MediTrack.Dtos.DoctorDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediTrack.Enums;

namespace MediTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        public DoctorsController(IDoctorService doctorService)

        {

            _doctorService = doctorService;


        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<List<GetByIdDoctorDto>>> GetAllDoctors()
        {

           
                var doctors = await _doctorService.GetAllDoctorsAsync();
                return Ok(doctors);


        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]

        public async Task<ActionResult<GetByIdDoctorDto>> GetDoctorById(int id)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(id);
                return Ok(doctor);
            }
            catch (KeyNotFoundException ex)
            {
                 
              return NotFound(new {message=ex.Message});

            }
        }

        [HttpGet("familyDoctor/{familyDoctorId:int}")]

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<GetByIdDoctorDto>> GetFamilyDoctorById(int familyDoctorId)
        {
            try
            {
                var familyDoctor = await _doctorService.GetFamilyDoctorByIdAsync(familyDoctorId);
               
                return Ok(familyDoctor);
            }
            catch (KeyNotFoundException ex)
            {

                return NotFound(new {message=ex.Message});

            }
        }

        [HttpGet("familyDoctors")]

        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<List<GetByIdDoctorDto>>> GetAllFamilyDoctors()
        {

            try
            {
                var familyDoctors = await _doctorService.GetFamilyDoctorsAsync();
                return Ok(familyDoctors);
            }

            catch(KeyNotFoundException ex)
            {

                return NotFound(new {message=ex.Message});
            }

          
        }


        [HttpPut("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateDoctorDto>>  Update(int id, UpdateDoctorDto updateDoctorDto)
        {
            try
            {
                var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, updateDoctorDto);

                return Ok(updatedDoctor);
            }
            catch (KeyNotFoundException ex)
            {

                return NotFound(new {message=ex.Message});
            }



        }

        [HttpDelete("{id:int:min(1)}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
               var status= await _doctorService.DeleteDoctorAsync(id);
                if (status == DoctorStatus.PendingDeletion)
                    return Accepted(new { message = "Doktorun bitmemiş randevuları veya kayıtlı hastaları var. Yeni randevu alınamayacak şekilde işaretlendi; bunlar bitince tekrar silebilirsiniz." });   // 202
                return NoContent();
                
            }
            catch (KeyNotFoundException ex)
            {


                return NotFound(new { message=ex.Message });

            }
        }
    }

}