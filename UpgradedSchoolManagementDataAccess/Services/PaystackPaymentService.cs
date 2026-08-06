using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class PaystackPaymentService : IPaystackPaymentService
    {
        private const string BaseUrl = "https://api.paystack.co";
        private static readonly HttpClient _http = new HttpClient();

        private readonly IAppSettingsService _appSettingsService;

        public PaystackPaymentService(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("UpgradedSchoolManagement/1.0");
            }
        }

        public async Task<(bool Enabled, string? SecretKey, string? PublicKey)> GetSettingsAsync()
        {
            return await _appSettingsService.GetPaystackSettingsAsync();
        }

        public async Task<PaystackInitializeResult> InitializeAsync(string email, string reference, decimal amount, string callbackUrl)
        {
            var result = new PaystackInitializeResult();

            var settings = await GetSettingsAsync();
            if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                result.Success = false;
                result.Message = "Online payment is not enabled. Contact the school administrator.";
                return result;
            }

            var amountInKobo = (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

            var body = new Dictionary<string, object>
            {
                ["email"] = email,
                ["amount"] = amountInKobo,
                ["reference"] = reference,
                ["currency"] = "NGN",
                ["callback_url"] = callbackUrl
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/transaction/initialize");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);
                request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                using var response = await _http.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                result.Success = root.TryGetProperty("status", out var st) && st.GetBoolean();
                result.Message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null;

                if (result.Success && root.TryGetProperty("data", out var data))
                {
                    result.AuthorizationUrl = data.TryGetProperty("authorization_url", out var authUrl) ? authUrl.GetString() : null;
                    result.Reference = data.TryGetProperty("reference", out var refEl) ? refEl.GetString() : reference;
                    result.AccessCode = data.TryGetProperty("access_code", out var code) ? code.GetString() : null;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Could not reach Paystack: {ex.Message}";
            }

            return result;
        }

        public async Task<PaystackVerificationResult> VerifyAsync(string reference)
        {
            var result = new PaystackVerificationResult { Reference = reference, Success = false };

            var settings = await GetSettingsAsync();
            if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                result.Message = "Online payment is not enabled.";
                return result;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/transaction/verify/{Uri.EscapeDataString(reference)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

                using var response = await _http.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                result.Success = root.TryGetProperty("status", out var st) && st.GetBoolean();
                result.Message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null;

                if (root.TryGetProperty("data", out var data))
                {
                    result.Status = data.TryGetProperty("status", out var status) ? status.GetString() : string.Empty;
                    result.Reference = data.TryGetProperty("reference", out var refEl) ? refEl.GetString() : reference;
                    result.Currency = data.TryGetProperty("currency", out var currency) ? currency.GetString() : "NGN";
                    if (data.TryGetProperty("amount", out var amountEl) && amountEl.TryGetInt64(out var amountKobo))
                    {
                        result.Amount = amountKobo / 100m;
                    }
                    if (data.TryGetProperty("customer", out var customer) && customer.TryGetProperty("email", out var emailEl))
                    {
                        result.CustomerEmail = emailEl.GetString();
                    }
                    if (data.TryGetProperty("paid_at", out var paidAt) && DateTime.TryParse(paidAt.GetString(), out var paidDate))
                    {
                        result.PaidAt = paidDate.ToUniversalTime();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Could not reach Paystack: {ex.Message}";
            }

            return result;
        }

        public async Task<bool> IsValidWebhookSignatureAsync(string payload, string? signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return false;
            }

            var settings = await GetSettingsAsync();
            if (string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                return false;
            }

            var computed = ComputeHmacSha512(settings.SecretKey, payload);
            return FixedTimeEquals(computed, signatureHeader);
        }

        private static string ComputeHmacSha512(string secretKey, string payload)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var left = Encoding.UTF8.GetBytes(a);
            var right = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(left, right);
        }
    }
}
