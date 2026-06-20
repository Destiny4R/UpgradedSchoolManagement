using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementModels.DTOs;

namespace UpgradedSchoolManagementDataAccess.IServices
{
    public interface IViewSelectionService
    {
        Task<IEnumerable<SelectListItem>> GetSchoolClassesForDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetSchoolSubclassesForDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetSessionsForDropdownAsync();
        Task<IEnumerable<SelectListItem>> GetSubjectsForDropdownAsync();
        IEnumerable<SelectListItem> GetTermForDropdown();
        Task<List<AssessmentConfigDto>> GetAssessmentConfigsByClassAsync(int schoolClassId);
    }
}
