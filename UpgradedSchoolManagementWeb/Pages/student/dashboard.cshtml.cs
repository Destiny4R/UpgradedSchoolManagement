using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.student
{
    [Authorize]
    public class dashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
