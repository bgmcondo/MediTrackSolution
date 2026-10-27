namespace MediTrack.Entities
{
    public class Doctor
    {
        public int  Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public String FirstName { get; set; }= null!;

        public string  LastName { get; set; } = null!;

        public string Branch { get; set; } = null!;

        public string Title { get; set; } = null!;

        public bool IsFamilyDoctor {  get; set; }

        public ICollection<Patient> Patients { get; set; } =  new List<Patient>(); //Çünkü bir doktorun henüz hiç hastası olmayabilir. Bu durumda koleksiyonun null değil, boş bir liste olması daha güvenlidir.

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}
