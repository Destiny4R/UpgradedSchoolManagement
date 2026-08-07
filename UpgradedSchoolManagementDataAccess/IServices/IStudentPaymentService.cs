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

        // ── ONLINE PAYMENT (PAYSTACK) ──────────────────────────────────────────

        /// <summary>
        /// Returns the flat list of payable items for a term registration with
        /// remaining balance and online-payment state, for the student dashboard.
        /// </summary>
        Task<List<PendingOnlinePaymentItemVM>> GetPendingPaymentsAsync(int termRegId);

        /// <summary>
        /// Creates a Pending online payment, initializes a Paystack transaction,
        /// and returns the hosted payment URL. The payment stays Pending until the
        /// Paystack webhook confirms it.
        /// </summary>
        Task<ApiResponse<InitiateOnlinePaymentResponse>> InitiateOnlinePaymentAsync(
            int termRegId, int paymentItemId, decimal? amount, string? recordedBy, string callbackUrl);

        /// <summary>
        /// Applies a Paystack verification result to a payment (called from the
        /// webhook and inline JS popup completion). Idempotent — transitions payment
        /// to Completed status and Approved state.
        /// </summary>
        Task<ApiResponse<bool>> ApplyPaystackVerificationAsync(string reference, PaystackVerificationResult verification, string? verifiedBy = null);

        /// <summary>
        /// Confirms an online payment by re-verifying with Paystack (used on the
        /// student callback page so the UI updates without waiting for the webhook).
        /// </summary>
        Task<ApiResponse<bool>> ConfirmOnlinePaymentAsync(string reference, string? confirmedBy);

        /// <summary>
        /// Cancels a pending online payment attempt (e.g. the student closed the
        /// Paystack popup or the attempt was abandoned). Marks the transaction Cancelled and
        /// leaves the StudentPayment unpaid so the student can retry immediately. Idempotent —
        /// a successful/verified transaction is never cancelled. A later provider success
        /// still honours the charge because the provider is the source of truth.
        /// </summary>
        Task<ApiResponse<bool>> CancelOnlinePaymentAsync(string reference, string? cancelledBy);

        /// <summary>
        /// Marks a successful online payment as Verified (admin action).
        /// Only online payments with VerificationStatus Successful can be verified,
        /// and only once. Verification also approves the payment for reporting.
        /// </summary>
        Task<ApiResponse<bool>> VerifyOnlinePaymentAsync(int paymentId, string? verifiedBy);

        /// <summary>
        /// Returns a filtered, paged list of online payments for the admin
        /// verification DataTable.
        /// </summary>
        Task<DataTablesResponse<OnlinePaymentListRowDto>> GetOnlinePaymentsPagedAsync(
            DataTablesRequest request, int? sessionFilter = null, int? termFilter = null,
            int? classFilter = null, int? verificationStatusFilter = null);
    }
}
