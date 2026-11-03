namespace MediTrack.Enums
{
    public enum DoctorStatus
    {
        Active = 0,           // normal çalışıyor
        PendingDeletion = 1,  // silinmesine karar verildi, yeni randevu yok
        Deleted = 2           // tamamen pasif
    }
}