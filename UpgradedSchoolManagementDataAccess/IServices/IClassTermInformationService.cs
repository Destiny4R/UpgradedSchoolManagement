using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IClassTermInformationService
    {
        Task<DataTablesResponse<ClassTermInformationDto>> GetClassTermInformations(DataTablesRequest request);
        Task<ClassTermInformationRequest?> GetById(int id);
        Task<ApiResponse<int>> Create(ClassTermInformationRequest request);
        Task<ApiResponse<bool>> Update(ClassTermInformationRequest request);
        Task<ApiResponse<bool>> Delete(int id);
    }
}
