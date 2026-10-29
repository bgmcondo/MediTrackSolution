
namespace MediTrack.Dtos.FamilyDoctorDto
{
    public class GetByIdFamilyDoctorHistoryDto
    {
        // Geçmiş kaydının benzersiz ID bilgisi.
        // Response DTO olduğu için ID'yi döndürebiliriz.
        public int Id { get; set; }

        // Geçmiş kaydının ait olduğu hasta.
        public int PatientId { get; set; }

        // Hastanın adı ve soyadı birleştirilerek gösterilir.
        // Service katmanında veritabanından alınır.
        public string PatientFullName { get; set; } = string.Empty;

        // Hastaya atanmış aile hekiminin ID bilgisi.
        public int? FamilyDoctorId { get; set; }



        // Aile hekiminin adı ve soyadı birleştirilerek gösterilir.
        // Service katmanında hesaplanır.
        public string FamilyDoctorFullName { get; set; } = string.Empty;

        // Aile hekiminin atandığı tarih.
        // DateTime.Now kullanmıyoruz.
        // Çünkü gerçek tarih veritabanından getirilecek.
        public DateTime AssignedAt { get; set; }

        // Aile hekimliği ilişkisinin sona erdiği tarih.
        // null ise atama hâlâ devam ediyor demektir.
        // DateTime? null değerine izin verir.
        public DateTime? UnassignedAt { get; set; }
    }
}
