using Microsoft.AspNetCore.Mvc;

namespace MonAmour.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ - MonAmour";
            return View();
        }
    }
}