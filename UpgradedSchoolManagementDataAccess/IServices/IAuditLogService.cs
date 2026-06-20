using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string userName, string action, string module, string? description = null, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
        Task<DataTablesResponse<AuditLog>> GetLogs(DataTablesRequest request);
        Task<ApiResponse<bool>> DeleteOldLogs(int daysOld);
    }
}