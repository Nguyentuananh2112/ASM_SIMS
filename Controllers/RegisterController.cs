using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public RegisterController(SimsDataContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // GET: Hiển thị trang đăng ký
        [HttpGet]
        public IActionResult Index()
        {
            return View(new RegisterViewModel());
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại chưa
                if (await _dbContext.Accounts.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Kiểm tra username đã tồn tại chưa (tùy chọn)
                if (await _dbContext.Accounts.AnyAsync(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(model);
                }

                // Kiểm tra số điện thoại đã tồn tại chưa
                if (await _dbContext.Accounts.AnyAsync(a => a.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                    return View(model);
                }

                var account = new Account
                {
                    RoleId = model.Role switch // Chuyển đổi Role thành RoleId (giả định)
                    {
                        "Admin" => 1,
                        "Teacher" => 2,
                        "Student" => 3,
                        _ => 3 // Mặc định là Student nếu không khớp
                    },
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Accounts.Add(account);
                await _dbContext.SaveChangesAsync();

                //TempData["MessageRegister"] = "Registration successful! Please sign in.";
                return RedirectToAction("Index", "Login");
            }
            return View(model);
        }
    }
}