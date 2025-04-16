using System.ComponentModel.DataAnnotations;
using ASM_SIMS.Validations;

namespace ASM_SIMS.Models
{
    public class TeacherViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public string Phone { get; set; }

        public string Address { get; set; }

        public int? AccountId { get; set; }

        public int? CourseId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [AllowedSizeFile(3*1024*1024)]
        [AllowedTypeFile(new string[] { ".jpg", ".png", ".jpeg", ".gif" })]
        public IFormFile? ViewImage { get; set; }

        public string? Image { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Thêm các trường liên quan đến Class
        public List<ClassRoomViewModel> ClassRooms { get; set; } = new List<ClassRoomViewModel>();
        
        // Giữ lại cho mục đích tương thích với mã hiện tại
        public int? SelectedClassRoomId { get; set; }
        
        // Cho phép chọn nhiều lớp học
        public List<int> SelectedClassRoomIds { get; set; } = new List<int>();
    }
}