using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IResultManagerService
    {
        Task<List<ResultSheetDto>> GetResultSheetAsync(TermObjects model);
        Task<EditResultDto?> GetEditResultDataAsync(long termRegId);
        Task<ApiResponse<bool>> SaveResultsAsync(SaveResultDto model);
        Task<List<AssessmentSheetDto>> GetAssessmentSheetAsync(TermObjects model);
        Task<ApiResponse<AssessmentImportResult>> ImportAssessmentScoresAsync(
            int sessionId, Term termValue, int schoolClassId, int subClassId, Stream excelStream);
    }
}
