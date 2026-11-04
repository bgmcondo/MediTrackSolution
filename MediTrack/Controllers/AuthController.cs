using MediTrack.Dtos.AuthDto;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Yeni admin: sadece mevcut bir Admin oluşturabilir
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminDto dto)
        {
            var result = await _authService.RegisterAdminAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Hasta kaydı:
        // - Giriş yapmamış biri kendi hesabını açabilir
        // - Admin hasta kaydedebilir
        // - Giriş yapmış Doctor/Patient kayıt yapamaz (403)
        // NOT: [AllowAnonymous] olsa bile istekte token varsa User dolu gelir.
        // Bunun için Program.cs'te AddAuthentication(JwtBearerDefaults.AuthenticationScheme) olmalı.
        [HttpPost("register-patient")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPatient(RegisterPatientDto dto)
        {
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Giriş yapmış kullanıcılar yeni hasta kaydı oluşturamaz" });

            var result = await _authService.RegisterPatientAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Doktor kaydı: sadece Admin
        [HttpPost("register-doctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto dto)
        {
            var result = await _authService.RegisterDoctorAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Giriş: herkese açık.
        // Başarısız girişte 401: kimlik doğrulanamadı (yanlış e-posta/şifre, kilitli hesap, silinmiş doktor)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }
    }
}