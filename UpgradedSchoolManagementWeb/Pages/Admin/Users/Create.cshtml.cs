using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Users
{
    [Authorize(Policy = "User.Create")]
    public class CreateModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;

        public CreateModel(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public List<ApplicationRole> Roles { get; set; } = new();

        public async Task OnGet()
        {
            Roles = await _userManagementService.GetAllRoles();
        }
    }
}
