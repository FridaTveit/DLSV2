﻿namespace DigitalLearningSolutions.Web.Tests.Helpers
{
    using System.Linq;
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Data.Models.Courses;
    using DigitalLearningSolutions.Web.Helpers;
    using DigitalLearningSolutions.Web.Tests.TestHelpers;
    using FluentAssertions;
    using NUnit.Framework;

    public class SearchHelperTests
    {
        private CurrentCourse[] currentCourses;
        private NamedItem[] currentCoursesWithSelfAssessment;
        private CompletedCourse[] completedCourses;

        [SetUp]
        public void SetUp()
        {
            currentCourses = new[]
            {
                CurrentCourseHelper.CreateDefaultCurrentCourse(71, "b: course"),
                CurrentCourseHelper.CreateDefaultCurrentCourse(72, "C: Course"),
                CurrentCourseHelper.CreateDefaultCurrentCourse(73, "A: Course")
            };
            currentCoursesWithSelfAssessment = new NamedItem[]
            {
                CurrentCourseHelper.CreateDefaultCurrentCourse(71, "d: course"),
                CurrentCourseHelper.CreateDefaultCurrentCourse(72, "C: Course"),
                SelfAssessmentHelper.SelfAssessment(74, "a: self assessment"),
                CurrentCourseHelper.CreateDefaultCurrentCourse(73, "A: Course")
            };
            completedCourses = new[]
            {
                CompletedCourseHelper.CreateDefaultCompletedCourse(71, "First course"),
                CompletedCourseHelper.CreateDefaultCompletedCourse(72, "Course 20: the best course"),
                CompletedCourseHelper.CreateDefaultCompletedCourse(73, "Last course 30105 and a lot of other text")
            };
        }

        [TestCase("A:", new[] { 73 })]
        [TestCase(null, new[] { 71, 72, 73 })]
        [TestCase("course", new[] { 71, 72, 73 })]
        [TestCase("Course", new[] { 71, 72, 73 })]
        [TestCase("self", new int[] {})]
        public void Current_courses_should_be_filtered_correctly(
            string searchString,
            int[] expectedIds
        )
        {
            // When
            var result = SearchHelper.FilterNamedItems(currentCourses, searchString);
            var filteredIds = result.Select(course => course.Id);

            // Then
            filteredIds.Should().Equal(expectedIds);
        }

        [TestCase("A:", new[] { 74, 73 })]
        [TestCase(null, new[] { 71, 72, 74, 73 })]
        [TestCase("course", new[] { 71, 72, 73 })]
        [TestCase("Course", new[] { 71, 72, 73 })]
        [TestCase("self", new[] { 74 })]
        public void Current_courses_with_self_assessment_should_be_filtered_correctly(
            string searchString,
            int[] expectedIds
        )
        {
            // When
            var result = SearchHelper.FilterNamedItems(currentCoursesWithSelfAssessment, searchString);
            var filteredIds = result.Select(course => course.Id);

            // Then
            filteredIds.Should().Equal(expectedIds);
        }

        [TestCase(null, new[] { 71, 72, 73 })]
        [TestCase("2", new[] { 72 })]
        [TestCase("200", new int[] { })]
        [TestCase("40", new int[] { })]
        [TestCase("somecourse", new int[] { })]
        [TestCase("course 3010", new[] { 73 })]
        [TestCase("course 30105", new[] { 73 })]
        public void Completed_courses_should_be_filtered_correctly(
            string searchString,
            int[] expectedIds
        )
        {
            // When
            var result = SearchHelper.FilterNamedItems(completedCourses, searchString);
            var filteredIds = result.Select(course => course.Id);

            // Then
            filteredIds.Should().Equal(expectedIds);
        }
    }
}