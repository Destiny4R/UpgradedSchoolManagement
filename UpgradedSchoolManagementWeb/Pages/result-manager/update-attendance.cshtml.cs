using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    public class update_attendanceModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public List<AttendanceViewModel> attendances { get; set; } = new List<AttendanceViewModel>();

        public int ToTalAttendance { get; set; }
        public update_attendanceModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appsettings = _unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId).Result;
            if (appsettings == null)
            {
                TempData["Error"] = "You are yet to setup the class on the setting page, please do before proceeding";
                return RedirectToPage("index");
            }
            var resultModel = new TermObjects
            {
                SessionId = appsettings.SesseionTable.Id,
                Term = appsettings.Term.Value,
                schoolClassId = appsettings.SchoolClasses.Id,
                SubclassId = appsettings.SubClassTable.Id
            };
            var generalinfo = _unitOfWork.TermGeneralInformationServices.GetBySessionAndTerm(appsettings.SesseionTable.Id, (int)appsettings.Term.Value).Result;
            if (generalinfo == null)
            {
                TempData["Error"] = "School general information is not available for the selected setting for session and term.";
                return RedirectToPage("index");
            }
            ToTalAttendance = generalinfo.DaySchoolOpen;

            attendances = _unitOfWork.TermRegistrationServices.GetAllStudentAttendanceTermRegistrationsAsync(resultModel).Result;
            return Page();
        }

        public IActionResult OnPost(List<AttendanceViewModel> attendances)
        {
            if (attendances == null || attendances.Count == 0)
            {
                TempData["Error"] = "No attendance data to update.";
                return RedirectToPage("index");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appsettings = _unitOfWork.AppSettingsServices.GetAppSettingsByUserIdAsync(userId).Result;
            if (appsettings == null)
            {
                TempData["Error"] = "You are yet to setup the class on the setting page, please do before proceeding";
                return RedirectToPage("index");
            }
            var resultModel = new TermObjects
            {
                SessionId = appsettings.SesseionTable.Id,
                Term = appsettings.Term.Value,
                schoolClassId = appsettings.SchoolClasses.Id,
                SubclassId = appsettings.SubClassTable.Id
            };
            var updateResult = _unitOfWork.TermRegistrationServices.UpdateStudentAttendanceAsync(attendances, resultModel).Result;
            if (updateResult.Success)
            {
                TempData["Success"] = updateResult.Message;
            }
            else
            {
                TempData["Error"] = updateResult.Message;
                return RedirectToPage();
            }
            return RedirectToPage("index");
        }
}
}

