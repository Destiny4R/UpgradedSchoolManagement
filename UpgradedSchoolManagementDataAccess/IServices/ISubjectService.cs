using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ISubjectService
    {
        Task<DataTablesResponse<SubjectTable>> GetSubjects(DataTablesRequest request);
        Task<SubjectTable?> GetSubjectById(int id);
        Task<ApiResponse<SubjectTable>> CreateSubject(string name, string? code);
        Task<ApiResponse<SubjectTable>> UpdateSubject(int id, string name, string? code);
        Task<ApiResponse<bool>> DeleteSubject(int id);
        Task<ApiResponse<bool>> ToggleSubjectStatus(int id);
        IEnumerable<SubjectTable> GetAllActiveSubjects();
    }
}