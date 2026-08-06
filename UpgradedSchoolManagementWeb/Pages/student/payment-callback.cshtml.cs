using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;

namespace UpgradedSchoolManagementWeb.Pages.student
{
    [Authorize]
    public class payment_callbackModel : PageModel
    {
        private readonly IStudentPaymentService _studentPaymentService;
        private readonly IAuditLogService _auditLogService;

        public payment_callbackModel(
            IStudentPaymentService studentPaymentService,
            IAuditLogService auditLogService)
        {
            _studentPaymentService = studentPaymentService;
            _auditLogService = auditLogService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Reference { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Reference))
            {
                Success = false;
                Message = "No payment reference was provided.";
                return;
            }

            var username = User.Identity?.Name;
            var result = await _studentPaymentService.ConfirmOnlinePaymentAsync(Reference, username);

            Success = result.Success;
            Message = result.Message;

            if (result.Success)
            {
                IsConfirmed = true;
                await _auditLogService.LogAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    username ?? "",
                    "ONLINE_PAYMENT_CALLBACK", "Payments",
                    $"Payment callback processed for reference {Reference}.");
            }
        }
    }
}
