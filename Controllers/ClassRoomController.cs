using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ASM_SIMS.Controllers
{
    public class ClassRoomController : Controller
    {
        private readonly SimsDataContext _dbContext;

        // Constructor to inject the DbContext
        public ClassRoomController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Index: Show a list of classrooms
        public IActionResult Index()
        {
            // Check for session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var classRooms = _dbContext.ClassRooms
                .Where(c => c.DeletedAt == null)  // Only get active classrooms
                .Include(c => c.Course)           // Include associated Course data
                //.Include(c => c.Teacher)          // Include associated Teacher data
                .Select(c => new ClassRoomViewModel
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    CourseId = c.CourseId,
                    TeacherId = c.TeacherId,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Schedule = c.Schedule,
                    Location = c.Location,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

            // Assign ViewBag values for Courses and Teachers
            ViewBag.Courses = _dbContext.Courses
                .Where(c => c.DeletedAt == null)
                .ToList();
            ViewBag.Teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .ToList();

            ViewData["Title"] = "Class Rooms";
            return View(classRooms);
        }

        // GET: Display the create class form
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Courses = _dbContext.Courses
                .Where(c => c.DeletedAt == null)
                .ToList();
            ViewBag.Teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .ToList();
            return View(new ClassRoomViewModel());
        }

        // POST: Handle the class creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClassRoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var classRoom = new ClassRoom
                    {
                        ClassName = model.ClassName,
                        CourseId = model.CourseId ?? 0,
                        TeacherId = model.TeacherId ?? 0,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Schedule = model.Schedule,
                        Location = model.Location,
                        Status = "Active",
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.ClassRooms.Add(classRoom);
                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Validation errors occurred. Check ModelState for details.");
            }

            ViewBag.Courses = _dbContext.Courses
                .Where(c => c.DeletedAt == null)
                .ToList();
            ViewBag.Teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .ToList();
            return View(model);
        }

        // GET: Display the edit form for class
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var classRoom = _dbContext.ClassRooms.Find(id);
            if (classRoom == null || classRoom.DeletedAt != null) return NotFound();

            var model = new ClassRoomViewModel
            {
                Id = classRoom.Id,
                ClassName = classRoom.ClassName,
                CourseId = classRoom.CourseId,
                TeacherId = classRoom.TeacherId,
                StartDate = classRoom.StartDate,
                EndDate = classRoom.EndDate,
                Location = classRoom.Location,
                Status = classRoom.Status
            };
            ViewBag.Courses = _dbContext.Courses.ToList();
            ViewBag.Teachers = _dbContext.Teachers.ToList();
            return View(model);
        }

        // POST: Handle the edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClassRoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == model.Id);
                    if (classRoom == null)
                    {
                        TempData["save"] = false;
                        return RedirectToAction(nameof(Index));
                    }

                    // Update fields
                    classRoom.ClassName = model.ClassName;
                    classRoom.CourseId = model.CourseId ?? 0;
                    //classRoom.TeacherId = model.TeacherId ?? 0;
                    classRoom.StartDate = model.StartDate;
                    classRoom.EndDate = model.EndDate;
                    classRoom.Schedule = model.Schedule;
                    classRoom.Location = model.Location;
                    classRoom.Status = model.Status;
                    classRoom.UpdatedAt = DateTime.Now;

                    _dbContext.SaveChanges();
                    TempData["save"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ModelState.AddModelError("", "Validation errors: " + string.Join(", ", errors));
                System.Diagnostics.Debug.WriteLine("Validation errors: " + string.Join(", ", errors));
            }

            ViewBag.Courses = _dbContext.Courses
                .Where(c => c.DeletedAt == null)
                .ToList();
            ViewBag.Teachers = _dbContext.Teachers
                .Where(t => t.DeletedAt == null)
                .ToList();
            return View(model);
        }

        // POST: Handle the delete class
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == id);
                if (classRoom == null)
                {
                    TempData["save"] = false;
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.ClassRooms.Remove(classRoom);
                _dbContext.SaveChanges();
                TempData["save"] = true;
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Add students to the class
        [HttpGet]
        public IActionResult AddStudentToClass(int classRoomId)
        {
            var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == classRoomId && c.DeletedAt == null);
            if (classRoom == null)
            {
                return NotFound();
            }

            // Ensure Students are included in the query and filter properly
            var students = _dbContext.Students
                .Where(s => s.DeletedAt == null && (s.ClassRoomId == null || s.ClassRoomId == classRoomId))
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Address = s.Address,
                    Status = s.Status ?? string.Empty,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    AccountId = s.AccountId,
                    IsSelected = s.ClassRoomId.HasValue && s.ClassRoomId == classRoomId // Selected if belongs to this class
                }).ToList();

            var model = new AssignStudentsViewModel
            {
                ClassRoomId = classRoomId,
                ClassRoomName = classRoom?.ClassName ?? "N/A",
                Students = students
            };

            return View(model);
        }

        // POST: Assign selected students to the class
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStudentToClass(AssignStudentsViewModel model)
        {
            var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == model.ClassRoomId && c.DeletedAt == null);
            if (classRoom == null)
            {
                return NotFound();
            }

            try
            {
                // Get selected student IDs
                var selectedStudentIds = model.Students
                    .Where(s => s.IsSelected)
                    .Select(s => s.Id)
                    .ToList();

                // Update ClassRoomId for selected students
                var studentsToAdd = _dbContext.Students
                    .Where(s => selectedStudentIds.Contains(s.Id))
                    .ToList();
                foreach (var student in studentsToAdd)
                {
                    student.ClassRoomId = model.ClassRoomId;
                }

                // Remove ClassRoomId for unselected students
                var unselectedStudentIds = model.Students
                    .Where(s => !s.IsSelected)
                    .Select(s => s.Id)
                    .ToList();
                var studentsToRemove = _dbContext.Students
                    .Where(s => unselectedStudentIds.Contains(s.Id) && s.ClassRoomId == model.ClassRoomId)
                    .ToList();
                foreach (var student in studentsToRemove)
                {
                    student.ClassRoomId = null;
                }

                _dbContext.SaveChanges();
                TempData["save"] = true;
                return RedirectToAction(nameof(Details), new { id = model.ClassRoomId });
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                ModelState.AddModelError("", $"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }

            // Reload students and display them again
            model.Students = _dbContext.Students
                .Where(s => s.DeletedAt == null && (s.ClassRoomId == null || s.ClassRoomId == model.ClassRoomId))
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Address = s.Address,
                    Status = s.Status ?? string.Empty,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    AccountId = s.AccountId,
                    IsSelected = model.Students.Any(m => m.Id == s.Id && m.IsSelected)
                }).ToList();

            model.ClassRoomName = classRoom?.ClassName ?? "N/A";
            return View(model);
        }

        // Details: Show class information and student list
        [HttpGet]
        public IActionResult Details(int id)
        {
            var classRoom = _dbContext.ClassRooms
                .Include(c => c.Course)
                //.Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

            if (classRoom == null)
            {
                return NotFound();
            }

            var model = new ClassRoomViewModel
            {
                Id = classRoom.Id,
                ClassName = classRoom.ClassName ?? string.Empty,
                CourseId = classRoom.CourseId,
                TeacherId = classRoom.TeacherId,
                StartDate = classRoom.StartDate,
                EndDate = classRoom.EndDate,
                Schedule = classRoom.Schedule ?? string.Empty,
                Location = classRoom.Location,
                Status = classRoom.Status ?? string.Empty,
                CreatedAt = classRoom.CreatedAt,
                UpdatedAt = classRoom.UpdatedAt
            };

            ViewBag.Students = classRoom.Students?
                .Where(s => s.DeletedAt == null)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Address = s.Address,
                    Status = s.Status ?? string.Empty,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    AccountId = s.AccountId
                }).ToList() ?? new List<StudentViewModel>();

            ViewBag.CourseName = classRoom.Course?.NameCourse ?? "N/A";
            ViewBag.TeacherName = classRoom.Teacher?.FullName ?? "N/A";
            return View(model);
        }
    }
}
