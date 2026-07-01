using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    [Authorize(Policy = "Result.Edit")]
    public class manage_informationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionView { get; set; }

        public manage_informationModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
            LoadDropdowns();
        }

        private void LoadDropdowns()
        {
            SelectionView = new()
            {
                AcademicSession = _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result,
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown(),
                SchoolClasses = _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result,
                SubClass = _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result
            };
        }
    }
}
