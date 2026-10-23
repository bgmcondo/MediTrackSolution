using MediTrack.Entities;

namespace MediTrack.Dtos.AppointmentDto
{
    public class UpdateAppointmentDto
    {

        // Id EKLEMEDİK:
        // Güncellenecek randevunun Id bilgisi URL üzerinden alınır.
        // Örnek: PUT /api/appointments/10

        // PatientId EKLEMEDİK:
        // Hasta kendi randevusunu güncelliyor.
        // Doktor veya tarih değişse bile randevunun sahibi değişmez.
        // Ayrıca kullanıcının bu randevuya erişim yetkisi kontrol edilmelidir.

        // DoctorId:
        // Hasta randevu aldığı doktoru değiştirmek isteyebilir.
        // Bu nedenle DoctorId güncellenebilir



        public int DoctorId { get; set; }

        // AppointmentDate:
        // Hasta randevusunun tarihini veya saatini değiştirebilir.
        public DateTime AppointmentDate { get; set; }


        // DoctorName EKLEMEDİK:
        // Doktorun adı kullanıcıya gösterilmek için kullanılır.
        // Güncelleme sırasında DoctorId göndermek yeterlidir.
        // Doktorun adı Response DTO üzerinden gösterilebilir.

        // IsFamilyDoctorAppointment EKLEMEDİK:
        // Bu bilgi kullanıcı tarafından belirlenmemelidir.
        // Service katmanında otomatik hesaplanır.
        // patient.CurrentFamilyDoctorId == dto.DoctorId
        // İki Id eşitse true, değilse false olur.

        // AppointmentStatus EKLEMEDİK:
        // Hasta kendi randevusunu Approved veya Rejected yapamamalıdır.
        // Onaylama ve reddetme işlemleri yetkili personel tarafından
        // ayrı bir DTO veya Service metodu ile gerçekleştirilmelidir.
        // Randevu değiştirildiğinde gerekirse Pending durumuna alınır.



    }
}
