using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public StudentController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Display the list of students
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var students = _dbContext.Students
                .Where(s => s.DeletedAt == null)
                .Include(s => s.ClassRoom)
                .Include(s => s.Course)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            ViewData["Title"] = "Students";
            return View(students);
        }

        // Display the form to add a student
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(new StudentViewModel());
        }

        // Handle adding a student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates in FullName, Email, Phone
                if (_dbContext.Students.Any(s => s.FullName == model.FullName && s.DeletedAt == null))
                {
                    ModelState.AddModelError("FullName", "Student name already exists.");
                }
                if (_dbContext.Students.Any(s => s.Email == model.Email && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                }
                if (_dbContext.Students.Any(s => s.Phone == model.Phone && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
                    ViewBag.Courses = _dbContext.Courses.ToList();
                    return View(model);
                }

                try
                {
                    var account = new Account
                    {
                        RoleId = 1,
                        Username = model.Email.Split('@')[0],
                        Password = "defaultPassword123",
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address ?? "",
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Accounts.Add(account);
                    _dbContext.SaveChanges();

                    var student = new Student
                    {
                        AccountId = account.Id,
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        ClassRoomId = model.ClassRoomId,
                        CourseId = model.CourseId,
                        Status = model.Status,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Students.Add(student);
                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error adding student: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Display the form to edit a student
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _dbContext.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Course)
                .FirstOrDefault(s => s.Id == id && s.DeletedAt == null);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                ClassRoomId = student.ClassRoomId,
                CourseId = student.CourseId,
                Status = student.Status
            };
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Handle editing a student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicates in FullName, Email, Phone (excluding the current record)
                if (_dbContext.Students.Any(s => s.FullName == model.FullName && s.Id != model.Id && s.DeletedAt == null))
                {
                    ModelState.AddModelError("FullName", "Student name already exists.");
                }
                if (_dbContext.Students.Any(s => s.Email == model.Email && s.Id != model.Id && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                }
                if (_dbContext.Students.Any(s => s.Phone == model.Phone && s.Id != model.Id && s.DeletedAt == null))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
                    ViewBag.Courses = _dbContext.Courses.ToList();
                    return View(model);
                }

                try
                {
                    var student = _dbContext.Students
                        .FirstOrDefault(s => s.Id == model.Id && s.DeletedAt == null);

                    if (student == null)
                    {
                        return NotFound();
                    }

                    student.FullName = model.FullName;
                    student.Email = model.Email;
                    student.Phone = model.Phone;
                    student.Address = model.Address;
                    student.ClassRoomId = model.ClassRoomId;
                    student.CourseId = model.CourseId;
                    student.Status = model.Status;
                    student.UpdatedAt = DateTime.Now;

                    _dbContext.Students.Update(student);
                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error editing student: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
            ViewBag.ClassRooms = _dbContext.ClassRooms.ToList();
            ViewBag.Courses = _dbContext.Courses.ToList();
            return View(model);
        }

        // Handle deleting a student (soft delete)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var student = _dbContext.Students
                .FirstOrDefault(s => s.Id == id && s.DeletedAt == null);

            if (student == null)
            {
                return NotFound();
            }

            try
            {
                student.DeletedAt = DateTime.Now;
                student.Status = "Deleted";
                _dbContext.Students.Remove(student);
                _dbContext.SaveChanges();
                TempData["save"] = true;
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                ModelState.AddModelError("", $"Error deleting student: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}