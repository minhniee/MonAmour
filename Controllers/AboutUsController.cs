using Microsoft.AspNetCore.Mvc;

namespace MonAmour.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
