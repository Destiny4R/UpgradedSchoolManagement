using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class ViewSelectionService : IViewSelectionService
    {
        private readonly ApplicationDbContext _db;

        public ViewSelectionService(ApplicationDbContext db)
        {
            this._db = db;
        }
        public async Task<IEnumerable<SelectListItem>> GetSchoolClassesForDropdownAsync()
        {
            var classes = await _db.SchoolClasses.OrderBy(k => k.Name).ToListAsync();
            return classes.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
        }

        public async Task<IEnumerable<SelectListItem>> GetSchoolSubclassesForDropdownAsync()
        {
            var subclasses = await _db.SubClassTables.OrderBy(k => k.Name).ToListAsync();
            return subclasses.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
        }

        public async Task<IEnumerable<SelectListItem>> GetSessionsForDropdownAsync()
        {
            var sessions = await _db.SesseionTables.OrderByDescending(s => s.CreatedDate).ToListAsync();
            return sessions.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
        }
        public async Task<IEnumerable<SelectListItem>> GetSubjectsForDropdownAsync()
        {
            try
            {
                var sessions = await _db.SubjectTables.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                return sessions.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                });
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }
        // Since terms are defined as an enum, we can directly convert them to SelectListItem without a database call
        public IEnumerable<SelectListItem> GetTermForDropdown()
        {
            var  terms = Enum.GetValues<ConstantEnums.Term>()
                .Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
            return terms;
        }

        public async Task<List<AssessmentConfigDto>> GetAssessmentConfigsByClassAsync(int schoolClassId)
        {
            var configs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == schoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            return configs.Select(ac => new AssessmentConfigDto
            {
                Id = ac.Id,
                AssessmentName = ac.AssessmentName,
                AssessmentScore = ac.AssessmentScore,
                DisplayOrder = ac.DisplayOrder,
                SchoolClassId = ac.SchoolClassId,
                CreatedDate = ac.CreatedDate,
                UpdatedDate = ac.UpdatedDate
            }).ToList();
        }
    }
}
