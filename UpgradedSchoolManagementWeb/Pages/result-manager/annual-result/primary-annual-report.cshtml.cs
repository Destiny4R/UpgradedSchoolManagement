using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementWeb.Services;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.annual_result
{
    [Authorize(Policy = "Result.View")]
    public class primary_annual_reportModel : AnnualReportPageBaseModel
    {
        public primary_annual_reportModel(AnnualReportService annualReportService, ApplicationDbContext db)
            : base(annualReportService, db)
        {
        }
    }
}
