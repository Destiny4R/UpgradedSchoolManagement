using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IAppSettingsService
    {
        Task<AppSettings> GetAppSettingsByUserIdAsync(string userId);
        Task UpsertAppSettingsAsync(AppSettings appSettings);
        /// <summary>Returns the settings row flagged as admin (holds Paystack configuration).</summary>
        Task<AppSettings?> GetAdminAppSettingsAsync();
        /// <summary>Returns the global Paystack online-payment settings (null/disabled when not configured).</summary>
        Task<(bool Enabled, string? SecretKey, string? PublicKey)> GetPaystackSettingsAsync();

    }
}
