using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _db;

        public DashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();

            model.Summary = await BuildSummaryAsync();
            model.RevenueTrend = await BuildRevenueTrendAsync();
            model.RegistrationTrend = await BuildRegistrationTrendAsync();
            model.CompulsoryCompliance = await BuildCompulsoryComplianceAsync();
            model.StudentDistribution = await BuildStudentDistributionAsync();
            model.AdmissionTrend = await BuildAdmissionTrendAsync();
            model.AcademicPerformance = await BuildAcademicPerformanceAsync();
            model.RecentActivities = await BuildRecentActivitiesAsync();
            model.Alerts = await BuildAlertsAsync();
            model.ClassPerformance = await BuildClassPerformanceAsync();
            model.TeacherActivities = await BuildTeacherActivityAsync();
            model.QuickActions = GetQuickActions();

            return model;
        }

        private async Task<DashboardSummary> BuildSummaryAsync()
        {
            var summary = new DashboardSummary();

            var students = _db.StudentsTables;
            var termRegs = _db.TermRegistrations;
            var payments = _db.StudentPayments.Where(sp => sp.State == PaymentState.Approved && sp.Status == PaymentStatus.Completed);
            var paymentItems = _db.StudentPaymentItems;
            var setups = _db.PaymentSetups;
            var results = _db.ResultTables;
            var classes = _db.SchoolClasses;
            var subjects = _db.SubjectTables;
            var sessions = _db.SesseionTables;
            var employees = _db.EmployeeTables;
            var parents = _db.ParentGuardians;

            summary.TotalStudents = await students.CountAsync();
            summary.ActiveStudents = await students.CountAsync(s => s.ApplicationUser != null && s.ApplicationUser.IsActive);
            summary.TotalTeachers = await employees.CountAsync();
            summary.TotalParents = await parents.CountAsync();
            summary.TotalClasses = await classes.CountAsync(c => c.IsActive);
            summary.TotalSubjects = await subjects.CountAsync(s => s.IsActive);
            summary.TotalSessions = await sessions.CountAsync();
            summary.TotalTermRegistrations = await termRegs.CountAsync();

            var paidTotals = await payments
                .GroupBy(sp => 1)
                .Select(g => new { Total = g.Sum(x => (decimal?)x.TotalAmount) })
                .FirstOrDefaultAsync();
            summary.TotalRevenueCollected = paidTotals?.Total ?? 0;

            var compulsorySetups = await setups
                .Where(ps => ps.IsCompulsory && ps.IsActive)
                .GroupBy(ps => new { ps.SessionId, ps.Term, ps.SchoolClassId })
                .Select(g => new { g.Key.SessionId, g.Key.Term, g.Key.SchoolClassId, Expected = g.Sum(x => (decimal?)x.Amount) ?? 0 })
                .ToListAsync();

            decimal totalOutstanding = 0;
            foreach (var cs in compulsorySetups)
            {
                var expected = cs.Expected;
                var termRegIds = await _db.TermRegistrations
                    .Where(tr => tr.SessionId == cs.SessionId && tr.Term == cs.Term && tr.SchoolClassId == cs.SchoolClassId)
                    .Select(tr => tr.Id)
                    .ToListAsync();

                var collected = await paymentItems
                    .Where(pi => termRegIds.Contains(pi.StudentPayment.TermRegId)
                        && pi.StudentPayment.State == PaymentState.Approved
                        && pi.StudentPayment.Status == PaymentStatus.Completed)
                    .SumAsync(pi => (decimal?)pi.AmountPaid) ?? 0;

                if (collected < expected)
                    totalOutstanding += expected - collected;
            }
            summary.OutstandingPayments = totalOutstanding;

            var allTermRegIds = await termRegs.Select(tr => tr.Id).ToListAsync();
            var resultTermRegIds = await results.Where(r => r.Status).Select(r => r.TermRegId).Distinct().ToListAsync();
            summary.ResultsPublished = resultTermRegIds.Count;
            summary.PendingResults = allTermRegIds.Count - summary.ResultsPublished;
            summary.StudentsWithResults = resultTermRegIds.Count;
            summary.TotalStudentsNew = await students.CountAsync(s => s.Id > 0);

            return summary;
        }

        private async Task<List<RevenueTrendPoint>> BuildRevenueTrendAsync()
        {
            var recentSessions = await _db.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(10)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var points = new List<RevenueTrendPoint>();
            var terms = new[] { Term.First, Term.Second, Term.Third };

            foreach (var session in recentSessions)
            {
                foreach (var term in terms)
                {
                    var termRegIds = await _db.TermRegistrations
                        .Where(tr => tr.SessionId == session.Id && tr.Term == term)
                        .Select(tr => tr.Id)
                        .ToListAsync();

                    var amount = await _db.StudentPaymentItems
                        .Where(pi => termRegIds.Contains(pi.StudentPayment.TermRegId)
                            && pi.StudentPayment.State == PaymentState.Approved
                            && pi.StudentPayment.Status == PaymentStatus.Completed)
                        .SumAsync(pi => (decimal?)pi.AmountPaid) ?? 0;

                    points.Add(new RevenueTrendPoint
                    {
                        SessionName = session.Name,
                        Term = term.ToString(),
                        Amount = amount,
                        SortOrder = session.Id * 3 + (int)term
                    });
                }
            }

            return points.OrderBy(p => p.SortOrder).ToList();
        }

        private async Task<List<RegistrationTrendPoint>> BuildRegistrationTrendAsync()
        {
            var recentSessions = await _db.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(10)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var points = new List<RegistrationTrendPoint>();
            var terms = new[] { Term.First, Term.Second, Term.Third };

            foreach (var session in recentSessions)
            {
                foreach (var term in terms)
                {
                    var count = await _db.TermRegistrations
                        .CountAsync(tr => tr.SessionId == session.Id && tr.Term == term);

                    points.Add(new RegistrationTrendPoint
                    {
                        SessionName = session.Name,
                        Term = term.ToString(),
                        Count = count,
                        SortOrder = session.Id * 3 + (int)term
                    });
                }
            }

            return points.OrderBy(p => p.SortOrder).ToList();
        }

        private async Task<List<CompulsoryComplianceDto>> BuildCompulsoryComplianceAsync()
        {
            var recentSessions = await _db.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(5)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var result = new List<CompulsoryComplianceDto>();
            var terms = new[] { Term.First, Term.Second, Term.Third };

            foreach (var session in recentSessions)
            {
                foreach (var term in terms)
                {
                    var setups = await _db.PaymentSetups
                        .Where(ps => ps.SessionId == session.Id && ps.Term == term && ps.IsCompulsory && ps.IsActive)
                        .ToListAsync();

                    if (!setups.Any()) continue;

                    var expected = setups.Sum(s => s.Amount);

                    var termRegIds = await _db.TermRegistrations
                        .Where(tr => tr.SessionId == session.Id && tr.Term == term)
                        .Select(tr => tr.Id)
                        .ToListAsync();

                    var collected = await _db.StudentPaymentItems
                        .Where(pi => termRegIds.Contains(pi.StudentPayment.TermRegId)
                            && pi.StudentPayment.State == PaymentState.Approved
                            && pi.StudentPayment.Status == PaymentStatus.Completed)
                        .SumAsync(pi => (decimal?)pi.AmountPaid) ?? 0;

                    result.Add(new CompulsoryComplianceDto
                    {
                        SessionName = session.Name,
                        Term = term.ToString(),
                        Expected = expected,
                        Collected = collected,
                        SortOrder = session.Id * 3 + (int)term
                    });
                }
            }

            return result.OrderBy(r => r.SortOrder).ToList();
        }

        private async Task<StudentDistributionDto> BuildStudentDistributionAsync()
        {
            var dto = new StudentDistributionDto();

            var students = _db.StudentsTables;

            var genderData = await students
                .GroupBy(s => s.Gender)
                .Select(g => new NamedCount { Name = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();
            dto.ByGender = genderData;

            var classData = await _db.SchoolClasses
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Resulttype,
                    Count = _db.TermRegistrations.Count(tr => tr.SchoolClassId == c.Id)
                })
                .ToListAsync();

            dto.ByClass = classData.Select(c => new NamedCount { Name = c.Name, Count = c.Count }).ToList();

            dto.BySection = classData
                .GroupBy(c => c.Resulttype)
                .Select(g => new NamedCount
                {
                    Name = g.Key.ToString(),
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            var subClassData = await _db.SubClassTables
                .Where(sc => sc.IsActive)
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    Count = _db.TermRegistrations.Count(tr => tr.SubClassId == sc.Id)
                })
                .ToListAsync();

            dto.BySubClass = subClassData.Select(sc => new NamedCount { Name = sc.Name, Count = sc.Count }).ToList();

            return dto;
        }

        private async Task<List<AdmissionTrendDto>> BuildAdmissionTrendAsync()
        {
            return await _db.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(10)
                .OrderBy(s => s.Id)
                .Select(s => new AdmissionTrendDto
                {
                    SessionName = s.Name,
                    Count = _db.TermRegistrations.Count(tr => tr.SessionId == s.Id)
                })
                .ToListAsync();
        }

        private async Task<AcademicPerformanceDto> BuildAcademicPerformanceAsync()
        {
            var dto = new AcademicPerformanceDto();

            var sessionsWithResults = await _db.SesseionTables
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();

            var configs = await _db.AssessmentConfigurations.ToListAsync();
            var maxPerSubject = configs.Any() ? configs.Sum(c => c.AssessmentScore) : 100;

            var terms = new[] { Term.First, Term.Second, Term.Third };
            double overallTotal = 0;
            int overallCount = 0;
            int passCount = 0;
            int distCount = 0;

            foreach (var term in terms)
            {
                var termRegIds = await _db.TermRegistrations
                    .Where(tr => sessionsWithResults.Select(s => s.Id).Contains(tr.SessionId) && tr.Term == term)
                    .Select(tr => tr.Id)
                    .ToListAsync();

                var results = await _db.ResultTables
                    .Where(r => termRegIds.Contains(r.TermRegId) && r.Status)
                    .ToListAsync();

                if (!results.Any()) continue;

                var studentAverages = results
                    .GroupBy(r => r.TermRegId)
                    .Select(g => g.Sum(r => (r.ScoreOne ?? 0) + (r.ScoreTwo ?? 0) + (r.ScoreThree ?? 0)
                        + (r.ScoreFour ?? 0) + (r.ScoreFive ?? 0) + (r.ScoreSix ?? 0)))
                    .ToList();

                var termTotal = studentAverages.Sum();
                var termCount = studentAverages.Count;

                dto.TotalResults += termCount;
                overallTotal += termTotal;
                overallCount += termCount;

                var termPassRate = maxPerSubject > 0
                    ? studentAverages.Count(s => s / (maxPerSubject * results.GroupBy(r => r.TermRegId).First().Count()) * 100 >= 40)
                    : 0;
                var termDistRate = maxPerSubject > 0
                    ? studentAverages.Count(s => s / (maxPerSubject * results.GroupBy(r => r.TermRegId).First().Count()) * 100 >= 75)
                    : 0;

                var trRate = termCount > 0 ? Math.Round((double)termPassRate / termCount * 100, 1) : 0;
                var tdRate = termCount > 0 ? Math.Round((double)termDistRate / termCount * 100, 1) : 0;

                dto.PassRateByTerm.Add(new NamedCount { Name = term.ToString(), Count = (int)trRate });
                dto.DistinctionRateByTerm.Add(new NamedCount { Name = term.ToString(), Count = (int)tdRate });
                passCount += termPassRate;
                distCount += termDistRate;
            }

            dto.OverallAverage = overallCount > 0 ? Math.Round(overallTotal / overallCount, 1) : 0;
            dto.PassRate = overallCount > 0 ? Math.Round((double)passCount / overallCount * 100, 1) : 0;
            dto.DistinctionRate = overallCount > 0 ? Math.Round((double)distCount / overallCount * 100, 1) : 0;

            return dto;
        }

        private async Task<List<ClassPerformanceDto>> BuildClassPerformanceAsync()
        {
            var classes = await _db.SchoolClasses.Where(c => c.IsActive).ToListAsync();
            var configs = await _db.AssessmentConfigurations.ToListAsync();
            var maxPerSubject = configs.Any() ? configs.Sum(c => c.AssessmentScore) : 100;

            var result = new List<ClassPerformanceDto>();
            foreach (var cls in classes)
            {
                var termRegIds = await _db.TermRegistrations
                    .Where(tr => tr.SchoolClassId == cls.Id)
                    .Select(tr => tr.Id)
                    .ToListAsync();

                if (!termRegIds.Any()) continue;

                var results = await _db.ResultTables
                    .Where(r => termRegIds.Contains(r.TermRegId) && r.Status)
                    .ToListAsync();

                if (!results.Any()) continue;

                var studentScores = results
                    .GroupBy(r => r.TermRegId)
                    .Select(g => g.Sum(r => (r.ScoreOne ?? 0) + (r.ScoreTwo ?? 0) + (r.ScoreThree ?? 0)
                        + (r.ScoreFour ?? 0) + (r.ScoreFive ?? 0) + (r.ScoreSix ?? 0)))
                    .ToList();

                var avgScore = studentScores.Any()
                    ? Math.Round(studentScores.Average(), 1) : 0;

                var passCount = studentScores.Count(s => s > 0);
                var passRate = studentScores.Count > 0
                    ? Math.Round((double)passCount / studentScores.Count * 100, 1) : 0;

                result.Add(new ClassPerformanceDto
                {
                    ClassName = cls.Name,
                    AverageScore = avgScore,
                    StudentCount = studentScores.Count,
                    PassRate = passRate
                });
            }

            return result.OrderByDescending(r => r.AverageScore).ToList();
        }

        private async Task<List<RecentActivityDto>> BuildRecentActivitiesAsync()
        {
            var activities = new List<RecentActivityDto>();

            var recentPayments = await _db.StudentPayments
                .OrderByDescending(p => p.PaymentDate)
                .Take(3)
                .Select(p => new RecentActivityDto
                {
                    Type = "payment",
                    Title = $"Payment of {SD.ToNaira(p.TotalAmount)}",
                    Description = $"Reference: {p.Reference}",
                    Icon = "bi-cash-coin",
                    TimeAgo = FormatTimeAgo(p.PaymentDate),
                    Link = "/Admin/Finance/Payments"
                })
                .ToListAsync();
            activities.AddRange(recentPayments);

            var recentRegistrations = await _db.TermRegistrations
                .OrderByDescending(tr => tr.CreatedDate)
                .Take(3)
                .Select(tr => new RecentActivityDto
                {
                    Type = "registration",
                    Title = $"Term registered",
                    Description = $"{tr.StudentsTable.Surname} {tr.StudentsTable.FirstName} - {tr.Term}",
                    Icon = "bi-journal-plus",
                    TimeAgo = FormatTimeAgo(tr.CreatedDate),
                    Link = "/admin/student-data/students-reg/index"
                })
                .ToListAsync();
            activities.AddRange(recentRegistrations);

            var recentResults = await _db.ResultTables
                .Where(r => r.Status)
                .OrderByDescending(r => r.UpdatedDate)
                .Take(3)
                .Select(r => new RecentActivityDto
                {
                    Type = "result",
                    Title = "Result published",
                    Description = $"{r.Subject.Name} - {r.TermRegistration.StudentsTable.Surname}",
                    Icon = "bi-clipboard2-check",
                    TimeAgo = FormatTimeAgo(r.UpdatedDate),
                    Link = "/result-manager/index"
                })
                .ToListAsync();
            activities.AddRange(recentResults);

            return activities.OrderByDescending(a => a.TimeAgo).Take(8).ToList();
        }

        private async Task<List<DashboardAlertDto>> BuildAlertsAsync()
        {
            var alerts = new List<DashboardAlertDto>();

            var activeSession = await _db.SesseionTables.FirstOrDefaultAsync(s => s.IsActive);

            if (activeSession != null)
            {
                var terms = new[] { Term.First, Term.Second, Term.Third };
                foreach (var term in terms)
                {
                    var regCount = await _db.TermRegistrations
                        .CountAsync(tr => tr.SessionId == activeSession.Id && tr.Term == term);

                    if (regCount == 0)
                    {
                        alerts.Add(new DashboardAlertDto
                        {
                            Type = "warning",
                            Message = $"No registrations found for {activeSession.Name} - {term} term",
                            ActionLink = "/admin/student-data/students-reg/index",
                            ActionText = "Register Now"
                        });
                    }
                }

                var classesWithoutResults = await _db.SchoolClasses
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        ClassId = c.Id,
                        HasResults = _db.ResultTables.Any(r => r.TermRegistration.SchoolClassId == c.Id && r.Status)
                    })
                    .ToListAsync();

                var missingClasses = classesWithoutResults.Count(c => !c.HasResults);
                if (missingClasses > 0)
                {
                    alerts.Add(new DashboardAlertDto
                    {
                        Type = "info",
                        Message = $"{missingClasses} class(es) have no published results yet",
                        ActionLink = "/result-manager/index",
                        ActionText = "Publish Results"
                    });
                }
            }

            var outstandingCompliance = await _db.StudentPayments
                .CountAsync(sp => sp.State == PaymentState.Pending);
            if (outstandingCompliance > 0)
            {
                alerts.Add(new DashboardAlertDto
                {
                    Type = "danger",
                    Message = $"{outstandingCompliance} payment(s) pending approval",
                    ActionLink = "/Admin/Finance/Payments",
                    ActionText = "Review Payments"
                });
            }

            return alerts;
        }

        private async Task<TeacherActivityDto> BuildTeacherActivityAsync()
        {
            var dto = new TeacherActivityDto();
            dto.TotalTeachers = await _db.EmployeeTables.CountAsync();

            var subjectCount = await _db.SubjectTables.CountAsync(s => s.IsActive);
            dto.AssignedSubjects = subjectCount;

            var classCount = await _db.SchoolClasses.CountAsync(c => c.IsActive);
            dto.AssignedClasses = classCount;

            var totalTermRegs = await _db.TermRegistrations.CountAsync();
            var resultsDone = await _db.ResultTables.Where(r => r.Status).Select(r => r.TermRegId).Distinct().CountAsync();
            dto.ResultsSubmitted = resultsDone;
            dto.ResultsPending = totalTermRegs - resultsDone;

            return dto;
        }

        private static List<QuickActionDto> GetQuickActions()
        {
            return new List<QuickActionDto>
            {
                new() { Title = "Register Student", Icon = "bi-person-plus-fill", Link = "/admin/student-data/students-admission/index", Color = "blue" },
                new() { Title = "Create Payment", Icon = "bi-cash-stack", Link = "/Admin/Finance/Payments", Color = "green" },
                new() { Title = "Register Term", Icon = "bi-journal-plus", Link = "/admin/student-data/students-reg/index", Color = "purple" },
                new() { Title = "Publish Results", Icon = "bi-clipboard2-data", Link = "/result-manager/index", Color = "orange" },
                new() { Title = "Manage Classes", Icon = "bi-building", Link = "/Admin/Academic/manage-school", Color = "red" },
                new() { Title = "Manage Subjects", Icon = "bi-book", Link = "/Admin/Academic/manage-subjects", Color = "blue" },
                new() { Title = "View Reports", Icon = "bi-file-earmark-bar-graph", Link = "/Admin/Finance/Reports/ClassReport", Color = "green" },
                new() { Title = "Fee Setup", Icon = "bi-gear", Link = "/Admin/Finance/Setup", Color = "purple" }
            };
        }

        private static string FormatTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return dateTime.ToString("MMM dd");
        }
    }
}
