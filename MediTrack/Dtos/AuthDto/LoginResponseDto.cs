namespace MediTrack.Dtos.AuthDto
{
    public class LoginResponseDto
    {
        // Frontend'in endpoint'lere göndereceği profil id'leri.
        // Token'da sadece ApplicationUser.Id (ör. "a3f9c2...") var; ama endpoint'ler
        // Patients/Doctors tablolarındaki id'yi istiyor (ör. api/appointments/patient/12).
        // Kullanıcının rolüne göre bunlardan yalnızca biri dolu olur, diğeri null döner:
        //   Hasta  -> PatientId dolu, DoctorId null
        //   Doktor -> DoctorId dolu, PatientId null
        //   Admin  -> ikisi de null
        // AdminId YOK: Admin'in ayrı bir profil tablosu yok ve hiçbir endpoint admin'in
        // kendi id'sini istemiyor. Admin yetkisi id ile değil, roldeki "Admin" ile kontrol ediliyor.
        public int? PatientId { get; set; } 

        public int? DoctorId { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }

        public string Email { get; set; } = null!;

        public IList <string> Roles { get; set; } = new List<string>(); //// IList<T>, liste işlemlerini (ekleme, silme, güncelleme ve indeksle erişim) tanımlayan bir arayüzdür. List<T> gibi bu arayüzü uygulayan koleksiyonlarla kullanılabilir.
        // Ayrıca ASP.NET Core Identity'nin GetRolesAsync() metodu IList<string> döndürdüğü için bu türü tercih ettik.

    }
}
