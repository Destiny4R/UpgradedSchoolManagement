using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IUserManagementService
    {
        Task<DataTablesResponse<UserListDto>> GetUsers(DataTablesRequest request);
        Task<ApiResponse<object>> CreateUser(string email, string password, string fullName, List<string>? roles = null);
        Task<ApiResponse<object>> UpdateUser(string id, string fullName, bool isActive);
        Task<ApiResponse<object>> DeleteUser(string id);
        Task<List<string>> GetUserRoleIds(string userId);
        Task<ApiResponse<object>> AssignRoles(string userId, List<string> roleIds);
        Task<ApiResponse<object>> ResetUserPassword(string userId, string newPassword);
        Task<List<ApplicationRole>> GetAllRoles();
    }

    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? StaffId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Roles { get; set; } = new();
        public int Emp { get; set; }
    }
}
