using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using UpgradedSchoolManagementUltitlities;

namespace UpgradedSchoolManagementWeb.Pages
{
    [Authorize(Policy = "Settings.View")]
    public class app_settingsModel : PageModel
    {
        private readonly ILogger<app_settingsModel> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        [BindProperty]
        public SelectionViewModal SelectionView { get; set; }

        [BindProperty]
        public AppsettingViewModel appsettingView { get; set; }

        [BindProperty]
        public bool isAdmin { get; set; } = false;

        public app_settingsModel(ILogger<app_settingsModel> logger, IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        public async Task OnGetAsync()
        {
            await LoadDropdownAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var settings = await _unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId);
                if (settings != null)
                {
                    appsettingView = new AppsettingViewModel();

                    appsettingView.Id = settings.Id;
                    appsettingView.Term = settings.Term.HasValue ? (ConstantEnums.Term)settings.Term.Value : null;
                    appsettingView.SchoolClassId = settings.SchoolClassId ?? 0;
                    appsettingView.SubClassId = settings.SubClassId ?? 0;
                    appsettingView.SessionId = settings.SessionId ?? 0;
                    appsettingView.PrincipalName = settings.PrincipalName ?? "";
                    appsettingView.PrincipalSignatureUrl = settings.PrincipalSignature;
                    appsettingView.CashierName = settings.CashierName ?? "";
                    appsettingView.CashierSignatureUrl = settings.CashierSignature;
                    appsettingView.CanPrintResult = settings.CanPrintResult;
                    appsettingView.EnableOnlinePayment = settings.EnableOnlinePayment;
                    appsettingView.IsAdmin = settings.IsAdmin;

                    isAdmin = settings.IsAdmin;

                    // Paystack keys are only exposed to the admin row (never sent to non-admin clients)
                    if (settings.IsAdmin)
                    {
                        appsettingView.PaystackSecretKey = settings.PaystackSecretKey ?? "";
                        appsettingView.PaystackPublicKey = settings.PaystackPublicKey ?? "";
                    }
                }
            }
        }

        public async Task<IActionResult> OnPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "User not authenticated.";
                return RedirectToPage();
            }

            var existing = await _unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId);

            var settings = existing ?? new AppSettings();
            settings.ApplicationUserId = userId;
            settings.Term = appsettingView.Term;
            settings.SchoolClassId = appsettingView.SchoolClassId;
            settings.SubClassId = appsettingView.SubClassId;
            settings.SessionId = appsettingView.SessionId;
            settings.PrincipalName = appsettingView.PrincipalName;
            settings.CashierName = appsettingView.CashierName;
            settings.CanPrintResult = appsettingView.CanPrintResult;

            if (existing == null)
            {
                settings.IsAdmin = false;
            }

            if (appsettingView.PrincipalSignature != null)
            {
                settings.PrincipalSignature = await ImageCompressor.CompressAndSaveImageAsync(appsettingView.PrincipalSignature, _env.WebRootPath);
            }

            if (appsettingView.CashierSignature != null)
            {
                settings.CashierSignature = await ImageCompressor.CompressAndSaveImageAsync(appsettingView.CashierSignature, _env.WebRootPath);
            }

            // Paystack configuration may only be written on the admin settings row
            if (settings.IsAdmin)
            {
                settings.PaystackSecretKey = appsettingView.PaystackSecretKey;
                settings.PaystackPublicKey = appsettingView.PaystackPublicKey;
                settings.EnableOnlinePayment = appsettingView.EnableOnlinePayment;
            }

            await _unitOfWork.AppSettingsServices.UpsertAppSettingsAsync(settings);

            await _unitOfWork.AuditLogService.LogAsync(
                userId: userId,
                userName: User.Identity?.Name ?? "Unknown",
                action: existing == null ? "CREATE" : "UPDATE",
                module: "AppSettings",
                description: $"App settings {(existing == null ? "created" : "updated")} for user {userId}",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: Request.Headers["User-Agent"].ToString()
            );

            TempData["Success"] = "App settings saved successfully.";
            return RedirectToPage();
        }

        public async Task LoadDropdownAsync()
        {
            SelectionView = new()
            {
                AcademicSession = await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync(),
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown(),
                SchoolClasses = await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync(),
                SubClass = await _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync()
            };
        }
    }

}
