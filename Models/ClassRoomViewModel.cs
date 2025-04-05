using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.Models
{
    public class ClassRoomViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        [StringLength(60, ErrorMessage = "Class names cannot be longer than 60 characters.")]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "The course is mandatory")]
        public int? CourseId { get; set; }

        [Required(ErrorMessage = "Lecturer is required")]
        public int? TeacherId { get; set; }

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

        [Required(ErrorMessage = "Status is required")]
        //[StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
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