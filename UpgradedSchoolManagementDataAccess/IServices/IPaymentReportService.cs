using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IPaymentReportService
    {
        Task<ClassReportResponse> GetClassReportAsync(
            int sessionId, int? term = null, int? classId = null,
            int? subClassId = null, int? categoryId = null, int? paymentItemId = null,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc");

        Task<SchoolReportResponse> GetSchoolReportAsync(
            int sessionId, int term,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc");

        Task<CategoryItemReportResponse> GetCategoryItemReportAsync(
            int sessionId, int term, int? categoryId = null, int? classId = null,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc");

        // Dashboard methods
        Task<List<DashboardCategorySummary>> GetDashboardCategorySummaryAsync(int sessionId, int term);
        Task<List<DashboardItemSummary>> GetDashboardItemSummaryAsync(int sessionId, int term);
        Task<DashboardCategoryTrend> GetDashboardCategoryTrendAsync(int recentSessionCount);
        Task<DashboardItemChart> GetDashboardItemChartAsync(int sessionId, int term);
        Task<DashboardTermRegistrationChart> GetDashboardTermRegistrationChartAsync(int sessionId);
        Task<List<RecentPaymentItem>> GetRecentPaymentsAsync(int count = 10, int? sessionId = null, int? termId = null);
    }
}
