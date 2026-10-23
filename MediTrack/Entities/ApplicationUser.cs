using Microsoft.AspNetCore.Identity;

namespace MediTrack.Entities
{
    public class ApplicationUser:IdentityUser
    {

        //Id özelliğini miras alıyor o yüzden eklememize gerek yok. Ama isterseniz ekleyebilirsiniz.
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
