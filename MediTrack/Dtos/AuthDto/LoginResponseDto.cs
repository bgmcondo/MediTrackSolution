namespace MediTrack.Dtos.AuthDto
{
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }

        public string Email { get; set; } = null!;

        public IList <string> Roles { get; set; } = new List<string>(); //// IList<T>, liste işlemlerini (ekleme, silme, güncelleme ve indeksle erişim) tanımlayan bir arayüzdür. List<T> gibi bu arayüzü uygulayan koleksiyonlarla kullanılabilir.
        // Ayrıca ASP.NET Core Identity'nin GetRolesAsync() metodu IList<string> döndürdüğü için bu türü tercih ettik.

    }
}
