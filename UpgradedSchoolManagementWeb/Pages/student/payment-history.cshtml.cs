using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.student
{
    [Authorize(Roles = "Student")]
    public class payment_historyModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
