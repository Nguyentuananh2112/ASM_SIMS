using ASM_SIMS.DB;
using ASM_SIMS.Validations;
using System.ComponentModel.DataAnnotations;
namespace ASM_SIMS.Models
{
    public class CourseViewModel
    {
        public List<CourseDetail> courseList { get; set; }
    }
    public class UniqueCourseNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var courseDetail = (CourseDetail)validationContext.ObjectInstance;
                var dbContext = (SimsDataContext)validationContext.GetService(typeof(SimsDataContext));

                if (dbContext != null && dbContext.Courses.Any(c => c.NameCourse == value.ToString() && c.Id != courseDetail.Id && c.DeletedAt == null))
                {
                    return new ValidationResult("Tên khóa học đã tồn tại");
                }
            }
            return ValidationResult.Success;
        }
    }



    public class CourseDetail
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [UniqueCourseName]

        public string NameCourse { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateOnly StartDate { get; set; } // Đổi từ DateOnly sang DateTime

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [CustomValidation(typeof(CourseDetail), nameof(ValidateEndDate))]
        public DateOnly EndDate { get; set; } // Đổi từ DateOnly sang DateTime

        [Range(0, 5, ErrorMessage = "Đánh giá phải từ 0 đến 5")]
        public int Vote { get; set; }

        public bool Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static ValidationResult ValidateEndDate(DateOnly endDate, ValidationContext context)
        {
            var instance = (CourseDetail)context.ObjectInstance;
            if (endDate < instance.StartDate)
            {
                return new ValidationResult("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu");
            }
            return ValidationResult.Success;
        }
        public List<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>(); // Quan hệ nhiều-nhiều
    }
}
