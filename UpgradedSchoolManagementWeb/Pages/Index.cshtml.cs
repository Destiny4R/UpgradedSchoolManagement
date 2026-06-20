using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementWeb.Services;

namespace UpgradedSchoolManagementWeb.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly DashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardViewModel Dashboard { get; set; } = new();

        public IndexModel(DashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && await _userManager.IsInRoleAsync(user, "Student"))
            {
                return RedirectToPage("/student/dashboard");
            }

            Dashboard = await _dashboardService.GetDashboardDataAsync();
            return Page();
        }
    }
}
