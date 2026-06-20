using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Academic.manage_school
{
    [Authorize(Policy = "Class.View")]
    public class IndexModel : PageModel
    {
    }
}
