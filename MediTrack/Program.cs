using MediTrack.Data;
using MediTrack.Entities;
using MediTrack.Interfaces;
using MediTrack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;   // YENİ: AuthorizationPolicyBuilder için
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using MediTrack.Extensions;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// ================= OPENAPWT "Authorize" butonu (ayrıntılar: Extensions/OpenApiExtensions.cs)
builder.Services.AddOpenApiWithJwt();
// ================= AUTHENTICATION (JWT) =================
// Token doğrulama kuralları + 401/403 mesajları (ayrıntılar: Extensions/AuthenticationExtensions.cs)
builder.Services.AddJwtAuthentication(builder.Configuration);

// SQL Server bağlantısıI + SWAGGER =================
// API dokümanı + Swagger'daki J
builder.Services.AddDbContext<MediTrackDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ================= IDENTITY =================
// AddIdentityCore -> Identity'nin sadece "çekirdek" kısmını kurar:
//    kullanıcı oluşturma, şifre hash'leme, şifre kontrolü (UserManager).
//    AddIdentity'den farkı: cookie ayarlarını ve SignInManager'ı EKLEMEZ.
//    Biz JWT kullandığımız için cookie'ye ihtiyacımız yok, bu yüzden Core yeterli.
// <ApplicationUser> -> kullanıcı sınıfımız. AspNetUsers tablosundaki bir satırı temsil eder.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Aynı email ile ikinci bir kullanıcı oluşturulmasını engeller.
    // AuthService'teki FindByEmailAsync kontrolüne ek bir güvenlik katmanıdır:
    // iki kayıt isteği aynı anda gelirse ikisi de o kontrolden geçebilir,
    // ama bu ayar sayesinde CreateAsync ikinciyi reddeder.
    options.User.RequireUniqueEmail = true;

    // YENİ: Hesap kilitleme ayarları (varsayılanlar bunlar; açıkça yazmak kuralı görünür kılar).
    // AuthService'teki AccessFailedAsync / IsLockedOutAsync bu ayarlara göre çalışır.
    options.Lockout.MaxFailedAccessAttempts = 5;                       // 5 hatalı denemeden sonra
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // 5 dakika kilitle
    options.Lockout.AllowedForNewUsers = true;                         // yeni hesaplar da kilitlenebilir
})
    // Rol desteğini ekler ve RoleManager<IdentityRole>'ü sisteme kaydeder.
    // AddIdentityCore rolleri otomatik getirmez, bu satır olmazsa
    // AuthService'teki RoleManager çözülemez ve uygulama hata verir.
    // AddToRoleAsync, GetRolesAsync gibi metotlar da bu sayede çalışır.
    .AddRoles<IdentityRole>()

    // Kullanıcıların ve rollerin HANGİ veritabanına kaydedileceğini söyler.
    // MediTrackDbContext verildiği için UserManager da aynı context'i kullanır.
    // Bu sayede AuthService'te başlattığımız transaction kullanıcı kaydını da kapsar:
    // bir şey ters giderse kullanıcı, rol ve doktor/hasta kaydı birlikte geri alınır.
    // (Bunun için MediTrackDbContext'in IdentityDbContext'ten türemesi gerekir.)
    .AddEntityFrameworkStores<MediTrackDbContext>()

    // Tek kullanımlık güvenlik kodları (token) üreten sağlayıcıları ekler.
    // Bunlar JWT değildir, şu işler için kullanılır:
    //   - Şifre sıfırlama linki (GeneratePasswordResetTokenAsync)
    //   - Email doğrulama linki (GenerateEmailConfirmationTokenAsync)
    // Şu an kullanmıyoruz, ama ileride "şifremi unuttum" eklenirse hazır olur.
    .AddDefaultTokenProviders();



// ================= AUTHORIZATION =================
// YENİ: Varsayılan politika.
// [Authorize] veya [AllowAnonymous] yazılmamış HER endpoint giriş gerektirir.
// Bir controller'a [Authorize] yazmayı unutsanız bile endpoint korumasız kalmaz.
// Herkese açık olması gerekenler (login, register-patient) zaten [AllowAnonymous] ile işaretli.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ================= SERVİSLER =================
// YENİ: Servislerin içinden şu anki HTTP isteğine (ve isteği yapan kullanıcıya) erişmeyi sağlar.
// AccessService, token'daki kullanıcı bilgisini (HttpContext.User) bu sayede okur.
builder.Services.AddHttpContextAccessor();

// YENİ: Sahiplik/yetki kontrolleri. Diğer servisler constructor'da bunu istediği için
// bu satır olmazsa uygulama "Unable to resolve service for type IAccessService" hatası verir.
builder.Services.AddScoped<IAccessService, AccessService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IFamilyDoctorService, FamilyDoctorService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IPatientService, PatientService>();


// builder servislerden sonra app'i oluşturur, app ise middleware'leri çalıştırır
var app = builder.Build();

// ================= SEED: ROLLER VE İLK ADMIN =================
// YENİ: Uygulama her açıldığında çalışır; eksik olanları oluşturur, var olanlara dokunmaz.
// Neden gerekli? register-admin endpoint'i sadece Admin'e açık.
// Veritabanı boşken hiç admin yoksa, kimse ilk admin'i oluşturamaz (tavuk-yumurta sorunu).
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Rolleri oluştur (yoksa)
    foreach (var role in new[] { "Admin", "Doctor", "Patient" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // İlk admin'i oluştur (yoksa).
    // E-posta ve şifre koda yazılmaz, yapılandırmadan okunur (user-secrets önerilir).
    var adminEmail = builder.Configuration["SeedAdmin:Email"];
    var adminPassword = builder.Configuration["SeedAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword)
        && await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //AllowAnonymous şart

    app.MapOpenApi().AllowAnonymous();

    //yeni swagger arayüzü middleware de olduğu ve useauthroizationdan önce olduğu için giriş gerektirmiyor
    // Sadece Development'ta açık; canlı ortamda API dokümanı dışarıya gösterilmez.

    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "MediTrack API v1");
        // Authorize'a girilen token'ı tarayıcıda (localStorage) sakla:
        // sayfa yenilense ya da uygulama yeniden başlatılsa da tekrar girmek gerekmez.
        options.EnablePersistAuthorization();

    });
}

app.UseHttpsRedirection(); // gelen http isteğini https'e yönlendirir, https zorunlu hale gelir

app.UseAuthentication();   // önce: "sen kimsin?" -> token'ı okur, kullanıcıyı tanır
app.UseAuthorization();    // sonra: "bu kullanıcı bunu yapabilir mi?" -> rol kontrolü, policy kontrolü

app.MapControllers();      // controller'ları sisteme ekler, gelen isteği route'a göre ilgili controller'a yönlendirir

app.Run();