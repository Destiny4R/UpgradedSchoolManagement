using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Roles
{
    [Authorize(Policy = "Role.AssignPermission")]
    public class PermissionsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Name { get; set; } = string.Empty;

        public void OnGet()
        {
        }
    }
}
