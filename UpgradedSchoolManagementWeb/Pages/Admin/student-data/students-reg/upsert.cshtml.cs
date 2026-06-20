using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementDataAccess.Services;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.admin.student_data.students_reg
{
    public class upsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionViewModal { get; set; }
        [BindProperty]
        public TermRegistrationViewModel termReg { get; set; } = new();
        public upsertModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            LoadSelectionData();
        }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                var existingReg = await _unitOfWork.TermRegistrationServices.GetStudentTermRegistrationByIdAsync(id.Value);
                if (existingReg != null)
                {
                    termReg = existingReg;
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync(string termregnumber)
        {
            if (string.IsNullOrEmpty(termregnumber))
            {
                TempData["Error"] = "Provide student registration number before proceeding";
                return Page();
            }
            var result = _unitOfWork.StudentService.FindStudentByAdmissionNumberAsync(termregnumber).Result;
            if (result == null)
            {
                TempData["Error"] = "Student not found, please check and try again";
                return Page();
            }
            termReg = new TermRegistrationViewModel
            {
                StudentId = result.Id,
                //Id = result.Id,
                StudentRegNumber = result.AdmissionNumber,
                StudentName = $"{result.FullName}"
            };
            TempData["Success"] = "Student found successfully";
            return Page();
        }

        //Register
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Provide select all areas";
                return Page();
            }

            if (termReg.Id > 0)
            {
                // Block update if payments exist
                //var hasPayment = await _unitOfWork.TermRegistrationServices.HasPaymentForTermRegistrationAsync((int)termReg.Id);
                //if (hasPayment)
                //{
                //    TempData["Error"] = "This registration cannot be edited because payment records exist for it.";
                //    return RedirectToPage("index");
                //}

                var result = _unitOfWork.TermRegistrationServices.UpdateStudentTermRegistrationAsync(termReg).Result;
                if (result.Success)
                {
                    //    await _unitOfWork.LogService.LogUserActionAsync(
                    //        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    //        userName: User.Identity?.Name,
                    //        action: "Update",
                    //        entityType: "TermRegistration",
                    //        entityId: termReg.Id.ToString(),
                    //        message: $"Term registration updated for student ID {termReg.StudentId}",
                    //        ipAddress: GetClientIpAddress(),
                    //        details: $"Class: {termReg.SchoolClassId}, Term: {termReg.Term}"
                    //    );

                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                TempData["Error"] = result.Message;
                return Page();
            }
            else
            {
                var result = _unitOfWork.TermRegistrationServices.CreateStudentTermRegistrationAsync(termReg).Result;
                if (result.Success)
                {
                    //    await _unitOfWork.LogService.LogUserActionAsync(
                    //        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    //        userName: User.Identity?.Name,
                    //        action: "Create",
                    //        entityType: "TermRegistration",
                    //        entityId: result.Data.ToString(),
                    //        message: $"Term registration created for student ID {termReg.StudentRegNumber}: {DateTime.UtcNow}",
                    //        ipAddress: GetClientIpAddress(),
                    //        details: $"Class: {termReg.SchoolClassId}, Session: {termReg.SessionId}, Term: {termReg.Term}"
                    //    );
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                    TempData["Error"] = result.Message;
                    return Page();
            }
        }
        public void LoadSelectionData()
        {
            SelectionViewModal = new SelectionViewModal
            {
                SchoolClasses = _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result,
                SubClass = _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result,
                AcademicSession = _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result,
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown(),
                Subjects = _unitOfWork.ViewSelectionService.GetSubjectsForDropdownAsync().Result
            };
        }
    }
}
