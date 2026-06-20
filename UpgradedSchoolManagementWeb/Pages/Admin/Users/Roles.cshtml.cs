using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Users
{
    [Authorize(Policy = "User.AssignRole")]
    public class RolesModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;

        public RolesModel(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Name { get; set; } = string.Empty;

        public List<ApplicationRole> AllRoles { get; set; } = new();

        public async Task OnGet()
        {
            AllRoles = await _userManagementService.GetAllRoles();
        }
    }
}
