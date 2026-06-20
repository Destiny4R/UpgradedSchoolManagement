using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Users
{
    [Authorize(Policy = "User.Edit")]
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string FullName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public bool IsActive { get; set; } = true;

        public void OnGet()
        {
        }
    }
}
