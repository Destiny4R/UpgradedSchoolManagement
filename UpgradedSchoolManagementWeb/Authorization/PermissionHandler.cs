using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UpgradedSchoolManagementDataAccess.IServices;

namespace UpgradedSchoolManagementWeb.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private const string PermissionsCacheKey = "PermissionHandler.Permissions";

        private readonly IUserPermissionService _userPermissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionHandler(IUserPermissionService userPermissionService, IHttpContextAccessor httpContextAccessor)
        {
            _userPermissionService = userPermissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            List<string>? permissions = null;

            if (httpContext?.Items.TryGetValue(PermissionsCacheKey, out var cached) == true)
            {
                permissions = cached as List<string>;
            }

            if (permissions == null)
            {
                permissions = await _userPermissionService.GetUserPermissionsAsync(userId);
                if (httpContext != null)
                {
                    httpContext.Items[PermissionsCacheKey] = permissions;
                }
            }

            if (permissions.Contains("*") || permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }

            // Fallback to permission claims for principals whose claims were
            // refreshed at an earlier login.
            var permissionsClaim = context.User.FindAll("Permission");
            if (permissionsClaim.Any(c => c.Value == requirement.Permission || c.Value == "*"))
            {
                context.Succeed(requirement);
            }
        }
    }
}
