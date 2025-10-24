using Microsoft.AspNetCore.Mvc;

namespace CMCSPrototype.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult AccessDenied() => View();
        public IActionResult Logout()
        {
            Response.Cookies.Delete("CMCS_Role");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetRole(string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                Response.Cookies.Append("CMCS_Role", role);
            }
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
