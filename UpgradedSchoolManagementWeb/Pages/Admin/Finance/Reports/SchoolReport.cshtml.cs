using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Reports
{
    [Authorize(Policy = "Finance.Report")]
    public class SchoolReportModel : PageModel
    {
        public void OnGet() { }
    }
}
