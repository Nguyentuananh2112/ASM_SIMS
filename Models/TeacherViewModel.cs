using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        public int? CourseId { get; set; } // Thêm trường CourseId

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Nếu cần hiển thị danh sách lớp học hoặc khóa học liên quan, có thể thêm:
        // public List<int> ClassRoomIds { get; set; } // Hoặc thông tin khác
    }
}