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

        public string Reference { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public PaymentState State { get; set; }  = PaymentState.Pending;
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
    }
}
