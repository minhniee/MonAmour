using Microsoft.AspNetCore.Mvc;

namespace MonAmour.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Trang không tồn tại";
                    ViewBag.ErrorCode = "404";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Lỗi máy chủ nội bộ";
                    ViewBag.ErrorCode = "500";
                    break;
                case 403:
                    ViewBag.ErrorMessage = "Bạn không có quyền truy cập";
                    ViewBag.ErrorCode = "403";
                    break;
                default:
                    ViewBag.ErrorMessage = "Đã xảy ra lỗi";
                    ViewBag.ErrorCode = statusCode.ToString();
                    break;
            }
            
            return View("NotFound");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View("NotFound");
        }

        [Route("NotFound")]
        public new IActionResult NotFound()
        {
            ViewBag.ErrorMessage = "Trang không tồn tại";
            ViewBag.ErrorCode = "404";
            return View();
        }
    }
}
