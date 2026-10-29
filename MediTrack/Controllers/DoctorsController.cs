using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    public class DoctorsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
