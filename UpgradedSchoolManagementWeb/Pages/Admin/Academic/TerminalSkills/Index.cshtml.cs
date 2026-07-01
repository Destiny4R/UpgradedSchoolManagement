using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Academic.TerminalSkills
{
    [Authorize(Policy = "Settings.Manage")]
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
            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "CREATE",
                    module: "TerminalSkills",
                    description: $"Skill '{model.Name}' created",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }
            return new JsonResult(new { success = result.Success, message = result.Message, data = result.Data });
        }

        public async Task<IActionResult> OnPostUpdateSkillAsync([FromBody] UpdateResultSkillDto model)
        {
            var result = await _unitOfWork.ResultSkillServices.UpdateSkillAsync(model);
            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "UPDATE",
                    module: "TerminalSkills",
                    description: $"Skill '{model.Name}' updated",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostToggleSkillAsync([FromBody] ToggleSkillRequest model)
        {
            if (model == null || model.Id <= 0)
                return new JsonResult(new { success = false, message = "Invalid skill ID." });

            var result = await _unitOfWork.ResultSkillServices.ToggleSkillStatusAsync(model.Id);
            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "TOGGLE",
                    module: "TerminalSkills",
                    description: $"Skill ID {model.Id} toggled",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> OnPostAssignSkillsToClassAsync([FromBody] AssignSkillsToClassDto model)
        {
            if (model == null || model.SchoolClassIds == null || model.SchoolClassIds.Count == 0)
                return new JsonResult(new { success = false, message = "Invalid request. Select at least one class." });

            var allSucceeded = true;
            var messages = new List<string>();

            foreach (var classId in model.SchoolClassIds)
            {
                var result = await _unitOfWork.ResultSkillServices.AssignSkillsToClassAsync(
                    classId,
                    model.ResultSkillIds);

                if (!result.Success)
                {
                    allSucceeded = false;
                    messages.Add(result.Message);
                }
            }

            if (allSucceeded)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "ASSIGN",
                    module: "TerminalSkills",
                    description: $"Skills assigned to {model.SchoolClassIds.Count} class(es) (IDs: {string.Join(",", model.SchoolClassIds)})",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }

            var message = allSucceeded
                ? "Skills assigned successfully to all selected classes."
                : $"Some assignments failed: {string.Join("; ", messages)}";

            return new JsonResult(new { success = allSucceeded, message });
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
