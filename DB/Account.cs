using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Data;


namespace ASM_SIMS.DB
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("RoleId")]
        public int RoleId { get; set; }


        [Column("Username", TypeName = "Varchar(60)"), Required]
        public string Username { get; set; }

        [Column("Password", TypeName = "Varchar(100)"), Required]
        public string Password { get; set; }

        [Column("Email", TypeName = "Varchar(100)"), Required]
        public string Email { get; set; }

        [Column("Phone", TypeName = "Varchar(20)"), Required]
        public string Phone { get; set; }

        [Column("Address", TypeName = "Varchar(150)"), AllowNull]
        public string Address { get; set; }


        [AllowNull]
        public DateTime? CreatedAt { get; set; }

        [AllowNull]
        public DateTime? UpdatedAt { get; set; }

        [AllowNull]
        public DateTime? DeletedAt { get; set; }
   
        public List<Student> Students { get; set; } = new List<Student>();

    }
}
