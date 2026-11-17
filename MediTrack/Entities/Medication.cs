using System.ComponentModel.DataAnnotations;

namespace MediTrack.Entities
{
    public class Medication
    {
        public  int Id{ get; set; }
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; } //required null da olsa bir değer atanmasını zorunlu kılar 

        [Required]
        [MaxLength(13)]
        public required string Barcode { get; set; }

        [MaxLength(1000)]
        public  string? Description { get; set; }

        // Soft delete: false = piyasadan çekildi / katalogdan kaldırıldı.
        // Pasif ilaç listede görünmez ve yeni reçeteye yazılamaz,
        // ama eski reçetelerde adı görünmeye devam eder.
        public bool IsActive { get; set; } = true;


        public ICollection<Appointment> Appointments { get; set; }=new List<Appointment>(); 
        public ICollection<AppointmentMedication> AppointmentMedications { get; set; } = new List<AppointmentMedication>(); //Ara tablomuz var ama appointment içerinden kolayca medication bilgisi çekmek bu koleksiyon listesi ile daha kolay

    }
}
