﻿namespace DigitalLearningSolutions.Web.Tests.Controllers
{
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Data.Services;
    using DigitalLearningSolutions.Web.Controllers;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using FakeItEasy;
    using FluentAssertions;
    using FluentAssertions.AspNetCore.Mvc;
    using NUnit.Framework;

    public class LearningPortalControllerTests
    {
        private LearningPortalController controller;

        private ICourseService courseService;

        [SetUp]
        public void SetUp()
        {
            courseService = A.Fake<ICourseService>();
            controller = new LearningPortalController(courseService);
        }

        [Test]
        public void Current_action_should_return_view_result()
        {
            // Given
            var currentCourses = new[] {
                new Course { Id = 1, Name = "Course 1" },
                new Course { Id = 2, Name = "Course 2" }
            };
            A.CallTo(() => courseService.GetCurrentCourses()).Returns(currentCourses);

            // When
            var result = controller.Current();

            // Then
            var expectedModel = new CurrentViewModel(currentCourses);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }

        [Test]
        public void Completed_action_should_return_view_result()
        {
            // Given
            var headlineFigures = new HeadlineFigures
            {
                ActiveCentres = 339,
                Delegates = 329025,
                LearningTime = 649911,
                Completions = 162263
            };
            A.CallTo(() => courseService.GetHeadlineFigures()).Returns(headlineFigures);

            // When
            var result = controller.Completed();

            // Then
            var expectedModel = new CompletedViewModel(headlineFigures);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }

        [Test]
        public void Available_action_should_return_view_result()
        {
            // Given
            var headlineFigures = new HeadlineFigures
            {
                ActiveCentres = 339,
                Delegates = 329025,
                LearningTime = 649911,
                Completions = 162263
            };
            A.CallTo(() => courseService.GetHeadlineFigures()).Returns(headlineFigures);

            // When
            var result = controller.Available();

            // Then
            var expectedModel = new AvailableViewModel(headlineFigures);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }
    }
}
