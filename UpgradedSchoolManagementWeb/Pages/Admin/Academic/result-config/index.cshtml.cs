using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.admin.Academic.result_config
{
    [Authorize(Policy = "Settings.Manage")]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionViewModal { get; set; } = new();

        public indexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET — load the page
        // ─────────────────────────────────────────────────────────────────────
        public async Task OnGetAsync()
        {
            await LoadSelectionDataAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST — Save assessment configuration
        // Accepts the modal form via AJAX fetch; returns JSON.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostSaveConfigAsync([FromBody] ResultConfigViewModel model)
        {
            if (model == null)
                return new JsonResult(new { success = false, message = "Invalid request payload." });

            if (model.SchoolClassIds == null || !model.SchoolClassIds.Any())
                return new JsonResult(new { success = false, message = "Please select at least one class." });

            if (model.Assessments == null || !model.Assessments.Any())
                return new JsonResult(new { success = false, message = "Please add at least one assessment." });

            // Warn if scores don't add to 100 (non-blocking — the service still saves)
            var total = model.Assessments.Sum(a => a.Score);
            var warning = total != 100
                ? $"Note: Assessment scores total {total}, not 100."
                : null;

            var result = await _unitOfWork.ClassService.SaveAssessmentConfigs(
                model.SchoolClassIds,
                model.Assessments);

            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "SAVE",
                    module: "ResultConfig",
                    description: $"Assessment config saved for class(es): {string.Join(",", model.SchoolClassIds)}",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }

            return new JsonResult(new
            {
                success = result.Success,
                message = result.Success
                    ? result.Message + (warning != null ? $" ({warning})" : "")
                    : result.Message
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST handler — DataTables server-side source for the configs grid
        // Called via AJAX POST: ?handler=DataTable&classId=0
        // Body: DataTables JSON payload (draw, start, length, search, columns, order)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostDataTableAsync(
            [FromBody] DataTablesRequest request,
            [FromQuery] int classId = 0)
        {
            if (request == null)
                return new JsonResult(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>() });

            var result = await _unitOfWork.ClassService
                .GetClassAssessmentConfigs(request, classId);

            return new JsonResult(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET handler — return existing configs for a class (pre-populate modal)
        // Called via AJAX: ?handler=ConfigsByClass&classId=3
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnGetConfigsByClassAsync(int classId)
        {
            if (classId <= 0)
                return new JsonResult(new { success = false, message = "Invalid class ID." });

            var configs = await _unitOfWork.ClassService
                .GetAssessmentConfigsByClassId(classId);

            return new JsonResult(new { success = true, data = configs });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST handler — Delete a single assessment config row
        // Called via AJAX fetch with body: { id: 5 }
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostDeleteConfigAsync([FromBody] DeleteConfigRequest req)
        {
            if (req == null || req.Id <= 0)
                return new JsonResult(new { success = false, message = "Invalid ID." });

            var result = await _unitOfWork.ClassService.DeleteAssessmentConfig(req.Id);
            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "DELETE",
                    module: "ResultConfig",
                    description: $"Assessment config {req.Id} deleted",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST handler — Delete ALL configs for a class
        // Called via AJAX fetch with body: { id: 3 }  (classId)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostDeleteAllConfigsAsync([FromBody] DeleteConfigRequest req)
        {
            if (req == null || req.Id <= 0)
                return new JsonResult(new { success = false, message = "Invalid class ID." });

            var result = await _unitOfWork.ClassService.DeleteAllAssessmentConfigsByClassId(req.Id);
            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "DELETE",
                    module: "ResultConfig",
                    description: $"All assessment configs deleted for class {req.Id}",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }
            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST handler — Update a single assessment config row
        // Called via AJAX fetch with body: { id, name, score, displayOrder }
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateSingleConfigAsync(
            [FromBody] UpdateSingleConfigRequest req)
        {
            if (req == null || req.Id <= 0)
                return new JsonResult(new { success = false, message = "Invalid request." });

            if (string.IsNullOrWhiteSpace(req.Name))
                return new JsonResult(new { success = false, message = "Assessment name is required." });

            if (req.Score <= 0)
                return new JsonResult(new { success = false, message = "Max score must be greater than 0." });

            if (req.DisplayOrder <= 0)
                return new JsonResult(new { success = false, message = "Display order must be greater than 0." });

            var result = await _unitOfWork.ClassService.UpdateSingleAssessmentConfig(req.Id, req.Name, req.Score, req.DisplayOrder);

            if (result.Success)
            {
                await _unitOfWork.AuditLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "UPDATE",
                    module: "ResultConfig",
                    description: $"Assessment config {req.Id} updated: {req.Name} (Max: {req.Score})",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString()
                );
            }

            return new JsonResult(new { success = result.Success, message = result.Message });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private async Task LoadSelectionDataAsync()
        {
            SelectionViewModal = new SelectionViewModal
            {
                SchoolClasses = await _unitOfWork.ViewSelectionService
                    .GetSchoolClassesForDropdownAsync()
            };
        }
    }

    /// <summary>Simple request body DTO used by delete handlers.</summary>
    public class DeleteConfigRequest
    {
        public int Id { get; set; }
    }

    /// <summary>Request body DTO for updating a single assessment config row.</summary>
    public class UpdateSingleConfigRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
        public int DisplayOrder { get; set; }
    }
}
