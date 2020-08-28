namespace DigitalLearningSolutions.Web.Tests.ViewModels.LearningPortal
{
    using System;
    using System.Linq;
    using DigitalLearningSolutions.Data.Models.Courses;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal.Current;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;

    public class CurrentPageViewModelTests
    {
        private readonly IConfiguration config = A.Fake<IConfiguration>();
        private CurrentPageViewModel model;
        private CurrentCourse[] currentCourses;

        [SetUp]
        public void SetUp()
        {
            A.CallTo(() => config["CurrentSystemBaseUrl"]).Returns("http://www.dls.nhs.uk");

            currentCourses = new[]
            {
                new CurrentCourse
                {
                    CustomisationID = 71,
                    CourseName = "B: Course",
                    HasDiagnostic = true,
                    HasLearning = true,
                    IsAssessed = true,
                    StartedDate = new DateTime(2010, 1, 31),
                    LastAccessed = new DateTime(2010, 2, 22),
                    CompleteByDate = new DateTime(2010, 3, 22),
                    DiagnosticScore = 123,
                    Passes = 4,
                    Sections = 6,
                    SupervisorAdminId = 0,
                    GroupCustomisationId = 0,
                },
                new CurrentCourse
                {
                    CustomisationID = 72,
                    CourseName = "C: Course",
                    HasDiagnostic = true,
                    HasLearning = true,
                    IsAssessed = false,
                    StartedDate = new DateTime(2010, 2, 1),
                    LastAccessed = new DateTime(2011, 2, 22),
                    CompleteByDate = new DateTime(2011, 3, 22),
                    DiagnosticScore = 0,
                    Passes = 14,
                    Sections = 16,
                    SupervisorAdminId = 12,
                    GroupCustomisationId = 34,
                },
                new CurrentCourse
                {
                    CustomisationID = 73,
                    CourseName = "A: Course",
                    HasDiagnostic = false,
                    HasLearning = true,
                    IsAssessed = true,
                    StartedDate = new DateTime(2001, 1, 22),
                    LastAccessed = new DateTime(2011, 2, 23),
                    CompleteByDate = null,
                    DiagnosticScore = 0,
                    Passes = 0,
                    Sections = 6,
                    SupervisorAdminId = 0,
                    GroupCustomisationId = 0,
                },
            };

            model = new CurrentPageViewModel(
                currentCourses,
                config,
                null,
                "Course Name",
                "Ascending",
                null,
                null
            );
        }

        [TestCase(
            0,
            73,
            "A: Course",
            false,
            true,
            true,
            "2001-1-22",
            "2011-2-23",
            null,
            0,
            0,
            06,
            false,
            false,
            "http://www.dls.nhs.uk/tracking/learn?CustomisationID=73&lp=1"
        )]
        [TestCase(
            1,
            71,
            "B: Course",
            true,
            true,
            true,
            "2010-1-31",
            "2010-2-22",
            "2010-3-22",
            123,
            4,
            6,
            false,
            false,
            "http://www.dls.nhs.uk/tracking/learn?CustomisationID=71&lp=1"
        )]
        [TestCase(
            2,
            72,
            "C: Course",
            true,
            true,
            false,
            "2010-2-1",
            "2011-2-22",
            "2011-3-22",
            0,
            14,
            16,
            true,
            true,
            "http://www.dls.nhs.uk/tracking/learn?CustomisationID=72&lp=1"
        )]
        public void Current_courses_should_map_to_view_models_in_the_correct_order(
            int index,
            int expectedId,
            string expectedName,
            bool expectedDiagnostic,
            bool expectedLearning,
            bool expectedAssessmentAndCertification,
            DateTime expectedStart,
            DateTime expectedLastAccessed,
            DateTime? expectedCompleteBy,
            int expectedDiagnosticScore,
            int expectedPasses,
            int expectedSections,
            bool expectedIsSupervisor,
            bool expectedIsGroup,
            string expectedLaunchUrl)
        {
            var course = model.CurrentCourses.ElementAt(index) as CurrentCourseViewModel;
            course.Id.Should().Be(expectedId);
            course.Name.Should().Be(expectedName);
            course.HasDiagnosticAssessment.Should().Be(expectedDiagnostic);
            course.HasLearningContent.Should().Be(expectedLearning);
            course.HasLearningAssessmentAndCertification.Should().Be(expectedAssessmentAndCertification);
            course.StartedDate.Should().Be(expectedStart);
            course.LastAccessedDate.Should().Be(expectedLastAccessed);
            course.CompleteByDate.Should().Be(expectedCompleteBy);
            course.DiagnosticScore.Should().Be(expectedDiagnosticScore);
            course.PassedSections.Should().Be(expectedPasses);
            course.Sections.Should().Be(expectedSections);
            course.UserIsSupervisor.Should().Be(expectedIsSupervisor);
            course.IsEnrolledWithGroup.Should().Be(expectedIsGroup);
            course.LaunchUrl.Should().Be(expectedLaunchUrl);
        }
    }
}
