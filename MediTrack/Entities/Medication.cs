namespace MediTrack.Entities
{
    public class Medication
    {
        public  int Id{ get; set; }
        public required string Name { get; set; } //required null da olsa bir değer atanmasını zorunlu kılar 

        public required string Barcode { get; set; }

        public required string Description { get; set; }



        public ICollection<Appointment> Appointments { get; set; }=new List<Appointment>(); 
        public ICollection<AppointmentMedication> AppointmentMedications { get; set; } = new List<AppointmentMedication>(); //Ara tablomuz var ama appointment içerinden kolayca medication bilgisi çekmek bu koleksiyon listesi ile daha kolay

    }
}
