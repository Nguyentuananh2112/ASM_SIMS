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

        /*// GET: Hiển thị form tạo tài khoản Admin đầu tiên
        [HttpGet]
        public IActionResult CreateFirstAdmin()
        {
            if (_dbContext.Accounts.Any())
            {
                return RedirectToAction("Index"); // Nếu đã có tài khoản, không cho tạo nữa
            }
            return View(new RegisterViewModel());
        }

        // POST: Xử lý tạo tài khoản Admin đầu tiên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFirstAdmin(RegisterViewModel model)
        {
            if (_dbContext.Accounts.Any())
            {
                return RedirectToAction("Index"); // Nếu đã có tài khoản, không cho tạo
            }

            if (ModelState.IsValid)
            {
                var account = new Account
                {
                    RoleId = 1, // Admin
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Accounts.Add(account);
                await _dbContext.SaveChangesAsync();

                TempData["MessageLogin"] = "Admin account created successfully! Please sign in.";
                return RedirectToAction("Index");
            }
            return View(model);
        }*/

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
