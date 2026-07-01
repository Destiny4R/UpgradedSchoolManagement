using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;

namespace UpgradedSchoolManagementWeb.Pages.Admin.AcademicStructure
{
    [Authorize(Policy = "AcademicStructure.View")]
    public class IndexModel : PageModel
    {
        private readonly ISessionService _sessionService;
        private readonly IClassService _classService;
        private readonly ISubClassService _subClassService;
        private readonly ISubjectService _subjectService;

        public IndexModel(
            ISessionService sessionService,
            IClassService classService,
            ISubClassService subClassService,
            ISubjectService subjectService)
        {
            _sessionService = sessionService;
            _classService = classService;
            _subClassService = subClassService;
            _subjectService = subjectService;
        }

        public void OnGet()
        {
        }
    }
}