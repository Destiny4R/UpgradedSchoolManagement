using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.students_reg
{
    public class batch_registrationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionViewModal { get; set; }

        public batch_registrationModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            LoadSelectionData();
        }
        public void OnGet()
        {
        }
        public void LoadSelectionData()
        {
            SelectionViewModal = new SelectionViewModal
            {
                SchoolClasses = _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result,
                SubClass = _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result,
                AcademicSession = _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result,
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown()
                // We don't need Subjects dropdown for the target, as they are inherited from previous reg!
            };
        }
    }
}
