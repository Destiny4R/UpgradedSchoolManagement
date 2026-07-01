using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Items
{
    [Authorize(Policy = "Finance.InvoiceCreate")]
    public class UpsertModel : PageModel
    {
        private readonly IPaymentItemService _itemService;
        private readonly IPaymentCategoryService _categoryService;

        public UpsertModel(IPaymentItemService itemService, IPaymentCategoryService categoryService)
        {
            _itemService = itemService;
            _categoryService = categoryService;
        }

        public PaymentItemViewModel? Item { get; private set; }
        public List<PaymentCategoryViewModel> Categories { get; private set; } = new();
        public bool IsEdit => Item != null;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Categories = await _categoryService.GetActiveCategoriesAsync();

            if (id.HasValue && id.Value > 0)
            {
                Item = await _itemService.GetPaymentItemByIdAsync(id.Value);
                if (Item == null)
                    return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
