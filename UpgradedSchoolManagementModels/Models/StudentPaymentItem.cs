using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.Models
{
    public class StudentPaymentItem
    {
        public int Id { get; set; }
        [ForeignKey(nameof(StudentPaymentId))]
        public int StudentPaymentId { get; set; }
        public StudentPayment StudentPayment { get; set; }

        [ForeignKey(nameof(PaymentItemId))]
        public int PaymentItemId { get; set; }
        public PaymentItem PaymentItem { get; set; }

        public decimal AmountPaid { get; set; }
    }
}
