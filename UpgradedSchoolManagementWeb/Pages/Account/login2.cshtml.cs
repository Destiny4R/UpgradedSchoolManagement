using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Pages.Account
{
    public class login2Model : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Username is required."), DisplayName("Username")]
        public string username { get; set; }
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserPermissionService _userPermissionService;

        public login2Model(SignInManager<ApplicationUser> signInManager, IUserPermissionService userPermissionService)
        {
            _signInManager = signInManager;
            _userPermissionService = userPermissionService;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Add logic to handle login using only the username
            if (!ModelState.IsValid)
            {
                // If the model state is invalid, return the page with validation errors
                ModelState.AddModelError(string.Empty, "Please enter a valid username.");
                return Page();
            }
            //get the first letter of the username and check if it #
            if (username.Length == 0 || username[0] != '#')
            {
                ModelState.AddModelError(string.Empty, "Invalid username.");
                return Page();
            }

            //Check if the username exists in the database
            var user = await _signInManager.UserManager.FindByNameAsync(username.Replace("#", ""));
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username.");
                return Page();
            }
            // Attempt to sign in the user using the username
            await _signInManager.SignInAsync(user, isPersistent: false);

            await _userPermissionService.RefreshUserClaimsAsync(user.Id);
            await _signInManager.RefreshSignInAsync(user);

            if (User.IsInRole("Student"))
            {
                return RedirectToPage("/student/dashboard");
            }
            else
            {
                // If the user is not in any of the expected roles, redirect to a default page
                return RedirectToPage("/Index");
            }

        }
    }
}
