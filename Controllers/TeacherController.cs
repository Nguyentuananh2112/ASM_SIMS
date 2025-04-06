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

        // DIP: Inject SimsDataContext through constructor to reduce direct dependency
        public TeacherController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Display list of teachers
        public IActionResult Index()
        {
            // Check session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Account) // Include Account to retrieve account info if needed
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

        // Display form to add a teacher
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
            // Check for duplicate email
            var existingEmail = _dbContext.Teachers
                .Any(t => t.Email == model.Email && t.DeletedAt == null);

            if (existingEmail)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            // Check for duplicate phone number
            var existingPhone = _dbContext.Teachers
                .Any(t => t.Phone == model.Phone && t.DeletedAt == null);

            if (existingPhone)
            {
                ModelState.AddModelError("Phone", "Phone number already exists.");
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
                    ModelState.AddModelError("", $"Error while adding teacher: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.Courses = new SelectList(_dbContext.Courses, "Id", "NameCourse");
            return View(model);
        }

        // Display form to edit teacher
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
            ViewBag.Courses = _dbContext.Courses.ToList(); // Pass List<Courses> instead of SelectList
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeacherViewModel model)
        {
            // Check for duplicate email, excluding current record
            var existingEmail = _dbContext.Teachers
                .Any(t => t.Email == model.Email && t.Id != model.Id && t.DeletedAt == null);

            if (existingEmail)
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            // Check for duplicate phone number, excluding current record
            var existingPhone = _dbContext.Teachers
                .Any(t => t.Phone == model.Phone && t.Id != model.Id && t.DeletedAt == null);

            if (existingPhone)
            {
                ModelState.AddModelError("Phone", "Phone number already exists.");
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
                    ModelState.AddModelError("", $"Error while editing teacher: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.Courses = _dbContext.Courses.ToList(); // Pass List<Courses> instead of SelectList
            return View(model);
        }

        // Handle deleting teacher (soft delete)
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
                ModelState.AddModelError("", $"Error while deleting teacher: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
