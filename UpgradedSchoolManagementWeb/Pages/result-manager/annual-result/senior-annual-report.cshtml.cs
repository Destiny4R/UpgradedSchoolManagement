using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementWeb.Services;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.annual_result
{
    [Authorize(Policy = "Result.View")]
    public class senior_annual_reportModel : PageModel
    {
        private readonly AnnualReportService _annualReportService;
        private readonly ApplicationDbContext _db;

        public senior_annual_reportModel(AnnualReportService annualReportService, ApplicationDbContext db)
        {
            _annualReportService = annualReportService;
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SubClassId { get; set; }

        public AnnualReportViewModel Report { get; set; } = new();

        public async Task OnGet()
        {
            if (StudentId > 0 && SessionId > 0 && ClassId > 0 && SubClassId > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var student = await _db.StudentsTables
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                    if (student != null && StudentId != student.Id)
                    {
                        return;
                    }
                }

                Report = await _annualReportService.BuildAsync(StudentId, SessionId, ClassId, SubClassId);
            }
        }
    }
}
