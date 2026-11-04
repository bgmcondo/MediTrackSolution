namespace MediTrack.Interfaces
{
    public interface IAccessService
    {
        // İdari veriler (randevu listesi, aile hekimi geçmişi):
        // hastanın kendisi, ilişkili doktor, Admin
        Task EnsureCanAccessPatientAsync(int patientId);

        // Tıbbi veriler (reçete/ilaç geçmişi, tanı, doktor notları):
        // SADECE hastanın kendisi ve ilişkili doktor; Admin göremez
        Task EnsureCanAccessPatientMedicalDataAsync(int patientId);

        // Doktorun verileri (ör. randevu listesi): doktorun kendisi, Admin
        Task EnsureCanAccessDoctorAsync(int doctorId);

        // Randevuyu iptal etme: randevunun sahibi olan hasta, Admin
        Task EnsureCanCancelAppointmentAsync(int appointmentId);

        // Randevuyu güncelleme: SADECE randevunun sahibi olan hasta ve Admin görebilir
        Task EnsureCanUpdateAppointmentAsync(int appointmentId);
    }
}