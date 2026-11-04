using MediTrack.Data;
using MediTrack.Dtos.AuthDto;
using MediTrack.Entities;
using MediTrack.Enums;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediTrack.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager; // Kullanıcı oluşturma, şifre doğrulama ve rol atama işlemlerini yönetir.
        private readonly RoleManager<IdentityRole> _roleManager;    // Kullanıcı rolleri ile ilgili işlemleri yönetir.
        private readonly MediTrackDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            MediTrackDbContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = dbContext;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Hangi bilginin yanlış olduğunu söylemiyoruz: hangi e-postaların kayıtlı olduğu sızmasın
            const string loginError = "Email veya şifre hatalı ya da hesabınız geçici olarak kilitlendi.";

            // 1. Email ile kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            // 2. Kullanıcı yoksa veya hesap kilitliyse -> hata
            if (user == null || await _userManager.IsLockedOutAsync(user))
                return new LoginResponseDto { IsSuccess = false, Message = loginError };

            // 3. Şifre yanlışsa -> başarısız deneme sayacını artır (belirli sayıda hatada hesap kilitlenir)
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _userManager.AccessFailedAsync(user);
                return new LoginResponseDto { IsSuccess = false, Message = loginError };
            }

            // ── Buraya kadar kullanıcı doğrulanmadı, hiçbir bilgi verilmedi ──

            // 4. Kullanıcının rollerini al
            var roles = await _userManager.GetRolesAsync(user);

            // 5. Profil id'lerini bul. Rolüne göre yalnızca biri dolar, diğeri null kalır.
            int? patientId = null;
            int? doctorId = null;

            if (roles.Contains("Patient"))
            {
                patientId = await _context.Patients
                    .Where(p => p.ApplicationUserId == user.Id)
                    .Select(p => (int?)p.Id)
                    .FirstOrDefaultAsync();
            }

            if (roles.Contains("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);

                // Silinmiş (soft delete) doktor giriş yapamaz.
                // PendingDeletion durumundaki doktor girebilir, bitmemiş randevuları devam ediyor.
                if (doctor == null || doctor.Status == DoctorStatus.Deleted)
                    return new LoginResponseDto { IsSuccess = false, Message = loginError };

                doctorId = doctor.Id;
            }

            // 6. Giriş kesin olarak başarılı -> sayacı sıfırla
            await _userManager.ResetAccessFailedCountAsync(user);

            // 7. JWT token üret
            // Claim'ler: token'ın içine (payload) konacak bilgiler
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),                      // kullanıcı Id (ApplicationUser.Id)
                new Claim(ClaimTypes.Email, user.Email!),                           // e-posta
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())   // her token'a benzersiz kimlik
            };

            // Her rolü ayrı bir claim olarak ekle
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // İmza: Program.cs'teki JWT key ile aynı olmalı
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key ayarı bulunamadı.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Geçerlilik süresi
            var expirationMinutesText = _configuration["Jwt:ExpirationMinutes"]
                ?? throw new InvalidOperationException("Jwt:ExpirationMinutes ayarı bulunamadı.");
            if (!double.TryParse(expirationMinutesText, out var expirationMinutes))
                throw new InvalidOperationException("Jwt:ExpirationMinutes ayarı geçersiz.");

            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Token nesnesi oluştur
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            // Nesneyi "xxx.yyy.zzz" şeklindeki metne çevir.
            // Frontend bu token'ı saklar ve sonraki her istekte header'da gönderir.
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 8. Başarılı cevap: id'ler SADECE burada doldurulur
            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Giriş başarılı.",
                Token = tokenString,
                Expiration = expiration,
                Email = user.Email!,
                Roles = roles,
                PatientId = patientId,   // hasta değilse null
                DoctorId = doctorId      // doktor değilse null
            };
        }

        public async Task<RegisterResponseDto> RegisterAdminAsync(RegisterAdminDto registerAdminDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var existingUser = await _userManager.FindByEmailAsync(registerAdminDto.Email);
            if (existingUser != null)
                return Failure("Bu email adresi zaten kayıtlı.");

            // 2. Transaction başlat: Commit çağrılmadan çıkılırsa her şey geri alınır
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // 3. Yeni ApplicationUser oluştur
            //    PasswordHash'i ELLE ATAMA, Identity kendisi hash'ler
            var newUser = new ApplicationUser { UserName = registerAdminDto.Email, Email = registerAdminDto.Email };

            var result = await _userManager.CreateAsync(newUser, registerAdminDto.Password);
            if (!result.Succeeded)
                return Failure(result.Errors.Select(e => e.Description));

            // 4. Rolü ata
            var roleResult = await AddToRoleAsync(newUser, "Admin");
            if (!roleResult.Succeeded)
                return Failure(roleResult.Errors.Select(e => e.Description));

            // 5. Her şey başarılı -> kalıcı yap
            await transaction.CommitAsync();

            return Success(newUser, "Admin", "Admin kaydı başarıyla oluşturuldu.");
        }

        public async Task<RegisterResponseDto> RegisterDoctorAsync(RegisterDoctorDto registerDoctorDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var existingUser = await _userManager.FindByEmailAsync(registerDoctorDto.Email);
            if (existingUser != null)
                return Failure("Bu email adresi zaten kayıtlı.");

            // 2. Transaction başlat
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // 3. Yeni ApplicationUser oluştur
            var newUser = new ApplicationUser { UserName = registerDoctorDto.Email, Email = registerDoctorDto.Email };

            var result = await _userManager.CreateAsync(newUser, registerDoctorDto.Password);
            if (!result.Succeeded)
                return Failure(result.Errors.Select(e => e.Description));

            // 4. Rolü ata
            var roleResult = await AddToRoleAsync(newUser, "Doctor");
            if (!roleResult.Succeeded)
                return Failure(roleResult.Errors.Select(e => e.Description));

            // 5. Doctor entity'si oluştur ve ApplicationUser ile bağla
            var doctor = new Doctor
            {
                ApplicationUserId = newUser.Id,
                FirstName = registerDoctorDto.FirstName,
                LastName = registerDoctorDto.LastName,
                Branch = registerDoctorDto.Branch,
                Title = registerDoctorDto.Title,
                IsFamilyDoctor = registerDoctorDto.IsFamilyDoctor,
            };

            try
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Failure("Doktor kaydı oluşturulamadı.");
            }

            // 6. Her şey başarılı -> kalıcı yap
            await transaction.CommitAsync();

            return Success(newUser, "Doctor", "Doktor kaydı başarıyla oluşturuldu.");
        }

        public async Task<RegisterResponseDto> RegisterPatientAsync(RegisterPatientDto registerPatientDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var existingUser = await _userManager.FindByEmailAsync(registerPatientDto.Email);
            if (existingUser != null)
                return Failure("Bu email adresi zaten kayıtlı.");

            // 2. Transaction başlat
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // 3. Yeni ApplicationUser oluştur
            var newUser = new ApplicationUser { UserName = registerPatientDto.Email, Email = registerPatientDto.Email };

            var result = await _userManager.CreateAsync(newUser, registerPatientDto.Password);
            if (!result.Succeeded)
                return Failure(result.Errors.Select(e => e.Description));

            // 4. Rolü ata
            var roleResult = await AddToRoleAsync(newUser, "Patient");
            if (!roleResult.Succeeded)
                return Failure(roleResult.Errors.Select(e => e.Description));

            // 5. Patient entity'si oluştur ve ApplicationUser ile bağla.
            //    CurrentFamilyDoctorId boş kalır: yeni hastanın henüz aile hekimi yok.
            var patient = new Patient
            {
                ApplicationUserId = newUser.Id,
                FirstName = registerPatientDto.FirstName,
                LastName = registerPatientDto.LastName,
                NationalId = registerPatientDto.NationalId
            };

            try
            {
                _context.Patients.Add(patient);    // eklenecekler listesine koy (henüz kaydedilmedi)
                await _context.SaveChangesAsync(); // veritabanına gerçekten yaz
            }
            catch (DbUpdateException)
            {
                return Failure("Hasta kaydı oluşturulamadı.");
            }

            // 6. Her şey başarılı -> kalıcı yap
            await transaction.CommitAsync();

            return Success(newUser, "Patient", "Hasta kaydı başarıyla oluşturuldu.");
        }

        // ---------------- Yardımcı metotlar ----------------
        // Üç register metodunda tekrar eden kodlar burada toplandı.

        // Rol yoksa oluşturur, sonra kullanıcıyı o role ekler.
        private async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            return await _userManager.AddToRoleAsync(user, role);
        }

        // Tek mesajlı hata cevabı
        private static RegisterResponseDto Failure(string error)
            => Failure(new[] { error });

        // Birden fazla hata mesajı içeren cevap (ör. Identity'nin şifre kuralları)
        private static RegisterResponseDto Failure(IEnumerable<string> errors)
            => new RegisterResponseDto
            {
                IsSuccess = false,
                Message = "Kayıt Başarısız",
                Errors = errors.ToList()
            };

        private static RegisterResponseDto Success(ApplicationUser user, string role, string message)
            => new RegisterResponseDto
            {
                IsSuccess = true,
                UserId = user.Id,
                Email = user.Email!,
                Role = role,
                Errors = new List<string>(),
                Message = message
            };
    }
}