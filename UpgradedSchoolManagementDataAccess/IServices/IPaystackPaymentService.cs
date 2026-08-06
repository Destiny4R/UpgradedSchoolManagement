using System.Threading.Tasks;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public class PaystackInitializeResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AuthorizationUrl { get; set; }
        public string? Reference { get; set; }
        public string? AccessCode { get; set; }
    }

    public interface IPaystackPaymentService
    {
        /// <summary>
        /// Reads the global Paystack settings from the admin app-settings row.
        /// </summary>
        Task<(bool Enabled, string? SecretKey, string? PublicKey)> GetSettingsAsync();

        /// <summary>Creates a Paystack transaction and returns the hosted payment URL.</summary>
        Task<PaystackInitializeResult> InitializeAsync(string email, string reference, decimal amount, string callbackUrl);

        /// <summary>Verifies a transaction on Paystack and returns its normalized status.</summary>
        Task<PaystackVerificationResult> VerifyAsync(string reference);

        /// <summary>
        /// Validates the x-paystack-signature header (HMAC-SHA512 of the raw payload using the secret key).
        /// </summary>
        Task<bool> IsValidWebhookSignatureAsync(string payload, string? signatureHeader);
    }
}
