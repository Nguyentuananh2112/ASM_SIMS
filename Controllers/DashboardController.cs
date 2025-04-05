using Microsoft.AspNetCore.Mvc;

namespace ASM_SIMS.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // kiem tra session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["Title"] = "Dashboard - Hi There!";

            return View();
        }
    }
}
