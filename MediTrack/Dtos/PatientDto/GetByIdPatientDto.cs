using MediTrack.Entities;

namespace MediTrack.Dtos.PatientDto
{
    public class GetByIdPatientDto
    {
        public int Id { get; set; }


        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string NationalId { get; set; }



        public int CurrentFamilyDoctorId { get; set; } // Eğer Id'yi vermeseydik yine de CurrentFamilyDoctor üzerinden erişebilirdik. Ama bu şekilde daha hızlı erişim sağlanır.

        // null! başlangıçta null atanmasını sağlar, ancak derleyicinin
        // null uyarısını bastırır. Değerin null olmayacağını garanti etmez.
        //Çalışma zamanı: Değer hâlâ null olabilir ve NullReferenceException oluşabilir
        // public Doctor? Doctor da kullanılabilirdi ama derleme zamanında uyarı verir yine çalışma zamanında exception fırlatır 



 
    }
}
