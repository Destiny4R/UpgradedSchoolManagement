using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.students_reg
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork unitOfWork;

        public SelectionViewModal SelectionViewModal { get; set; }
        public indexModel(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            LoadSelectionData();
        }
        public void OnGet()
        {
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
