using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.ViewModels;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Setup
{
    public class UpsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionViewModal SelectionView { get; set; }
        public UpsertModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            LoadDropdownsAsync().GetAwaiter().GetResult();
        }

        // ── Bound form model ────────────────────────────────────────
        [BindProperty]
        public PaymentSetupViewModel Input { get; set; } = new();

        // ── Read-only display data ───────────────────────────────────
        public PaymentSetupViewModel? Setup { get; private set; }
        public List<PaymentItemViewModel> Items { get; private set; } = new();

        public bool IsEdit => Setup != null;

        // ── GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> OnGetAsync(int? id)
        {

            if (id.HasValue && id.Value > 0)
            {
                Setup = await _unitOfWork.PaymentSetupService.GetPaymentSetupByIdAsync(id.Value);
                if (Setup == null)
                {
                    TempData["Error"] = "Setup item not found, try again";
                    return RedirectToPage("Index");
                }
                    

                // Pre-populate the bound model for edit
                Input = new PaymentSetupViewModel
                {
                    Id          = Setup.Id,
                    PaymentItemId = Setup.PaymentItemId,
                    SessionId   = Setup.SessionId,
                    Term        = Setup.Term,
                    ClassId     = Setup.ClassId,
                    Amount      = Setup.Amount,
                    IsCompulsory = Setup.IsCompulsory,
                    IsActive    = Setup.IsActive
                };
            }

            return Page();
        }

        // ── POST: Create (batch) ─────────────────────────────────────
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _unitOfWork.PaymentSetupService.CreateBatchPaymentSetupAsync(Input);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToPage("Index");
            }

            TempData["Error"] = result.Message;
            return Page();
        }

        // ── POST: Update ─────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                await ReloadForEditAsync();
                return Page();
            }

            var result = await _unitOfWork.PaymentSetupService.UpdatePaymentSetupAsync(Input);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToPage("Index");
            }

            TempData["Error"] = result.Message;
            await ReloadForEditAsync();
            return Page();
        }

        // ── Helpers ──────────────────────────────────────────────────
        private async Task LoadDropdownsAsync()
        {
            SelectionView = new SelectionViewModal
            {
                AcademicSession = await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync(),
                SchoolClasses = await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync(),
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown()
            };
            Items = await _unitOfWork.PaymentItemService.GetActiveItemsAsync();
        }

        private async Task ReloadForEditAsync()
        {
            if (Input.Id > 0)
                Setup = await _unitOfWork.PaymentSetupService.GetPaymentSetupByIdAsync(Input.Id);
        }
    }
}
