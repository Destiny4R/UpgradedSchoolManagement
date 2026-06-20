using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IRoleService
    {
        Task<DataTablesResponse<RoleListDto>> GetRoles(DataTablesRequest request);
        Task<ApiResponse<object>> CreateRole(string name, string? description);
        Task<ApiResponse<object>> UpdateRole(string id, string name, string? description);
        Task<ApiResponse<object>> DeleteRole(string id);
        Task<ApplicationRole?> GetRoleById(string id);
        Task<List<Permission>> GetAllPermissions();
        Task<List<string>> GetRolePermissionCodes(string roleId);
        Task<ApiResponse<object>> AssignPermissions(string roleId, List<int> permissionIds);
    }

    public class RoleListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
