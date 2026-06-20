using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.ViewModels
{
    /// <summary>
    /// Request model for looking up a student's payable items
    /// </summary>
    public class PaymentLookupViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string AdmissionNo { get; set; }
    }

    /// <summary>
    /// Represents a single payable item within a category group
    /// </summary>
    public class PayableItemViewModel
    {
        public int PaymentItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal AlreadyPaid { get; set; }
        /// <summary>True when this item was configured as compulsory in PaymentSetup.</summary>
        public bool IsCompulsory { get; set; }
        public decimal Remaining => ExpectedAmount - AlreadyPaid;
        public bool IsFullyPaid => Remaining <= 0;
    }

    /// <summary>
    /// Groups payable items by category for UI accordion display
    /// </summary>
    public class CategoryGroupViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<PayableItemViewModel> Items { get; set; } = new();
    }

    /// <summary>
    /// Full data needed for the make-payment page
    /// </summary>
    public class MakePaymentPageViewModel
    {
        public int TermRegistrationId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public List<CategoryGroupViewModel> CategoryGroups { get; set; } = new();
    }

    /// <summary>
    /// Line item submitted from the payment form
    /// </summary>
    public class StudentPaymentItemVM
    {
        public int PaymentItemId { get; set; }
        public decimal AmountPaid { get; set; }
    }

    /// <summary>
    /// Payload submitted when creating a payment
    /// </summary>
    public class CreatePaymentViewModel
    {
        [Required]
        public int TermRegistrationId { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }

        [Required]
        public List<StudentPaymentItemVM> Items { get; set; } = new();
    }

    /// <summary>
    /// Receipt display after payment
    /// </summary>
    public class PaymentReceiptViewModel
    {
        public int PaymentId { get; set; }
        public long TermRegId { get; set; }
        public int SessionId { get; set; }
        public int Term { get; set; }
        public string Reference { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public string? Narration { get; set; }
        public string? RecordedBy { get; set; }
        public string? RejectMessage { get; set; }
        public string? EvidenceFilePath { get; set; }
        public List<ReceiptLineItem> LineItems { get; set; } = new();
    }

    public class ReceiptLineItem
    {
        public int PaymentItemId { get; set; }
        public string CategoryName { get; set; }
        public string ItemName { get; set; }
        public decimal AmountPaid { get; set; }
    }

    /// <summary>
    /// Consolidated receipt showing all payments for a term registration grouped by category
    /// </summary>
    public class ConsolidatedReceiptViewModel
    {
        public int TermRegistrationId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public DateTime PrintDate { get; set; } = DateTime.UtcNow;
        public List<ReceiptCategoryGroup> Categories { get; set; } = new();
        public decimal GrandTotal => Categories.Sum(c => c.CategoryTotal);
    }

    /// <summary>
    /// Groups receipt line items under a single payment category with a subtotal
    /// </summary>
    public class ReceiptCategoryGroup
    {
        public string CategoryName { get; set; }
        public List<string> PaymentReferences { get; set; } = new();
        public List<ReceiptLineItem> Items { get; set; } = new();
        public decimal CategoryTotal => Items.Sum(i => i.AmountPaid);
    }

    /// <summary>
    /// Request model for updating payment state (approve/reject/cancel)
    /// </summary>
    public class UpdatePaymentStateRequest
    {
        [Required]
        public int PaymentState { get; set; }

        [StringLength(120)]
        public string? RejectMessage { get; set; }
    }

    /// <summary>
    /// Notification item for pending payment approvals
    /// </summary>
    public class PendingPaymentNotification
    {
        public int PaymentId { get; set; }
        public string Reference { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TimeAgo { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // NEW — Single-item payment flow models
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Request payload for the new single-item student lookup.
    /// </summary>
    public class PaymentItemLookupRequest
    {
        [Required]
        public int SessionId { get; set; }

        [Required]
        public Term Term { get; set; }

        [Required]
        public int PaymentItemId { get; set; }

        [Required]
        public string AdmissionNo { get; set; } = string.Empty;
    }

    /// <summary>
    /// A single row in the payment history table for one payment item.
    /// </summary>
    public class PaymentHistoryRow
    {
        public int PaymentId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string? RecordedBy { get; set; }
        public string State { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        /// <summary>Remaining balance after this payment was recorded.</summary>
        public decimal RunningBalance { get; set; }
    }

    /// <summary>
    /// Aggregated balance information for a single payment item.
    /// </summary>
    public class ItemBalanceSummary
    {
        public decimal TotalDue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Outstanding { get; set; }
        /// <summary>"Unpaid" | "Partially Paid" | "Fully Paid"</summary>
        public string PaymentStatusLabel { get; set; } = "Unpaid";
    }

    /// <summary>
    /// Full result returned from LookupByItemAsync — student info, item info,
    /// current balance, and complete payment history.
    /// </summary>
    public class SingleItemLookupResult
    {
        // Student / registration
        public int TermRegistrationId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;

        // Payment item
        public int PaymentItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsCompulsory { get; set; }

        // Calculated balance
        public ItemBalanceSummary Balance { get; set; } = new();

        // Full history (oldest first)
        public List<PaymentHistoryRow> History { get; set; } = new();
    }

    /// <summary>
    /// Payload for recording a new single-item part payment.
    /// </summary>
    public class CreateSingleItemPaymentVM
    {
        [Required]
        public int TermRegistrationId { get; set; }

        [Required]
        public int PaymentItemId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal AmountPaid { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }
    }

    /// <summary>
    /// Payload for the idempotent edit of an existing pending payment's amount.
    /// </summary>
    public class UpdatePaymentAmountVM
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal NewAmount { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // FULL TERM RECEIPT
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// A single row in the fee item breakdown (expected vs paid per item).
    /// </summary>
    public class ReceiptBreakdownRow
    {
        public string CategoryName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Expected { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance => Expected - Paid;
    }

    /// <summary>
    /// A single row in the payment history.
    /// </summary>
    public class ReceiptPaymentHistoryRow
    {
        public string Reference { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Row DTO for student-facing payment history DataTable.
    /// </summary>
    public class StudentPaymentListDto
    {
        public int Id { get; set; }
        public long TermRegId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? Narration { get; set; }
        public string Session { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public List<string> ItemNames { get; set; } = new();
    }

    /// <summary>
    /// Full term receipt data — student info, expected vs paid breakdown,
    /// payment history, and summary totals.
    /// </summary>
    public class FullTermReceiptViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubClassName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public DateTime PrintDate { get; set; } = DateTime.UtcNow;

        public decimal TotalExpected { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Outstanding => TotalExpected - TotalPaid;

        public List<ReceiptBreakdownRow> Breakdown { get; set; } = new();
        public List<ReceiptPaymentHistoryRow> Payments { get; set; } = new();
    }
}
