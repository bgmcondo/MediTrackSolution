using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Controllers
{
    public class AppointmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
