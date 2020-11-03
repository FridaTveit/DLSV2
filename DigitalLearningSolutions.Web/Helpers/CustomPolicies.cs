namespace DigitalLearningSolutions.Web.Helpers
{
    using Microsoft.AspNetCore.Authorization;

    public class CustomPolicies
    {
        public const string UserOnly = "UserOnly";
        public const string FrameworkDeveloperOnly = "FrameworkDeveloperOnly";

        public static AuthorizationPolicyBuilder ConfigurePolicyUserOnly(AuthorizationPolicyBuilder policy)
        {
            return policy.RequireAssertion(
                context => context.User.GetCustomClaimAsInt(CustomClaimTypes.LearnCandidateId) != null
                           && context.User.GetCustomClaimAsBool(CustomClaimTypes.LearnUserAuthenticated) == true);
        }

        public static AuthorizationPolicyBuilder ConfigurePolicyFrameworkDeveloperOnly(AuthorizationPolicyBuilder policy)
        {
            // TODO HEEDLS-164: check IsFrameworkDeveloper
            return policy.RequireAssertion(context => context.User.GetCustomClaimAsInt(CustomClaimTypes.UserAdminId) != null);
        }
    }
}
