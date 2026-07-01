using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Fees
{
    [Authorize(Policy = "Finance.InvoiceCreate")]
    public class UpsertModel : PageModel
    {
        private readonly IPaymentCategoryService _categoryService;

        public UpsertModel(IPaymentCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public PaymentCategoryViewModel? Category { get; private set; }
        public bool IsEdit => Category != null;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                Category = await _categoryService.GetPaymentCategoryByIdAsync(id.Value);
                if (Category == null)
                    return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
