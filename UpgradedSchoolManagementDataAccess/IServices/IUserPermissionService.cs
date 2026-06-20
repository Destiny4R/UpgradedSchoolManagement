namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IUserPermissionService
    {
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task RefreshUserClaimsAsync(string userId);
    }
}