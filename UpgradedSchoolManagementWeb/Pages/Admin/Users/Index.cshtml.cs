using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using System.Text.Json;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Users
{
    [Authorize(Policy = "User.View")]
    public class IndexModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAuthorizationService _authorizationService;

        public string PermissionsJson { get; set; } = "{}";
        public bool CanCreate { get; set; }

        public IndexModel(IUserManagementService userManagementService, IAuthorizationService authorizationService)
        {
            _userManagementService = userManagementService;
            _authorizationService = authorizationService;
        }

        public async Task OnGet()
        {
            var roles = await _userManagementService.GetAllRoles();
            ViewData["Roles"] = roles;

            CanCreate = (await _authorizationService.AuthorizeAsync(User, "User.Create")).Succeeded;

            var permissions = new Dictionary<string, bool>
            {
                ["User.Create"] = CanCreate,
                ["User.Edit"] = (await _authorizationService.AuthorizeAsync(User, "User.Edit")).Succeeded,
                ["User.AssignRole"] = (await _authorizationService.AuthorizeAsync(User, "User.AssignRole")).Succeeded,
                ["User.Delete"] = (await _authorizationService.AuthorizeAsync(User, "User.Delete")).Succeeded,
            };
            PermissionsJson = JsonSerializer.Serialize(permissions);
        }
    }
}
