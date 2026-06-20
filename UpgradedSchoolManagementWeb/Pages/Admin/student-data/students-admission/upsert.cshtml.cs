using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.students_admission
{
    public class upsertModel : PageModel
    {
        private readonly IStudentService _studentService;

        public upsertModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public StudentsTable? Student { get; private set; }
        public bool IsEdit => Student != null;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                Student = await _studentService.GetStudentById(id.Value);
                if (Student == null)
                    return RedirectToPage("index");
            }
            return Page();
        }
    }
}
