using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly ApplicationDbContext _db;

        public AppSettingsService(ApplicationDbContext db)
        {
            this._db = db;
        }
        public async Task<AppSettings> GetAppSettingsByUserIdAsync(string userId)
        {
            return await _db.Appsettings.Include(n=>n.SesseionTable).Include(n => n.SchoolClasses).Include(n => n.SubClassTable).FirstOrDefaultAsync(x => x.ApplicationUserId == userId);
        }

        public async Task UpsertAppSettingsAsync(AppSettings appSettings)
        {
            if (appSettings.IsAdmin)
            {
                var currentAdmin = await _db.Appsettings
                    .FirstOrDefaultAsync(x => x.IsAdmin && x.Id != appSettings.Id);
                if (currentAdmin != null)
                {
                    currentAdmin.IsAdmin = false;
                }
            }

            var entry = _db.Entry(appSettings);
            if (entry.State == EntityState.Detached)
            {
                appSettings.CreatedDate = DateTime.UtcNow;
                appSettings.UpdatedDate = DateTime.UtcNow;
                _db.Appsettings.Add(appSettings);
            }
            else
            {
                appSettings.UpdatedDate = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }
}
