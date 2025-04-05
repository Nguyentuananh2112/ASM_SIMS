using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.DB
{
    public class ClassRoom
    {
     
            [Key]
            public int Id { get; set; }

            [ForeignKey("CourseId")]
            public int CourseId { get; set; }

            [ForeignKey("TeacherId")]
            public int TeacherId { get; set; } // Không nullable vì ClassRoom phải có Teacher

            [Column("ClassName", TypeName = "Varchar(60)"), Required]
            public string ClassName { get; set; } = string.Empty; // Khởi tạo mặc định

            [Column("StartDate"), Required]
            public DateOnly StartDate { get; set; }

            [Column("EndDate"), Required]
            public DateOnly EndDate { get; set; }

            [Column("Schedule", TypeName = "Varchar(100)"), Required]
            public string Schedule { get; set; } = string.Empty; // Khởi tạo mặc định

            [Column("Location", TypeName = "Varchar(100)")]
            public string? Location { get; set; } // Nullable vì không có [Required]

            [Column("Status", TypeName = "Varchar(20)"), Required]
            public string Status { get; set; } = string.Empty; // Khởi tạo mặc định

            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? DeletedAt { get; set; }

            // Quan hệ điều hướng
            public Courses Course { get; set; } = null!; // Khởi tạo để tránh CS8618
            public Teacher Teacher { get; set; } = null!; // Khởi tạo để tránh CS8618
            public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}

