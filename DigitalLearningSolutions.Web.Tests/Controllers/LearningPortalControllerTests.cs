﻿namespace DigitalLearningSolutions.Web.Tests.Controllers
{
    using System;
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Data.Services;
    using DigitalLearningSolutions.Web.Controllers;
    using DigitalLearningSolutions.Web.Tests.TestHelpers;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using FakeItEasy;
    using FluentAssertions;
    using FluentAssertions.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    public class LearningPortalControllerTests
    {
        private LearningPortalController controller;
        private ICourseService courseService;
        private IConfiguration config;
        private const string BaseUrl = "https://www.dls.nhs.uk";
        private const int CandidateId = 254480;

        [SetUp]
        public void SetUp()
        {
            courseService = A.Fake<ICourseService>();
            var logger = A.Fake<ILogger<LearningPortalController>>();
            config = A.Fake<IConfiguration>();
            A.CallTo(() => config["CurrentSystemBaseUrl"]).Returns(BaseUrl);
            controller = new LearningPortalController(courseService, logger, config);
        }

        [Test]
        public void Current_action_should_return_view_result()
        {
            // Given
            var currentCourses = new[] {
                CurrentCourseHelper.CreateDefaultCurrentCourse(),
                CurrentCourseHelper.CreateDefaultCurrentCourse()
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var result = controller.Current();

            // Then
            var expectedModel = new CurrentViewModel(currentCourses, config);
            result.Should().BeViewResult()
                .Model.Should().BeEquivalentTo(expectedModel);
        }

        [Test]
        public void Current_course_should_be_overdue_when_complete_by_date_is_in_the_past()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse(completeByDate: DateTime.Today - TimeSpan.FromDays(1));
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);

            // Then
            course.Overdue().Should().BeTrue();
        }

        [Test]
        public void Current_course_should_not_be_overdue_when_complete_by_date_is_in_the_future()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse(completeByDate: DateTime.Today + TimeSpan.FromDays(1));
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.Overdue().Should().BeFalse();
        }

        [Test]
        public void Current_course_should_not_have_diagnostic_score_without_diagnostic_assessment()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse(hasDiagnostic: false);
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.HasDiagnosticScore().Should().BeFalse();
        }

        [Test]
        public void Current_course_should_not_have_diagnostic_score_without_diagnostic_score_value()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse(diagnosticScore: null);
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.HasDiagnosticScore().Should().BeFalse();
        }

        [Test]
        public void Current_course_should_have_diagnostic_score_with_diagnostic_score_value_and_diagnostic_assessment()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse();
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.HasDiagnosticScore().Should().BeTrue();
        }

        [Test]
        public void Current_course_should_not_have_passed_sections_without_learning_assessment()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse(isAssessed: false);
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.HasPassedSections().Should().BeFalse();
        }

        [Test]
        public void Current_course_should_have_passed_sections_with_learning_assessment()
        {
            // Given
            var currentCourse = CurrentCourseHelper.CreateDefaultCurrentCourse();
            var currentCourses = new[] {
                currentCourse
            };
            A.CallTo(() => courseService.GetCurrentCourses(CandidateId)).Returns(currentCourses);

            // When
            var course = CurrentCourseHelper.CurrentCourseViewModelFromController(controller);


            // Then
            course.HasPassedSections().Should().BeTrue();
        }

        [Test]
        public void Setting_a_valid_complete_by_date_should_call_the_course_service()
        {
            // Given
            const int newDay = 29;
            const int newMonth = 7;
            const int newYear = 2020;
            var newDate = new DateTime(newYear, newMonth, newDay);
            const int progressId = 1;

            // When
            controller.SetCompleteByDate(1, newDay,  newMonth, newYear, 1);

            // Then
            A.CallTo(() => courseService.SetCompleteByDate(progressId, CandidateId, newDate)).MustHaveHappened();
        }

        [Test]
        public void Setting_an_empty_complete_by_date_should_call_the_course_service_with_null()
        {
            // Given
            const int progressId = 1;

            // When
            controller.SetCompleteByDate(1, 0, 0, 0, 1);

            // Then
            A.CallTo(() => courseService.SetCompleteByDate(progressId, CandidateId, null)).MustHaveHappened();
        }

        [Test]
        public void Setting_a_valid_complete_by_date_should_redirect_to_current_courses()
        {
            // When
            var result = (RedirectToActionResult)controller.SetCompleteByDate(1, 29, 7, 2020, 1);

            // Then
            result.ActionName.Should().Be("Current");
        }

        [Test]
        public void Setting_an_invalid_complete_by_date_should_not_call_the_course_service()
        {
            // When
            controller.SetCompleteByDate(1, 31, 2, 2020, 1);

            // Then
            A.CallTo(() => courseService.SetCompleteByDate(1, CandidateId, A<DateTime>._)).MustNotHaveHappened();
        }

        [Test]
        public void Setting_an_invalid_complete_by_date_should_redirect_with_an_error_message()
        {
            // When
            var result = (RedirectToActionResult)controller.SetCompleteByDate(1, 31, 2, 2020, 1);

            // Then
            result.ActionName.Should().Be("SetCompleteByDate");
            result.RouteValues["id"].Should().Be(1);
            result.RouteValues["errorMessage"].Should().Be("Please enter a valid date");
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
