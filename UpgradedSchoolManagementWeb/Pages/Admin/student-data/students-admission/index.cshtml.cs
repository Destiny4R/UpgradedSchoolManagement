using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.students_admission
{
    [Authorize(Policy = "Student.View")]
    public class indexModel : PageModel
    {
        public void OnGet() { }
    }
}
