using MediTrack.Data;
using MediTrack.Interfaces;
using MediTrack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();
//sql server  bağlantısı

builder.Services.AddDbContext<MediTrackDbContext>(
   options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Uygulamaya kimlik doğrulama (authentication) sistemini ekler.
// "Bu isteği kim gönderiyor?" sorusunu cevaplayacak mekanizmayı kurar.
// Parantez içindeki options ile bu sistemin nasıl çalışacağını ayarlıyoruz.
builder.Services.AddAuthentication(options =>
{
    //Bir istek geldiğinde kimliği varsayılan olarak JWT ile kontrol et.
    // AddIdentity varsayılan olarak cookie kullanır, bu satır onu JWT ile değiştirir.
    // JwtBearerDefaults.AuthenticationScheme aslında sadece "Bearer" yazısıdır.

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // Kimliği doğrulanmamış bir kişi korumalı bir endpointe girmeye çalışırsa JWT usulüyle tepki verir
    //Yazılmazsa cookie sistemi devreye girer ve account/login sayfasına yönlendirmeye çalışır(404 verir)
    //Bu satır sayesinde doğrudan 401 unauthorized döner
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
    //JWT doğrulama yöntemini sisteme ekler ve tokenın nasıl kontrol edileceğini tanımlar
    //Gelen token'ın geçerli sayılabilmesi için kontrol listesi
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        //Tokenın kimin ürettiğini  kontrol et (Issuer=Token veren taraf, yani bizim API)
        //Başka bir sistemin ürettiği tokeni kabul etmemek için
        ValidateIssuer = true,
        // Tokenın kimin için üretildiğini kontrol et  (Audience = token'ı kullanacak taraf, yani client).
        // Başka bir uygulama için üretilmiş token'ın burada kullanılmasını engeller.
        ValidateAudience = true,
        // Token'ın süresi dolmuş mu kontrol et.
        // Süresi geçmiş token reddedilir, böylece çalınsa bile sonsuza kadar kullanılamaz.
        ValidateLifetime = true,

        // Token'ın imzasını kontrol et (en önemli kontrol).
        // Biri token'ın içini değiştirirse (örn. rolü Admin yaparsa) imza tutmaz ve token reddedilir.
        ValidateIssuerSigningKey = true,

        // Beklenen issuer değeri: appsettings.json'daki "Jwt:Issuer" (MediTrack).
        // Token'daki issuer bununla aynı değilse reddedilir.
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // Beklenen audience değeri: appsettings.json'daki "Jwt:Audience" (MediTrackClient).
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // İmzayı kontrol ederken kullanılacak gizli anahtar.
        // Configuration["Jwt:Key"] -> appsettings'teki gizli anahtarı okur.
        // ! -> derleyiciye "bu değer null olmaz" der.
        // Encoding.UTF8.GetBytes -> yazıyı byte dizisine çevirir (algoritmalar byte ile çalışır).
        // SymmetricSecurityKey -> aynı anahtar hem imzalamada (login) hem doğrulamada (burada) kullanılır.
        // Bu yüzden login'de token üretirken de AYNI Jwt:Key kullanılmalıdır.
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))

    }

    );



builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentMedicationService, AppointmentMedicationService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService,DoctorService>();
builder.Services.AddScoped<IFamilyDoctorService, FamilyDoctorService>();
builder.Services.AddScoped<IMedicationService,MedicationService>();
builder.Services.AddScoped<IPatientService, PatientService>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
