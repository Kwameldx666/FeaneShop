using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error404()
        {
            return View();
        }
    }
}
