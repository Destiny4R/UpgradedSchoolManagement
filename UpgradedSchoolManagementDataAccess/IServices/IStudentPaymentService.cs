using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IStudentPaymentService
    {
        Task<MakePaymentPageViewModel> GetPayableItemsAsync(int termRegistrationId);
        Task<ApiResponse<MakePaymentPageViewModel>> LookupPayableItemsAsync(string admissionNo, int classId, int categoryId);
        Task<ApiResponse<int>> CreatePaymentAsync(CreatePaymentViewModel model, string? evidenceFilePath = null);
        Task<PaymentReceiptViewModel> GetReceiptAsync(int paymentId);
        Task<PaymentReceiptViewModel> GetPaymentDetailAsync(int paymentId);
        Task<ApiResponse<bool>> UpdatePaymentStateAsync(int paymentId, PaymentState state, string? rejectMessage);
        Task<ConsolidatedReceiptViewModel> GetConsolidatedReceiptAsync(int termRegId);
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentsDataTableAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null,
            string? statusFilter = null, int? stateFilter = null);
        Task<List<PendingPaymentNotification>> GetPendingPaymentNotificationsAsync(int maxCount = 20);
        /// <summary>
        /// Returns whether every compulsory fee for the given term registration is
        /// fully paid and approved. Used to gate result access.
        /// </summary>
        Task<(bool hasPaid, List<string> unpaidItems)> HasPaidAllCompulsoryFeesAsync(int termRegId);

        // ── NEW: Single-item payment flow ─────────────────────────────────────

        /// <summary>
        /// Looks up a student by admission number and validates they are registered
        /// for the given session/term, then returns item balance + payment history.
        /// </summary>
        Task<ApiResponse<SingleItemLookupResult>> LookupByItemAsync(
            int sessionId, Term term, int paymentItemId, string admissionNo);

        /// <summary>
        /// Records a new part-payment for a single payment item.
        /// Enforces overpayment prevention and stores RecordedBy for audit.
        /// </summary>
        Task<ApiResponse<int>> CreateSingleItemPaymentAsync(
            CreateSingleItemPaymentVM model, string? recordedBy);

        /// <summary>
        /// Idempotently updates the amount of an existing Pending payment.
        /// Only Pending state payments may be edited.
        /// Calling this twice with identical values is a no-op (returns success).
        /// </summary>
        Task<ApiResponse<bool>> UpdatePaymentAmountAsync(
            UpdatePaymentAmountVM model, string? updatedBy);

        /// <summary>
        /// Returns a filtered, paged list of payments for a single student (by their StudentsTable.Id).
        /// </summary>
        Task<DataTablesResponse<StudentPaymentListDto>> GetStudentPaymentsPagedAsync(
            int studentId, DataTablesRequest request);

        /// <summary>
        /// Loads the full term receipt for a given term registration,
        /// including expected vs paid breakdown and individual payment history.
        /// </summary>
        Task<FullTermReceiptViewModel?> GetFullTermReceiptAsync(int termRegId);
    }
}
