using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    public class MedicationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
