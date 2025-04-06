using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SimsDataContext _dbContext;

        // DIP: Tiêm SimsDataContext qua constructor
        public LoginController(SimsDataContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            //// Kiểm tra xem database có tài khoản nào không
            //ViewBag.CanCreateFirstAdmin = !_dbContext.Accounts.Any();
            return View(new LoginViewModel());
        }


        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra thông tin đăng nhập từ database
                var account = await _dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Email == model.Email && a.Password == model.Password && a.DeletedAt == null);

                if (account != null)
                {
                    // Lưu thông tin vào session
                    HttpContext.Session.SetString("UserId", account.Id.ToString());
                    HttpContext.Session.SetString("Username", account.Username);
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ViewData["MessageLogin"] = "Invalid email or password";
                }
            }
            return View(model);
        }

       

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa session
            return RedirectToAction("Index", "Login");
        }
    }
}
