using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.terminal_result
{
    [Authorize(Policy = "Result.View")]
    public class detailModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public detailModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var termReg = await _db.TermRegistrations
                .Include(tr => tr.SchoolClasses)
                .FirstOrDefaultAsync(tr => tr.Id == Id);

            if (termReg == null || termReg.SchoolClasses == null)
                return Content("Terminal result not found.", "text/plain");

            return termReg.SchoolClasses.Resulttype switch
            {
                ResultType.Nursery => RedirectToPage("nursery", new { id = Id }),
                ResultType.Primary => RedirectToPage("primary", new { id = Id }),
                ResultType.Jss => RedirectToPage("junior-result", new { id = Id }),
                ResultType.SSS => RedirectToPage("senior-result", new { id = Id }),
                _ => Content("Unsupported result type.", "text/plain")
            };
        }
    }
}
