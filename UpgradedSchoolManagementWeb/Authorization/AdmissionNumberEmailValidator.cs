using Microsoft.AspNetCore.Identity;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementWeb.Authorization
{
    /// <summary>
    /// Replaces the default email validator so that admission numbers
    /// (e.g. EDU/STD/2025/001) are accepted as the email field.
    /// Only uniqueness is enforced; format is not checked.
    /// </summary>
    public class AdmissionNumberEmailValidator : IUserValidator<ApplicationUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                // Enforce uniqueness only
                var owner = await manager.FindByEmailAsync(user.Email);
                if (owner != null && !string.Equals(owner.Id, user.Id, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DuplicateEmail",
                        Description = $"Email '{user.Email}' is already in use."
                    });
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }
    }
}
