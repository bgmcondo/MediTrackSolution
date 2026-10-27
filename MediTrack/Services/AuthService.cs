using MediTrack.Data;
using MediTrack.Dtos.AuthDto;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;

namespace MediTrack.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager; //Kullanıcı oluşturma, şifre doğrulama ve rol atama işlemlerini yönetir.
        private readonly RoleManager<IdentityRole> _roleManager;//Kullanıcı rolleri ile ilgili işlemleri yönetir.

        private readonly MediTrackDbContext _context;

        private readonly IConfiguration _configuration;
        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,MediTrackDbContext dbContext,IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = dbContext;
            _configuration = configuration;
        }


        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // 1. Email ile kullanıcıyı bul (FindByEmailAsync)
            var user=await _userManager.FindByEmailAsync(loginDto.Email);

            // 2. Kullanıcı yoksa -> IsSuccess = false, "Email veya şifre hatalı" döndür
            if (user == null)
          
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = "Bu email kayıtlı değil",
                   Email = loginDto.Email,
                };

            
            // 3. Şifreyi kontrol et (CheckPasswordAsync)
            var isPasswordValid=await _userManager.CheckPasswordAsync(user, loginDto.Password);

            // 4. Şifre yanlışsa -> yine aynı genel hata mesajını döndür (hangisinin yanlış olduğunu söyleme)
            if (!isPasswordValid)
                return new LoginResponseDto
                {
                    IsSuccess=false,
                    Message="Email veya şifre hatalı",
                  
                   
                };
            // 5. Kullanıcının rollerini al (GetRolesAsync)
            var roles = await _userManager.GetRolesAsync(user);
            // 6. (İleride) JWT token üret
            //Claim'ler:token'ın içine payload konacak bilgiler
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier,user.Id),    // kullanıcı Id
                 new Claim(ClaimTypes.Email,user.Email!),        // e-posta
                 new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) // her token'a benzersiz kimlik
            };

            //Her rolü ayrı bir claim olarak ekle 
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            //İmza Program.cs'teki JWT key kullanılmalı
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
            var credentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            // Geçerlilik süresi

            var expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresMinutes"]!));
            //token nesnesi oluştur
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
               audience: _configuration["Jwt:Audience"],
               claims: claims,
               expires: expiration,
               signingCredentials: credentials

                );
            // Nesneyi "xxx.yyy.zzz" şeklindeki metne çevir
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //Kullanıcı bir kez e-posta ve şifreyle giriş yapar.
            // Sunucu bilgileri doğrular ve ona imzalı bir token verir.
            //Frontend bu token’ı saklar ve sonraki her istekte header’da gönderir:

            // 7. LoginResponseDto doldur: UserId, Email, Role, Token, IsSuccess = true
            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Giriş başarılı.",
                Token = tokenString,
                Expiration = expiration,
                Email = user.Email!,
                Roles = roles
            };
        }

        public async Task<RegisterResponseDto> RegisterAdminAsync(RegisterAdminDto registerAdminDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var existingUser= await _userManager.FindByEmailAsync(registerAdminDto.Email);
            // 2. Varsa -> IsSuccess = false, "Bu email zaten kayıtlı" döndür
            if (existingUser != null)

                return new RegisterResponseDto
                {


                    IsSuccess = false,
                    Email=existingUser.Email!,

                    Message = "Kayıt Başarısız",
                    Errors = new List<string> { "Bu email zaten kayıtlı" }

                };

            
            // 3. Yeni IdentityUser oluştur: UserName ve Email ver
            //    - PasswordHash'i ELLE ATAMA, Identity kendisi hash'ler
            //    - UserName'de boşluk olmasın (Identity varsayılan olarak izin vermez), Email'i kullanabilirsin
            // 4. CreateAsync(user, password) ile kaydet
            // 5. result.Succeeded false ise -> hataları döndür
            // 6. "Admin" rolü yoksa oluştur (RoleExistsAsync / CreateAsync)
            // 7. Kullanıcıyı "Admin" rolüne ekle (AddToRoleAsync)
            // 8. Rol ekleme başarısızsa -> kullanıcıyı sil, hataları döndür
            // 9. Başarılı RegisterResponseDto döndür
        }

        public async Task<RegisterResponseDto> RegisterDoctorAsync(RegisterDoctorDto registerDoctorDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var existingUser = await _userManager.FindByEmailAsync(registerDoctorDto.Email);
            // 2. Varsa -> hata döndür
            if (existingUser != null)
                return new RegisterResponseDto
                {
     
                    IsSuccess=false,
                    Errors = new List<string> { "Bu email adresi zaten kayıtlı." }


                };
                // 3. Yeni IdentityUser oluştur (UserName, Email)
                var newUser = new IdentityUser
                {
                    UserName = registerDoctorDto.Email,
                    Email = registerDoctorDto.Email,

                };
                // 4. CreateAsync(user, password) ile kaydet
                var result = await _userManager.CreateAsync(newUser, registerDoctorDto.Password);

                // 5. result.Succeeded false ise -> hataları döndür
                if (!result.Succeeded)
                
                    return new RegisterResponseDto
                    {
                        IsSuccess = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };


                
                // 6. "Doctor" rolü yoksa oluştur
                var roleExists = await _roleManager.RoleExistsAsync("Doctor");
                if (!roleExists)
                
                    await _roleManager.CreateAsync(new IdentityRole("Doctor"));


                


                // 7. Kullanıcıyı "Doctor" rolüne ekle
                var roleResult = await _userManager.AddToRoleAsync(newUser, "Doctor");

                // 8. Rol ekleme başarısızsa -> kullanıcıyı sil, hataları döndür
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(newUser);
                return new RegisterResponseDto
                {
                    IsSuccess = false,
                    Errors = roleResult.Errors.Select(e => e.Description).ToList()

                };
                }
                // 9. (Varsa) Doctor entity'si oluştur, UserId = newUser.Id ile bağla, DbContext'e ekle ve SaveChangesAsync
                var doctor = new Doctor
                {
                   ApplicationUserId=newUser.Id,
                   FirstName = registerDoctorDto.FirstName,
                   LastName= registerDoctorDto.LastName,
                   Branch= registerDoctorDto.Branch


                };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // 10. Başarılı RegisterResponseDto döndür (Role = "Doctor")

            return new RegisterResponseDto
                {
                    UserId=newUser.Id,
                    Email=newUser.Email,
                    IsSuccess = true,
                    Role="Doctor",
                    Message= "Doktor kaydı başarıyla oluşturuldu."



                };
            } 

        public async Task<RegisterResponseDto> RegisterPatientAsync(RegisterPatientDto registerPatientDto)
        {
            // 1. Email ile kullanıcı var mı kontrol et
            var user = await _userManager.FindByEmailAsync(registerPatientDto.Email);

            // 2. Varsa -> exception fırlatma, diğerleri gibi hata DTO'su döndür
            if (user != null)
                return new RegisterResponseDto
                {
                    IsSuccess = false,
                    Errors = new List<string> { "Bu email adresi zaten kayıtlı." }


                };
            // 3. Yeni IdentityUser oluştur (UserName, Email)
            var newUser = new IdentityUser
                {
                  UserName=registerPatientDto.Email,
                  Email=registerPatientDto.Email



            };
              
          
                // 4. CreateAsync(user, password) ile kaydet
                var result= await _userManager.CreateAsync(newUser,registerPatientDto.Password);

            // 5. result.Succeeded false ise -> hataları döndür
            if (!result.Succeeded)
                return new RegisterResponseDto
                {
                    IsSuccess = false,
                    Errors =result.Errors.Select(e=>e.Description).ToList(),
                };
                // 6. "Patient" rolü yoksa oluştur
                var roleExists = await _roleManager.RoleExistsAsync("Patient");
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Patient"));

            }

            // 7. Kullanıcıyı "Patient" rolüne ekle, başarısızsa kullanıcıyı sil ve hata döndür
             await _userManager.AddToRoleAsync(newUser, "Patient");

            // 8. Patient entity'si oluştur: UserId = newUser.Id, FirstName, LastName
            var patient = new Patient
            {
                ApplicationUserId= newUser.Id,                      // IdentityUser ile bağlantı
                FirstName = registerPatientDto.FirstName,
                LastName = registerPatientDto.LastName,
                NationalId= registerPatientDto.NationalId,

            };

            

            // 9. DbContext'e ekle ve SaveChangesAsync ile kaydet
            _context.Patients.Add(patient);   // eklenecekler listesine koy (henüz kaydedilmedi)
            await _context.SaveChangesAsync(); // veritabanına gerçekten yaz
            // 10. Başarılı RegisterResponseDto döndür (Role = "Patient")

            return new RegisterResponseDto
            {
                IsSuccess = true,
                UserId = newUser.Id,
                Email = newUser.Email!,
                Role = "Patient",
                Errors = new List<string>()


            };
        }


    }
    } 