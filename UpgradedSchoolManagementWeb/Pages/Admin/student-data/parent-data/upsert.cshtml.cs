using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.parent_data
{
    public class upsertModel : PageModel
    {
        private readonly IParentGuardianService _parentGuardianService;

        public upsertModel(IParentGuardianService parentGuardianService)
        {
            _parentGuardianService = parentGuardianService;
        }

        public ParentGuardianDto? Parent { get; private set; }
        public bool IsEdit => Parent != null;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                Parent = await _parentGuardianService.GetParentByIdAsync(id.Value);
                if (Parent == null)
                    return RedirectToPage("index");
            }
            return Page();
        }
    }
}
