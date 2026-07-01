using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.admin.Academic.result_config
{
    [Authorize(Policy = "Settings.Manage")]
    public class upsertModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
