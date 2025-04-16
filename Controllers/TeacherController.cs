using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM_SIMS.Helpers;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using ASM_SIMS.Validations;

namespace ASM_SIMS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly SimsDataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor: Injects SimsDataContext and IWebHostEnvironment for dependency injection
        public TeacherController(SimsDataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        // Displays a list of all active teachers
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var teachers = _dataContext.Teachers
                .Where(t => t.DeletedAt == null)
                .Include(t => t.Course)
                .Include(t => t.ClassRooms)
                .AsNoTracking()
                .ToList()
                .Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Email = t.Email,
                    Phone = t.Phone,
                    Address = t.Address,
                    CourseId = t.CourseId,
                    Status = t.Status,
                    Image = t.Image,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ClassRooms = t.ClassRooms?.Select(c => new ClassRoomViewModel
                    {
                        Id = c.Id,
                        ClassName = c.ClassName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status
                    }).ToList() ?? new List<ClassRoomViewModel>()
                })
                .ToList();

            // Sửa lại: Lấy tất cả ClassRoom mà Teacher là giáo viên chính (TeacherId)
            // Sử dụng GroupBy thay vì ToDictionary để xử lý nhiều lớp học có cùng TeacherId
            var classRoomsWithMainTeacher = _dataContext.ClassRooms
                .Where(c => c.TeacherId.HasValue && c.DeletedAt == null)
                .GroupBy(c => c.TeacherId.Value)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(c => new ClassRoomViewModel
                    {
                        Id = c.Id,
                        ClassName = c.ClassName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status
                    }).ToList()
                );

            // Bổ sung thông tin lớp học từ mối quan hệ TeacherId
            foreach (var teacher in teachers)
            {
                // Nếu teacher là giáo viên chính của lớp nào đó, thêm vào danh sách ClassRooms
                if (classRoomsWithMainTeacher.TryGetValue(teacher.Id, out var mainClassRooms))
                {
                    foreach (var mainClassRoom in mainClassRooms)
                    {
                        // Kiểm tra xem đã có lớp này trong danh sách chưa
                        if (!teacher.ClassRooms.Any(c => c.Id == mainClassRoom.Id))
                        {
                            teacher.ClassRooms.Add(mainClassRoom);
                        }
                    }
                }
            }

            ViewBag.Courses = _dataContext.Courses.ToList();
            ViewBag.ClassRooms = _dataContext.ClassRooms.Where(c => c.DeletedAt == null).ToList();
            ViewData["Title"] = "Teacher List";
            return View(teachers);
        }

        // Displays the form to add a new teacher
        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Courses = _dataContext.Courses.ToList();
            ViewBag.ClassRooms = _dataContext.ClassRooms.ToList();
            return View(new TeacherViewModel());
        }

        // Saves an uploaded file to the server and returns the file name
        private string SaveFile(IFormFile file)
        {
            if (file == null) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "SIMS", "Uploads", "Images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Use MemoryStream to avoid file locking issues
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    memoryStream.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }

        // Deletes a file from the server
        private void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "SIMS", "Uploads", "Images", fileName);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (IOException)
                {
                    // Ignore deletion errors to prevent blocking the operation
                }
            }
        }

        // Handles the creation of a new teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TeacherViewModel model, IFormFile ViewImage)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            // Validate unique email
            if (_dataContext.Teachers.Any(t => t.Email == model.Email && t.DeletedAt == null))
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
            }

            // Validate unique phone number
            if (_dataContext.Teachers.Any(t => t.Phone == model.Phone && t.DeletedAt == null))
            {
                ModelState.AddModelError("Phone", "This phone number is already registered.");
            }

            // Validate course and classroom assignment - không cho phép hai giáo viên dạy cùng môn trong cùng lớp
            if (model.SelectedClassRoomId.HasValue && model.CourseId.HasValue)
            {
                var isCourseClassAssigned = _dataContext.Teachers
                    .Where(t => t.DeletedAt == null && t.CourseId == model.CourseId)
                    .SelectMany(t => t.ClassRooms)
                    .Any(c => c.Id == model.SelectedClassRoomId.Value);

                if (isCourseClassAssigned)
                {
                    var className = _dataContext.ClassRooms
                        .Where(c => c.Id == model.SelectedClassRoomId)
                        .Select(c => c.ClassName)
                        .FirstOrDefault();
                    var courseName = _dataContext.Courses
                        .Where(c => c.Id == model.CourseId)
                        .Select(c => c.NameCourse)
                        .FirstOrDefault();

                    ModelState.AddModelError("CourseId", $"The course '{courseName}' is already assigned to class '{className}'.");
                    ModelState.AddModelError("SelectedClassRoomId", $"The class '{className}' already has a teacher for course '{courseName}'.");
                }
            }

            if (ModelState.IsValid)
            {
                using (var transaction = _dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        string imageFileName = SaveFile(ViewImage);

                        // Create associated account
                        var account = new Account
                        {
                            RoleId = 2, // Teacher role
                            Username = model.Email.Split('@')[0],
                            Password = "defaultPassword123", // Consider hashing in production
                            Email = model.Email,
                            Phone = model.Phone,
                            Address = model.Address ?? string.Empty,
                            CreatedAt = DateTime.Now
                        };
                        _dataContext.Accounts.Add(account);
                        _dataContext.SaveChanges();

                        // Create teacher record
                        var teacher = new Teacher
                        {
                            AccountId = account.Id,
                            FullName = model.FullName,
                            Email = model.Email,
                            Phone = model.Phone,
                            Address = model.Address,
                            Image = imageFileName,
                            CourseId = model.CourseId,
                            Status = model.Status,
                            CreatedAt = DateTime.Now,
                            ClassRooms = new List<ClassRoom>()
                        };

                        _dataContext.Teachers.Add(teacher);
                        _dataContext.SaveChanges();

                        // Assign classroom if selected
                        if (model.SelectedClassRoomId.HasValue)
                        {
                            var classRoom = _dataContext.ClassRooms
                                .FirstOrDefault(c => c.Id == model.SelectedClassRoomId.Value);

                            if (classRoom != null)
                            {
                                // Thêm teacher vào danh sách teachers của classroom (quan hệ nhiều-nhiều)
                                teacher.ClassRooms.Add(classRoom);
                                
                                // Nếu lớp chưa có giáo viên chính, gán teacher này làm giáo viên chính
                                if (!classRoom.TeacherId.HasValue)
                                {
                                    classRoom.TeacherId = teacher.Id;
                                }
                                
                                _dataContext.SaveChanges();
                            }
                        }

                        transaction.Commit();
                        TempData["Success"] = "Teacher added successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Failed to add teacher.";
                        ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    }
                }
            }

            ViewBag.Courses = _dataContext.Courses.ToList();
            ViewBag.ClassRooms = _dataContext.ClassRooms.Where(c => c.DeletedAt == null).ToList();
            return View(model);
        }

        // Displays the form to edit an existing teacher
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var teacher = _dataContext.Teachers
                .Include(t => t.Course)
                .Include(t => t.ClassRooms)
                .FirstOrDefault(t => t.Id == id && t.DeletedAt == null);

            if (teacher == null)
            {
                return NotFound();
            }

            var model = new TeacherViewModel
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Address = teacher.Address,
                Image = teacher.Image,
                CourseId = teacher.CourseId,
                Status = teacher.Status,
                SelectedClassRoomId = teacher.ClassRooms.FirstOrDefault()?.Id
            };

            ViewBag.Courses = new SelectList(_dataContext.Courses.ToList(), "Id", "NameCourse");
            ViewBag.ClassRooms = new SelectList(_dataContext.ClassRooms.Where(c => c.DeletedAt == null).ToList(), "Id", "ClassName");
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive" });
            return View(model);
        }

        // Handles updates to an existing teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TeacherViewModel model, IFormFile ViewImage)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            ModelState.Remove("ViewImage");

            // Validate unique email
            if (_dataContext.Teachers.Any(t => t.Email == model.Email && t.Id != model.Id && t.DeletedAt == null))
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
            }

            // Validate unique phone number
            if (_dataContext.Teachers.Any(t => t.Phone == model.Phone && t.Id != model.Id && t.DeletedAt == null))
            {
                ModelState.AddModelError("Phone", "This phone number is already registered.");
            }

            // Validate course and classroom assignment - không cho phép hai giáo viên dạy cùng môn trong cùng lớp
            if (model.SelectedClassRoomId.HasValue && model.CourseId.HasValue)
            {
                var isCourseClassAssigned = _dataContext.Teachers
                    .Where(t => t.DeletedAt == null && t.Id != model.Id && t.CourseId == model.CourseId)
                    .SelectMany(t => t.ClassRooms)
                    .Any(c => c.Id == model.SelectedClassRoomId.Value);

                if (isCourseClassAssigned)
                {
                    var className = _dataContext.ClassRooms
                        .Where(c => c.Id == model.SelectedClassRoomId)
                        .Select(c => c.ClassName)
                        .FirstOrDefault();
                    var courseName = _dataContext.Courses
                        .Where(c => c.Id == model.CourseId)
                        .Select(c => c.NameCourse)
                        .FirstOrDefault();

                    ModelState.AddModelError("CourseId", $"The course '{courseName}' is already assigned to class '{className}'.");
                    ModelState.AddModelError("SelectedClassRoomId", $"The class '{className}' already has a teacher for course '{courseName}'.");
                }
            }

            if (ModelState.IsValid)
            {
                using (var transaction = _dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        var teacher = _dataContext.Teachers
                            .Include(t => t.ClassRooms)
                            .FirstOrDefault(t => t.Id == model.Id && t.DeletedAt == null);

                        if (teacher == null)
                        {
                            return NotFound();
                        }

                        // Update image if a new one is uploaded
                        if (ViewImage != null)
                        {
                            DeleteFile(teacher.Image);
                            teacher.Image = SaveFile(ViewImage);
                        }

                        // Update teacher details
                        teacher.FullName = model.FullName;
                        teacher.Email = model.Email;
                        teacher.Phone = model.Phone;
                        teacher.Address = model.Address;
                        teacher.Status = model.Status;
                        teacher.CourseId = model.CourseId;
                        teacher.UpdatedAt = DateTime.Now;

                        // Trước khi xóa lớp hiện tại, lưu danh sách các lớp mà giáo viên này là giáo viên chính
                        var oldClassRoomsAsMainTeacher = _dataContext.ClassRooms
                            .Where(c => c.TeacherId == teacher.Id)
                            .ToList();
                        
                        // Xóa tham chiếu TeacherId trong bảng ClassRoom nếu teacher này là giáo viên chính
                        foreach (var classRoom in oldClassRoomsAsMainTeacher)
                        {
                            classRoom.TeacherId = null;
                        }
                        
                        // Update classroom assignment (quan hệ nhiều-nhiều)
                        teacher.ClassRooms.Clear();
                        if (model.SelectedClassRoomId.HasValue)
                        {
                            var classRoom = _dataContext.ClassRooms
                                .FirstOrDefault(c => c.Id == model.SelectedClassRoomId.Value);

                            if (classRoom != null)
                            {
                                teacher.ClassRooms.Add(classRoom);
                                
                                // Nếu lớp chưa có giáo viên chính, gán teacher này làm giáo viên chính
                                if (!classRoom.TeacherId.HasValue)
                                {
                                    classRoom.TeacherId = teacher.Id;
                                }
                            }
                        }

                        _dataContext.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Teacher updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Failed to update teacher.";
                        ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    }
                }
            }

            ViewBag.Courses = new SelectList(_dataContext.Courses.ToList(), "Id", "NameCourse", model.CourseId);
            ViewBag.ClassRooms = new SelectList(_dataContext.ClassRooms.Where(c => c.DeletedAt == null).ToList(), "Id", "ClassName", model.SelectedClassRoomId);
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive" }, model.Status);
            return View(model);
        }

        // Soft-deletes a teacher by marking them as deleted
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Login");
            }

            var teacher = _dataContext.Teachers
                .Include(t => t.ClassRooms)
                .FirstOrDefault(t => t.Id == id); // Bỏ điều kiện DeletedAt vì sẽ xóa cứng

            if (teacher == null)
            {
                return NotFound();
            }

            try
            {
                // Cập nhật TeacherId trong ClassRoom thành null
                var classRoomsAsMainTeacher = _dataContext.ClassRooms
                    .Where(c => c.TeacherId == teacher.Id)
                    .ToList();
                
                foreach (var classRoom in classRoomsAsMainTeacher)
                {
                    classRoom.TeacherId = null;
                }
                
                // Xóa quan hệ nhiều-nhiều với ClassRoom
                teacher.ClassRooms.Clear();
                
                // Xóa cứng: Xóa hoàn toàn bản ghi khỏi cơ sở dữ liệu
                _dataContext.Teachers.Remove(teacher);
                _dataContext.SaveChanges();
                TempData["Success"] = "Teacher deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete teacher.";
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}