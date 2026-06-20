using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Academic.manage_session
{
    [Authorize(Policy = "Session.View")]
    public class IndexModel : PageModel
    {
    }
}
