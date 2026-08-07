using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradedSchoolManagementModels.Models
{
    public class ConstantEnums
    {
        public enum Gender { Male = 1, Female = 2 }
        public enum Term { First = 1, Second = 2, Third = 3 }
        public enum PaymentStatus
        {
            Pending = 1,
            Completed = 2,
            Failed = 3,
            Reversed = 4
        }

        public enum PaymentState
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3,
            Cancelled = 4
        }

        public enum PaymentSource
        {
            Manual = 1,
            Online = 2
        }

        public enum PaymentVerificationStatus
        {
            Pending = 1,
            Successful = 2,
            Failed = 3,
            Verified = 4
        }

        public enum PaystackWebhookEvent
        {
            ChargeSuccess = 1,
            TransferSuccess = 2
        }

        /// <summary>
        /// The payment provider used to attempt a transaction (Paystack, Flutterwave, ...).
        /// </summary>
        public enum PaymentProvider
        {
            Paystack = 1,
            Flutterwave = 2
        }

        /// <summary>
        /// Lifecycle of a single payment transaction attempt.
        /// Only a Successful transaction marks the related StudentPayment as paid.
        /// </summary>
        public enum PaymentTransactionStatus
        {
            Pending = 1,
            Successful = 3,
            Failed = 4,
            Cancelled = 5,
            Expired = 6
        }

        public enum ResultType
        {
            Nursery = 1,
            Primary = 2,
            Jss = 3,
            SSS = 4
        }

        public enum ResultSkillDomain
        {
            Affective = 1,
            Psychomotor = 2
        }
    }
}
