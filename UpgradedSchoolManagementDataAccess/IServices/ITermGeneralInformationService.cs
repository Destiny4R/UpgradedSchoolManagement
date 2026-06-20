using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface ITermGeneralInformationService
    {
        Task<DataTablesResponse<TermGeneralInformationDto>> GetTermGeneralInformations(DataTablesRequest request);
        Task<TermGeneralInformationRequest?> GetById(int id);
        Task<TermGeneralInformationRequest?> GetBySessionAndTerm(int sessionId, int termId);
        Task<ApiResponse<int>> Create(TermGeneralInformationRequest request);
        Task<ApiResponse<bool>> Update(TermGeneralInformationRequest request);
        Task<ApiResponse<bool>> Delete(int id);
    }
}
