using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork unitOfWork;

        public SelectionViewModal SelectionViewModal { get; set; }
        public List<ResultSheetDto> ResultSheet { get; set; } = new List<ResultSheetDto>();

        public string term, session, schoolclass, subclass;
        public indexModel(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            LoadSelectionData();
        }
        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appsettings = unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId).Result;

            term = appsettings.Term.ToString();
            session = appsettings.SesseionTable.Name;
            schoolclass = $"{appsettings.SchoolClasses.Name} - {appsettings.SubClassTable.Name}";
            var resultModel = new TermObjects
            {
                SessionId = appsettings.SesseionTable.Id,
                Term = appsettings.Term.Value,
                schoolClassId = appsettings.SchoolClasses.Id,
                SubclassId = appsettings.SubClassTable.Id
            };

            ResultSheet = unitOfWork.ResultManagerServices.GetResultSheetAsync(resultModel).Result;
        }
        public void LoadSelectionData()
        {
            // Implementation for loading selection data
            SelectionViewModal = new SelectionViewModal
            {
                SchoolClasses = unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result,
                SubClass = unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result,
                AcademicSession = unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result,
                Terms = unitOfWork.ViewSelectionService.GetTermForDropdown(),
                Subjects = unitOfWork.ViewSelectionService.GetSubjectsForDropdownAsync().Result
            };
        }
    }
}
