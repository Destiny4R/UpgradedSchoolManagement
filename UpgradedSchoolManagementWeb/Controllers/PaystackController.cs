using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Controllers
{
    /// <summary>
    /// Handles Paystack webhook deliveries. The endpoint is anonymous and validates
    /// the HMAC-SHA512 x-paystack-signature header before applying any state change.
    /// </summary>
    [Route("paystack")]
    public class PaystackController : Controller
    {
        private readonly IStudentPaymentService _studentPaymentService;
        private readonly IPaystackPaymentService _paystackService;
        private readonly IAuditLogService _auditLogService;

        public PaystackController(
            IStudentPaymentService studentPaymentService,
            IPaystackPaymentService paystackService,
            IAuditLogService auditLogService)
        {
            _studentPaymentService = studentPaymentService;
            _paystackService = paystackService;
            _auditLogService = auditLogService;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            string payload;
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(payload))
                return BadRequest(new { status = false, message = "Empty payload." });

            var signature = Request.Headers["x-paystack-signature"].ToString();
            var isValidSignature = await _paystackService.IsValidWebhookSignatureAsync(payload, signature);
            if (!isValidSignature)
            {
                await _auditLogService.LogAsync(
                    "", "paystack", "WEBHOOK_REJECTED", "Payments",
                    "Paystack webhook rejected: invalid signature.");
                return Unauthorized(new { status = false, message = "Invalid signature." });
            }

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                var eventName = root.TryGetProperty("event", out var evt) ? evt.GetString() : string.Empty;
                var data = root.TryGetProperty("data", out var d) ? d : default;

                if (string.IsNullOrEmpty(eventName) || data.ValueKind != JsonValueKind.Object)
                    return Ok(new { status = true });

                var reference = data.TryGetProperty("reference", out var refEl) ? refEl.GetString() : string.Empty;
                if (string.IsNullOrEmpty(reference))
                    return Ok(new { status = true });

                var verification = new PaystackVerificationResult
                {
                    Reference = reference,
                    Currency = data.TryGetProperty("currency", out var cur) ? cur.GetString() : "NGN"
                };

                if (eventName.Equals("charge.success", StringComparison.OrdinalIgnoreCase))
                {
                    verification.Success = true;
                    verification.Status = data.TryGetProperty("status", out var st) ? st.GetString() : "success";
                    if (data.TryGetProperty("amount", out var amountEl) && amountEl.TryGetInt64(out var amountKobo))
                        verification.Amount = amountKobo / 100m;
                    if (data.TryGetProperty("paid_at", out var paidAt) && DateTime.TryParse(paidAt.GetString(), out var paidDate))
                        verification.PaidAt = paidDate.ToUniversalTime();
                }
                else
                {
                    verification.Success = false;
                    verification.Status = "failed";
                }

                var result = await _studentPaymentService.ApplyPaystackVerificationAsync(reference, verification);

                if (result.Success)
                {
                    await _auditLogService.LogAsync(
                        "", "paystack", "WEBHOOK_PROCESSED", "Payments",
                        $"Paystack webhook {eventName} processed for reference {reference}.");
                }

                return Ok(new { status = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
    }
}
