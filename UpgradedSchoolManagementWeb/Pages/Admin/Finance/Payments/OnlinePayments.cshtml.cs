using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UpgradedSchoolManagementWeb.Pages.Admin.Finance.Payments
{
    [Authorize(Policy = "Finance.View")]
    public class OnlinePaymentsModel : PageModel
    {
        public bool CanApprove { get; private set; }

        public async Task OnGetAsync([FromServices] IAuthorizationService authorizationService)
        {
            CanApprove = (await authorizationService.AuthorizeAsync(User, "Finance.PaymentApprove")).Succeeded;
        }
    }
}
