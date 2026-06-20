using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IResultSkillService
    {
        Task<List<ResultSkillDto>> GetActiveSkillsAsync();
        Task<List<ResultSkillDto>> GetAssignedSkillsByClassIdAsync(int schoolClassId);
        Task<DataTablesResponse<ResultSkillDto>> GetSkillsForDataTableAsync(DataTablesRequest request);
        Task<DataTablesResponse<ResultSkillDto>> GetAssignedSkillsForDataTableAsync(DataTablesRequest request, int schoolClassId);
        Task<ApiResponse<ResultSkillDto>> CreateSkillAsync(CreateResultSkillDto dto);
        Task<ApiResponse<bool>> UpdateSkillAsync(UpdateResultSkillDto dto);
        Task<ApiResponse<bool>> ToggleSkillStatusAsync(int id);
        Task<ApiResponse<bool>> AssignSkillsToClassAsync(int schoolClassId, List<int> resultSkillIds);
        Task<ApiResponse<bool>> EnsureTerminalSkillRatingsForTermRegistrationAsync(long termRegId);
        Task<TerminalResultDto?> GetTerminalResultAsync(long termRegId);
    }
}
