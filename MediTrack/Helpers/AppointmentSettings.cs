namespace MediTrack.Helpers
{
    public static class AppointmentSettings
    {
        public static readonly TimeSpan WorkStart = new(9, 0, 0);    // 09:00
        public static readonly TimeSpan WorkEnd = new(17, 0, 0);     // 17:00
        public static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(10);

        public static bool IsValidSlot(DateTime date)
        {
            var time = date.TimeOfDay;

            return time >= WorkStart                          // 09:00'dan önce olamaz
                && time + SlotDuration <= WorkEnd             // 17:00'yi geçemez (son slot 16:30)
                && (time - WorkStart).Ticks % SlotDuration.Ticks == 0;  // 10 dakikanın katı olmalı (14:17 olamaz)
        }
    }
}
