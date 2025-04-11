using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_SIMS.Controllers
{
    public class ClassRoomController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public ClassRoomController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Kiểm tra session trong mỗi action
        private IActionResult CheckSession()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        // Lấy danh sách lớp học active
        private List<ClassRoomViewModel> GetActiveClassRooms()
        {
            return _dbContext.ClassRooms
                .Where(c => c.DeletedAt == null)
                .Include(c => c.Course)
                .Include(c => c.Teacher)
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
        }

        // Lấy thông tin chi tiết lớp học
        private ClassRoomViewModel GetClassRoomDetails(int id)
        {
            var classRoom = _dbContext.ClassRooms
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

            if (classRoom == null) return null;

            return new ClassRoomViewModel
            {
                Id = classRoom.Id,
                ClassName = classRoom.ClassName ?? string.Empty,
                CourseId = classRoom.CourseId,
                TeacherId = classRoom.TeacherId,
                StartDate = classRoom.StartDate,
                EndDate = classRoom.EndDate,
                Schedule = classRoom.Schedule ?? string.Empty,
                Location = classRoom.Location,
                Status = classRoom.Status,
                CreatedAt = classRoom.CreatedAt,
                UpdatedAt = classRoom.UpdatedAt
            };
        }

        // Điền dữ liệu cho ViewBag
        private void PopulateViewBag()
        {
            ViewBag.Courses = _dbContext.Courses.Where(c => c.DeletedAt == null).ToList();
            ViewBag.Teachers = _dbContext.Teachers.Where(t => t.DeletedAt == null).ToList();
        }

        // Xử lý lỗi chung
        private IActionResult HandleError(Exception ex, string actionName, ClassRoomViewModel model = null)
        {
            TempData["save"] = false;
            if (model != null)
            {
                ModelState.AddModelError("", $"Error in {actionName}: {ex.Message}");
                PopulateViewBag();
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var classRooms = GetActiveClassRooms();
            PopulateViewBag();
            ViewData["Title"] = "Class Rooms";
            return View(classRooms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            PopulateViewBag();
            return View(new ClassRoomViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClassRoomViewModel model)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (!ModelState.IsValid)
            {
                PopulateViewBag();
                return View(model);
            }

            try
            {
                if (_dbContext.ClassRooms.Any(c => c.ClassName == model.ClassName && c.DeletedAt == null))
                {
                    ModelState.AddModelError("ClassName", "Class name already exists.");
                    PopulateViewBag();
                    return View(model);
                }

                var classRoom = new ClassRoom
                {
                    ClassName = model.ClassName,
                    CourseId = model.CourseId!.Value,
                    TeacherId = model.TeacherId!.Value,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Schedule = model.Schedule,
                    Location = model.Location,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    Students = new List<Student>()
                };
                _dbContext.ClassRooms.Add(classRoom);
                _dbContext.SaveChanges();
                TempData["save"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Create", model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

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
                Schedule = classRoom.Schedule,
                Location = classRoom.Location,
                Status = classRoom.Status
            };
            PopulateViewBag();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClassRoomViewModel model)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (!ModelState.IsValid)
            {
                PopulateViewBag();
                return View(model);
            }

            try
            {
                if (_dbContext.ClassRooms.Any(c => c.ClassName == model.ClassName && c.Id != model.Id && c.DeletedAt == null))
                {
                    ModelState.AddModelError("ClassName", "Class name already exists.");
                    PopulateViewBag();
                    return View(model);
                }

                var classRoom = _dbContext.ClassRooms
                    .Include(c => c.Students)
                    .FirstOrDefault(c => c.Id == model.Id && c.DeletedAt == null);

                if (classRoom == null) return NotFound();

                classRoom.ClassName = model.ClassName;
                classRoom.CourseId = model.CourseId!.Value;
                classRoom.TeacherId = model.TeacherId!.Value;
                classRoom.StartDate = model.StartDate;
                classRoom.EndDate = model.EndDate;
                classRoom.Schedule = model.Schedule;
                classRoom.Location = model.Location;
                classRoom.Status = model.Status;
                classRoom.UpdatedAt = DateTime.Now;

                _dbContext.ClassRooms.Update(classRoom);
                _dbContext.SaveChanges();
                TempData["save"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Edit", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

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
                return HandleError(ex, "Delete");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AddStudentToClass(int classRoomId)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == classRoomId && c.DeletedAt == null);
            if (classRoom == null) return NotFound();

            var students = _dbContext.Students
                .Where(s => s.DeletedAt == null && (s.ClassRoomId == null || s.ClassRoomId == classRoomId))
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Address = s.Address,
                    Status = s.Status,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    AccountId = s.AccountId,
                    IsSelected = s.ClassRoomId.HasValue && s.ClassRoomId == classRoomId
                }).ToList();

            var model = new AssignStudentsViewModel
            {
                ClassRoomId = classRoomId,
                ClassRoomName = classRoom.ClassName ?? "N/A",
                Students = students
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStudentToClass(AssignStudentsViewModel model)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var classRoom = _dbContext.ClassRooms.FirstOrDefault(c => c.Id == model.ClassRoomId && c.DeletedAt == null);
            if (classRoom == null) return NotFound();

            try
            {
                var selectedStudentIds = model.Students
                    .Where(s => s.IsSelected)
                    .Select(s => s.Id)
                    .ToList();

                var studentsToAdd = _dbContext.Students
                    .Where(s => selectedStudentIds.Contains(s.Id))
                    .ToList();
                foreach (var student in studentsToAdd)
                {
                    student.ClassRoomId = model.ClassRoomId;
                }

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
                ModelState.AddModelError("", $"Error: {ex.Message}");
                TempData["save"] = false;

                model.Students = _dbContext.Students
                    .Where(s => s.DeletedAt == null && (s.ClassRoomId == null || s.ClassRoomId == model.ClassRoomId))
                    .Select(s => new StudentViewModel
                    {
                        Id = s.Id,
                        FullName = s.FullName ?? string.Empty,
                        Email = s.Email ?? string.Empty,
                        Phone = s.Phone ?? string.Empty,
                        Address = s.Address,
                        Status = s.Status,
                        ClassRoomId = s.ClassRoomId,
                        CourseId = s.CourseId,
                        AccountId = s.AccountId,
                        IsSelected = model.Students.Any(m => m.Id == s.Id && m.IsSelected)
                    }).ToList();

                model.ClassRoomName = classRoom.ClassName ?? "N/A";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var model = GetClassRoomDetails(id);
            if (model == null) return NotFound();

            ViewBag.Students = _dbContext.Students
                .Where(s => s.ClassRoomId == id && s.DeletedAt == null)
                .Select(s => new StudentViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName ?? string.Empty,
                    Email = s.Email ?? string.Empty,
                    Phone = s.Phone ?? string.Empty,
                    Address = s.Address,
                    Status = s.Status,
                    ClassRoomId = s.ClassRoomId,
                    CourseId = s.CourseId,
                    AccountId = s.AccountId
                }).ToList();

            ViewBag.CourseName = _dbContext.Courses.Find(model.CourseId)?.NameCourse ?? "N/A";
            ViewBag.TeacherName = _dbContext.Teachers.Find(model.TeacherId)?.FullName ?? "N/A";
            return View(model);
        }
    }
}