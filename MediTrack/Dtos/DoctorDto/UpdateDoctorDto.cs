namespace MediTrack.Dtos.DoctorDto
{
    public class UpdateDoctorDto
    {
        // Id'yi almamıza gerek yok çünkü id  URL üzerinden alınacak. Bu yüzden sadece güncellenmesi gereken alanları alıyoruz.


        public string Branch { get; set; } = null!;
        public string Title { get; set; } = null!;
        public bool IsFamilyDoctor { get; set; }
    }
}
