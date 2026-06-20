using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IPaymentCategoryService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentCategoriesAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc");
        Task<PaymentCategoryViewModel> GetPaymentCategoryByIdAsync(int id);
        Task<List<PaymentCategoryViewModel>> GetActiveCategoriesAsync();
        Task<ApiResponse<int>> CreatePaymentCategoryAsync(PaymentCategoryViewModel model);
        Task<ApiResponse<bool>> UpdatePaymentCategoryAsync(PaymentCategoryViewModel model);
        Task<ApiResponse<bool>> DeletePaymentCategoryAsync(int id);
        Task<ApiResponse<bool>> TogglePaymentCategoryAsync(int id);
    }
}
