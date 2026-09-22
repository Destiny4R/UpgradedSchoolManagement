using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.Models
{
    /// <summary>
    /// A single payment attempt against a <see cref="StudentPayment"/>. One
    /// StudentPayment can have many transaction attempts; only a successful,
    /// verified transaction marks the StudentPayment as paid. Rows are never
    /// deleted — they form a permanent audit trail of every payment attempt.
    /// </summary>
    public class PaymentTransaction
    {
        public int Id { get; set; }

        [ForeignKey(nameof(StudentPaymentId))]
        public int StudentPaymentId { get; set; }
        public StudentPayment StudentPayment { get; set; }

        /// <summary>
        /// Unique transaction reference sent to the payment provider. Enforced as a
        /// unique database index so two attempts can never share the same reference.
        /// </summary>
        [Required]
        [StringLength(120)]
        public string Reference { get; set; }

        public decimal Amount { get; set; }

        /// <summary>The payment provider used for this attempt.</summary>
        public PaymentProvider Provider { get; set; }

        /// <summary>Lifecycle state of this attempt.</summary>
        public PaymentTransactionStatus Status { get; set; }

        /// <summary>The provider's own transaction ID returned after a successful charge.</summary>
        [StringLength(120)]
        public string? ProviderTransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>When the provider confirmed the charge was paid.</summary>
        public DateTime? PaidAt { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }

        [StringLength(256)]
        public string? RecordedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>Human-readable reason when the attempt failed, was cancelled, or expired.</summary>
        [StringLength(500)]
        public string? FailReason { get; set; }

        /// <summary>
        /// Optimistic concurrency token. EF Core uses this to detect concurrent
        /// modifications — if two threads both read a Pending transaction and try
        /// to mark it Successful, exactly one SaveChangesAsync succeeds and the
        /// other throws DbUpdateConcurrencyException.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
