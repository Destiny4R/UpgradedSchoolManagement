using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    [Authorize(Policy = "Result.Edit")]
    public class edit_resultModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public edit_resultModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public EditResultDto EditResult { get; set; } = new();

        [BindProperty]
        public List<SaveSubjectResultDto> Subjects { get; set; } = new();

        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var data = await _unitOfWork.ResultManagerServices.GetEditResultDataAsync(Id);
            if (data == null)
            {
                ErrorMessage = "Term registration not found.";
                return RedirectToPage("index");
            }

            EditResult = data;

            Subjects = data.Subjects.Select(s => new SaveSubjectResultDto
            {
                ResultTableId = s.ResultTableId,
                SubjectId = s.SubjectId,
                ScoreOne = s.ScoreOne,
                ScoreTwo = s.ScoreTwo,
                ScoreThree = s.ScoreThree,
                ScoreFour = s.ScoreFour,
                ScoreFive = s.ScoreFive,
                ScoreSix = s.ScoreSix
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var data = await _unitOfWork.ResultManagerServices.GetEditResultDataAsync(Id);
                if (data != null)
                {
                    EditResult = data;
                    for (int i = 0; i < Subjects.Count && i < data.Subjects.Count; i++)
                    {
                        Subjects[i].ResultTableId = data.Subjects[i].ResultTableId;
                    }
                }
                return Page();
            }

            var saveDto = new SaveResultDto
            {
                TermRegId = Id,
                Subjects = Subjects
            };

            var result = await _unitOfWork.ResultManagerServices.SaveResultsAsync(saveDto);

            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "SAVE",
                    module: "ResultManagement",
                    description: $"Results saved for term registration {Id}",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );

                TempData["Success"]  = result.Message;
                return RedirectToPage("index");
            }

            ErrorMessage = result.Message;

            var reloadData = await _unitOfWork.ResultManagerServices.GetEditResultDataAsync(Id);
            if (reloadData != null) EditResult = reloadData;

            return Page();
        }
    }
}
