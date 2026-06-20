using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using System.Text.Json;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Roles
{
    [Authorize(Policy = "Role.View")]
    public class IndexModel : PageModel
    {
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;

        public string PermissionsJson { get; set; } = "{}";
        public bool CanCreate { get; set; }

        public IndexModel(IRoleService roleService, IAuthorizationService authorizationService)
        {
            _roleService = roleService;
            _authorizationService = authorizationService;
        }

        public async Task OnGet()
        {
            CanCreate = (await _authorizationService.AuthorizeAsync(User, "Role.Create")).Succeeded;

            var permissions = new Dictionary<string, bool>
            {
                ["Role.Create"] = CanCreate,
                ["Role.Edit"] = (await _authorizationService.AuthorizeAsync(User, "Role.Edit")).Succeeded,
                ["Role.AssignPermission"] = (await _authorizationService.AuthorizeAsync(User, "Role.AssignPermission")).Succeeded,
                ["Role.Delete"] = (await _authorizationService.AuthorizeAsync(User, "Role.Delete")).Succeeded,
            };
            PermissionsJson = JsonSerializer.Serialize(permissions);
        }
    }
}
