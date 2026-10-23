namespace MediTrack.Dtos.DoctorDto
{
    public class GetByIdDoctorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Branch { get; set; } = null!;
        public string Title { get; set; } = null!;
        public bool IsFamilyDoctor { get; set; }
    }
}
