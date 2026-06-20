using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IClassService
    {
        // ─── School Class CRUD ───────────────────────────────────────────────────
        Task<DataTablesResponse<SchoolClasses>> GetClasses(DataTablesRequest request);
        Task<SchoolClasses?> GetClassById(int id);
        Task<ApiResponse<SchoolClasses>> CreateClass(string name, int displayOrder = 0, int resultType = 0);
        Task<ApiResponse<SchoolClasses>> UpdateClass(int id, string name, int displayOrder, int resultType = 0);
        Task<ApiResponse<bool>> DeleteClass(int id);
        Task<ApiResponse<bool>> ToggleClassStatus(int id);

        // ─── Assessment Configuration ────────────────────────────────────────────

        /// <summary>
        /// Returns a DataTables-paged list of AssessmentConfiguration rows.
        /// Pass classId = 0 to return configs for all classes.
        /// </summary>
        Task<DataTablesResponse<AssessmentConfigDto>> GetClassAssessmentConfigs(DataTablesRequest request, int classId);

        /// <summary>
        /// Returns all assessment configs for a single class (used to pre-populate the edit modal).
        /// </summary>
        Task<List<AssessmentConfigDto>> GetAssessmentConfigsByClassId(int classId);

        /// <summary>
        /// Saves (replace-all) the given assessment list for every classId supplied.
        /// Existing configs for each class are deleted before inserting the new set.
        /// </summary>
        Task<ApiResponse<bool>> SaveAssessmentConfigs(
            List<int> classIds,
            List<ResultConfigAssessmentViewModel> assessments);

        /// <summary>Deletes a single AssessmentConfiguration row by its Id.</summary>
        Task<ApiResponse<bool>> DeleteAssessmentConfig(int id);

        /// <summary>Deletes ALL assessment configs for a given class.</summary>
        Task<ApiResponse<bool>> DeleteAllAssessmentConfigsByClassId(int classId);

        /// <summary>
        /// Updates the name, max score, and display order of a single assessment config row.
        /// Does not affect other rows for the same class.
        /// </summary>
        Task<ApiResponse<bool>> UpdateSingleAssessmentConfig(int id, string name, double score, int displayOrder);
    }
}