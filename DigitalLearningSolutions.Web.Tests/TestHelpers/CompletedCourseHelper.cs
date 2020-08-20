﻿namespace DigitalLearningSolutions.Web.Tests.TestHelpers
{
    using System;
    using System.Linq;
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Web.Controllers;
    using DigitalLearningSolutions.Web.Controllers.LearningPortalController;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using Microsoft.AspNetCore.Mvc;

    public static class CompletedCourseHelper
    {
        public static CompletedCourse CreateDefaultCompletedCourse(
            int customisationId = 1,
            string courseName = "Course 1",
            bool hasDiagnostic = true,
            bool hasLearning = true,
            bool isAssessed = true,
            int? diagnosticScore = 1,
            int passes = 1,
            int sections = 1,
            int progressId = 1,
            DateTime? evaluated = null
        )
        {
            return new CompletedCourse
            {
                CustomisationID = customisationId,
                CourseName = courseName,
                HasDiagnostic = hasDiagnostic,
                HasLearning = hasLearning,
                IsAssessed = isAssessed,
                DiagnosticScore = diagnosticScore,
                Passes = passes,
                Sections = sections,
                ProgressID = progressId,
                Evaluated = evaluated
            };
        }

        public static CompletedViewModel CompletedViewModelFromController(LearningPortalController controller)
        {
            var result = controller.Completed() as ViewResult;
            return result.Model as CompletedViewModel;
        }
    }
}
