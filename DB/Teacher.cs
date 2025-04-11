using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.DB
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AccountId")] // Thêm dòng này
        public int AccountId { get; set; } // Thêm dòng này

        [ForeignKey("CourseId")] // Thêm khóa ngoại rõ ràng
        public int? CourseId { get; set; } // Cho phép null nếu không bắt buộc

        [Column("FullName", TypeName = "Varchar(100)"), Required]
        public string FullName { get; set; }

        [Column("Email", TypeName = "Varchar(100)"), Required]
        public string Email { get; set; }

        [Column("Phone", TypeName = "Varchar(20)"), Required]
        public string Phone { get; set; }

        [Column("Address", TypeName = "Varchar(150)")]
        public string? Address { get; set; }

        [Column("Status", TypeName = "Varchar(20)"), Required]
        public string Status { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Account Account { get; set; }
        public Courses Course { get; set; }

    }
}
