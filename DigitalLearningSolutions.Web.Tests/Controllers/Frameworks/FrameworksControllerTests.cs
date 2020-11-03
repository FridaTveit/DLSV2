namespace DigitalLearningSolutions.Web.Tests.Controllers.Frameworks
{
    using System.Security.Claims;
    using DigitalLearningSolutions.Web.Controllers.FrameworksController;
    using DigitalLearningSolutions.Web.ViewModels.FrameworkDevelopment;
    using FakeItEasy;
    using FluentAssertions;
    using FluentAssertions.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    internal class FrameworksControllerTests
    {
        private FrameworksController controller;
        private const int AdminId = 11;

        [SetUp]
        public void SetUp()
        {
            var logger = A.Fake<ILogger<FrameworksController>>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("UserAdminID", AdminId.ToString())
            }, "mock"));
            controller = new FrameworksController(logger)
            {
                ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext { User = user } }
            };
        }

        [Test]
        public void Index_action_should_return_view_result()
        {
            // When
            var result = controller.Index();

            // Then
            result.Should().BeViewResult()
                .Model.Should().BeOfType<FrameworksListViewModel>();
        }
    }
}
