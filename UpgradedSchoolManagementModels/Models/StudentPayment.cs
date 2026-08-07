using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.Models
{
    public class StudentPayment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(TermRegId))]
        public long TermRegId { get; set; }
        public TermRegistration TermRegistration { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Final reference for a completed payment. Null while the payment is only an
        /// in-progress attempt — it is populated from the successful PaymentTransaction
        /// only after the provider has verified the payment.
        /// </summary>
        public string? Reference { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public PaymentState State { get; set; }  = PaymentState.Pending;

        /// <summary>Where the payment was collected: manual cash/bank or online via Paystack.</summary>
        public PaymentSource PaymentSource { get; set; } = PaymentSource.Manual;

        /// <summary>Online verification lifecycle: Pending -> Successful/Failed -> Verified.</summary>
        public PaymentVerificationStatus VerificationStatus { get; set; } = PaymentVerificationStatus.Pending;

        /// <summary>Paystack transaction reference (also stored in Reference for online payments).</summary>
        [StringLength(120)]
        public string? PaystackReference { get; set; }

        /// <summary>UserName of the staff member who verified an online payment.</summary>
        [StringLength(256)]
        public string? VerifiedBy { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }

        [StringLength(120)]
        public string? RejectMessage { get; set; }
        [StringLength(420)]
        public string? EvidenceFilePath { get; set; }

        /// <summary>UserName of the staff member who recorded or last edited this payment.</summary>
        [StringLength(256)]
        public string? RecordedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StudentPaymentItem> PaymentItems { get; set; }

        /// <summary>
        /// Every payment attempt (successful, failed, cancelled, abandoned) made
        /// against this payment. Only one successful transaction marks it as paid.
        /// </summary>
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    }
}
