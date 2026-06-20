using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IPaymentSetupService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentSetupsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null);
        Task<PaymentSetupViewModel> GetPaymentSetupByIdAsync(int id);
        Task<ApiResponse<int>> CreatePaymentSetupAsync(PaymentSetupViewModel model);
        Task<ApiResponse<string>> CreateBatchPaymentSetupAsync(PaymentSetupViewModel model);
        Task<ApiResponse<bool>> UpdatePaymentSetupAsync(PaymentSetupViewModel model);
        Task<ApiResponse<bool>> DeletePaymentSetupAsync(int id);
        Task<ApiResponse<bool>> TogglePaymentSetupAsync(int id);
    }
}
