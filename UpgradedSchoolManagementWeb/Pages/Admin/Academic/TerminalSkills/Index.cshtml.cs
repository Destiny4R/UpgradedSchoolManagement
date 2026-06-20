using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Academic.TerminalSkills
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SelectionViewModal SelectionViewModal { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadSelectionDataAsync();
        }

        public async Task<IActionResult> OnPostSaveSkillAsync([FromBody] CreateResultSkillDto model)
        {
            var result = await _unitOfWork.ResultSkillServices.CreateSkillAsync(model);
            return new JsonResult(new { success = result.Success, message = result.Message, data = result.Data });
        }

        public async Task<IActionResult> OnPostUpdateSkillAsync([FromBody] UpdateResultSkillDto model)
        {
            var result = await _unitOfWork.ResultSkillServices.UpdateSkillAsync(model);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostToggleSkillAsync([FromBody] ToggleSkillRequest model)
        {
            if (model == null || model.Id <= 0)
                return new JsonResult(new { success = false, message = "Invalid skill ID." });

            var result = await _unitOfWork.ResultSkillServices.ToggleSkillStatusAsync(model.Id);
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostAssignSkillsToClassAsync([FromBody] AssignSkillsToClassDto model)
        {
            if (model == null)
                return new JsonResult(new { success = false, message = "Invalid request." });

            var result = await _unitOfWork.ResultSkillServices.AssignSkillsToClassAsync(
                model.SchoolClassId,
                model.ResultSkillIds);

            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostSkillsDataTableAsync([FromBody] DataTablesRequest request)
        {
            if (request == null)
                return new JsonResult(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

            var result = await _unitOfWork.ResultSkillServices.GetSkillsForDataTableAsync(request);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostAssignedSkillsDataTableAsync(
            [FromBody] DataTablesRequest request,
            [FromQuery] int classId = 0)
        {
            if (request == null)
                return new JsonResult(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

            var result = await _unitOfWork.ResultSkillServices.GetAssignedSkillsForDataTableAsync(request, classId);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetSkillsByClassAsync(int classId)
        {
            if (classId <= 0)
                return new JsonResult(new { success = false, message = "Invalid class ID." });

            var skills = await _unitOfWork.ResultSkillServices.GetAssignedSkillsByClassIdAsync(classId);
            return new JsonResult(new { success = true, data = skills });
        }

        public async Task<IActionResult> OnGetActiveSkillsAsync()
        {
            var skills = await _unitOfWork.ResultSkillServices.GetActiveSkillsAsync();
            return new JsonResult(new { success = true, data = skills });
        }

        private async Task LoadSelectionDataAsync()
        {
            SelectionViewModal = new SelectionViewModal
            {
                SchoolClasses = await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync()
            };
        }
    }

    public class ToggleSkillRequest
    {
        public int Id { get; set; }
    }
}
