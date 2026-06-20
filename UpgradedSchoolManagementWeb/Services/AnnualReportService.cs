using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Services
{
    public class AnnualReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IResultSkillService _resultSkillService;
        private readonly SchoolConfigurationSetup _schoolConfig;

        public AnnualReportService(ApplicationDbContext db, IResultSkillService resultSkillService,
            IOptions<SchoolConfigurationSetup> schoolConfig)
        {
            _db = db;
            _resultSkillService = resultSkillService;
            _schoolConfig = schoolConfig.Value;
        }

        public async Task<AnnualReportViewModel> BuildAsync(int studentId, int sessionId, int classId, int subClassId)
        {
            var model = new AnnualReportViewModel
            {
                SchoolName = _schoolConfig.Name,
                SchoolAddress = _schoolConfig.Address,
                SchoolPhone = _schoolConfig.ContactPhone,
                SchoolEmail = _schoolConfig.ContactEmail,
                SchoolLogoUrl = _schoolConfig.LogoUrl
            };

            var student = await _db.StudentsTables.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return model;

            var session = await _db.SesseionTables.FirstOrDefaultAsync(s => s.Id == sessionId);
            var schoolClass = await _db.SchoolClasses.FirstOrDefaultAsync(c => c.Id == classId);
            var subClass = await _db.SubClassTables.FirstOrDefaultAsync(s => s.Id == subClassId);

            model.SessionName = session?.Name ?? sessionId.ToString();
            model.ClassName = schoolClass != null
                ? subClass != null ? $"{schoolClass.Name} {subClass.Name}" : schoolClass.Name
                : classId.ToString();
            model.StudentName = $"{student.Surname} {student.OtherName} {student.FirstName}".Trim();
            model.PicturePath = student.PicturePath;
            model.AdmissionNumber = student.AdmissionNumber ?? "N/A";
            model.DateOfBirth = student.DateOfBirth.ToString("dd/MM/yyyy") ?? "N/A";
            model.Gender = student.Gender.ToString();
            model.Age = GetAge(student.DateOfBirth);

            var termRegs = await _db.TermRegistrations
                .Include(tr => tr.SchoolClasses)
                .Where(tr => tr.StudentId == studentId && tr.SessionId == sessionId)
                .AsNoTracking()
                .ToListAsync();

            if (termRegs.Count == 0) return model;

            var termRegIds = termRegs.Select(tr => tr.Id).ToList();
            var allTermRegs = await _db.TermRegistrations
                .Where(tr => tr.SessionId == sessionId
                    && tr.SchoolClassId == classId
                    && tr.SubClassId == subClassId)
                .AsNoTracking()
                .ToListAsync();
            var allTermRegIds = allTermRegs.Select(tr => tr.Id).ToList();

            var allResultTables = await _db.ResultTables
                .Where(rt => allTermRegIds.Contains(rt.TermRegId) && rt.Status)
                .AsNoTracking()
                .ToListAsync();

            var subjectIds = allResultTables.Select(rt => rt.SubjectId).Distinct().ToHashSet();
            var subjects = await _db.SubjectTables
                .Where(s => s.IsActive && subjectIds.Contains(s.Id))
                .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            var configs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == classId)
                .OrderBy(ac => ac.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);

            var resultsByReg = allResultTables.GroupBy(rt => rt.TermRegId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var regByTerm = termRegs.ToDictionary(tr => tr.Term);
            model.TotalStudents = allTermRegs.Select(tr => tr.StudentId).Distinct().Count();

            // Attendance
            var term1Reg = regByTerm.GetValueOrDefault(Term.First);
            var term2Reg = regByTerm.GetValueOrDefault(Term.Second);
            var term3Reg = regByTerm.GetValueOrDefault(Term.Third);

            model.Term1Present = term1Reg?.Attendance;
            model.Term2Present = term2Reg?.Attendance;
            model.Term3Present = term3Reg?.Attendance;

            var term1Info = term1Reg != null ? await _db.TermGeneralInformations
                .FirstOrDefaultAsync(tgi => tgi.SessionId == sessionId && tgi.Term == Term.First) : null;
            var term2Info = term2Reg != null ? await _db.TermGeneralInformations
                .FirstOrDefaultAsync(tgi => tgi.SessionId == sessionId && tgi.Term == Term.Second) : null;
            var term3Info = term3Reg != null ? await _db.TermGeneralInformations
                .FirstOrDefaultAsync(tgi => tgi.SessionId == sessionId && tgi.Term == Term.Third) : null;

            model.Term1OutOf = term1Info?.DaySchoolOpen ?? 0;
            model.Term2OutOf = term2Info?.DaySchoolOpen ?? 0;
            model.Term3OutOf = term3Info?.DaySchoolOpen ?? 0;
            model.AnnualPresent = (model.Term1Present ?? 0) + (model.Term2Present ?? 0) + (model.Term3Present ?? 0);
            model.AnnualOutOf = (model.Term1OutOf ?? 0) + (model.Term2OutOf ?? 0) + (model.Term3OutOf ?? 0);
            model.AnnualAttendancePercent = model.AnnualOutOf > 0
                ? Math.Round((decimal)model.AnnualPresent / model.AnnualOutOf * 100, 1) : 0;

            // Per-subject term rankings - compute once for all students
            var subjectRankings = await ComputeSubjectRankingsAsync(allResultTables, allTermRegIds);

            // Annual per-subject rankings (averaged across terms)
            var annualSubjectRankings = ComputeAnnualSubjectRankings(allResultTables, allTermRegs, subjects);

            // Build per-subject data
            double grandTotal = 0;
            double annualSum = 0;
            int annualSubjectsCount = 0;

            foreach (var subject in subjects)
            {
                var t1Reg = regByTerm.GetValueOrDefault(Term.First);
                var t2Reg = regByTerm.GetValueOrDefault(Term.Second);
                var t3Reg = regByTerm.GetValueOrDefault(Term.Third);

                var t1Res = t1Reg != null && resultsByReg.TryGetValue(t1Reg.Id, out var t1List)
                    ? t1List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;
                var t2Res = t2Reg != null && resultsByReg.TryGetValue(t2Reg.Id, out var t2List)
                    ? t2List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;
                var t3Res = t3Reg != null && resultsByReg.TryGetValue(t3Reg.Id, out var t3List)
                    ? t3List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;

                double? t1Score = t1Res != null ? GetTotalScore(t1Res) : null;
                double? t2Score = t2Res != null ? GetTotalScore(t2Res) : null;
                double? t3Score = t3Res != null ? GetTotalScore(t3Res) : null;

                var t1Grade = t1Score.HasValue ? SD.GetGradeAndRemark((decimal)t1Score.Value).Grade : string.Empty;
                var t2Grade = t2Score.HasValue ? SD.GetGradeAndRemark((decimal)t2Score.Value).Grade : string.Empty;
                var t3Grade = t3Score.HasValue ? SD.GetGradeAndRemark((decimal)t3Score.Value).Grade : string.Empty;

                var available = new[] { t1Score, t2Score, t3Score }.Where(t => t.HasValue).Select(t => t.Value).ToList();
                var annualAvg = available.Any() ? available.Average() : 0.0;
                var annualGrade = available.Any() ? SD.GetGradeAndRemark((decimal)annualAvg).Grade : string.Empty;

                // Per-subject rankings
                int t1Pos = 1, t1Total = 0, t2Pos = 1, t2Total = 0, t3Pos = 1, t3Total = 0, annPos = 1, annTotal = 0;
                var t1TermRegId = t1Reg?.Id ?? 0;
                var t2TermRegId = t2Reg?.Id ?? 0;
                var t3TermRegId = t3Reg?.Id ?? 0;

                if (t1Score.HasValue && subjectRankings.TryGetValue((subject.Id, Term.First), out var t1Rank))
                {
                    t1Pos = t1Rank.Positions.GetValueOrDefault(t1TermRegId, 1);
                    t1Total = t1Rank.TotalStudents;
                }
                if (t2Score.HasValue && subjectRankings.TryGetValue((subject.Id, Term.Second), out var t2Rank))
                {
                    t2Pos = t2Rank.Positions.GetValueOrDefault(t2TermRegId, 1);
                    t2Total = t2Rank.TotalStudents;
                }
                if (t3Score.HasValue && subjectRankings.TryGetValue((subject.Id, Term.Third), out var t3Rank))
                {
                    t3Pos = t3Rank.Positions.GetValueOrDefault(t3TermRegId, 1);
                    t3Total = t3Rank.TotalStudents;
                }

                // Annual per-subject ranking (precomputed)
                if (annualSubjectRankings.TryGetValue(subject.Id, out var annRankData))
                {
                    annPos = annRankData.Positions.GetValueOrDefault(studentId, 1);
                    annTotal = annRankData.TotalStudents;
                }

                var t1ScoreVal = t1Score.HasValue ? Math.Round(t1Score.Value, 1) : (double?)null;
                var t2ScoreVal = t2Score.HasValue ? Math.Round(t2Score.Value, 1) : (double?)null;
                var t3ScoreVal = t3Score.HasValue ? Math.Round(t3Score.Value, 1) : (double?)null;

                grandTotal += (t1Score ?? 0) + (t2Score ?? 0) + (t3Score ?? 0);

                if (available.Any())
                {
                    annualSum += annualAvg;
                    annualSubjectsCount++;
                }

                model.Subjects.Add(new AnnualSubjectReport
                {
                    SubjectName = subject.Name,
                    Term1Score = t1ScoreVal,
                    Term1Grade = t1Grade,
                    Term1Pos = t1Pos,
                    Term1TotalStudents = t1Total,
                    Term2Score = t2ScoreVal,
                    Term2Grade = t2Grade,
                    Term2Pos = t2Pos,
                    Term2TotalStudents = t2Total,
                    Term3Score = t3ScoreVal,
                    Term3Grade = t3Grade,
                    Term3Pos = t3Pos,
                    Term3TotalStudents = t3Total,
                    AnnualAvg = (decimal)Math.Round(annualAvg, 1),
                    AnnualGrade = annualGrade,
                    AnnualPos = annPos,
                    AnnualTotalStudents = annTotal
                });
            }

            model.GrandTotal = (decimal)Math.Round(grandTotal, 1);
            model.OverallAverage = annualSubjectsCount > 0 && maxPerSubject > 0
                ? (decimal)Math.Round(annualSum / annualSubjectsCount, 1)
                : 0;
            var overallGradeInfo = SD.GetGradeAndRemark(model.OverallAverage);
            model.OverallGrade = overallGradeInfo.Grade;
            model.OverallRemark = overallGradeInfo.Remark;

            // Overall annual position (standard-competition ranking)
            var overallAnnAvgs = new Dictionary<long, double>();
            foreach (var sid in allTermRegs.Select(tr => tr.StudentId).Distinct())
            {
                var studentRegs = allTermRegs.Where(tr => tr.StudentId == sid).ToList();
                var subjectAvgs = new List<double>();
                foreach (var subj in subjects)
                {
                    var termScores = new List<double>();
                    foreach (var tr in studentRegs)
                    {
                        var rr = allResultTables.FirstOrDefault(rt => rt.TermRegId == tr.Id && rt.SubjectId == subj.Id);
                        if (rr != null) termScores.Add(GetTotalScore(rr));
                    }
                    if (termScores.Count > 0)
                        subjectAvgs.Add(termScores.Average());
                }
                if (subjectAvgs.Count > 0)
                    overallAnnAvgs[sid] = Math.Round(subjectAvgs.Average(), 1);
            }

            var sortedOverall = overallAnnAvgs.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
            int? currentOverallPos = null;
            double? previousOverallAvg = null;
            for (int i = 0; i < sortedOverall.Count; i++)
            {
                if (previousOverallAvg == null || Math.Abs(sortedOverall[i].Value - previousOverallAvg.Value) > 0.0001)
                    currentOverallPos = i + 1;
                if (sortedOverall[i].Key == studentId)
                    model.OverallPosition = currentOverallPos ?? 1;
                previousOverallAvg = sortedOverall[i].Value;
            }

            // Term-by-term overall averages
            var term1Regs = allTermRegs.Where(tr => tr.Term == Term.First).ToList();
            var term2Regs = allTermRegs.Where(tr => tr.Term == Term.Second).ToList();
            var term3Regs = allTermRegs.Where(tr => tr.Term == Term.Third).ToList();

            var term1AllResults = allResultTables
                .Where(rt => term1Regs.Any(tr => tr.Id == rt.TermRegId)).ToList();
            var term2AllResults = allResultTables
                .Where(rt => term2Regs.Any(tr => tr.Id == rt.TermRegId)).ToList();
            var term3AllResults = allResultTables
                .Where(rt => term3Regs.Any(tr => tr.Id == rt.TermRegId)).ToList();

            model.Term1Avg = ComputeTermOverallAverage(term1AllResults, term1Regs, studentId, (int)maxPerSubject, out int t1OverallPos);
            model.Term1OverallPos = t1OverallPos;
            model.Term2Avg = ComputeTermOverallAverage(term2AllResults, term2Regs, studentId, (int)maxPerSubject, out int t2OverallPos);
            model.Term2OverallPos = t2OverallPos;
            model.Term3Avg = ComputeTermOverallAverage(term3AllResults, term3Regs, studentId, (int)maxPerSubject, out int t3OverallPos);
            model.Term3OverallPos = t3OverallPos;

            // Skill ratings
            await LoadSkillRatingsAsync(model, termRegIds);

            // Remarks
            model.FormTeacherName = await GetFormTeacherNameAsync(sessionId, classId, subClassId);
            model.PrincipalName = await GetPrincipalNameAsync(sessionId);
            model.FormTeacherRemark = SD.GetClassTeacherRemark(model.OverallAverage);
            model.PrincipalRemark = SD.GetPrincipalRemark((double)model.OverallAverage);
            model.PromotedToClass = await GetPromotedToClassAsync(classId, subClassId);

            return model;
        }

        private decimal? ComputeTermOverallAverage(List<ResultTable> termResults, List<TermRegistration> termRegs, int studentId, int maxPerSubject, out int position)
        {
            position = 1;
            if (termRegs.Count == 0 || maxPerSubject <= 0) return null;

            var studentTermReg = termRegs.FirstOrDefault(tr => tr.StudentId == studentId);
            if (studentTermReg == null) return null;

            var studentResults = termResults.Where(rt => rt.TermRegId == studentTermReg.Id).ToList();
            if (studentResults.Count == 0) return null;

            var studentTotal = studentResults.Sum(r => GetTotalScore(r));
            var studentAvg = Math.Round((decimal)(studentTotal / (studentResults.Count * maxPerSubject) * 100), 1);

            var allAvgs = new Dictionary<long, decimal>();
            foreach (var tr in termRegs)
            {
                var results = termResults.Where(rt => rt.TermRegId == tr.Id).ToList();
                if (results.Count == 0) continue;
                var total = results.Sum(r => GetTotalScore(r));
                allAvgs[tr.Id] = Math.Round((decimal)(total / (results.Count * maxPerSubject) * 100), 1);
            }

            var sorted = allAvgs.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
            int? curPos = null;
            decimal? prev = null;
            for (int i = 0; i < sorted.Count; i++)
            {
                if (prev == null || Math.Abs(sorted[i].Value - prev.Value) > 0.0001m)
                    curPos = i + 1;
                if (sorted[i].Key == studentTermReg.Id)
                    position = curPos ?? 1;
                prev = sorted[i].Value;
            }

            return studentAvg;
        }

        private async Task<Dictionary<(int SubjectId, Term Term), SubjectRankingData>> ComputeSubjectRankingsAsync(
            List<ResultTable> allResults, List<long> allTermRegIds)
        {
            var result = new Dictionary<(int, Term), SubjectRankingData>();
            var termRegLookup = await _db.TermRegistrations
                .Where(tr => allTermRegIds.Contains(tr.Id))
                .AsNoTracking()
                .ToDictionaryAsync(tr => tr.Id);

            foreach (var group in allResults.GroupBy(rt => rt.SubjectId))
            {
                foreach (var termGroup in group.GroupBy(rt => termRegLookup.TryGetValue(rt.TermRegId, out var tr) ? tr.Term : Term.First))
                {
                    var totals = termGroup
                        .Select(rt => new { rt.TermRegId, Total = GetTotalScore(rt) })
                        .OrderByDescending(x => x.Total)
                        .ThenBy(x => x.TermRegId)
                        .ToList();

                    if (totals.Count == 0) continue;

                    var positions = new Dictionary<long, int>();
                    int? currentPos = null;
                    double? previousScore = null;

                    for (int i = 0; i < totals.Count; i++)
                    {
                        if (previousScore == null || Math.Abs(totals[i].Total - previousScore.Value) > 0.0001)
                            currentPos = i + 1;
                        positions[totals[i].TermRegId] = currentPos ?? 1;
                        previousScore = totals[i].Total;
                    }

                    result[(group.Key, termGroup.Key)] = new SubjectRankingData
                    {
                        Positions = positions,
                        TotalStudents = totals.Count
                    };
                }
            }

            return result;
        }

        private Dictionary<int, SubjectRankingData> ComputeAnnualSubjectRankings(
            List<ResultTable> allResults, List<TermRegistration> allTermRegs, List<SubjectTable> subjects)
        {
            var result = new Dictionary<int, SubjectRankingData>();

            var regByStudent = allTermRegs.GroupBy(tr => tr.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var subject in subjects)
            {
                var annualAvgs = new Dictionary<long, double>();

                foreach (var kv in regByStudent)
                {
                    var termScores = new List<double>();
                    foreach (var tr in kv.Value)
                    {
                        var rr = allResults.FirstOrDefault(rt => rt.TermRegId == tr.Id && rt.SubjectId == subject.Id);
                        if (rr != null) termScores.Add(GetTotalScore(rr));
                    }
                    if (termScores.Count > 0)
                        annualAvgs[kv.Key] = termScores.Average();
                }

                if (annualAvgs.Count == 0) continue;

                var sorted = annualAvgs.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
                var positions = new Dictionary<long, int>();
                int? curPos = null;
                double? prev = null;

                for (int i = 0; i < sorted.Count; i++)
                {
                    if (prev == null || Math.Abs(sorted[i].Value - prev.Value) > 0.0001)
                        curPos = i + 1;
                    positions[sorted[i].Key] = curPos ?? 1;
                    prev = sorted[i].Value;
                }

                result[subject.Id] = new SubjectRankingData
                {
                    Positions = positions,
                    TotalStudents = sorted.Count
                };
            }

            return result;
        }

        private async Task LoadSkillRatingsAsync(AnnualReportViewModel model, List<long> termRegIds)
        {
            var ratings = await _db.StudentResultSkillRatings
                .Include(r => r.ResultSkill)
                .Where(r => termRegIds.Contains(r.TermRegId) && r.ResultSkill != null && r.ResultSkill.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var regTerms = await _db.TermRegistrations
                .Where(tr => termRegIds.Contains(tr.Id))
                .AsNoTracking()
                .ToDictionaryAsync(tr => tr.Id, tr => tr.Term);

            var skillsByName = new Dictionary<string, SkillRatingItem>();
            foreach (var rating in ratings)
            {
                if (rating.ResultSkill == null) continue;
                var skillName = rating.ResultSkill.Name;
                var term = regTerms.GetValueOrDefault(rating.TermRegId);

                if (!skillsByName.TryGetValue(skillName, out var item))
                {
                    item = new SkillRatingItem { SkillName = skillName };
                    skillsByName[skillName] = item;
                }

                if (term == Term.First) item.Term1Score = rating.Score;
                else if (term == Term.Second) item.Term2Score = rating.Score;
                else if (term == Term.Third) item.Term3Score = rating.Score;
            }

            foreach (var kv in skillsByName)
            {
                var firstRating = ratings.FirstOrDefault(r => r.ResultSkill != null && r.ResultSkill.Name == kv.Key);
                if (firstRating?.ResultSkill?.Domain == ResultSkillDomain.Affective)
                    model.AffectiveDomain.Add(kv.Value);
                else if (firstRating?.ResultSkill?.Domain == ResultSkillDomain.Psychomotor)
                    model.PsychomotorSkills.Add(kv.Value);
            }
        }

        private async Task<string> GetFormTeacherNameAsync(int sessionId, int classId, int subClassId)
        {
            var info = await _db.ClassTermInformations
                .Where(cti => cti.SessionId == sessionId
                    && cti.SchoolClassId == classId
                    && cti.SubClassId == subClassId)
                .Select(cti => cti.ClassTeacherName)
                .FirstOrDefaultAsync();
            return info ?? "N/A";
        }

        private async Task<string> GetPrincipalNameAsync(int sessionId)
        {
            var info = await _db.TermGeneralInformations
                .Where(tgi => tgi.SessionId == sessionId)
                .Select(tgi => tgi.PrincipalName)
                .FirstOrDefaultAsync();
            return info ?? "N/A";
        }

        private async Task<string> GetPromotedToClassAsync(int classId, int subClassId)
        {
            var cls = await _db.SchoolClasses.FirstOrDefaultAsync(c => c.Id == classId);
            if (cls == null) return "N/A";
            var nextClassId = classId + 1;
            var nextClass = await _db.SchoolClasses.FirstOrDefaultAsync(c => c.Id == nextClassId);
            if (nextClass == null) return $"{cls.Name} (Next)";
            var sub = await _db.SubClassTables.FirstOrDefaultAsync(s => s.Id == subClassId);
            return sub != null ? $"{nextClass.Name} {sub.Name}" : nextClass.Name;
        }

        private static double GetTotalScore(ResultTable r)
        {
            return (r.ScoreOne ?? 0) + (r.ScoreTwo ?? 0) + (r.ScoreThree ?? 0)
                + (r.ScoreFour ?? 0) + (r.ScoreFive ?? 0) + (r.ScoreSix ?? 0);
        }

        private static int GetAge(DateTime? dateOfBirth)
        {
            if (dateOfBirth == null) return 0;
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }

        private class SubjectRankingData
        {
            public Dictionary<long, int> Positions { get; set; } = new();
            public int TotalStudents { get; set; }
        }
    }
}
