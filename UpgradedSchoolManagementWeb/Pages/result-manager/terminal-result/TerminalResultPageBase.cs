using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager.terminal_result
{
    public abstract class TerminalResultPageBaseModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IResultSkillService _resultSkillService;
        private readonly SchoolConfigurationSetup _schoolConfig;

        protected TerminalResultPageBaseModel(ApplicationDbContext db, IResultSkillService resultSkillService,
            IOptions<SchoolConfigurationSetup> schoolConfig)
        {
            _db = db;
            _resultSkillService = resultSkillService;
            _schoolConfig = schoolConfig.Value;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        public TerminalResultPageViewModel Result { get; set; } = new();

        public abstract ResultType ResultType { get; }

        public async Task<IActionResult> OnGetAsync()
        {
            var termReg = await _db.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Include(tr => tr.SchoolClasses)
                .Include(tr => tr.SubClassTable)
                .Include(tr => tr.SesseionTable)
                .FirstOrDefaultAsync(tr => tr.Id == Id);

            if (termReg == null || termReg.SchoolClasses == null)
                return Content("Terminal result not found.", "text/plain");

            if (termReg.SchoolClasses.Resulttype != ResultType)
                return RedirectToPage("detail", new { id = Id });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var student = await _db.StudentsTables
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (student != null && termReg.StudentId != student.Id)
                    return Content("You are not authorized to view this result.", "text/plain");
            }

            var access = await ValidateResultAccessAsync(termReg);
            if (!access.Success)
            {
                Result = new TerminalResultPageViewModel
                {
                    SchoolName = _schoolConfig.Name,
                    SchoolLogoUrl = _schoolConfig.LogoUrl,
                    PicturePath = termReg.StudentsTable?.PicturePath,
                    IsAllowed = false,
                    AccessMessage = access.Message,
                    OutstandingPayments = access.OutstandingPayments,
                    TotalOutstanding = access.TotalOutstanding,
                    StudentName = GetStudentName(termReg),
                    ClassName = GetClassName(termReg)
                };
                return Page();
            }

            Result = await BuildResultAsync(termReg);
            return Page();
        }

        private async Task<AccessValidationResult> ValidateResultAccessAsync(TermRegistration termReg)
        {
            var adminSettings = await _db.Appsettings
                .Where(a => a.IsAdmin)
                .OrderByDescending(a => a.UpdatedDate)
                .FirstOrDefaultAsync();

            if (adminSettings?.CanPrintResult == true)
                return AccessValidationResult.Allow();

            var compulsorySetups = await _db.PaymentSetups
                .Include(ps => ps.PaymentItem)
                .Where(ps => ps.SessionId == termReg.SessionId
                    && ps.Term == termReg.Term
                    && ps.SchoolClassId == termReg.SchoolClassId
                    && ps.IsCompulsory
                    && ps.IsActive
                    && ps.PaymentItem.IsActive)
                .ToListAsync();

            if (!compulsorySetups.Any())
                return AccessValidationResult.Allow();

            var paidAmounts = await _db.StudentPaymentItems
                .Include(spi => spi.StudentPayment)
                .Where(spi => spi.StudentPayment.TermRegId == termReg.Id
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
            var message = $"Result access is restricted because compulsory payment items are outstanding: {itemLines}. Total outstanding: ₦{total:0.00}. Please complete payment before viewing or printing this result.";

            return AccessValidationResult.Deny(message, outstanding, total);
        }

        private async Task<TerminalResultPageViewModel> BuildResultAsync(TermRegistration termReg)
        {
            var configs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == termReg.SchoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .ToListAsync();

            var results = await _db.ResultTables
                .Include(rt => rt.Subject)
                .Where(rt => rt.TermRegId == termReg.Id && rt.Status)
                .OrderBy(rt => rt.Subject != null ? rt.Subject.DisplayOrder : 0)
                .ThenBy(rt => rt.Subject != null ? rt.Subject.Name : string.Empty)
                .ToListAsync();

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);
            var classAverages = await CalculateClassAveragesAsync(termReg, maxPerSubject);
            var subjectRankings = await CalculateSubjectRankingsAsync(termReg);
            var academicRows = new List<TerminalResultAcademicRowDto>();
            decimal academicTotal = 0;

            foreach (var result in results)
            {
                var scores = new[] { result.ScoreOne, result.ScoreTwo, result.ScoreThree, result.ScoreFour, result.ScoreFive, result.ScoreSix };
                var total = scores.Sum(score => score ?? 0);
                academicTotal += (decimal)total;
                var (grade, remark) = SD.GetGradeAndRemark((decimal)total);

                academicRows.Add(new TerminalResultAcademicRowDto
                {
                    SubjectName = result.Subject?.Name ?? "Unknown Subject",
                    Assessments = configs.Select((config, index) => new ResultTableAssessmentDto
                    {
                        AssessmentName = config.AssessmentName,
                        AssessmentScore = config.AssessmentScore,
                        StudentScore = index < scores.Length ? scores[index] : null
                    }).ToList(),
                    TotalScore = (decimal)total,
                    Grade = grade,
                    Remark = remark,
                    SubjectPosition = subjectRankings.TryGetValue(result.SubjectId, out var subjectRanking)
                        ? subjectRanking.Positions.TryGetValue(result.TermRegId, out var subjectPosition)
                            ? FormatPosition(subjectPosition)
                            : string.Empty
                        : string.Empty,
                    HighestScore = subjectRankings.TryGetValue(result.SubjectId, out var subjectStats)
                        ? (decimal)subjectStats.HighestScore
                        : 0,
                    LowestScore = subjectRankings.TryGetValue(result.SubjectId, out subjectStats)
                        ? (decimal)subjectStats.LowestScore
                        : 0,
                    ClassAverage = subjectRankings.TryGetValue(result.SubjectId, out subjectStats)
                        ? (decimal)Math.Round(subjectStats.ClassAverage, 2)
                        : 0
                });
            }

            var submittedCount = results.Count;
            var academicAverage = submittedCount > 0 && maxPerSubject > 0
                ? Math.Round((decimal)((double)academicTotal / (submittedCount * maxPerSubject) * 100), 2)
                : 0;

            var (_, overallRemark) = SD.GetGradeAndRemark(academicAverage);
            var firstName = termReg.StudentsTable?.FirstName ?? "Student";
            var classTeacherRemark = GenerateClassTeacherRemark(firstName, academicAverage, overallRemark);
            var principalRemark = GeneratePrincipalRemark(firstName, academicAverage, overallRemark);

            if (submittedCount > 0)
                await _resultSkillService.EnsureTerminalSkillRatingsForTermRegistrationAsync(termReg.Id);

            var assignedSkills = await _resultSkillService.GetAssignedSkillsByClassIdAsync(termReg.SchoolClassId);
            var ratings = await _db.StudentResultSkillRatings
                .Include(r => r.ResultSkill)
                .Where(r => r.TermRegId == termReg.Id)
                .ToListAsync();

            var skillRatings = ratings
                .Where(r => r.ResultSkill != null)
                .Select(r => new StudentResultSkillRatingDto
                {
                    ResultSkillId = r.ResultSkillId,
                    SkillName = r.ResultSkill!.Name,
                    Domain = r.ResultSkill.Domain,
                    DomainName = r.ResultSkill.Domain.ToString(),
                    Score = r.Score,
                    ScoreLabel = GetScoreLabel(r.Score)
                })
                .ToList();

            var termInfo = await _db.TermGeneralInformations
                .FirstOrDefaultAsync(tgi => tgi.SessionId == termReg.SessionId && tgi.Term == termReg.Term);

            var classTermInfo = await _db.ClassTermInformations
                .FirstOrDefaultAsync(cti => cti.SessionId == termReg.SessionId
                    && cti.Term == termReg.Term
                    && cti.SchoolClassId == termReg.SchoolClassId
                    && cti.SubClassId == termReg.SubClassId);

            var classSize = await _db.TermRegistrations.CountAsync(tr =>
                tr.SessionId == termReg.SessionId
                && tr.Term == termReg.Term
                && tr.SchoolClassId == termReg.SchoolClassId
                && tr.SubClassId == termReg.SubClassId);

            var daysOpen = termInfo?.DaySchoolOpen ?? 0;
            var daysPresent = termReg.Attendance ?? 0;
            var daysAbsent = daysOpen > daysPresent ? daysOpen - daysPresent : 0;

            return new TerminalResultPageViewModel
            {
                SchoolName = _schoolConfig.Name,
                SchoolLogoUrl = _schoolConfig.LogoUrl,
                PicturePath = termReg.StudentsTable?.PicturePath,
                IsAllowed = true,
                AccessMessage = string.Empty,
                TermRegId = termReg.Id,
                StudentName = GetStudentName(termReg),
                AdmissionNumber = termReg.StudentsTable?.AdmissionNumber ?? string.Empty,
                ClassName = GetClassName(termReg),
                Session = termReg.SesseionTable?.Name ?? string.Empty,
                Term = GetTermName(termReg.Term),
                ResultType = termReg.SchoolClasses?.Resulttype.ToString() ?? string.Empty,
                Gender = termReg.StudentsTable?.Gender.ToString() ?? string.Empty,
                DateOfBirth = termReg.StudentsTable?.DateOfBirth.ToString("dd/MM/yyyy") ?? string.Empty,
                Age = GetAge(termReg.StudentsTable?.DateOfBirth),
                Attendance = termReg.Attendance,
                DaysPresent = daysPresent,
                DaysAbsent = daysAbsent,
                TimesLate = 0,
                ClassSize = classSize,
                 ClassPosition = classAverages.TryGetValue(termReg.Id, out var classPosition) ? FormatPosition((int)classPosition) : string.Empty,
                NextClass = string.Empty,
                NextTermFees = classTermInfo?.NextTermFees.ToString("0.00") ?? string.Empty,
                NextTermStart = termInfo?.NextTermStart.ToString("dd MMMM yyyy") ?? string.Empty,
                ClassTeacherName = classTermInfo?.ClassTeacherName ?? string.Empty,
                PrincipalName = termInfo?.PrincipalName ?? string.Empty,
                AcademicTotal = academicTotal,
                AcademicAverage = academicAverage,
                OverallRemark = overallRemark,
                ClassTeacherRemark = classTeacherRemark,
                PrincipalRemark = principalRemark,
                AcademicRows = academicRows,
                AffectiveSkills = skillRatings.Where(s => s.Domain == ResultSkillDomain.Affective).ToList(),
                PsychomotorSkills = skillRatings.Where(s => s.Domain == ResultSkillDomain.Psychomotor).ToList()
            };
        }

        private async Task<Dictionary<long, decimal>> CalculateClassAveragesAsync(TermRegistration termReg, double maxPerSubject)
        {
            var termRegIds = await _db.TermRegistrations
                .Where(tr => tr.SessionId == termReg.SessionId
                    && tr.Term == termReg.Term
                    && tr.SchoolClassId == termReg.SchoolClassId
                    && tr.SubClassId == termReg.SubClassId)
                .Select(tr => tr.Id)
                .ToListAsync();

            var resultRows = await _db.ResultTables
                .Where(rt => termRegIds.Contains(rt.TermRegId) && rt.Status)
                .ToListAsync();

            var averages = new Dictionary<long, decimal>();
            foreach (var id in termRegIds)
                averages[id] = 0m;

            if (maxPerSubject <= 0)
                return averages;

            // Calculate average for each student
            var studentAverages = new Dictionary<long, decimal>();
            foreach (var group in resultRows.GroupBy(rt => rt.TermRegId))
            {
                var total = group.Sum(GetResultTotal);
                var average = group.Any()
                    ? Math.Round((decimal)((double)total / (group.Count() * maxPerSubject) * 100), 2)
                    : 0m;
                studentAverages[group.Key] = average;
            }

            // Rank students by average (highest first), handling ties
            var sorted = studentAverages
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .ToList();

            if (!sorted.Any())
                return averages;

            int? currentPosition = null;
            decimal? previousAverage = null;

            for (var i = 0; i < sorted.Count; i++)
            {
                // If average changed, update position; otherwise tie at same position
                if (previousAverage == null || Math.Abs(sorted[i].Value - previousAverage.Value) > 0.0001m)
                    currentPosition = i + 1;

                averages[sorted[i].Key] = currentPosition ?? 1;
                previousAverage = sorted[i].Value;
            }

            return averages;
        }

        private async Task<Dictionary<int, SubjectRankingStats>> CalculateSubjectRankingsAsync(TermRegistration termReg)
        {
            var termRegIds = await _db.TermRegistrations
                .Where(tr => tr.SessionId == termReg.SessionId
                    && tr.Term == termReg.Term
                    && tr.SchoolClassId == termReg.SchoolClassId
                    && tr.SubClassId == termReg.SubClassId)
                .Select(tr => tr.Id)
                .ToListAsync();

            var subjectRows = await _db.ResultTables
                .Where(rt => termRegIds.Contains(rt.TermRegId)
                    && rt.Status
                    && rt.SubjectId > 0)
                .ToListAsync();

            var rankings = new Dictionary<int, SubjectRankingStats>();
            foreach (var group in subjectRows.GroupBy(rt => rt.SubjectId))
            {
                var totals = group
                    .Select(rt => new SubjectScore { TermRegId = rt.TermRegId, Total = GetResultTotal(rt) })
                    .OrderByDescending(x => x.Total)
                    .ThenBy(x => x.TermRegId)
                    .ToList();

                if (!totals.Any())
                    continue;

                var positions = new Dictionary<long, int>();
                int? currentPosition = null;
                double? previousScore = null;

                for (var i = 0; i < totals.Count; i++)
                {
                    if (previousScore == null || Math.Abs(totals[i].Total - previousScore.Value) > 0.0001)
                        currentPosition = i + 1;

                    positions[totals[i].TermRegId] = currentPosition ?? 1;
                    previousScore = totals[i].Total;
                }

                rankings[group.Key] = new SubjectRankingStats
                {
                    Positions = positions,
                    HighestScore = totals.Max(x => x.Total),
                    LowestScore = totals.Min(x => x.Total),
                    ClassAverage = totals.Average(x => x.Total)
                };
            }

            return rankings;
        }

        private static double GetResultTotal(ResultTable result)
        {
            return (result.ScoreOne ?? 0) +
                   (result.ScoreTwo ?? 0) +
                   (result.ScoreThree ?? 0) +
                   (result.ScoreFour ?? 0) +
                   (result.ScoreFive ?? 0) +
                   (result.ScoreSix ?? 0);
        }

        private static string FormatPosition(int position)
        {
            var suffix = "th";
            var lastDigit = position % 10;
            var lastTwoDigits = position % 100;

            if (lastDigit == 1 && lastTwoDigits != 11)
                suffix = "st";
            else if (lastDigit == 2 && lastTwoDigits != 12)
                suffix = "nd";
            else if (lastDigit == 3 && lastTwoDigits != 13)
                suffix = "rd";

            return $"{position}{suffix}";
        }

        private static string GetStudentName(TermRegistration termReg)
        {
            return termReg.StudentsTable != null
                ? $"{termReg.StudentsTable.Surname} {termReg.StudentsTable.OtherName} {termReg.StudentsTable.FirstName}".Trim()
                : string.Empty;
        }

        private static string GetClassName(TermRegistration termReg)
        {
            return termReg.SubClassTable != null
                ? $"{termReg.SchoolClasses?.Name} - {termReg.SubClassTable.Name}"
                : termReg.SchoolClasses?.Name ?? string.Empty;
        }

        private static string GetTermName(Term term)
        {
            return term switch
            {
                Term.First => "First Term",
                Term.Second => "Second Term",
                Term.Third => "Third Term",
                _ => term.ToString()
            };
        }

        private static string GetAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null || dateOfBirth.Value == default)
                return string.Empty;

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age <= 0 ? string.Empty : $"{age} year{(age == 1 ? string.Empty : "s")}";
        }

        private static string GenerateClassTeacherRemark(string firstName, decimal average, string overallRemark)
        {
            return SD.GetClassTeacherRemark(average);
        }

        private static string GeneratePrincipalRemark(string firstName, decimal average, string overallRemark)
        {
            return SD.GetPrincipalRemark((double)average);
        }

        private static string GetScoreLabel(byte score)
        {
            return score switch
            {
                1 => "Poor",
                2 => "Fail",
                3 => "Good",
                4 => "Very Good",
                5 => "Excellent",
                _ => string.Empty
            };
        }

        private class SubjectScore
        {
            public long TermRegId { get; set; }
            public double Total { get; set; }
        }

        private class SubjectRankingStats
        {
            public Dictionary<long, int> Positions { get; set; } = new();
            public double HighestScore { get; set; }
            public double LowestScore { get; set; }
            public double ClassAverage { get; set; }
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
