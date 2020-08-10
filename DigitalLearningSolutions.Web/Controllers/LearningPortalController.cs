namespace DigitalLearningSolutions.Web.Controllers
{
    using System;
    using System.Linq;
    using DigitalLearningSolutions.Data.Services;
    using DigitalLearningSolutions.Web.Helpers;
    using DigitalLearningSolutions.Web.ControllerHelpers;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize(Policy = CustomPolicies.UserOnly)]
    public class LearningPortalController : Controller
    {
        private readonly ICentresService centresService;
        private readonly IConfigService configService;
        private readonly ICourseService courseService;
        private readonly IUnlockService unlockService;
        private readonly ILogger<LearningPortalController> logger;
        private readonly IConfiguration config;

        public LearningPortalController(
            ICentresService centresService,
            IConfigService configService,
            ICourseService courseService,
            IUnlockService unlockService,
            ILogger<LearningPortalController> logger,
            IConfiguration config)
        {
            this.centresService = centresService;
            this.configService = configService;
            this.courseService = courseService;
            this.unlockService = unlockService;
            this.logger = logger;
            this.config = config;
        }

        public IActionResult Current(string sortBy = "Course Name", string sortDirection = "Ascending")
        {
            var currentCourses = courseService.GetCurrentCourses(GetCandidateId());
            var bannerText = GetBannerText();
            var model = new CurrentViewModel(currentCourses, config, sortBy, sortDirection, bannerText);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LearningPortal/Current/CompleteBy/{id:int}")]
        public IActionResult SetCompleteByDate(int id, int day, int month, int year, int progressId)
        {
            if (day == 0 && month == 0 && year == 0)
            {
                courseService.SetCompleteByDate(progressId, GetCandidateId(), null);
                return RedirectToAction("Current");
            }

            var validationResult = DateValidator.ValidateDate(day, month, year);
            if (!validationResult.DateValid)
            {
                return RedirectToAction("SetCompleteByDate", new { id, day, month, year });
            }

            var completeByDate = new DateTime(year, month, day);
            courseService.SetCompleteByDate(progressId, GetCandidateId(), completeByDate);
            return RedirectToAction("Current");
        }

        [Route("/LearningPortal/Current/CompleteBy/{id:int}")]
        public IActionResult SetCompleteByDate(int id, int? day, int? month, int? year)
        {
            var currentCourses = courseService.GetCurrentCourses(GetCandidateId());
            var course = currentCourses.FirstOrDefault(c => c.CustomisationID == id);
            if (course == null)
            {
                logger.LogWarning($"Attempt to set complete by date for course with id {id} which is not a current course for user with id {GetCandidateId()}");
                return StatusCode(404);
            }

            var model = new CurrentCourseViewModel(course, config);
            if (model.CompleteByDate != null && !model.SelfEnrolled)
            {
                logger.LogWarning(
                    $"Attempt to set complete by date for course with id {id} for user with id ${GetCandidateId()} " +
                    "but the complete by date has already been set and the user has not self enrolled"
                    );
                return StatusCode(403);
            }

            if (day != null && month != null && year != null)
            {
                model.CompleteByValidationResult = DateValidator.ValidateDate(day.Value, month.Value, year.Value);
            }

            return View(model);
        }

        [Route("/LearningPortal/Current/Remove/{id:int}")]
        public IActionResult RemoveCurrentCourseConfirmation(int id)
        {
            var currentCourses = courseService.GetCurrentCourses(GetCandidateId());
            var course = currentCourses.FirstOrDefault(c => c.CustomisationID == id);
            if (course == null)
            {
                logger.LogWarning($"Attempt to remove course with id {id} which is not a current course for user with id {GetCandidateId()}");
                return StatusCode(404);
            }

            var model = new CurrentCourseViewModel(course, config);
            return View(model);
        }

        [Route("/LearningPortal/Current/Remove/{progressId:int}")]
        [HttpPost]
        public IActionResult RemoveCurrentCourse(int progressId)
        {
            courseService.RemoveCurrentCourse(progressId, GetCandidateId());
            return RedirectToAction("Current");
        }

        [Route("/LearningPortal/Current/RequestUnlock/{progressId:int}")]
        public IActionResult RequestUnlock(int progressId)
        {
            var currentCourses = courseService.GetCurrentCourses(GetCandidateId());
            var course = currentCourses.FirstOrDefault(c => c.ProgressID == progressId && c.PLLocked);
            if (course == null)
            {
                logger.LogWarning(
                    $"Attempt to unlock course with progress id {progressId} however found no course with that progress id " +
                    $"and PLLocked for user with id {GetCandidateId()}"
                    );
                return StatusCode(404);
            }

            unlockService.SendUnlockRequest(progressId);
            return View("UnlockCurrentCourse");
        }

        public IActionResult Completed()
        {
            var completedCourses = courseService.GetCompletedCourses();
            var bannerText = GetBannerText();
            var model = new CompletedViewModel(completedCourses, bannerText);
            return View(model);
        }

        public IActionResult Available()
        {
            var availableCourses = courseService.GetAvailableCourses();
            var bannerText = GetBannerText();
            var model = new AvailableViewModel(availableCourses, bannerText);
            return View(model);
        }

        public IActionResult AccessibilityHelp()
        {
            var accessibilityText = configService.GetConfigValue(ConfigService.AccessibilityHelpText);
            if (accessibilityText == null)
            {
                logger.LogError("Accessibility text from Config table is null");
                return StatusCode(500);
            }
            var model = new AccessibilityHelpViewModel(accessibilityText);
            return View(model);
        }

        public IActionResult Terms()
        {
            var termsText = configService.GetConfigValue(ConfigService.TermsText);
            if (termsText == null)
            {
                logger.LogError("Terms text from Config table is null");
                return StatusCode(500);
            }
            var model = new TermsViewModel(termsText);
            return View(model);
        }

        public IActionResult Error()
        {
            var model = GetErrorModel();
            Response.StatusCode = 500;
            return View("Error/UnknownError", model);
        }

        [Route("/LearningPortal/StatusCode/{code:int}")]
        public new IActionResult StatusCode(int code)
        {
            var model = GetErrorModel();
            Response.StatusCode = code;

            return code switch
            {
                404 => View("Error/PageNotFound", model),
                403 => View("Error/Forbidden", model),
                _ => View("Error/UnknownError", model)
            };
        }

        private int GetCandidateId()
        {
            return User.GetCustomClaimAsRequiredInt(CustomClaimTypes.LearnCandidateId);
        }

        private string? GetBannerText()
        {
            var centreId = User.GetCustomClaimAsInt(CustomClaimTypes.UserCentreId);
            var bannerText = centreId == null
                ? null
                : centresService.GetBannerText(centreId.Value);
            return bannerText;
        }

        private ErrorViewModel GetErrorModel()
        {
            try
            {
                var bannerText = GetBannerText();
                return new ErrorViewModel(bannerText);
            }
            catch
            {
                return new ErrorViewModel(null);
            }
        }
    }
}
