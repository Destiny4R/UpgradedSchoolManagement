using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.ViewModels;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementDataAccess.Services
{
    public class PaymentReportService : IPaymentReportService
    {
        private readonly ApplicationDbContext _context;

        public PaymentReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────
        // 1.  CLASS-LEVEL REPORT
        // ────────────────────────────────────────────────────────
        public async Task<ClassReportResponse> GetClassReportAsync(
            int sessionId, int? term = null, int? classId = null,
            int? subClassId = null, int? categoryId = null, int? paymentItemId = null,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc")
        {
            // All students registered for this session (optionally filtered)
            var termRegQuery = _context.TermRegistrations
                .Include(tr => tr.StudentsTable).ThenInclude(s => s.ApplicationUser)
                .Where(tr => tr.SessionId == sessionId);

            if (term.HasValue && term.Value > 0)
                termRegQuery = termRegQuery.Where(tr => tr.Term == (Term)term.Value);
            if (classId.HasValue && classId.Value > 0)
                termRegQuery = termRegQuery.Where(tr => tr.SchoolClassId == classId.Value);
            if (subClassId.HasValue && subClassId.Value > 0)
                termRegQuery = termRegQuery.Where(tr => tr.SubClassId == subClassId.Value);

            var termRegs = await termRegQuery.ToListAsync();

            // Expected amounts from PaymentSetup
            var setupQuery = _context.PaymentSetups
                .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(ps => ps.SessionId == sessionId
                          && ps.IsActive
                          && ps.PaymentItem.IsActive
                          && ps.PaymentItem.PaymentCategory.IsActive);

            if (term.HasValue && term.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.Term == (Term)term.Value);
            if (classId.HasValue && classId.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.SchoolClassId == classId.Value);
            if (categoryId.HasValue && categoryId.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.PaymentItem.CategoryId == categoryId.Value);
            if (paymentItemId.HasValue && paymentItemId.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.PaymentItemId == paymentItemId.Value);

            var setups = await setupQuery.ToListAsync();

            // Actual payments by term registration ids
            var termRegIds = termRegs.Select(tr => tr.Id).ToList();
            var paymentQuery = _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed);

            if (categoryId.HasValue && categoryId.Value > 0)
                paymentQuery = paymentQuery.Where(spi => spi.PaymentItem.CategoryId == categoryId.Value);
            if (paymentItemId.HasValue && paymentItemId.Value > 0)
                paymentQuery = paymentQuery.Where(spi => spi.PaymentItemId == paymentItemId.Value);

            var payments = await paymentQuery.ToListAsync();

            // Build rows
            var rows = new List<ClassReportRow>();

            if (paymentItemId.HasValue && paymentItemId.Value > 0)
            {
                // Item-level: one row per student for that specific item
                var itemSetups = setups.Where(s => s.PaymentItemId == paymentItemId.Value).ToList();
                foreach (var tr in termRegs)
                {
                    // Find the expected amount for this student's class
                    var expected = itemSetups.Where(s => s.SchoolClassId == tr.SchoolClassId).Sum(s => s.Amount);
                    if (expected == 0) continue; // this item doesn't apply to this student's class

                    var studentName = tr.StudentsTable?.FullName ?? "N/A";
                    var admissionNo = tr.StudentsTable?.ApplicationUser?.UserName ?? "N/A";
                    var paid = payments
                        .Where(p => p.StudentPayment.TermRegId == tr.Id && p.PaymentItemId == paymentItemId.Value)
                        .Sum(p => p.AmountPaid);

                    var firstSetup = itemSetups.FirstOrDefault();
                    rows.Add(new ClassReportRow
                    {
                        StudentName = studentName,
                        AdmissionNo = admissionNo,
                        CategoryName = firstSetup?.PaymentItem?.PaymentCategory?.Name ?? "N/A",
                        PaymentItemName = firstSetup?.PaymentItem?.Name ?? "N/A",
                        Expected = expected,
                        Paid = paid
                    });
                }
            }
            else
            {
                // Category-level: one row per student per category (original behavior)
                var categorySetups = setups
                    .GroupBy(s => new { s.PaymentItem.CategoryId, s.PaymentItem.PaymentCategory.Name })
                    .Select(g => new { g.Key.CategoryId, CategoryName = g.Key.Name, Expected = g.Sum(x => x.Amount) })
                    .ToList();

                foreach (var tr in termRegs)
                {
                    var studentName = tr.StudentsTable?.FullName ?? "N/A";
                    var admissionNo = tr.StudentsTable?.ApplicationUser?.UserName ?? "N/A";

                    // Filter category setups to those applicable to this student's class
                    var classSetups = setups.Where(s => s.SchoolClassId == tr.SchoolClassId).ToList();
                    var classCategorySetups = classSetups
                        .GroupBy(s => new { s.PaymentItem.CategoryId, s.PaymentItem.PaymentCategory.Name })
                        .Select(g => new { g.Key.CategoryId, CategoryName = g.Key.Name, Expected = g.Sum(x => x.Amount) })
                        .ToList();

                    foreach (var cat in classCategorySetups)
                    {
                        var paid = payments
                            .Where(p => p.StudentPayment.TermRegId == tr.Id && p.PaymentItem.CategoryId == cat.CategoryId)
                            .Sum(p => p.AmountPaid);

                        rows.Add(new ClassReportRow
                        {
                            StudentName = studentName,
                            AdmissionNo = admissionNo,
                            CategoryName = cat.CategoryName,
                            PaymentItemName = "—",
                            Expected = cat.Expected,
                            Paid = paid
                        });
                    }
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                rows = rows.Where(r =>
                    r.StudentName.ToLower().Contains(s) ||
                    r.AdmissionNo.ToLower().Contains(s) ||
                    r.CategoryName.ToLower().Contains(s) ||
                    (r.PaymentItemName != null && r.PaymentItemName.ToLower().Contains(s))).ToList();
            }

            var recordsTotal = rows.Count;

            // Sort
            rows = SortClassRows(rows, sortColumn, sortDirection);

            // Summary
            var summary = new ClassReportSummary
            {
                TotalStudents = termRegs.Count,
                StudentsPaid = termRegs.Count(tr =>
                    payments.Any(p => p.StudentPayment.TermRegId == tr.Id)),
                TotalExpected = rows.Sum(r => r.Expected),
                TotalCollected = rows.Sum(r => r.Paid)
            };

            // Paging
            var pagedRows = rows.Skip(skip).Take(pageSize).ToList();

            return new ClassReportResponse
            {
                Summary = summary,
                Rows = pagedRows,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsTotal
            };
        }

        // ────────────────────────────────────────────────────────
        // 2.  SCHOOL-WIDE REPORT
        // ────────────────────────────────────────────────────────
        public async Task<SchoolReportResponse> GetSchoolReportAsync(
            int sessionId, int term,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc")
        {
            var termEnum = (Term)term;

            // All term registrations for this session/term
            var termRegs = await _context.TermRegistrations
                .Include(tr => tr.SchoolClasses)
                .Where(tr => tr.SessionId == sessionId && tr.Term == termEnum)
                .ToListAsync();

            // All active setups for this session/term
            var setups = await _context.PaymentSetups
                .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Include(ps => ps.SchoolClass)
                .Where(ps => ps.SessionId == sessionId
                          && ps.Term == termEnum
                          && ps.IsActive
                          && ps.PaymentItem.IsActive
                          && ps.PaymentItem.PaymentCategory.IsActive)
                .ToListAsync();

            // All actual payments for these registrations
            var termRegIds = termRegs.Select(tr => tr.Id).ToList();
            var payments = await _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed)
                .ToListAsync();

            // Group setups by Category × Item (aggregate across all classes)
            var setupGroups = setups
                .GroupBy(s => new
                {
                    s.PaymentItem.CategoryId,
                    CategoryName = s.PaymentItem.PaymentCategory.Name,
                    s.PaymentItemId,
                    ItemName = s.PaymentItem.Name
                })
                .ToList();

            var rows = new List<SchoolReportRow>();
            foreach (var sg in setupGroups)
            {
                // For this item, get the class IDs it applies to
                var itemClassIds = sg.Select(s => s.SchoolClassId).Distinct().ToList();
                var itemTermRegs = termRegs.Where(tr => itemClassIds.Contains(tr.SchoolClassId)).ToList();
                var itemTermRegIds = itemTermRegs.Select(tr => tr.Id).ToList();

                // Expected = sum of setup amounts per class × students in that class
                var expectedTotal = 0m;
                foreach (var setup in sg)
                {
                    var classStudentCount = termRegs.Count(tr => tr.SchoolClassId == setup.SchoolClassId);
                    expectedTotal += setup.Amount * classStudentCount;
                }

                var itemPayments = payments
                    .Where(p => itemTermRegIds.Contains(p.StudentPayment.TermRegId)
                             && p.PaymentItemId == sg.Key.PaymentItemId)
                    .ToList();

                var studentsPaid = itemTermRegs
                    .Count(tr => itemPayments.Any(p => p.StudentPayment.TermRegId == tr.Id));

                rows.Add(new SchoolReportRow
                {
                    CategoryName = sg.Key.CategoryName,
                    ItemName = sg.Key.ItemName,
                    TotalStudents = itemTermRegs.Count,
                    StudentsPaid = studentsPaid,
                    TotalExpected = expectedTotal,
                    TotalCollected = itemPayments.Sum(p => p.AmountPaid)
                });
            }

            // Sort by category first, then item — so rowspan grouping works on the client
            rows = rows.OrderBy(r => r.CategoryName).ThenBy(r => r.ItemName).ToList();

            // Assign rowspan helper values
            var categoryGroups = rows.GroupBy(r => r.CategoryName).ToList();
            foreach (var cg in categoryGroups)
            {
                var items = cg.ToList();
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].CategoryItemCount = items.Count;
                    items[i].IsFirstInCategory = (i == 0);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                rows = rows.Where(r =>
                    r.CategoryName.ToLower().Contains(s) ||
                    r.ItemName.ToLower().Contains(s)).ToList();
            }

            var recordsTotal = rows.Count;

            // Summary
            var summary = new SchoolReportSummary
            {
                TotalStudents = termRegs.Count,
                StudentsPaid = termRegs.Count(tr =>
                    payments.Any(p => p.StudentPayment.TermRegId == tr.Id)),
                TotalExpected = rows.Sum(r => r.TotalExpected),
                TotalCollected = rows.Sum(r => r.TotalCollected),
                CategoryBreakdown = rows
                    .GroupBy(r => r.CategoryName)
                    .Select(g => new CategoryRevenueSummary
                    {
                        CategoryName = g.Key,
                        TotalExpected = g.Sum(x => x.TotalExpected),
                        TotalCollected = g.Sum(x => x.TotalCollected)
                    }).ToList()
            };

            // Paging
            var pagedRows = rows.Skip(skip).Take(pageSize).ToList();

            return new SchoolReportResponse
            {
                Summary = summary,
                Rows = pagedRows,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsTotal
            };
        }

        // ────────────────────────────────────────────────────────
        // 3.  CATEGORY ITEM-LEVEL REPORT
        // ────────────────────────────────────────────────────────
        public async Task<CategoryItemReportResponse> GetCategoryItemReportAsync(
            int sessionId, int term, int? categoryId = null, int? classId = null,
            int skip = 0, int pageSize = 10, string searchTerm = "",
            int sortColumn = 0, string sortDirection = "asc")
        {
            var termEnum = (Term)term;

            // Setups
            var setupQuery = _context.PaymentSetups
                .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Include(ps => ps.SchoolClass)
                .Where(ps => ps.SessionId == sessionId
                          && ps.Term == termEnum
                          && ps.IsActive
                          && ps.PaymentItem.IsActive
                          && ps.PaymentItem.PaymentCategory.IsActive);

            if (categoryId.HasValue && categoryId.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.PaymentItem.CategoryId == categoryId.Value);
            if (classId.HasValue && classId.Value > 0)
                setupQuery = setupQuery.Where(ps => ps.SchoolClassId == classId.Value);

            var setups = await setupQuery.ToListAsync();

            // Term registrations for matching classes
            var classIds = setups.Select(s => s.SchoolClassId).Distinct().ToList();
            var termRegs = await _context.TermRegistrations
                .Where(tr => tr.SessionId == sessionId
                          && tr.Term == termEnum
                          && classIds.Contains(tr.SchoolClassId))
                .ToListAsync();

            var termRegIds = termRegs.Select(tr => tr.Id).ToList();

            // Actual payments
            var paymentQuery = _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed);

            if (categoryId.HasValue && categoryId.Value > 0)
                paymentQuery = paymentQuery.Where(spi => spi.PaymentItem.CategoryId == categoryId.Value);

            var payments = await paymentQuery.ToListAsync();

            // termRegId -> classId
            var termRegClassMap = termRegs.ToDictionary(tr => tr.Id, tr => tr.SchoolClassId);

            // Group setups: Item × Class
            var itemGroups = setups
                .GroupBy(s => new
                {
                    s.PaymentItemId,
                    ItemName = s.PaymentItem.Name,
                    CategoryName = s.PaymentItem.PaymentCategory.Name,
                    s.SchoolClassId,
                    ClassName = s.SchoolClass.Name
                })
                .ToList();

            var rows = new List<CategoryItemReportRow>();
            foreach (var ig in itemGroups)
            {
                var classTermRegs = termRegs.Where(tr => tr.SchoolClassId == ig.Key.SchoolClassId).ToList();
                var classTermRegIds = classTermRegs.Select(tr => tr.Id).ToList();
                var itemPayments = payments
                    .Where(p => classTermRegIds.Contains(p.StudentPayment.TermRegId)
                             && p.PaymentItemId == ig.Key.PaymentItemId)
                    .ToList();

                var studentsPaid = classTermRegs
                    .Count(tr => itemPayments.Any(p => p.StudentPayment.TermRegId == tr.Id));

                rows.Add(new CategoryItemReportRow
                {
                    ItemName = ig.Key.ItemName,
                    CategoryName = ig.Key.CategoryName,
                    ClassName = ig.Key.ClassName,
                    TotalStudents = classTermRegs.Count,
                    StudentsPaid = studentsPaid,
                    ExpectedAmount = ig.Sum(s => s.Amount) * classTermRegs.Count,
                    AmountCollected = itemPayments.Sum(p => p.AmountPaid)
                });
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                rows = rows.Where(r =>
                    r.ItemName.ToLower().Contains(s) ||
                    r.CategoryName.ToLower().Contains(s) ||
                    r.ClassName.ToLower().Contains(s)).ToList();
            }

            var recordsTotal = rows.Count;

            // Sort
            rows = SortCategoryItemRows(rows, sortColumn, sortDirection);

            // Summary
            var summary = new CategoryItemReportSummary
            {
                TotalItems = rows.Select(r => r.ItemName).Distinct().Count(),
                TotalStudentsPaid = rows.Sum(r => r.StudentsPaid),
                TotalAmountCollected = rows.Sum(r => r.AmountCollected)
            };

            // Paging
            var pagedRows = rows.Skip(skip).Take(pageSize).ToList();

            return new CategoryItemReportResponse
            {
                Summary = summary,
                Rows = pagedRows,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsTotal
            };
        }

        // ── Sorting helpers ──

        private static List<ClassReportRow> SortClassRows(List<ClassReportRow> rows, int col, string dir)
        {
            var asc = dir.Equals("asc", StringComparison.OrdinalIgnoreCase);
            return col switch
            {
                0 => asc ? rows.OrderBy(r => r.StudentName).ToList() : rows.OrderByDescending(r => r.StudentName).ToList(),
                1 => asc ? rows.OrderBy(r => r.AdmissionNo).ToList() : rows.OrderByDescending(r => r.AdmissionNo).ToList(),
                2 => asc ? rows.OrderBy(r => r.CategoryName).ToList() : rows.OrderByDescending(r => r.CategoryName).ToList(),
                3 => asc ? rows.OrderBy(r => r.PaymentItemName).ToList() : rows.OrderByDescending(r => r.PaymentItemName).ToList(),
                4 => asc ? rows.OrderBy(r => r.Expected).ToList() : rows.OrderByDescending(r => r.Expected).ToList(),
                5 => asc ? rows.OrderBy(r => r.Paid).ToList() : rows.OrderByDescending(r => r.Paid).ToList(),
                6 => asc ? rows.OrderBy(r => r.Outstanding).ToList() : rows.OrderByDescending(r => r.Outstanding).ToList(),
                _ => rows.OrderBy(r => r.StudentName).ToList()
            };
        }

        private static List<SchoolReportRow> SortSchoolRows(List<SchoolReportRow> rows, int col, string dir)
        {
            var asc = dir.Equals("asc", StringComparison.OrdinalIgnoreCase);
            return col switch
            {
                0 => asc ? rows.OrderBy(r => r.CategoryName).ToList() : rows.OrderByDescending(r => r.CategoryName).ToList(),
                1 => asc ? rows.OrderBy(r => r.ItemName).ToList() : rows.OrderByDescending(r => r.ItemName).ToList(),
                2 => asc ? rows.OrderBy(r => r.TotalStudents).ToList() : rows.OrderByDescending(r => r.TotalStudents).ToList(),
                3 => asc ? rows.OrderBy(r => r.StudentsPaid).ToList() : rows.OrderByDescending(r => r.StudentsPaid).ToList(),
                4 => asc ? rows.OrderBy(r => r.TotalExpected).ToList() : rows.OrderByDescending(r => r.TotalExpected).ToList(),
                5 => asc ? rows.OrderBy(r => r.TotalCollected).ToList() : rows.OrderByDescending(r => r.TotalCollected).ToList(),
                6 => asc ? rows.OrderBy(r => r.Outstanding).ToList() : rows.OrderByDescending(r => r.Outstanding).ToList(),
                _ => rows.OrderBy(r => r.CategoryName).ToList()
            };
        }

        private static List<CategoryItemReportRow> SortCategoryItemRows(List<CategoryItemReportRow> rows, int col, string dir)
        {
            var asc = dir.Equals("asc", StringComparison.OrdinalIgnoreCase);
            return col switch
            {
                0 => asc ? rows.OrderBy(r => r.ItemName).ToList() : rows.OrderByDescending(r => r.ItemName).ToList(),
                1 => asc ? rows.OrderBy(r => r.CategoryName).ToList() : rows.OrderByDescending(r => r.CategoryName).ToList(),
                2 => asc ? rows.OrderBy(r => r.ClassName).ToList() : rows.OrderByDescending(r => r.ClassName).ToList(),
                3 => asc ? rows.OrderBy(r => r.TotalStudents).ToList() : rows.OrderByDescending(r => r.TotalStudents).ToList(),
                4 => asc ? rows.OrderBy(r => r.StudentsPaid).ToList() : rows.OrderByDescending(r => r.StudentsPaid).ToList(),
                5 => asc ? rows.OrderBy(r => r.ExpectedAmount).ToList() : rows.OrderByDescending(r => r.ExpectedAmount).ToList(),
                6 => asc ? rows.OrderBy(r => r.AmountCollected).ToList() : rows.OrderByDescending(r => r.AmountCollected).ToList(),
                7 => asc ? rows.OrderBy(r => r.Outstanding).ToList() : rows.OrderByDescending(r => r.Outstanding).ToList(),
                _ => rows.OrderBy(r => r.ItemName).ToList()
            };
        }

        // ────────────────────────────────────────────────────────
        // DASHBOARD METHODS
        // ────────────────────────────────────────────────────────

        public async Task<List<DashboardCategorySummary>> GetDashboardCategorySummaryAsync(int sessionId, int term)
        {
            var termEnum = (Term)term;

            var categories = await _context.PaymentCategories
                .Where(c => c.IsActive)
                .Include(c => c.PaymentItems)
                .AsNoTracking()
                .ToListAsync();

            var setups = await _context.PaymentSetups
                .Include(ps => ps.PaymentItem)
                .Where(ps => ps.SessionId == sessionId && ps.Term == termEnum && ps.IsActive && ps.PaymentItem.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var termRegIds = await _context.TermRegistrations
                .Where(tr => tr.SessionId == sessionId && tr.Term == termEnum)
                .Select(tr => tr.Id)
                .ToListAsync();

            var payments = await _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Include(spi => spi.PaymentItem)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<DashboardCategorySummary>();
            foreach (var cat in categories)
            {
                var catItemIds = cat.PaymentItems.Select(pi => pi.Id).ToList();
                var expected = setups.Where(s => catItemIds.Contains(s.PaymentItemId)).Sum(s => s.Amount);
                var collected = payments.Where(p => catItemIds.Contains(p.PaymentItemId)).Sum(p => p.AmountPaid);

                if (expected == 0 && collected == 0) continue;

                result.Add(new DashboardCategorySummary
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name,
                    Expected = expected,
                    Collected = collected
                });
            }
            return result;
        }

        public async Task<List<DashboardItemSummary>> GetDashboardItemSummaryAsync(int sessionId, int term)
        {
            var termEnum = (Term)term;

            var setups = await _context.PaymentSetups
                .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(ps => ps.SessionId == sessionId && ps.Term == termEnum && ps.IsActive
                          && ps.PaymentItem.IsActive && ps.PaymentItem.PaymentCategory.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var termRegIds = await _context.TermRegistrations
                .Where(tr => tr.SessionId == sessionId && tr.Term == termEnum)
                .Select(tr => tr.Id)
                .ToListAsync();

            var payments = await _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed)
                .AsNoTracking()
                .ToListAsync();

            var grouped = setups.GroupBy(s => new { s.PaymentItemId, s.PaymentItem.Name, CatName = s.PaymentItem.PaymentCategory.Name });
            var result = new List<DashboardItemSummary>();
            foreach (var g in grouped)
            {
                var expected = g.Sum(s => s.Amount);
                var collected = payments.Where(p => p.PaymentItemId == g.Key.PaymentItemId).Sum(p => p.AmountPaid);

                result.Add(new DashboardItemSummary
                {
                    ItemId = g.Key.PaymentItemId,
                    ItemName = g.Key.Name,
                    CategoryName = g.Key.CatName,
                    Expected = expected,
                    Collected = collected
                });
            }
            return result.OrderBy(r => r.CategoryName).ThenBy(r => r.ItemName).ToList();
        }

        public async Task<DashboardCategoryTrend> GetDashboardCategoryTrendAsync(int recentSessionCount)
        {
            var sessions = await _context.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(recentSessionCount)
                .AsNoTracking()
                .ToListAsync();

            sessions = sessions.OrderBy(s => s.Id).ToList();

            var categories = await _context.PaymentCategories
                .Where(c => c.IsActive)
                .Include(c => c.PaymentItems)
                .AsNoTracking()
                .ToListAsync();

            var sessionIds = sessions.Select(s => s.Id).ToList();

            var payments = await _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment).ThenInclude(sp => sp.TermRegistration)
                .Include(spi => spi.PaymentItem)
                .Where(spi => sessionIds.Contains(spi.StudentPayment.TermRegistration.SessionId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed)
                .AsNoTracking()
                .ToListAsync();

            var result = new DashboardCategoryTrend
            {
                Sessions = sessions.Select(s => s.Name).ToList()
            };

            foreach (var cat in categories)
            {
                var catItemIds = cat.PaymentItems.Select(pi => pi.Id).ToHashSet();
                var series = new DashboardCategoryTrendSeries { CategoryName = cat.Name };

                foreach (var session in sessions)
                {
                    var amount = payments
                        .Where(p => p.StudentPayment.TermRegistration.SessionId == session.Id && catItemIds.Contains(p.PaymentItemId))
                        .Sum(p => p.AmountPaid);
                    series.Amounts.Add(amount);
                }

                if (series.Amounts.Any(a => a > 0))
                    result.Series.Add(series);
            }

            return result;
        }

        public async Task<DashboardItemChart> GetDashboardItemChartAsync(int sessionId, int term)
        {
            var termEnum = (Term)term;

            var setups = await _context.PaymentSetups
                .Include(ps => ps.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                .Where(ps => ps.SessionId == sessionId && ps.Term == termEnum && ps.IsActive
                          && ps.PaymentItem.IsActive && ps.PaymentItem.PaymentCategory.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var termRegIds = await _context.TermRegistrations
                .Where(tr => tr.SessionId == sessionId && tr.Term == termEnum)
                .Select(tr => tr.Id)
                .ToListAsync();

            var payments = await _context.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Where(spi => termRegIds.Contains(spi.StudentPayment.TermRegId)
                           && spi.StudentPayment.State == PaymentState.Approved
                           && spi.StudentPayment.Status != PaymentStatus.Reversed
                           && spi.StudentPayment.Status != PaymentStatus.Failed)
                .AsNoTracking()
                .ToListAsync();

            var items = setups
                .GroupBy(s => new { s.PaymentItemId, s.PaymentItem.Name })
                .OrderBy(g => g.Key.Name)
                .ToList();

            var result = new DashboardItemChart();
            foreach (var g in items)
            {
                result.Labels.Add(g.Key.Name);
                result.Expected.Add(g.Sum(s => s.Amount));
                result.Collected.Add(payments.Where(p => p.PaymentItemId == g.Key.PaymentItemId).Sum(p => p.AmountPaid));
            }
            return result;
        }

        public async Task<DashboardTermRegistrationChart> GetDashboardTermRegistrationChartAsync(int sessionId)
        {
            var registrations = await _context.TermRegistrations
                .Where(tr => tr.SessionId == sessionId)
                .GroupBy(tr => tr.Term)
                .Select(g => new { Term = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new DashboardTermRegistrationChart();
            foreach (var termVal in Enum.GetValues<Term>())
            {
                result.Labels.Add(termVal.ToString() + " Term");
                result.Counts.Add(registrations.FirstOrDefault(r => r.Term == termVal)?.Count ?? 0);
            }
            return result;
        }

        public async Task<List<RecentPaymentItem>> GetRecentPaymentsAsync(int count = 10, int? sessionId = null, int? termId = null)
        {
            var query = _context.StudentPayments
                .Where(sp => sp.State == PaymentState.Approved
                          && sp.Status != PaymentStatus.Reversed
                          && sp.Status != PaymentStatus.Failed);

            if (sessionId.HasValue)
                query = query.Where(sp => sp.TermRegistration.SessionId == sessionId.Value);
            if (termId.HasValue)
                query = query.Where(sp => (int)sp.TermRegistration.Term == termId.Value);

            var result = await query
                .OrderByDescending(sp => sp.PaymentDate)
                .Take(count)
                .Select(sp => new RecentPaymentItem
                {
                    Id = sp.Id,
                    Reference = sp.Reference,
                    StudentName = sp.TermRegistration.StudentsTable.Surname + " " + sp.TermRegistration.StudentsTable.FirstName,
                    ClassName = sp.TermRegistration.SchoolClasses.Name,
                    TotalAmount = sp.TotalAmount,
                    Status = sp.Status.ToString(),
                    PaymentDate = sp.PaymentDate
                })
                .ToListAsync();

            return result;
        }
    }
}
