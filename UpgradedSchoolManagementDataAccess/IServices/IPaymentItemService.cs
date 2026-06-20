using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IPaymentItemService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentItemsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? categoryFilter = null);
        Task<PaymentItemViewModel> GetPaymentItemByIdAsync(int id);
        Task<List<PaymentItemViewModel>> GetActiveItemsAsync(int? categoryId = null);
        Task<ApiResponse<int>> CreatePaymentItemAsync(PaymentItemViewModel model);
        Task<ApiResponse<bool>> UpdatePaymentItemAsync(PaymentItemViewModel model);
        Task<ApiResponse<bool>> DeletePaymentItemAsync(int id);
        Task<ApiResponse<bool>> TogglePaymentItemAsync(int id);
        Task<List<PaymentItemViewModel>> GetActiveItemsWithCategoryAsync();
    }
}
