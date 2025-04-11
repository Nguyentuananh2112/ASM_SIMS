using ASM_SIMS.Controllers;
using ASM_SIMS.DB;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        private readonly DbContextOptions<SimsDataContext> _dbOptions;

        public UnitTest1()
        {
            _dbOptions = new DbContextOptionsBuilder<SimsDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private SimsDataContext GetNewContext()
        {
            var context = new SimsDataContext(_dbOptions);
            context.Database.EnsureCreated();
            return context;
        }

        private void ClearDatabase(SimsDataContext context)
        {
            context.Teachers.RemoveRange(context.Teachers);
            context.Accounts.RemoveRange(context.Accounts);
            context.Courses.RemoveRange(context.Courses);
            context.SaveChanges();
        }

        [Fact]
        public void Create_Post_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            using var context = GetNewContext();
            ClearDatabase(context);
            context.Teachers.Add(new Teacher
            {
                FullName = "Existing Teacher",
                Email = "jane@example.com",
                Phone = "987654321",
                Address = "456 Street",
                Status = "Active",
                CreatedAt = DateTime.Now // Add CreatedAt if required
            });
            context.Courses.Add(new Courses { Id = 1, NameCourse = "Math" });
            context.SaveChanges();

            var controller = new TeacherController(context);
            var invalidModel = new TeacherViewModel
            {
                FullName = "Jane Doe",
                Email = "jane@example.com", // Duplicate email
                Phone = "123456789",
                Address = "123 Street",
                Status = "Active",
                CourseId = 1
            };

            // Act
            var result = controller.Create(invalidModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidModel, result.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Email"));
            Assert.Equal("Email already exists.", controller.ModelState["Email"].Errors[0].ErrorMessage);
            Assert.NotNull(controller.ViewBag.Courses);
            var courses = (SelectList)controller.ViewBag.Courses;
            Assert.Single(courses.Items.Cast<Courses>());
            Assert.Equal("Math", courses.Items.Cast<Courses>().First().NameCourse);
            Assert.Equal(1, context.Teachers.Count());
        }

        [Fact]
        public void Create_Post_DuplicatePhone_ReturnsViewWithError()
        {
            // Arrange
            using var context = GetNewContext();
            ClearDatabase(context);
            context.Teachers.Add(new Teacher
            {
                FullName = "Existing Teacher",
                Email = "existing@example.com",
                Phone = "123456789",
                Address = "456 Street",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.Courses.Add(new Courses { Id = 1, NameCourse = "Math" });
            context.SaveChanges();

            var controller = new TeacherController(context);
            var invalidModel = new TeacherViewModel
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Phone = "123456789", // Duplicate phone number
                Address = "123 Street",
                Status = "Active",
                CourseId = 1
            };

            // Act
            var result = controller.Create(invalidModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidModel, result.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Phone"));
            Assert.Equal("Phone number already exists.", controller.ModelState["Phone"].Errors[0].ErrorMessage);
            Assert.NotNull(controller.ViewBag.Courses);
            var courses = (SelectList)controller.ViewBag.Courses;
            Assert.Single(courses.Items.Cast<Courses>());
            Assert.Equal("Math", courses.Items.Cast<Courses>().First().NameCourse);
            Assert.Equal(1, context.Teachers.Count());
        }

        [Fact]
        public void Edit_Post_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            using var context = GetNewContext();
            ClearDatabase(context);
            context.Teachers.Add(new Teacher
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "123456789",
                Address = "123 Street",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.Teachers.Add(new Teacher
            {
                Id = 2,
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Phone = "987654321",
                Address = "456 Street",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.Courses.Add(new Courses { Id = 1, NameCourse = "Math" });
            context.SaveChanges();

            var controller = new TeacherController(context);
            var invalidModel = new TeacherViewModel
            {
                Id = 1,
                FullName = "John Doe",
                Email = "jane@example.com", // Duplicate email with teacher Id = 2
                Phone = "123456789",
                Address = "123 Street",
                Status = "Active",
                CourseId = 1
            };

            // Act
            var result = controller.Edit(invalidModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidModel, result.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Email"));
            Assert.Equal("Email already exists.", controller.ModelState["Email"].Errors[0].ErrorMessage);
            Assert.NotNull(controller.ViewBag.Courses);
            var courses = (System.Collections.Generic.List<Courses>)controller.ViewBag.Courses;
            Assert.Single(courses);
            Assert.Equal("Math", courses[0].NameCourse);
            Assert.Equal(2, context.Teachers.Count());
        }

        [Fact]
        public void Edit_Post_DuplicatePhone_ReturnsViewWithError()
        {
            // Arrange
            using var context = GetNewContext();
            ClearDatabase(context);
            context.Teachers.Add(new Teacher
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "123456789",
                Address = "123 Street",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.Teachers.Add(new Teacher
            {
                Id = 2,
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Phone = "987654321",
                Address = "456 Street",
                Status = "Active",
                CreatedAt = DateTime.Now
            });
            context.Courses.Add(new Courses { Id = 1, NameCourse = "Math" });
            context.SaveChanges();

            var controller = new TeacherController(context);
            var invalidModel = new TeacherViewModel
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "987654321", // Duplicate phone number with teacher Id = 2
                Address = "123 Street",
                Status = "Active",
                CourseId = 1
            };

            // Act
            var result = controller.Edit(invalidModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidModel, result.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Phone"));
            Assert.Equal("Phone number already exists.", controller.ModelState["Phone"].Errors[0].ErrorMessage);
            Assert.NotNull(controller.ViewBag.Courses);
            var courses = (System.Collections.Generic.List<Courses>)controller.ViewBag.Courses;
            Assert.Single(courses);
            Assert.Equal("Math", courses[0].NameCourse);
            Assert.Equal(2, context.Teachers.Count());
        }
    }
}
