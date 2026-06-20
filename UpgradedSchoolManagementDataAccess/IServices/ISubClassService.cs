using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ISubClassService
    {
        Task<DataTablesResponse<SubClassTable>> GetSubClasses(DataTablesRequest request);
        Task<SubClassTable?> GetSubClassById(int id);
        Task<ApiResponse<SubClassTable>> CreateSubClass(string name);
        Task<ApiResponse<SubClassTable>> UpdateSubClass(int id, string name);
        Task<ApiResponse<bool>> DeleteSubClass(int id);
        Task<ApiResponse<bool>> ToggleSubClassStatus(int id);
        Task<List<SubClassTable>> GetSubClasses();
    }
}