using Microsoft.AspNetCore.Mvc;

namespace CMCSPrototype.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index(string role = "Lecturer")
        {
            ViewData["Role"] = role;
            return View();
        }
    }
}
