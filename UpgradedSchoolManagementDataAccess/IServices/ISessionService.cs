using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ISessionService
    {
        Task<DataTablesResponse<SesseionTable>> GetSessions(DataTablesRequest request);
        Task<SesseionTable?> GetSessionById(int id);
        Task<ApiResponse<SesseionTable>> CreateSession(string name);
        Task<ApiResponse<SesseionTable>> UpdateSession(int id, string name);
        Task<ApiResponse<bool>> DeleteSession(int id);
        Task<ApiResponse<bool>> ToggleSessionStatus(int id);
        Task<int?> GetLatestActiveSessionIdAsync();
    }
}