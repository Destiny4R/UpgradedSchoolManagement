using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class StudentPaymentService : IStudentPaymentService
    {
        private readonly ApplicationDbContext _context;

        public StudentPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetTermName(Term term)
        {
            return term switch
            {
                Term.First => "First Term",
                Term.Second => "Second Term",
                Term.Third => "Third Term",
                _ => "Unknown"
            };
        }

        public async Task<MakePaymentPageViewModel> GetPayableItemsAsync(int termRegistrationId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(t => t.StudentsTable).ThenInclude(s => s.ApplicationUser)
                    .Include(t => t.SchoolClasses)
                    .Include(t => t.SesseionTable)
                    .FirstOrDefaultAsync(t => t.Id == termRegistrationId);

                if (termReg == null) return null;

                // Get payment setups matching this term registration's class, session, and term
                var paymentSetups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem)
                        .ThenInclude(pi => pi.PaymentCategory)
                    .Where(ps => ps.SessionId == termReg.SessionId
                              && ps.Term == termReg.Term
                              && ps.SchoolClassId == termReg.SchoolClassId
                              && ps.IsActive)
                    .Where(ps => ps.PaymentItem.IsActive && ps.PaymentItem.PaymentCategory.IsActive)
                    .ToListAsync();

                // Get already paid amounts for this student in this term registration
                var alreadyPaidAmounts = await _context.StudentPaymentItems
                    .Where(spi => spi.StudentPayment.TermRegId == termRegistrationId
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed
                               && spi.StudentPayment.State != PaymentState.Rejected
                               && spi.StudentPayment.State != PaymentState.Cancelled)
                    .GroupBy(spi => spi.PaymentItemId)
                    .Select(g => new { PaymentItemId = g.Key, TotalPaid = g.Sum(x => x.AmountPaid) })
                    .ToDictionaryAsync(x => x.PaymentItemId, x => x.TotalPaid);

                // Build category groups
                var categoryGroups = paymentSetups
                    .GroupBy(ps => new { ps.PaymentItem.CategoryId, ps.PaymentItem.PaymentCategory.Name })
                    .Select(g => new CategoryGroupViewModel
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        Items = g.Select(ps => new PayableItemViewModel
                        {
                            PaymentItemId = ps.PaymentItemId,
                            ItemName = ps.PaymentItem.Name,
                            CategoryName = g.Key.Name,
                            CategoryId = g.Key.CategoryId,
                            ExpectedAmount = ps.Amount,
                            IsCompulsory = ps.IsCompulsory,
                            AlreadyPaid = alreadyPaidAmounts.ContainsKey(ps.PaymentItemId)
                                ? alreadyPaidAmounts[ps.PaymentItemId] : 0
                        }).ToList()
                    })
                    .OrderBy(g => g.CategoryName)
                    .ToList();

                return new MakePaymentPageViewModel
                {
                    TermRegistrationId = termRegistrationId,
                    StudentId = termReg.StudentId,
                    StudentName = termReg.StudentsTable?.FullName ?? "Unknown",
                    AdmissionNo = termReg.StudentsTable?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = termReg.SchoolClasses?.Name ?? "Unknown",
                    SessionName = termReg.SesseionTable?.Name ?? "Unknown",
                    TermName = GetTermName(termReg.Term),
                    CategoryGroups = categoryGroups
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payable items: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<MakePaymentPageViewModel>> LookupPayableItemsAsync(string admissionNo, int classId, int categoryId)
        {
            try
            {
                // Get the latest active session as current
                var currentSession = await _context.SesseionTables
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();
                if (currentSession == null)
                    return new ApiResponse<MakePaymentPageViewModel> { Success = false, Message = "No active academic session found. Please activate a session first." };

                int currentSessionId = currentSession.Id;
                string sessionName = currentSession.Name;
                Term currentTerm = Term.First;
                string termName = GetTermName(currentTerm);

                // Verify student exists by admission number
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == admissionNo);
                if (student == null)
                    return new ApiResponse<MakePaymentPageViewModel> { Success = false, Message = "Student with this admission number does not exist." };

                // Check if student is registered for current Session, Term, and selected Class
                var termReg = await _context.TermRegistrations
                    .Include(t => t.SchoolClasses)
                    .Include(t => t.SesseionTable)
                    .FirstOrDefaultAsync(t => t.StudentId == student.Id
                                           && t.SessionId == currentSessionId
                                           && t.Term == currentTerm
                                           && t.SchoolClassId == classId);
                if (termReg == null)
                    return new ApiResponse<MakePaymentPageViewModel> { Success = false,
                        Message = $"Student '{student.FullName}' is not registered for {termName} in {sessionName} for the selected class." };

                // Check if payment setup exists for the selected category in this class/session/term
                var paymentSetups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem)
                        .ThenInclude(pi => pi.PaymentCategory)
                    .Where(ps => ps.SessionId == currentSessionId
                              && ps.Term == currentTerm
                              && ps.SchoolClassId == classId
                              && ps.IsActive
                              && ps.PaymentItem.IsActive
                              && ps.PaymentItem.PaymentCategory.IsActive
                              && ps.PaymentItem.CategoryId == categoryId)
                    .ToListAsync();

                if (!paymentSetups.Any())
                    return new ApiResponse<MakePaymentPageViewModel> { Success = false,
                        Message = "No payment items are configured for the selected category in this class/session/term." };

                // Get already paid amounts for this student in this term registration
                var alreadyPaidAmounts = await _context.StudentPaymentItems
                    .Where(spi => spi.StudentPayment.TermRegId == termReg.Id
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed
                               && spi.StudentPayment.State != PaymentState.Rejected
                               && spi.StudentPayment.State != PaymentState.Cancelled)
                    .GroupBy(spi => spi.PaymentItemId)
                    .Select(g => new { PaymentItemId = g.Key, TotalPaid = g.Sum(x => x.AmountPaid) })
                    .ToDictionaryAsync(x => x.PaymentItemId, x => x.TotalPaid);

                // Build category groups
                var categoryGroups = paymentSetups
                    .GroupBy(ps => new { ps.PaymentItem.CategoryId, ps.PaymentItem.PaymentCategory.Name })
                    .Select(g => new CategoryGroupViewModel
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        Items = g.Select(ps => new PayableItemViewModel
                        {
                            PaymentItemId = ps.PaymentItemId,
                            ItemName = ps.PaymentItem.Name,
                            CategoryName = g.Key.Name,
                            CategoryId = g.Key.CategoryId,
                            ExpectedAmount = ps.Amount,
                            IsCompulsory = ps.IsCompulsory,
                            AlreadyPaid = alreadyPaidAmounts.ContainsKey(ps.PaymentItemId)
                                ? alreadyPaidAmounts[ps.PaymentItemId] : 0
                        }).ToList()
                    })
                    .OrderBy(g => g.CategoryName)
                    .ToList();

                var result = new MakePaymentPageViewModel
                {
                    TermRegistrationId = (int)termReg.Id,
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    AdmissionNo = student.ApplicationUser?.UserName ?? admissionNo,
                    ClassName = termReg.SchoolClasses?.Name ?? "Unknown",
                    SessionName = sessionName,
                    TermName = termName,
                    CategoryGroups = categoryGroups
                };

                return new ApiResponse<MakePaymentPageViewModel> { Success = true, Message = "Student found and payment items loaded.", Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<MakePaymentPageViewModel> { Success = false, Message = $"Error looking up payable items: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<int>> CreatePaymentAsync(CreatePaymentViewModel model, string? evidenceFilePath = null)
        {
            try
            {
                if (model.Items == null || !model.Items.Any())
                    return new ApiResponse<int> { Success = false, Message = "No payment items selected" };

                // Remove items with zero or negative amounts
                model.Items = model.Items.Where(i => i.AmountPaid > 0).ToList();
                if (!model.Items.Any())
                    return new ApiResponse<int> { Success = false, Message = "All selected items have zero amounts" };

                var termReg = await _context.TermRegistrations
                    .FirstOrDefaultAsync(t => t.Id == model.TermRegistrationId);
                if (termReg == null)
                    return new ApiResponse<int> { Success = false, Message = "Term registration not found" };

                // Overpayment prevention
                foreach (var item in model.Items)
                {
                    var expected = await _context.PaymentSetups
                        .Where(ps => ps.PaymentItemId == item.PaymentItemId
                                  && ps.SessionId == termReg.SessionId
                                  && ps.Term == termReg.Term
                                  && ps.SchoolClassId == termReg.SchoolClassId)
                        .Select(ps => ps.Amount)
                        .FirstOrDefaultAsync();

                    if (expected == 0)
                        return new ApiResponse<int> { Success = false, Message = $"Payment item {item.PaymentItemId} is not configured for this class/session/term" };

                    var alreadyPaid = await _context.StudentPaymentItems
                        .Where(spi => spi.PaymentItemId == item.PaymentItemId
                                   && spi.StudentPayment.TermRegId == model.TermRegistrationId
                                   && spi.StudentPayment.Status != PaymentStatus.Reversed
                                   && spi.StudentPayment.Status != PaymentStatus.Failed
                                   && spi.StudentPayment.State != PaymentState.Rejected
                                   && spi.StudentPayment.State != PaymentState.Cancelled)
                        .SumAsync(spi => (decimal?)spi.AmountPaid) ?? 0;

                    if ((alreadyPaid + item.AmountPaid) > expected)
                        return new ApiResponse<int> { Success = false, Message = $"Overpayment detected for item. Expected: {expected}, Already Paid: {alreadyPaid}, Attempting: {item.AmountPaid}" };
                }

                var payment = new StudentPayment
                {
                    TermRegId = model.TermRegistrationId,
                    TotalAmount = model.Items.Sum(i => i.AmountPaid),
                    PaymentDate = DateTime.UtcNow,
                    Reference = SD.GenerateUniqueNumber(),
                    Status = PaymentStatus.Completed,
                    State = PaymentState.Pending,
                    Narration = model.Narration,
                    EvidenceFilePath = evidenceFilePath,
                    PaymentItems = model.Items.Select(i => new StudentPaymentItem
                    {
                        PaymentItemId = i.PaymentItemId,
                        AmountPaid = i.AmountPaid
                    }).ToList()
                };

                _context.StudentPayments.Add(payment);
                await _context.SaveChangesAsync();

                return new ApiResponse<int> { Success = true, Message = "Payment recorded successfully", Data = payment.Id };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = $"Error creating payment: {ex.Message}" };
            }
        }

        public async Task<PaymentReceiptViewModel> GetReceiptAsync(int paymentId)
        {
            try
            {
                var payment = await _context.StudentPayments
                    .Include(p => p.TermRegistration).ThenInclude(t => t.StudentsTable).ThenInclude(s => s.ApplicationUser)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SchoolClasses)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SesseionTable)
                    .Include(p => p.PaymentItems).ThenInclude(pi => pi.PaymentItem).ThenInclude(i => i.PaymentCategory)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null) return null;

                return new PaymentReceiptViewModel
                {
                    PaymentId = payment.Id,
                    TermRegId = payment.TermRegId,
                    SessionId = payment.TermRegistration?.SessionId ?? 0,
                    Term = (int)(payment.TermRegistration?.Term ?? Term.First),
                    Reference = payment.Reference,
                    StudentName = payment.TermRegistration?.StudentsTable?.FullName ?? "Unknown",
                    AdmissionNo = payment.TermRegistration?.StudentsTable?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = payment.TermRegistration?.SchoolClasses?.Name ?? "Unknown",
                    SessionName = payment.TermRegistration?.SesseionTable?.Name ?? "Unknown",
                    TermName = GetTermName(payment.TermRegistration?.Term ?? Term.First),
                    PaymentDate = payment.PaymentDate,
                    TotalAmount = payment.TotalAmount,
                    Status = payment.Status.ToString(),
                    State = payment.State.ToString(),
                    Narration = payment.Narration,
                    RecordedBy = payment.RecordedBy,
                    RejectMessage = payment.RejectMessage,
                    EvidenceFilePath = payment.EvidenceFilePath,
                    LineItems = payment.PaymentItems?.Select(pi => new ReceiptLineItem
                    {
                        PaymentItemId = pi.PaymentItemId,
                        CategoryName = pi.PaymentItem?.PaymentCategory?.Name ?? "Unknown",
                        ItemName = pi.PaymentItem?.Name ?? "Unknown",
                        AmountPaid = pi.AmountPaid
                    }).OrderBy(li => li.CategoryName).ThenBy(li => li.ItemName).ToList() ?? new()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving receipt: {ex.Message}", ex);
            }
        }

        public async Task<ConsolidatedReceiptViewModel> GetConsolidatedReceiptAsync(int termRegId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(t => t.StudentsTable).ThenInclude(s => s.ApplicationUser)
                    .Include(t => t.SchoolClasses)
                    .Include(t => t.SesseionTable)
                    .Include(t => t.SubClassTable)
                    .FirstOrDefaultAsync(t => t.Id == termRegId);

                if (termReg == null) return null;

                var paymentItems = await _context.StudentPaymentItems
                    .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(spi => spi.StudentPayment)
                    .Where(spi => spi.StudentPayment.TermRegId == termRegId
                                && spi.StudentPayment.Status != PaymentStatus.Reversed
                                && spi.StudentPayment.Status != PaymentStatus.Failed
                                && spi.StudentPayment.State != PaymentState.Rejected
                                && spi.StudentPayment.State != PaymentState.Cancelled)
                    .ToListAsync();

                var categories = paymentItems
                    .GroupBy(spi => spi.PaymentItem?.PaymentCategory?.Name ?? "Unknown")
                    .Select(g => new ReceiptCategoryGroup
                    {
                        CategoryName = g.Key,
                        PaymentReferences = g.Select(spi => spi.StudentPayment?.Reference)
                                             .Where(r => !string.IsNullOrEmpty(r))
                                             .Distinct()
                                             .OrderBy(r => r)
                                             .ToList(),
                        Items = g.GroupBy(spi => new { Name = spi.PaymentItem?.Name ?? "Unknown", spi.PaymentItemId })
                                  .Select(ig => new ReceiptLineItem
                                  {
                                      CategoryName = g.Key,
                                      ItemName = ig.Key.Name,
                                      AmountPaid = ig.Sum(x => x.AmountPaid)
                                  })
                                  .OrderBy(i => i.ItemName)
                                  .ToList()
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                return new ConsolidatedReceiptViewModel
                {
                    TermRegistrationId = termRegId,
                    StudentName = termReg.StudentsTable?.FullName ?? "Unknown",
                    AdmissionNo = termReg.StudentsTable?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = termReg.SchoolClasses?.Name ?? "Unknown",
                    SessionName = termReg.SesseionTable?.Name ?? "Unknown",
                    TermName = GetTermName(termReg.Term),
                    PrintDate = DateTime.UtcNow,
                    Categories = categories,
                    SubClassName = termReg.SubClassTable?.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving consolidated receipt: {ex.Message}", ex);
            }
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentsDataTableAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null,
            string? statusFilter = null, int? stateFilter = null)
        {
            try
            {
                var query = _context.StudentPayments
                    .Include(p => p.TermRegistration).ThenInclude(t => t.StudentsTable).ThenInclude(s => s.ApplicationUser)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SchoolClasses)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SesseionTable)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SubClassTable)
                    .Include(p => p.PaymentItems).ThenInclude(t => t.PaymentItem).ThenInclude(t => t.PaymentSetups)
                    .AsQueryable();

                if (sessionFilter.HasValue && sessionFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.SessionId == sessionFilter.Value);
                if (termFilter.HasValue && termFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.Term == (Term)termFilter.Value);
                if (classFilter.HasValue && classFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.SchoolClassId == classFilter.Value);
                if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<PaymentStatus>(statusFilter, true, out var parsedStatus))
                    query = query.Where(p => p.Status == parsedStatus);
                if (stateFilter.HasValue && stateFilter.Value > 0 && Enum.IsDefined(typeof(PaymentState), stateFilter.Value))
                    query = query.Where(p => p.State == (PaymentState)stateFilter.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.Reference.Contains(searchTerm) ||
                        p.TermRegistration.StudentsTable.Surname.Contains(searchTerm) ||
                        p.TermRegistration.StudentsTable.FirstName.Contains(searchTerm) ||
                        p.TermRegistration.StudentsTable.OtherName.Contains(searchTerm) ||
                        p.TermRegistration.StudentsTable.ApplicationUser.UserName.Contains(searchTerm));
                }

                int recordsTotal = await _context.StudentPayments.CountAsync();
                int recordsFiltered = await query.CountAsync();

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.PaymentDate)
                    : query.OrderBy(p => p.PaymentDate);

                var rawData = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.TermRegId,
                        StudentName = p.TermRegistration.StudentsTable.FullName,
                        AdmissionNo = p.TermRegistration.StudentsTable.ApplicationUser.UserName ?? "N/A",
                        ClassName = $"{p.TermRegistration.SchoolClasses.Name} - {p.TermRegistration.SubClassTable.Name}",
                        SessionName = p.TermRegistration.SesseionTable.Name,
                        p.TermRegistration.Term,
                        p.TotalAmount,
                        p.Reference,
                        p.Status,
                        p.State,
                        Fees = p.PaymentItems.SelectMany(pi => pi.PaymentItem.PaymentSetups).FirstOrDefault().Amount,
                        PaymentDate = p.PaymentDate.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                var data = rawData.Select(p => new
                {
                    p.Id,
                    p.TermRegId,
                    p.StudentName,
                    p.AdmissionNo,
                    p.ClassName,
                    p.SessionName,
                    TermName = GetTermName(p.Term),
                    TotalAmount = SD.ToNaira(p.TotalAmount),
                    p.Reference,
                    Status = p.Status.ToString(),
                    State = p.State.ToString(),
                    Fees = SD.ToNaira(p.Fees),
                    p.PaymentDate
                }).ToList();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student payments: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptViewModel> GetPaymentDetailAsync(int paymentId)
        {
            return await GetReceiptAsync(paymentId);
        }

        public async Task<ApiResponse<bool>> UpdatePaymentStateAsync(int paymentId, PaymentState state, string? rejectMessage)
        {
            try
            {
                var payment = await _context.StudentPayments.FindAsync(paymentId);
                if (payment == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment not found" };

                if (state == PaymentState.Rejected && string.IsNullOrWhiteSpace(rejectMessage))
                    return new ApiResponse<bool> { Success = false, Message = "A rejection message is required when rejecting a payment." };

                payment.State = state;
                payment.RejectMessage = state == PaymentState.Rejected ? rejectMessage : payment.RejectMessage;
                if (state == PaymentState.Rejected)
                    payment.Status = PaymentStatus.Reversed;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool> { Success = true, Message = $"Payment state updated to {state}", Data = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error updating payment state: {ex.Message}" };
            }
        }

        public async Task<List<PendingPaymentNotification>> GetPendingPaymentNotificationsAsync(int maxCount = 20)
        {
            var now = DateTime.UtcNow;

            var pending = await _context.StudentPayments
                .Include(p => p.TermRegistration)
                    .ThenInclude(tr => tr.StudentsTable)
                .Include(p => p.TermRegistration)
                    .ThenInclude(tr => tr.SchoolClasses)
                .Where(p => p.State == PaymentState.Pending && p.Status != PaymentStatus.Failed && p.Status != PaymentStatus.Reversed)
                .OrderByDescending(p => p.PaymentDate)
                .Take(maxCount)
                .Select(p => new PendingPaymentNotification
                {
                    PaymentId = p.Id,
                    Reference = p.Reference,
                    StudentName = p.TermRegistration.StudentsTable.Surname + " " + p.TermRegistration.StudentsTable.FirstName,
                    ClassName = p.TermRegistration.SchoolClasses.Name,
                    Amount = p.TotalAmount,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            foreach (var item in pending)
            {
                item.TimeAgo = GetTimeAgo(now, item.PaymentDate);
            }

            return pending;
        }

        private static string GetTimeAgo(DateTime now, DateTime date)
        {
            var diff = now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return date.ToString("MMM dd, yyyy");
        }

        /// <summary>
        /// Checks whether a student has fully paid every compulsory fee for their
        /// term registration. Used to gate access to academic results.
        /// </summary>
        public async Task<(bool hasPaid, List<string> unpaidItems)> HasPaidAllCompulsoryFeesAsync(int termRegId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .FirstOrDefaultAsync(t => t.Id == termRegId);

                if (termReg == null)
                    return (false, new List<string> { "Term registration not found." });

                var compulsorySetups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem)
                    .Where(ps => ps.SessionId == termReg.SessionId
                              && ps.Term == termReg.Term
                              && ps.SchoolClassId == termReg.SchoolClassId
                              && ps.IsCompulsory
                              && ps.IsActive
                              && ps.PaymentItem.IsActive)
                    .ToListAsync();

                if (!compulsorySetups.Any())
                    return (true, new List<string>());

                var paidAmounts = await _context.StudentPaymentItems
                    .Where(spi => spi.StudentPayment.TermRegId == termRegId
                               && spi.StudentPayment.State == PaymentState.Approved
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed)
                    .GroupBy(spi => spi.PaymentItemId)
                    .Select(g => new { PaymentItemId = g.Key, TotalPaid = g.Sum(x => x.AmountPaid) })
                    .ToDictionaryAsync(x => x.PaymentItemId, x => x.TotalPaid);

                var unpaidItems = new List<string>();

                foreach (var setup in compulsorySetups)
                {
                    var paid = paidAmounts.TryGetValue(setup.PaymentItemId, out var amount) ? amount : 0m;
                    if (paid < setup.Amount)
                        unpaidItems.Add(setup.PaymentItem.Name);
                }

                return unpaidItems.Count == 0
                    ? (true, new List<string>())
                    : (false, unpaidItems);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking compulsory fees: {ex.Message}", ex);
            }
        }

        // ── NEW: Single-item payment flow ─────────────────────────────────────────

        public async Task<ApiResponse<SingleItemLookupResult>> LookupByItemAsync(
            int sessionId, Term term, int paymentItemId, string admissionNo)
        {
            try
            {
                // 1. Validate student
                var student = await _context.StudentsTables
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == admissionNo);
                if (student == null)
                    return new ApiResponse<SingleItemLookupResult>
                    {
                        Success = false,
                        Message = $"No student found with admission number '{admissionNo}'."
                    };

                // 2. Verify term registration for this student / session / term
                var termReg = await _context.TermRegistrations
                    .Include(t => t.SchoolClasses)
                    .Include(t => t.SesseionTable)
                    .FirstOrDefaultAsync(t =>
                        t.StudentId == student.Id &&
                        t.SessionId == sessionId &&
                        t.Term == term);
                if (termReg == null)
                    return new ApiResponse<SingleItemLookupResult>
                    {
                        Success = false,
                        Message = $"'{student.FullName}' is not registered for {GetTermName(term)} in the selected academic session."
                    };

                // 3. Verify payment setup exists for this item / session / term / student's class
                var setup = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .FirstOrDefaultAsync(ps =>
                        ps.PaymentItemId == paymentItemId &&
                        ps.SessionId == sessionId &&
                        ps.Term == term &&
                        ps.SchoolClassId == termReg.SchoolClassId &&
                        ps.IsActive &&
                        ps.PaymentItem.IsActive);
                if (setup == null)
                    return new ApiResponse<SingleItemLookupResult>
                    {
                        Success = false,
                        Message = "The selected payment item is not configured for this student's class, session, and term."
                    };

                // 4. Load all valid payment history for this item in this term registration
                var paymentItems = await _context.StudentPaymentItems
                    .Include(spi => spi.StudentPayment)
                    .Where(spi =>
                        spi.PaymentItemId == paymentItemId &&
                        spi.StudentPayment.TermRegId == termReg.Id &&
                        spi.StudentPayment.Status != PaymentStatus.Reversed &&
                        spi.StudentPayment.Status != PaymentStatus.Failed &&
                        spi.StudentPayment.State != PaymentState.Rejected &&
                        spi.StudentPayment.State != PaymentState.Cancelled)
                    .OrderBy(spi => spi.StudentPayment.PaymentDate)
                    .ToListAsync();

                // 5. Build history rows with a running balance
                decimal runningBalance = setup.Amount;
                var history = new List<PaymentHistoryRow>();
                foreach (var spi in paymentItems)
                {
                    runningBalance -= spi.AmountPaid;
                    history.Add(new PaymentHistoryRow
                    {
                        PaymentId    = spi.StudentPaymentId,
                        Reference    = spi.StudentPayment.Reference,
                        PaymentDate  = spi.StudentPayment.PaymentDate,
                        AmountPaid   = spi.AmountPaid,
                        RecordedBy   = spi.StudentPayment.RecordedBy,
                        State        = spi.StudentPayment.State.ToString(),
                        Status       = spi.StudentPayment.Status.ToString(),
                        RunningBalance = runningBalance < 0 ? 0 : runningBalance
                    });
                }

                // 6. Build balance summary
                var totalPaid  = paymentItems.Sum(spi => spi.AmountPaid);
                var outstanding = setup.Amount - totalPaid;
                if (outstanding < 0) outstanding = 0;

                var statusLabel = outstanding <= 0 ? "Fully Paid"
                                : totalPaid > 0   ? "Partially Paid"
                                :                   "Unpaid";

                var result = new SingleItemLookupResult
                {
                    TermRegistrationId = (int)termReg.Id,
                    StudentId          = student.Id,
                    StudentName        = student.FullName,
                    AdmissionNo        = student.ApplicationUser?.UserName ?? admissionNo,
                    ClassName          = termReg.SchoolClasses?.Name ?? "Unknown",
                    SessionName        = termReg.SesseionTable?.Name ?? "Unknown",
                    TermName           = GetTermName(term),
                    PaymentItemId      = paymentItemId,
                    ItemName           = setup.PaymentItem.Name,
                    CategoryName       = setup.PaymentItem.PaymentCategory?.Name ?? "Unknown",
                    IsCompulsory       = setup.IsCompulsory,
                    Balance = new ItemBalanceSummary
                    {
                        TotalDue           = setup.Amount,
                        TotalPaid          = totalPaid,
                        Outstanding        = outstanding,
                        PaymentStatusLabel = statusLabel
                    },
                    History = history
                };

                return new ApiResponse<SingleItemLookupResult>
                {
                    Success = true,
                    Message = "Student found. Payment details loaded.",
                    Data    = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SingleItemLookupResult>
                {
                    Success = false,
                    Message = $"Lookup error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<int>> CreateSingleItemPaymentAsync(
            CreateSingleItemPaymentVM model, string? recordedBy)
        {
            try
            {
                // Validate term registration
                var termReg = await _context.TermRegistrations
                    .FirstOrDefaultAsync(t => t.Id == model.TermRegistrationId);
                if (termReg == null)
                    return new ApiResponse<int> { Success = false, Message = "Term registration not found." };

                // Validate payment setup
                var setup = await _context.PaymentSetups
                    .FirstOrDefaultAsync(ps =>
                        ps.PaymentItemId == model.PaymentItemId &&
                        ps.SessionId    == termReg.SessionId &&
                        ps.Term         == termReg.Term &&
                        ps.SchoolClassId == termReg.SchoolClassId &&
                        ps.IsActive);
                if (setup == null)
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "Payment item is not configured for this student's class, session, and term."
                    };

                if (model.AmountPaid <= 0)
                    return new ApiResponse<int> { Success = false, Message = "Amount must be greater than zero." };

                // Overpayment prevention
                var alreadyPaid = await _context.StudentPaymentItems
                    .Where(spi =>
                        spi.PaymentItemId == model.PaymentItemId &&
                        spi.StudentPayment.TermRegId == model.TermRegistrationId &&
                        spi.StudentPayment.Status != PaymentStatus.Reversed &&
                        spi.StudentPayment.Status != PaymentStatus.Failed &&
                        spi.StudentPayment.State  != PaymentState.Rejected &&
                        spi.StudentPayment.State  != PaymentState.Cancelled)
                    .SumAsync(spi => (decimal?)spi.AmountPaid) ?? 0;

                var maxAllowed = setup.Amount - alreadyPaid;
                if (model.AmountPaid > maxAllowed)
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = $"Overpayment blocked. Configured: ₦{setup.Amount:N2}, " +
                                  $"Already Paid: ₦{alreadyPaid:N2}, " +
                                  $"Maximum you can pay now: ₦{maxAllowed:N2}."
                    };

                var payment = new StudentPayment
                {
                    TermRegId    = model.TermRegistrationId,
                    TotalAmount  = model.AmountPaid,
                    PaymentDate  = DateTime.UtcNow,
                    Reference    = SD.GenerateUniqueNumber(),
                    Status       = PaymentStatus.Completed,
                    State        = PaymentState.Pending,
                    Narration    = model.Narration,
                    RecordedBy   = recordedBy,
                    UpdatedAt    = DateTime.UtcNow,
                    PaymentItems = new List<StudentPaymentItem>
                    {
                        new StudentPaymentItem
                        {
                            PaymentItemId = model.PaymentItemId,
                            AmountPaid    = model.AmountPaid
                        }
                    }
                };

                _context.StudentPayments.Add(payment);
                await _context.SaveChangesAsync();

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Payment of ₦{model.AmountPaid:N2} recorded successfully. Reference: {payment.Reference}.",
                    Data    = payment.Id
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int> { Success = false, Message = $"Error recording payment: {ex.Message}" };
            }
        }

        public async Task<FullTermReceiptViewModel?> GetFullTermReceiptAsync(int termRegId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(t => t.StudentsTable).ThenInclude(s => s.ApplicationUser)
                    .Include(t => t.SchoolClasses)
                    .Include(t => t.SubClassTable)
                    .Include(t => t.SesseionTable)
                    .FirstOrDefaultAsync(t => t.Id == termRegId);

                if (termReg == null) return null;

                // All payment setups for this class / session / term
                var setups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Where(ps => ps.SessionId == termReg.SessionId
                              && ps.Term == termReg.Term
                              && ps.SchoolClassId == termReg.SchoolClassId
                              && ps.IsActive
                              && ps.PaymentItem.IsActive)
                    .ToListAsync();

                // All valid payment items for this term registration
                var paymentItems = await _context.StudentPaymentItems
                    .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(spi => spi.StudentPayment)
                    .Where(spi => spi.StudentPayment.TermRegId == termRegId
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed
                               && spi.StudentPayment.State != PaymentState.Rejected
                               && spi.StudentPayment.State != PaymentState.Cancelled)
                    .ToListAsync();

                // Distinct individual payments for history
                var paymentIds = paymentItems.Select(spi => spi.StudentPaymentId).Distinct();
                var payments = await _context.StudentPayments
                    .Where(p => paymentIds.Contains(p.Id))
                    .OrderBy(p => p.PaymentDate)
                    .ToListAsync();

                // Build per-category + per-item breakdown with expected vs paid
                var breakdown = new List<ReceiptBreakdownRow>();
                foreach (var setup in setups.OrderBy(ps => ps.PaymentItem.PaymentCategory.Name)
                                            .ThenBy(ps => ps.PaymentItem.Name))
                {
                    var paid = paymentItems
                        .Where(spi => spi.PaymentItemId == setup.PaymentItemId)
                        .Sum(spi => spi.AmountPaid);

                    breakdown.Add(new ReceiptBreakdownRow
                    {
                        CategoryName = setup.PaymentItem.PaymentCategory?.Name ?? "Unknown",
                        ItemName = setup.PaymentItem.Name,
                        Expected = setup.Amount,
                        Paid = paid
                    });
                }

                // Build payment history
                var history = payments
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .Select(p => new ReceiptPaymentHistoryRow
                    {
                        Reference = p.Reference,
                        Date = p.PaymentDate,
                        Amount = p.TotalAmount,
                        Status = p.State == PaymentState.Pending
                            ? "Pending"
                            : p.State == PaymentState.Approved
                                ? "Completed"
                                : p.State == PaymentState.Rejected
                                    ? "Rejected"
                                    : p.Status.ToString()
                    })
                    .ToList();

                return new FullTermReceiptViewModel
                {
                    StudentName = termReg.StudentsTable?.FullName ?? "Unknown",
                    AdmissionNo = termReg.StudentsTable?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = termReg.SchoolClasses?.Name ?? "Unknown",
                    SubClassName = termReg.SubClassTable?.Name ?? "",
                    SessionName = termReg.SesseionTable?.Name ?? "Unknown",
                    TermName = GetTermName(termReg.Term),
                    PrintDate = DateTime.UtcNow,
                    TotalExpected = setups.Sum(s => s.Amount),
                    TotalPaid = paymentItems.Sum(spi => spi.AmountPaid),
                    Breakdown = breakdown,
                    Payments = history
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving full term receipt: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<bool>> UpdatePaymentAmountAsync(
            UpdatePaymentAmountVM model, string? updatedBy)
        {
            try
            {
                var payment = await _context.StudentPayments
                    .Include(p => p.PaymentItems)
                    .Include(p => p.TermRegistration)
                    .FirstOrDefaultAsync(p => p.Id == model.PaymentId);

                if (payment == null)
                    return new ApiResponse<bool> { Success = false, Message = "Payment not found." };

                if (payment.State != PaymentState.Pending)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Only payments in Pending state can be edited."
                    };

                if (payment.PaymentItems?.Count != 1)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Amount editing is only supported for single-item payments."
                    };

                var item = payment.PaymentItems.First();

                // Load the configured amount for this item
                var setup = await _context.PaymentSetups
                    .FirstOrDefaultAsync(ps =>
                        ps.PaymentItemId == item.PaymentItemId &&
                        ps.SessionId     == payment.TermRegistration.SessionId &&
                        ps.Term          == payment.TermRegistration.Term &&
                        ps.SchoolClassId == payment.TermRegistration.SchoolClassId &&
                        ps.IsActive);

                if (setup == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Payment item configuration not found."
                    };

                // Amount paid by OTHER transactions (exclude this one)
                var otherPaid = await _context.StudentPaymentItems
                    .Where(spi =>
                        spi.PaymentItemId     == item.PaymentItemId &&
                        spi.StudentPayment.TermRegId == payment.TermRegId &&
                        spi.StudentPaymentId  != payment.Id &&
                        spi.StudentPayment.Status != PaymentStatus.Reversed &&
                        spi.StudentPayment.Status != PaymentStatus.Failed &&
                        spi.StudentPayment.State  != PaymentState.Rejected &&
                        spi.StudentPayment.State  != PaymentState.Cancelled)
                    .SumAsync(spi => (decimal?)spi.AmountPaid) ?? 0;

                if (model.NewAmount <= 0)
                    return new ApiResponse<bool> { Success = false, Message = "Amount must be greater than zero." };

                var maxAllowed = setup.Amount - otherPaid;
                if (model.NewAmount > maxAllowed)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Overpayment blocked. Other transactions: ₦{otherPaid:N2}, " +
                                  $"Maximum for this payment: ₦{maxAllowed:N2}."
                    };

                // Idempotency — no change, return success silently
                if (item.AmountPaid == model.NewAmount &&
                    payment.Narration == model.Narration)
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "No changes detected. Payment is already up to date.",
                        Data    = true
                    };

                item.AmountPaid      = model.NewAmount;
                payment.TotalAmount  = model.NewAmount;
                payment.Narration    = model.Narration;
                payment.UpdatedAt    = DateTime.UtcNow;
                payment.RecordedBy   = updatedBy ?? payment.RecordedBy;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Payment updated to ₦{model.NewAmount:N2} successfully.",
                    Data    = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Error updating payment: {ex.Message}" };
            }
        }

        public async Task<DataTablesResponse<StudentPaymentListDto>> GetStudentPaymentsPagedAsync(
            int studentId, DataTablesRequest request)
        {
            try
            {
                var query = _context.StudentPayments
                    .Include(p => p.TermRegistration).ThenInclude(tr => tr.SesseionTable)
                    .Include(p => p.PaymentItems).ThenInclude(pi => pi.PaymentItem)
                    .Where(p => p.TermRegistration.StudentId == studentId)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrEmpty(request.Search?.Value))
                {
                    var sv = request.Search.Value.ToLower();
                    query = query.Where(p =>
                        p.Reference.ToLower().Contains(sv) ||
                        p.Narration != null && p.Narration.ToLower().Contains(sv));
                }

                var recordsFiltered = await query.CountAsync();

                query = (request.Order?.FirstOrDefault()?.Column) switch
                {
                    0 => request.Order!.First().Dir == "asc"
                        ? query.OrderBy(p => p.Reference) : query.OrderByDescending(p => p.Reference),
                    2 => request.Order!.First().Dir == "asc"
                        ? query.OrderBy(p => p.TotalAmount) : query.OrderByDescending(p => p.TotalAmount),
                    3 => request.Order!.First().Dir == "asc"
                        ? query.OrderBy(p => p.PaymentDate) : query.OrderByDescending(p => p.PaymentDate),
                    _ => query.OrderByDescending(p => p.PaymentDate)
                };

                var data = await query
                    .Skip(request.Start)
                    .Take(request.Length > 0 ? request.Length : 10)
                    .Select(p => new StudentPaymentListDto
                    {
                        Id = p.Id,
                        TermRegId = p.TermRegId,
                        Reference = p.Reference,
                        TotalAmount = p.TotalAmount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status.ToString(),
                        State = p.State.ToString(),
                        Narration = p.Narration,
                        Session = p.TermRegistration.SesseionTable.Name,
                        Term = p.TermRegistration.Term.ToString(),
                        ItemNames = p.PaymentItems.Select(pi => pi.PaymentItem.Name).ToList()
                    })
                    .ToListAsync();

                return new DataTablesResponse<StudentPaymentListDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = recordsTotal,
                    RecordsFiltered = recordsFiltered,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<StudentPaymentListDto>
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<StudentPaymentListDto>(),
                    Error = ex.Message
                };
            }
        }
    }
}
