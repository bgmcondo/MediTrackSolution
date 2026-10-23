using MediTrack.Entities;

namespace MediTrack.Dtos.DoctorDto
{
    public class CreateDoctorDto
    {

        public String FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Branch { get; set; } = null!;

        public string Title { get; set; } = null!;
        
        public bool IsFamilyDoctor { get; set; } 



    }
}
