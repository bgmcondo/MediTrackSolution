

namespace MediTrack.Entities
{
    public class Patient
    {
        public  int  Id { get; set; }

     
        public required string FirstName { get; set; } 
       
        public required string LastName {  get; set; }

        public required string NationalId { get; set; }

        // Hastanın Identity hesabının ID'si.
        // IdentityUser varsayılan olarak string ID kullanır.
        public string ApplicationUserId { get; set; } = null!;

        // Identity hesabına erişmek için Navigation Property.
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public  int CurrentFamilyDoctorId { get; set; } // Eğer Id'yi vermeseydik yine de CurrentFamilyDoctor üzerinden erişebilirdik. Ama bu şekilde daha hızlı erişim sağlanır.

        // null! başlangıçta null atanmasını sağlar, ancak derleyicinin
        // null uyarısını bastırır. Değerin null olmayacağını garanti etmez.
        //Çalışma zamanı: Değer hâlâ null olabilir ve NullReferenceException oluşabilir
        // public Doctor? Doctor da kullanılabilirdi ama derleme zamanında uyarı verir yine çalışma zamanında exception fırlatır 

        public Doctor CurrentFamilyDoctor { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<PatientFamilyDoctorHistory> PatientFamilyDoctorHistories { get; set; } = new List<PatientFamilyDoctorHistory>();


    }
}
