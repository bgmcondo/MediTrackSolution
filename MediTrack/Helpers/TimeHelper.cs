namespace MediTrack.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

        public static DateTime NowInTurkey()=>TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);



    }
}
