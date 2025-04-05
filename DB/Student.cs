using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_SIMS.DB
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AccountId")]
        public int AccountId { get; set; }

        [ForeignKey("ClassRoomId")]
        public int? ClassRoomId { get; set; } // Nullable vì sinh viên có thể chưa thuộc lớp nào

        [ForeignKey("CourseId")]
        public int? CourseId { get; set; } // Nullable vì sinh viên có thể chưa đăng ký khóa học

        [Column("FullName", TypeName = "Varchar(100)"), Required]
        public string FullName { get; set; }

        [Column("Email", TypeName = "Varchar(100)"), Required]
        public string Email { get; set; }

        [Column("Phone", TypeName = "Varchar(20)"), Required]
        public string Phone { get; set; }

        [Column("Address", TypeName = "Varchar(150)")]
        public string Address { get; set; }

        [Column("Status", TypeName = "Varchar(20)"), Required]
        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Account Account { get; set; }
        public ClassRoom ClassRoom { get; set; }
        public Courses Course { get; set; }
    }
}