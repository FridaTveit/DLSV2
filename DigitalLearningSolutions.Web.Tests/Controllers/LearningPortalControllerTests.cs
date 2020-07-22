﻿namespace DigitalLearningSolutions.Web.Tests.Controllers
{
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Data.Services;
    using DigitalLearningSolutions.Web.Controllers;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using FakeItEasy;
    using FluentAssertions;
    using FluentAssertions.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    public class LearningPortalControllerTests
    {
        private LearningPortalController controller;

        private ICourseService courseService;

        [SetUp]
        public void SetUp()
        {
            courseService = A.Fake<ICourseService>();
            var logger = A.Fake<ILogger<LearningPortalController>>();
            controller = new LearningPortalController(courseService, logger);
        }

        [Test]
        public void Current_action_should_return_view_result()
        {
            // Given
            const int candidateID = 1;
            var currentCourses = new[] {
                new CurrentCourse {
                    CustomisationID = 4,
                    CourseName = "Course 1",
                    HasDiagnostic = true,
                    HasLearning = true,
                    IsAssessed = true,
                    DiagnosticScore = 1,
                    Passes = 1,
                    Sections = 1,
                    SupervisorAdminId = 1,
                    GroupCustomisationId = 0
                },
                new CurrentCourse {
                    CustomisationID = 2,
                    CourseName = "Course 2",
                    HasDiagnostic = false,
                    HasLearning = false,
                    IsAssessed = true,
                    DiagnosticScore = 2,
                    Passes = 2,
                    Sections = 2,
                    SupervisorAdminId = 5,
                    GroupCustomisationId = 7
                },
            };
            A.CallTo(() => courseService.GetCurrentCourses(candidateID)).Returns(currentCourses);

            // When
            var result = controller.Current();

            // Then
            var expectedModel = new CurrentViewModel(currentCourses, candidateID);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }

        [Test]
        public void Completed_action_should_return_view_result()
        {
            // Given
            var completedCourses = new[] {
                new Course { Id = 1, Name = "Course 1" },
                new Course { Id = 2, Name = "Course 2" }
            };
            A.CallTo(() => courseService.GetCompletedCourses()).Returns(completedCourses);

            // When
            var result = controller.Completed();

            // Then
            var expectedModel = new CompletedViewModel(completedCourses);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }

        [Test]
        public void Available_action_should_return_view_result()
        {
            // Given
            var availableCourses = new[] {
                new Course { Id = 1, Name = "Course 1" },
                new Course { Id = 2, Name = "Course 2" }
            };
            A.CallTo(() => courseService.GetAvailableCourses()).Returns(availableCourses);

            // When
            var result = controller.Available();

            // Then
            var expectedModel = new AvailableViewModel(availableCourses);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }
    }
}
