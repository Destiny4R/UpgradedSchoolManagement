using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.PermissionConstants;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Payments
{
    [Authorize(Policy = "Finance.PaymentRecord")]
    public class MakeModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionView { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditPaymentId { get; set; }

        public string? EditPaymentDataJson { get; set; }

        public MakeModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnGet()
        {
            SelectionView = new SelectionViewModal
            {
                AcademicSession = await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync(),
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown()
            };

            var items = await _unitOfWork.PaymentItemService.GetActiveItemsWithCategoryAsync();
            SelectionView.PaymentItems = items.Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = i.Name
            });

            if (EditPaymentId.HasValue)
            {
                var detail = await _unitOfWork.StudentPaymentService.GetPaymentDetailAsync(EditPaymentId.Value);
                if (detail != null)
                {
                    EditPaymentDataJson = System.Text.Json.JsonSerializer.Serialize(detail, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    });
                }
            }
        }
    }
}
