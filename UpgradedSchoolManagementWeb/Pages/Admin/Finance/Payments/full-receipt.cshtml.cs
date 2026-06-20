using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Payments
{
    [Authorize]
    public class full_receiptModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public FullTermReceiptViewModel Receipt { get; set; }

        public full_receiptModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int? termRegId)
        {
            if (!termRegId.HasValue || termRegId.Value <= 0)
            {
                return RedirectToPage("/Admin/Finance/Payments/Index");
            }

            Receipt = await _unitOfWork.StudentPaymentService.GetFullTermReceiptAsync(termRegId.Value);
            if (Receipt == null)
            {
                return RedirectToPage("/Admin/Finance/Payments/Index");
            }

            return Page();
        }
    }
}
