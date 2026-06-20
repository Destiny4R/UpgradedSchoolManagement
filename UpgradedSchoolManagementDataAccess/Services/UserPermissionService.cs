using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserPermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    return new List<string> { "*" };

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = new List<string>();

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var rolePermissions = await _context.RolePermissions
                            .Where(rp => rp.RoleId == role.Id)
                            .Select(rp => rp.Permission.Code)
                            .ToListAsync();

                        permissions.AddRange(rolePermissions);
                    }
                }

                return PermissionHelper.ExpandPermissions(permissions.Distinct());
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                return (await _userManager.GetRolesAsync(user)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            try
            {
                var permissions = await GetUserPermissionsAsync(userId);
                return permissions.Contains("*") || permissions.Contains(permission);
            }
            catch
            {
                return false;
            }
        }

        public async Task RefreshUserClaimsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return;

                var existingClaims = await _userManager.GetClaimsAsync(user);
                var oldPermissionClaims = existingClaims.Where(c => c.Type == "Permission").ToList();
                foreach (var claim in oldPermissionClaims)
                {
                    await _userManager.RemoveClaimAsync(user, claim);
                }

                var permissions = await GetUserPermissionsAsync(userId);
                foreach (var permission in permissions)
                {
                    await _userManager.AddClaimAsync(user, new Claim("Permission", permission));
                }

                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            catch
            {
            }
        }
    }
}
