namespace DigitalLearningSolutions.Data.Tests.Services
{
    using System;
    using System.Linq;
    using System.Transactions;
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Data.Services;
    using DigitalLearningSolutions.Data.Tests.Helpers;
    using DigitalLearningSolutions.Web;
    using NUnit.Framework;
    using FluentAssertions;
    using FluentMigrator.Runner;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.DependencyInjection;

    [Parallelizable(ParallelScope.Fixtures)]
    public class CourseServiceTests
    {
        private CourseService courseService;

        [SetUp]
        public void Setup()
        {
            var connectionString = ServiceTestHelper.GetSqlConnectionString();
            var serviceCollection = new ServiceCollection().RegisterMigrationRunner(connectionString);
            serviceCollection.BuildServiceProvider().GetRequiredService<IMigrationRunner>().MigrateUp();

            var connection = new SqlConnection(connectionString);
            courseService = new CourseService(connection);
        }

        [Test]
        public void Get_current_courses_should_return_courses_for_candidate()
        {
            // When
            const int candidateId = 1;
            var result = courseService.GetCurrentCourses(candidateId).ToList();

            // Then
            var expectedFirstCourse = new CurrentCourse
            {
                CourseName = "Office 2013 Essentials for the Workplace - Erin Test 01",
                CustomisationID = 15853,
                LastAccessed = new DateTime(2019, 1, 22, 8, 20, 39, 133),
                StartedDate = new DateTime(2016, 7, 6, 11, 12, 15, 393),
                DiagnosticScore = 0,
                IsAssessed = true,
                HasDiagnostic = true,
                HasLearning = true,
                Passes = 2,
                Sections = 6,
                CompleteByDate = new DateTime(2018, 12, 31, 0, 0, 0, 0),
                GroupCustomisationId = 0,
                SupervisorAdminId = 0,
                ProgressID = 173218,
                PLLocked = false
            };
            result.Should().HaveCount(4);
            result.First().Should().BeEquivalentTo(expectedFirstCourse);
        }

        [Test]
        public void Get_completed_courses_should_return_applications()
        {
            // When
            var result = courseService.GetCompletedCourses().ToList();

            // Then
            var expectedFirstCourse = new Course
            {
                Name = "Combined Office Course",
                Id = 39
            };
            result.Should().HaveCount(37);
            result.First().Should().BeEquivalentTo(expectedFirstCourse);
        }

        [Test]
        public void Get_available_courses_should_return_applications()
        {
            // When
            var result = courseService.GetAvailableCourses().ToList();

            // Then
            var expectedFirstCourse = new Course
            {
                Name = "Mobile DoS",
                Id = 49
            };
            result.Should().HaveCount(45);
            result.First().Should().BeEquivalentTo(expectedFirstCourse);
        }

        [Test]
        public void Set_complete_by_date_should_update_db()
        {
            // Given
            const int candidateId = 1;
            const int progressId = 94323;
            var newCompleteByDate = new DateTime(2020, 7, 29);

            using (new TransactionScope())
            {
                // When
                courseService.SetCompleteByDate(progressId, candidateId, newCompleteByDate);
                var modifiedCourse = courseService.GetCurrentCourses(candidateId).ToList().First(c => c.ProgressID == progressId);

                // Then
                modifiedCourse.CompleteByDate.Should().Be(newCompleteByDate);
            }
        }

        [Test]
        public void Remove_current_course_should_prevent_a_course_from_being_returned()
        {
            using (new TransactionScope())
            {
                // Given
                const int progressId = 94323;
                const int candidateId = 1;

                // When
                courseService.RemoveCurrentCourse(progressId, candidateId);
                var courseReturned = courseService.GetCurrentCourses(candidateId).ToList().Any(c => c.ProgressID == progressId);

                // Then
                courseReturned.Should().BeFalse();
            }
        }

    }
}
