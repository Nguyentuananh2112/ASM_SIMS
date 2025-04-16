using Microsoft.EntityFrameworkCore;

namespace ASM_SIMS.DB
{
    public class SimsDataContext : DbContext
    {
        public SimsDataContext(DbContextOptions<SimsDataContext> options) : base(options)
        {
        }

        //Truyen vao DbSet de mapping voi bang trong database
        // gen ra cac bang trong database
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Account> Accounts { get; set; } // Sửa "Account" thành "Accounts" để tuân theo quy ước đặt tên
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình quan hệ giữa Student và Account
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Account)
                .WithMany(a => a.Students)
                .HasForeignKey(s => s.AccountId);

            // Cấu hình quan hệ giữa Student và ClassRoom
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassRoomId);

            // Cấu hình quan hệ giữa Student và Course
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Course)
                .WithMany()
                .HasForeignKey(s => s.CourseId);

            // Cấu hình quan hệ một-nhiều giữa Teacher và ClassRoom (Teacher chính)
            modelBuilder.Entity<ClassRoom>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .IsRequired(false); // TeacherId có thể null

            // Cấu hình quan hệ nhiều-nhiều giữa Teacher và ClassRoom
            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.ClassRooms)
                .WithMany(c => c.Teachers)
                .UsingEntity(j => j.ToTable("TeacherClassRooms"));
        }
    }
}
/* sử dụng xóa mềm (soft delete) với trường DeletedAt để giữ lịch sử dữ liệu,
 thay vì xóa trực tiếp.
Các thực thể được định nghĩa với các thuộc tính bắt buộc (Required) và không bắt buộc phù hợp với logic nghiệp vụ.*/