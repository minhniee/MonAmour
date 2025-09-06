using Microsoft.AspNetCore.Mvc;

namespace MonAmour.Controllers
{
    public class ConceptController1 : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Concept";
            return View();
        }
    }
}
