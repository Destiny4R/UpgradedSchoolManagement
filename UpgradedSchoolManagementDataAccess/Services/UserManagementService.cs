using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserManagementService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<DataTablesResponse<UserListDto>> GetUsers(DataTablesRequest request)
        {
            var query = _context.Users.Where(u => u.Employee != null).AsQueryable();
            var search = request.Search?.Value?.ToLower() ?? "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(search)));
            }

            var total = await query.CountAsync();

            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";
            var isAsc = sortDir == "asc";

            query = sortCol switch
            {
                1 => isAsc ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                2 => isAsc ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                5 => isAsc ? query.OrderBy(u => u.CreatedDate) : query.OrderByDescending(u => u.CreatedDate),
                _ => query.OrderByDescending(u => u.CreatedDate)
            };

            var users = await query
                .Skip(request.Start)
                .Take(request.Length)
                .ToListAsync();

            var data = new List<UserListDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var employee = _context.EmployeeTables.FirstOrDefault(k => k.ApplicationUserId == user.Id);
                data.Add(new UserListDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    StaffId = user.StaffId,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Emp = employee?.Id??0,
                    Roles = roles.ToList()
                });
            }

            return new DataTablesResponse<UserListDto>
            {
                Draw = request.Draw,
                RecordsTotal = total,
                RecordsFiltered = total,
                Data = data
            };
        }

        public async Task<ApiResponse<object>> CreateUser(string email, string password, string fullName, List<string>? roles = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new ApiResponse<object> { Success = false, Message = "Email is required" };

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return new ApiResponse<object> { Success = false, Message = "A user with this email already exists" };

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            if (roles != null && roles.Count > 0)
            {
                foreach (var roleId in roles)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }
            }

            return new ApiResponse<object> { Success = true, Message = "User created successfully" };
        }

        public async Task<ApiResponse<object>> UpdateUser(string id, string fullName, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "User not found" };

            user.FullName = fullName;
            user.IsActive = isActive;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ApiResponse<object> { Success = true, Message = "User updated successfully" };
        }

        public async Task<ApiResponse<object>> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "User not found" };

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                return new ApiResponse<object> { Success = false, Message = "SuperAdmin users cannot be deleted" };

            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in claims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ApiResponse<object> { Success = true, Message = "User deleted successfully" };
        }

        public async Task<List<string>> GetUserRoleIds(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            var roleNames = await _userManager.GetRolesAsync(user);
            return await _context.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();
        }

        public async Task<ApiResponse<object>> AssignRoles(string userId, List<string> roleIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "User not found" };

            var currentRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in currentRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            foreach (var roleId in roleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role != null && role.Name != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var permClaims = claims.Where(c => c.Type == "Permission").ToList();
            foreach (var c in permClaims)
            {
                await _userManager.RemoveClaimAsync(user, c);
            }
            var allPerms = await GetUserPermissionsAsync(userId);
            foreach (var p in allPerms)
            {
                await _userManager.AddClaimAsync(user, new Claim("Permission", p));
            }

            return new ApiResponse<object> { Success = true, Message = "Roles assigned successfully" };
        }

        public async Task<ApiResponse<object>> ResetUserPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "User not found" };

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return new ApiResponse<object> { Success = false, Message = "Password must be at least 6 characters" };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description))
                };

            return new ApiResponse<object> { Success = true, Message = "Password reset successfully" };
        }

        public async Task<List<ApplicationRole>> GetAllRoles()
        {
            return await _context.Roles.OrderBy(r => r.Name).ToListAsync();
        }

        private async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var permissionCodes = await _context.RolePermissions
                .Where(rp => userRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            return PermissionHelper.ExpandPermissions(permissionCodes);
        }
    }
}
