using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLogService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, IAuditLogService auditLogService)
        {
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> OnPost()
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user != null)
            {
                await _auditLogService.LogAsync(
                    user.Id,
                    user.UserName ?? user.Email ?? "Unknown",
                    "LOGOUT",
                    "Authentication",
                    "User logged out",
                    null, null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );
            }

            await _signInManager.SignOutAsync();

            return RedirectToPage("/Account/Login");
        }

        public async Task<IActionResult> OnGet()
        {
            return await OnPost();
        }
    }
}