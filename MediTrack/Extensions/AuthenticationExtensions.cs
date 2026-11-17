using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MediTrack.Extensions
{
    public static class AuthenticationExtensions
    {
        // JWT kimlik doğrulamasını kurar: token nasıl kontrol edilecek ve
        // 401 / 403 durumlarında istemciye hangi cevap dönecek.
     
        
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)

        {
            var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key ayarı bulunamadı (appsettings veya user-secrets).");
            services.AddAuthentication(options =>
            {
                // Bir istek geldiğinde kimliği varsayılan olarak JWT ile kontrol et.
                // JwtBearerDefaults.AuthenticationScheme aslında sadece "Bearer" yazısıdır.
                // Bu ayar sayesinde [AllowAnonymous] endpoint'lerde de (ör. register-patient)
                // token gönderilmişse User dolu gelir.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                // Kimliği doğrulanmamış bir kişi korumalı bir endpoint'e girmeye çalışırsa JWT usulüyle tepki ver.
                // Bu satır sayesinde doğrudan 401 Unauthorized döner.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           // JWT doğrulama yöntemini sisteme ekler ve token'ın nasıl kontrol edileceğini tanımlar.
           .AddJwtBearer(options =>
           {
               // Gelen token'ın geçerli sayılabilmesi için kontrol listesi:
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   // Token'ı kimin ürettiğini kontrol et (Issuer = token veren taraf, yani bizim API).
                   // Başka bir sistemin ürettiği token'ı kabul etmemek için.
                   ValidateIssuer = true,

                   // Token'ın kimin için üretildiğini kontrol et (Audience = token'ı kullanacak taraf, yani client).
                   // Başka bir uygulama için üretilmiş token'ın burada kullanılmasını engeller.
                   ValidateAudience = true,

                   // Token'ın süresi dolmuş mu kontrol et.
                   // Süresi geçmiş token reddedilir, böylece çalınsa bile sonsuza kadar kullanılamaz.
                   ValidateLifetime = true,

                   // Varsayılan olarak token, süresi dolduktan sonra 5 dakika daha geçerli sayılır
                   // (sunucu saatleri arasındaki küçük farkları tolere etmek için).
                   // Tek sunucu olduğu için bu toleransa gerek yok; süre dolunca token hemen reddedilsin.
                   ClockSkew = TimeSpan.Zero,

                   // Token'ın imzasını kontrol et (en önemli kontrol).
                   // Biri token'ın içini değiştirirse (örn. rolü Admin yaparsa) imza tutmaz ve token reddedilir.
                   ValidateIssuerSigningKey = true,

                   // Beklenen issuer değeri: appsettings.json'daki "Jwt:Issuer".
                   ValidIssuer = configuration["Jwt:Issuer"],

                   // Beklenen audience değeri: appsettings.json'daki "Jwt:Audience".
                   ValidAudience = configuration["Jwt:Audience"],

                   // İmzayı kontrol ederken kullanılacak gizli anahtar.
                   // SymmetricSecurityKey -> aynı anahtar hem imzalamada (login) hem doğrulamada (burada) kullanılır.
                   // Bu yüzden login'de token üretirken de AYNI Jwt:Key kullanılmalıdır.
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
               };

               // 401 ve 403 cevaplarına mesaj ekler.
               // Bu iki durum istek controller'a ULAŞMADAN oluşur (UseAuthentication / UseAuthorization),
               // bu yüzden servisteki AccessService ya da controller'daki Execute burada çalışmaz.
               // Mesaj formatı Execute'unkiyle aynı: { "message": "..." }
               options.Events = new JwtBearerEvents
               {
                   // 401: token yok, geçersiz ya da süresi dolmuş
                   OnChallenge = async context =>
                   {
                       context.HandleResponse(); // varsayılan boş cevabı engelle
                       context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                       await context.Response.WriteAsJsonAsync(new { message = "Bu işlem için giriş yapmanız gerekiyor." });
                   },

                   // 403: token geçerli ama rol yetmiyor ([Authorize(Roles = "...")])
                   OnForbidden = async context =>
                   {
                       context.Response.StatusCode = StatusCodes.Status403Forbidden;
                       await context.Response.WriteAsJsonAsync(new { message = "Bu işlem için yetkiniz yok." });
                   }
               };
           });

            return services;
        }
    }
}