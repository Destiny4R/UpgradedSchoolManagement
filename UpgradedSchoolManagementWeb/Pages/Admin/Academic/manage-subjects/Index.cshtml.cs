using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Academic.manage_subjects
{
    [Authorize(Policy = "Subject.View")]
    public class IndexModel : PageModel
    {
    }
}
