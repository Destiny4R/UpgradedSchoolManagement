using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.employee_data.employees
{
    [Authorize(Policy = "Teacher.View")]
    public class indexModel : PageModel
    {
        public void OnGet() { }
    }
}
