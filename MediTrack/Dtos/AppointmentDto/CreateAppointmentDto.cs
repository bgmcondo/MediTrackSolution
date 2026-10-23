using MediTrack.Entities;

namespace MediTrack.Dtos.AppointmentDto
{
    public class CreateAppointmentDto
    {
        //Patient ve Doctor nesnesini almamıza gerek yok çünkü bütün bilgileri id üzerinden alabiliriz. Bu yüzden sadece id'leri alıyoruz.
        public int PatientId { get; set; }
     
        public int DoctorId { get; set; }

    

        public DateTime AppointmentDate { get; set; }

      



        //Appointmentstatus'e de gerek yok çünkü default olarak pending atanacak.




    }
}
