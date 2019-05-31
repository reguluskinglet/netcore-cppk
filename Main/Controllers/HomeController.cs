using Microsoft.AspNetCore.Mvc;

namespace Rzdppk.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
