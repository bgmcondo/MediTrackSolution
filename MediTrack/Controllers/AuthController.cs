using Microsoft.AspNetCore.Mvc;
using MediTrack.Dtos;
using MediTrack.Interfaces;
using MediTrack.Dtos.AuthDto;


namespace MediTrack.Controllers
{
    // [Route] -> ADRES belirler: bu controller'a hangi URL'den ulaşılacağını söyler.
    // [controller] yazısı sınıf adıyla değiştirilir, sondaki "Controller" atılır:
    // AuthController -> "auth" => adres: /api/auth
    // Metotlardaki [HttpPost("login")] gibi ifadeler bu adresin sonuna eklenir: /api/auth/login
    [Route("api/[controller]")]
    // [ApiController] -> DAVRANIŞ belirler: adresle ilgisi yoktur, controller'ın nasıl çalışacağını söyler.
    // 1) Otomatik doğrulama: DTO'daki [Required], [EmailAddress] gibi kurallar tutmazsa
    //    metoda hiç girmeden otomatik 400 Bad Request döner.
    //    (Olmasaydı her metotta if (!ModelState.IsValid) return BadRequest(...) yazmak gerekirdi.)
    // 2) İstek gövdesindeki JSON'u otomatik olarak DTO parametresine çevirir.
    //    (Olmasaydı parametrelerin başına [FromBody] yazmak gerekirdi.)
    // 3) [ApiController] kullanan controller'da [Route] yazmak zorunludur.
    //    Yani ikisi birlikte kullanılır ama aynı şey değildir.
    [ApiController]

    //    - (Controller değil ControllerBase: View/sayfa döndürmeyeceğiz, sadece JSON
    public class AuthController : ControllerBase

    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminDto registerAdminDto)
        {
            if (registerAdminDto == null)
                return BadRequest();

            //eğer dto null değilse servisi çağrıyoruz
          var result= await _authService.RegisterAdminAsync(registerAdminDto);
          if(!result.IsSuccess)
              return BadRequest(result);
          return Ok(result);

        }
        [HttpPost("register-patient")]

        public async Task<IActionResult> RegisterPatient(RegisterPatientDto registerPatientDto)
        {
            if(registerPatientDto == null)
                return BadRequest();  
            var result=await _authService.RegisterPatientAsync(registerPatientDto);
            if(!result.IsSuccess)
                // BadRequest()       -> sadece 400 döner, gövde boş. Frontend "bir şeyler ters gitti" der,
                //                       NEYİN ters gittiğini bilemez.
                // BadRequest(result) -> 400 + { "isSuccess": false, "errors": ["Bu email adresi zaten kayıtlı."] }
                //                       Frontend hata mesajını ekranda gösterebilir, kullanıcı neyi düzelteceğini anlar.
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto registerDoctorDto)
        {
            if (registerDoctorDto == null)
                return BadRequest();
            var result=await _authService.RegisterDoctorAsync(registerDoctorDto);
            if(!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if(loginDto == null)
                return BadRequest();
          var result=await _authService.LoginAsync(loginDto);
            if (!result.IsSuccess)
                return BadRequest();
            return Ok(result);


        }
    }
}