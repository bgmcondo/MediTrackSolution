using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    public class FamilyDoctorsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
