using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    [Authorize(Policy = "Result.Edit")]
    public class assessmentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public assessmentModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SelectionViewModal SelectionView { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int TermValue { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SchoolClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SubClassId { get; set; }

        public string SessionName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubClassName { get; set; } = string.Empty;

        public List<AssessmentSheetDto> AssessmentSheet { get; set; } = new();
        public List<AssessmentConfigDto> AssessmentConfigs { get; set; } = new();
        public bool HasData => AssessmentSheet.Count > 0;

        public void OnGet()
        {
            LoadDropdowns();

            if (SessionId > 0 && TermValue > 0 && SchoolClassId > 0 && SubClassId > 0)
            {
                LoadSelectedNames();

                var termObj = new TermObjects
                {
                    SessionId = SessionId,
                    Term = (Term)TermValue,
                    schoolClassId = SchoolClassId,
                    SubclassId = SubClassId
                };

                AssessmentSheet = _unitOfWork.ResultManagerServices.GetAssessmentSheetAsync(termObj).Result;
                AssessmentConfigs = _unitOfWork.ViewSelectionService.GetAssessmentConfigsByClassAsync(SchoolClassId).Result;
            }
        }

        private void LoadSelectedNames()
        {
            var session = _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync().Result
                .FirstOrDefault(s => s.Value == SessionId.ToString());
            SessionName = session?.Text ?? SessionId.ToString();

            var term = _unitOfWork.ViewSelectionService.GetTermForDropdown()
                .FirstOrDefault(t => t.Value == TermValue.ToString());
            TermName = term?.Text ?? TermValue.ToString();

            var cls = _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync().Result
                .FirstOrDefault(c => c.Value == SchoolClassId.ToString());
            ClassName = cls?.Text ?? SchoolClassId.ToString();

            var sub = _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync().Result
                .FirstOrDefault(s => s.Value == SubClassId.ToString());
            SubClassName = sub?.Text ?? SubClassId.ToString();
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
