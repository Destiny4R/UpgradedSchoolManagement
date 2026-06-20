using Microsoft.AspNetCore.Authorization;

namespace UpgradedSchoolManagementWeb.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var permissionsClaim = context.User.FindAll("Permission");

            if (permissionsClaim.Any(c => c.Value == requirement.Permission || c.Value == "*"))
            {
                context.Succeed(requirement);
            }

            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}