namespace MediTrack.Entities
{
    public class PatientFamilyDoctorHistory
    {
        public  int  Id { get; set; }

        public int FamilyDoctorId { get; set; } //int normalde nullable olmadığı için buraya null! değeri atamaya gerek yok. Ama eğer nullable olmasını istiyorsak int? yapabiliriz.
        public int PatientId { get; set; } //int normalde nullable olmadığı için buraya null! değeri atamaya gerek yok. Ama eğer nullable olmasını istiyorsak int? yapabiliriz.

        public Doctor FamilyDoctor { get; set; } = null!; //null olabilir eğer null ise derleme zamanında uyarı vermez ama

        public Patient Patient { get; set; } = null!; //null olabilir eğer null ise derleme zamanında uyarı vermez ama

        public DateTime AssignedAt { get; set; }=DateTime.Now; //Atandığı tarih otomatik olarak atanacak şekilde ayarlanabilir.

        // ? işareti, normalde null kabul etmeyen değer tiplerinin
        // (int, DateTime, bool gibi) null değerini kabul etmesini sağlar.
        // Null; bilginin henüz belli olmadığını, bulunmadığını
        // veya ilgili durumun henüz gerçekleşmediğini ifade edebilir.
        // Örneğin DateTime? kullanırsak tarih belirtilmediğinde null olur.
        // DateTime kullanırsak varsayılan değer 01.01.0001 olur.
        // null ile 0, false veya varsayılan tarih aynı anlama gelmez.
        public DateTime? UnassignedAt { get; set; } 

        
    }
}
