using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPermissionService _userPermissionService;
        private readonly IAuditLogService _auditLogService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserPermissionService userPermissionService,
            IAuditLogService auditLogService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userPermissionService = userPermissionService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50)]
            [Display(Name ="Username/Admission Number")]
            public string Email { get; set; }

            [Required, StringLength(20)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                await _auditLogService.LogAsync(
                    "Unknown",
                    Input.Email,
                    "LOGIN_FAILED",
                    "Authentication",
                    "User not found",
                    null, null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            if (!user.IsActive)
            {
                await _auditLogService.LogAsync(
                    user.Id,
                    user.UserName ?? user.Email,
                    "LOGIN_FAILED",
                    "Authentication",
                    "Account disabled",
                    null, null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );

                ModelState.AddModelError(string.Empty, "Your account has been disabled. Please contact the administrator.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName ?? Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _userPermissionService.RefreshUserClaimsAsync(user.Id);

                await _auditLogService.LogAsync(
                    user.Id,
                    user.UserName ?? user.Email,
                    "LOGIN_SUCCESS",
                    "Authentication",
                    "User logged in successfully",
                    null, null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    return RedirectToPage("/student/dashboard");
                }

                return RedirectToPage("/Index");
            }

            if (result.IsLockedOut)
            {
                await _auditLogService.LogAsync(
                    user.Id,
                    user.UserName ?? user.Email,
                    "LOGIN_LOCKED",
                    "Authentication",
                    "Account locked out",
                    null, null,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );

                ModelState.AddModelError(string.Empty, "Your account has been locked out. Please try again later or contact the administrator.");
                return Page();
            }

            await _auditLogService.LogAsync(
                user.Id,
                user.UserName ?? user.Email,
                "LOGIN_FAILED",
                "Authentication",
                "Invalid password",
                null, null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            );

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            return Page();
        }
    }
}