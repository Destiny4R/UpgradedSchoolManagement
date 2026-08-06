using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementUltitlities;
using UpgradedSchoolManagementWeb.Services;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.annual_result
{
    public abstract class AnnualReportPageBaseModel : PageModel
    {
        private readonly AnnualReportService _annualReportService;
        private readonly ApplicationDbContext _db;

        protected AnnualReportPageBaseModel(AnnualReportService annualReportService, ApplicationDbContext db)
        {
            _annualReportService = annualReportService;
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public int StudentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SubClassId { get; set; }

        public AnnualReportViewModel Report { get; set; } = new();

        public bool IsAllowed { get; set; } = true;

        public string AccessMessage { get; set; } = string.Empty;

        public List<OutstandingPaymentDto> OutstandingPayments { get; set; } = new();

        public decimal TotalOutstanding { get; set; }

        public async Task OnGet()
        {
            if (StudentId > 0 && SessionId > 0 && ClassId > 0 && SubClassId > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var student = await _db.StudentsTables
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                    if (student != null && StudentId != student.Id)
                    {
                        IsAllowed = false;
                        AccessMessage = "You are not authorized to view this report.";
                        return;
                    }
                }

                var access = await ValidateReportAccessAsync();
                if (!access.Success)
                {
                    IsAllowed = false;
                    AccessMessage = access.Message;
                    OutstandingPayments = access.OutstandingPayments;
                    TotalOutstanding = access.TotalOutstanding;
                    return;
                }

                Report = await _annualReportService.BuildAsync(StudentId, SessionId, ClassId, SubClassId);
            }
        }

        private async Task<AccessValidationResult> ValidateReportAccessAsync()
        {
            var adminSettings = await _db.Appsettings
                .Where(a => a.IsAdmin)
                .OrderByDescending(a => a.UpdatedDate)
                .FirstOrDefaultAsync();

            if (adminSettings?.CanPrintResult == true)
                return AccessValidationResult.Allow();

            var termRegs = await _db.TermRegistrations
                .Where(tr => tr.StudentId == StudentId && tr.SessionId == SessionId)
                .Select(tr => new { tr.Id, tr.Term })
                .ToListAsync();

            var enrolledTerms = termRegs.Select(tr => tr.Term).Distinct().ToList();

            var compulsorySetups = await _db.PaymentSetups
                .Include(ps => ps.PaymentItem)
                .Where(ps => ps.SessionId == SessionId
                    && ps.SchoolClassId == ClassId
                    && enrolledTerms.Contains(ps.Term)
                    && ps.IsCompulsory
                    && ps.IsActive
                    && ps.PaymentItem.IsActive)
                .ToListAsync();

            if (!compulsorySetups.Any())
                return AccessValidationResult.Allow();

            var termRegIds = termRegs.Select(tr => tr.Id).ToList();

            var paidAmounts = await _db.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                    && spi.StudentPayment.State == PaymentState.Approved
                    && spi.StudentPayment.Status != PaymentStatus.Reversed
                    && spi.StudentPayment.Status != PaymentStatus.Failed)
                .GroupBy(spi => spi.PaymentItemId)
                .Select(g => new { PaymentItemId = g.Key, AmountPaid = g.Sum(x => x.AmountPaid) })
                .ToDictionaryAsync(x => x.PaymentItemId, x => x.AmountPaid);

            var outstanding = new List<OutstandingPaymentDto>();
            foreach (var setup in compulsorySetups)
            {
                var paid = paidAmounts.TryGetValue(setup.PaymentItemId, out var amountPaid) ? amountPaid : 0m;
                var balance = setup.Amount - paid;
                if (balance > 0)
                {
                    outstanding.Add(new OutstandingPaymentDto
                    {
                        PaymentItemName = setup.PaymentItem?.Name ?? $"Payment item {setup.PaymentItemId}",
                        ExpectedAmount = setup.Amount,
                        AmountPaid = paid,
                        Balance = balance
                    });
                }
            }

            if (!outstanding.Any())
                return AccessValidationResult.Allow();

            var total = outstanding.Sum(x => x.Balance);
            var itemLines = string.Join("; ", outstanding.Select(x => $"{x.PaymentItemName}: {SD.ToNaira(x.Balance)} outstanding"));
            var message = $"Report access is restricted because compulsory payment items are outstanding: {itemLines}. Total outstanding: {total:0.00}. Please complete payment before viewing or printing this report.";

            return AccessValidationResult.Deny(message, outstanding, total);
        }

        private class AccessValidationResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public List<OutstandingPaymentDto> OutstandingPayments { get; init; } = new();
            public decimal TotalOutstanding { get; init; }

            public static AccessValidationResult Allow()
            {
                return new AccessValidationResult { Success = true };
            }

            public static AccessValidationResult Deny(string message, List<OutstandingPaymentDto> outstandingPayments, decimal totalOutstanding)
            {
                return new AccessValidationResult
                {
                    Success = false,
                    Message = message,
                    OutstandingPayments = outstandingPayments,
                    TotalOutstanding = totalOutstanding
                };
            }
        }
    }
}
