using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Payments
{
    [Authorize(Policy = "Finance.View")]
    public class ReceiptModel : PageModel
    {
        public void OnGet() { }
    }
}
