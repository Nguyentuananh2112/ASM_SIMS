using System;
using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.Models
{
    public class ClassRoomViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(60, ErrorMessage = "Class name cannot be longer than 60 characters")]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int? CourseId { get; set; } // Sử dụng int? để tránh lỗi "The value '' is invalid"

        [Required(ErrorMessage = "Please select a lecturer")]
        public int? TeacherId { get; set; } // Sử dụng int? để tránh lỗi "The value '' is invalid"

        [Required(ErrorMessage = "Start date is required")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [CustomValidation(typeof(ClassRoomViewModel), nameof(ValidateEndDate))]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Class schedule is mandatory")]
        [StringLength(100, ErrorMessage = "Class schedule cannot be longer than 100 characters")]
        public string Schedule { get; set; }

        [StringLength(100, ErrorMessage = "Location must not be longer than 100 characters")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Please select a status")]
        [StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static ValidationResult ValidateEndDate(DateOnly endDate, ValidationContext context)
        {
            var instance = (ClassRoomViewModel)context.ObjectInstance;
            if (endDate < instance.StartDate)
            {
                return new ValidationResult("End date must be greater than or equal to start date");
            }
            return ValidationResult.Success;
        }
    }
}