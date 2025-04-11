using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ASM_SIMS.DB
{
    public class Courses
    {
        [AllowNull]
        public DateTime? CreatedAt { get; set; }

        [Key]
        public int Id { get; set; }

        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }

        [Column("NameCourse", TypeName = "Varchar(60)"), Required]
        public string NameCourse { get; set; }

        [Column("Description", TypeName = "Varchar(255)"), AllowNull]
        public string? Description { get; set; }

        [Column("StartDate"), Required]
        public DateOnly StartDate { get; set; }

        [Column("EndDate"), Required]
        public DateOnly EndDate { get; set; }

        [Column("Vote", TypeName = "Integer"), Required]
        public int Vote { get; set; }

        [Column("Status", TypeName = "Varchar(20)"), Required]
        public string Status { get; set; } 

        [AllowNull]
        public DateTime? UpdatedAt { get; set; }

        [AllowNull]
        public DateTime? DeletedAt { get; set; }

        // Thuộc tính điều hướng
        public Categories Category { get; set; }
    }
}