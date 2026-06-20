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
            LoadDropdown();
        }

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var settings = _unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId).Result;
                if (settings != null)
                {
                    appsettingView = new AppsettingViewModel();

                    appsettingView.Id = settings.Id;
                    appsettingView.Term = settings.Term.HasValue ? (ConstantEnums.Term)settings.Term.Value : null;
                    appsettingView.SchoolClassId = settings.SchoolClassId ?? 0;
                    appsettingView.SubClassId = settings.SubClassId ?? 0;
                    appsettingView.SessionId = settings.SessionId ?? 0;
                    appsettingView.PrincipalName = settings.PrincipalName ?? "";
                    appsettingView.CashierName = settings.CashierName ?? "";
                    appsettingView.CanPrintResult = settings.CanPrintResult;
                    
                    isAdmin = settings.IsAdmin;
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

            await _unitOfWork.AppSettingsServices.UpsertAppSettingsAsync(settings);

            TempData["Success"] = "App settings saved successfully.";
            return RedirectToPage();
        }

        public void LoadDropdown()
        {
            SelectionView = new()
            {
                AcademicSession = _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result,
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown(),
                SchoolClasses = _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result,
                SubClass = _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result
            };
        }
    }

}
