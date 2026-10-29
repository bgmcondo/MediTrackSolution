using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    public class PatientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
