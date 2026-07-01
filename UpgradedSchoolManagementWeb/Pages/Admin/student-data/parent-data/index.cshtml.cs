using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.parent_data
{
    [Authorize(Policy = "Student.View")]
    public class indexModel : PageModel
    {
        public void OnGet() { }
    }
}
