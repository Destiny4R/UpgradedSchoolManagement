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

    }
}
