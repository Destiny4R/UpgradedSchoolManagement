using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Admin.audit_logs
{
    [Authorize(Policy = "AuditLog.View")]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostDataTableAsync([FromBody] DataTablesRequest request)
        {
            var result = await _unitOfWork.AuditLogService.GetLogs(request);
            return new JsonResult(result);
        }
    }
}
