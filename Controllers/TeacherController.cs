using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly SimsDataContext _dbContext;

        // DIP: Tiêm SimsDataContext qua constructor để giảm phụ thuộc trực tiếp
        public TeacherController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Hiển thị danh sách giảng viên
        public IActionResult Index()
        {

            // kiem tra session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Account) // Include Account để lấy thông tin tài khoản nếu cần
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Email = t.Email,
                    Phone = t.Phone,
                    Address = t.Address,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();

            ViewData["Title"] = "Teachers";
            return View(teachers);
        }

        // Hiển thị form thêm giảng viên
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Courses = new SelectList(_dbContext.Courses, "Id", "NameCourse");
            return View(new TeacherViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TeacherViewModel model)
        {
            // Kiểm tra trùng lặp email
            var existingEmail = _dbContext.Teachers
                .Any(t => t.Email == model.Email && t.DeletedAt == null);

            if (existingEmail)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            // Kiểm tra trùng lặp số điện thoại
            var existingPhone = _dbContext.Teachers
                .Any(t => t.Phone == model.Phone && t.DeletedAt == null);

            if (existingPhone)
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var account = new Account
                    {
                        RoleId = 2,
                        Username = model.Email.Split('@')[0],
                        Password = "defaultPassword123",
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address ?? "",
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Accounts.Add(account);
                    _dbContext.SaveChanges();

                    var teacher = new Teacher
                    {
                        AccountId = account.Id,
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        CourseId = model.CourseId,
                        Status = model.Status,
                        CreatedAt = DateTime.Now,
                    };
                    _dbContext.Teachers.Add(teacher);
                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Lỗi khi thêm giảng viên: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.Courses = new SelectList(_dbContext.Courses, "Id", "NameCourse");
            return View(model);
        }

        // Hiển thị form sửa giảng viên
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _dbContext.Teachers.Find(id);
            if (teacher == null || teacher.DeletedAt != null) return NotFound();

            var model = new TeacherViewModel
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Address = teacher.Address,
                Status = teacher.Status,
                CourseId = teacher.CourseId
            };
            ViewBag.Courses = _dbContext.Courses.ToList(); // Truyền List<Courses> thay vì SelectList
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeacherViewModel model)
        {
            // Kiểm tra trùng lặp email, bỏ qua bản ghi hiện tại
            var existingEmail = _dbContext.Teachers
                .Any(t => t.Email == model.Email && t.Id != model.Id && t.DeletedAt == null);

            if (existingEmail)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            // Kiểm tra trùng lặp số điện thoại, bỏ qua bản ghi hiện tại
            var existingPhone = _dbContext.Teachers
                .Any(t => t.Phone == model.Phone && t.Id != model.Id && t.DeletedAt == null);

            if (existingPhone)
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var teacher = _dbContext.Teachers
                        .FirstOrDefault(t => t.Id == model.Id && t.DeletedAt == null);

                    if (teacher == null)
                    {
                        return NotFound();
                    }

                    teacher.FullName = model.FullName;
                    teacher.Email = model.Email;
                    teacher.Phone = model.Phone;
                    teacher.Address = model.Address;
                    teacher.Status = model.Status;
                    teacher.CourseId = model.CourseId;
                    teacher.UpdatedAt = DateTime.Now;

                    _dbContext.Teachers.Update(teacher);
                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Lỗi khi sửa giảng viên: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.Courses = _dbContext.Courses.ToList(); // Truyền List<Courses> thay vì SelectList
            return View(model);
        }

        
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var teacher = _dbContext.Teachers
                .FirstOrDefault(t => t.Id == id && t.DeletedAt == null);

            if (teacher == null)
            {
                return NotFound();
            }

            try
            {
                teacher.DeletedAt = DateTime.Now;
                teacher.Status = "Deleted";
                _dbContext.Teachers.Remove(teacher);
                _dbContext.SaveChanges();
                TempData["save"] = true;
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                ModelState.AddModelError("", $"Lỗi khi xóa giảng viênn: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
