using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementWeb.Services;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.annual_result
{
    public class senior_annual_reportModel : PageModel
    {
        private readonly AnnualReportService _annualReportService;

        public senior_annual_reportModel(AnnualReportService annualReportService)
        {
            _annualReportService = annualReportService;
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
                Report = await _annualReportService.BuildAsync(StudentId, SessionId, ClassId, SubClassId);
            }
        }
    }
}
