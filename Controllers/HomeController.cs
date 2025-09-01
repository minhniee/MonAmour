using Microsoft.AspNetCore.Mvc;

namespace Mon_Amour.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            ViewData["Title"] = "Trang ch? - MonAmour";
            return View();
        }
    }
}
