namespace DigitalLearningSolutions.Web.Controllers.FrameworksController
{
    using DigitalLearningSolutions.Web.Helpers;
    using DigitalLearningSolutions.Web.ViewModels.FrameworkDevelopment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize(Policy = CustomPolicies.FrameworkDeveloperOnly)]
    public class FrameworksController : Controller
    {
        private readonly ILogger<FrameworksController> logger;

        public FrameworksController(ILogger<FrameworksController> logger)
        {
            this.logger = logger;
        }

        public IActionResult Index()
        {
            var model = new FrameworksListViewModel();
            return View(model);
        }
    }
}
