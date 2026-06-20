using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<DataTablesResponse<RoleListDto>> GetRoles(DataTablesRequest request)
        {
            var query = _context.Roles.AsQueryable();
            var search = request.Search?.Value?.ToLower() ?? "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name!.ToLower().Contains(search) || (r.Description != null && r.Description.ToLower().Contains(search)));
            }

            var total = await query.CountAsync();

            var sortCol = request.Order?.FirstOrDefault()?.Column ?? 0;
            var sortDir = request.Order?.FirstOrDefault()?.Dir ?? "asc";
            var isAsc = sortDir == "asc";

            query = sortCol switch
            {
                1 => isAsc ? query.OrderBy(r => r.Name) : query.OrderByDescending(r => r.Name),
                3 => isAsc ? query.OrderBy(r => r.CreatedDate) : query.OrderByDescending(r => r.CreatedDate),
                _ => query.OrderByDescending(r => r.CreatedDate)
            };

            var roles = await query
                .Skip(request.Start)
                .Take(request.Length)
                .ToListAsync();

            var data = new List<RoleListDto>();
            foreach (var role in roles)
            {
                var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
                data.Add(new RoleListDto
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    Description = role.Description,
                    IsActive = role.IsActive,
                    UserCount = userCount,
                    CreatedDate = role.CreatedDate
                });
            }

            return new DataTablesResponse<RoleListDto>
            {
                Draw = request.Draw,
                RecordsTotal = total,
                RecordsFiltered = total,
                Data = data
            };
        }

        public async Task<ApiResponse<object>> CreateRole(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new ApiResponse<object> { Success = false, Message = "Role name is required" };

            var existing = await _roleManager.FindByNameAsync(name);
            if (existing != null)
                return new ApiResponse<object> { Success = false, Message = "A role with this name already exists" };

            var role = new ApplicationRole
            {
                Name = name,
                Description = description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ApiResponse<object> { Success = true, Message = "Role created successfully" };
        }

        public async Task<ApiResponse<object>> UpdateRole(string id, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new ApiResponse<object> { Success = false, Message = "Role name is required" };

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return new ApiResponse<object> { Success = false, Message = "Role not found" };

            if (role.Name == "SuperAdmin")
                return new ApiResponse<object> { Success = false, Message = "SuperAdmin role cannot be edited" };

            var duplicate = await _roleManager.FindByNameAsync(name);
            if (duplicate != null && duplicate.Id != id)
                return new ApiResponse<object> { Success = false, Message = "A role with this name already exists" };

            role.Name = name;
            role.Description = description;
            role.UpdatedDate = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ApiResponse<object> { Success = true, Message = "Role updated successfully" };
        }

        public async Task<ApiResponse<object>> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return new ApiResponse<object> { Success = false, Message = "Role not found" };

            if (role.Name == "SuperAdmin")
                return new ApiResponse<object> { Success = false, Message = "SuperAdmin role cannot be deleted" };

            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == id);
            if (userCount > 0)
            {
                var usersInRole = await _context.UserRoles.Where(ur => ur.RoleId == id).Select(ur => ur.UserId).ToListAsync();
                foreach (var userId in usersInRole)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                }
            }

            var rolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermissions);
            await _context.SaveChangesAsync();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return new ApiResponse<object> { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ApiResponse<object> { Success = true, Message = "Role deleted successfully" };
        }

        public async Task<ApplicationRole?> GetRoleById(string id)
        {
            return await _roleManager.FindByIdAsync(id);
        }

        public async Task<List<Permission>> GetAllPermissions()
        {
            return await _context.Permissions.OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
        }

        public async Task<List<string>> GetRolePermissionCodes(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission.Code)
                .ToListAsync();
        }

        public async Task<ApiResponse<object>> AssignPermissions(string roleId, List<int> permissionIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return new ApiResponse<object> { Success = false, Message = "Role not found" };

            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            foreach (var permId in permissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();

            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId).ToListAsync();
            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
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
                }
            }

            return new ApiResponse<object> { Success = true, Message = "Permissions updated successfully" };
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
