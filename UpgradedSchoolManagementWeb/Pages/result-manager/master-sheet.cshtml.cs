using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UpgradedSchoolManagementDataAccess.Data;
using UpgradedSchoolManagementDataAccess.IServices;
using UpgradedSchoolManagementModels.DTOs;
using UpgradedSchoolManagementModels.Models;
using UpgradedSchoolManagementModels.ViewModels;
using UpgradedSchoolManagementUltitlities;
using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementWeb.Pages.result_manager
{
    public class master_sheetModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;

        public master_sheetModel(ApplicationDbContext db, IUnitOfWork unitOfWork)
        {
            _db = db;
            _unitOfWork = unitOfWork;
        }

        public SelectionViewModal SelectionView { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SchoolClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SubClassId { get; set; }

        public MasterSheetViewModel MasterSheet { get; set; } = new();
        public bool HasData => MasterSheet.Rows.Count > 0;

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();

            if (SessionId > 0 && SchoolClassId > 0 && SubClassId > 0)
            {
                await LoadSelectedNamesAsync();
                await BuildMasterSheetAsync();
            }
        }

        private async Task BuildMasterSheetAsync()
        {
            var sessionName = MasterSheet.SessionName;
            var className = MasterSheet.ClassName;
            var subClassName = MasterSheet.SubClassName;

            var configs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == SchoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);

            var filteredRegs = await _db.TermRegistrations
                .Where(tr => tr.SessionId == SessionId
                    && tr.SchoolClassId == SchoolClassId
                    && tr.SubClassId == SubClassId)
                .AsNoTracking()
                .ToListAsync();

            if (filteredRegs.Count == 0) return;

            var studentIds = filteredRegs.Select(tr => tr.StudentId).Distinct().ToList();

            var allRegs = await _db.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Include(tr => tr.SchoolClasses)
                .Where(tr => tr.SessionId == SessionId && studentIds.Contains(tr.StudentId))
                .AsNoTracking()
                .ToListAsync();

            var allTermRegIds = allRegs.Select(tr => tr.Id).ToList();

            var resultTables = await _db.ResultTables
                .Where(rt => allTermRegIds.Contains(rt.TermRegId) && rt.Status)
                .AsNoTracking()
                .ToListAsync();

            var takenSubjectIds = resultTables
                .Select(rt => rt.SubjectId)
                .Distinct()
                .ToHashSet();

            var subjects = await _db.SubjectTables
                .Where(s => s.IsActive && takenSubjectIds.Contains(s.Id))
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            if (subjects.Count == 0) return;

            var firstClass = allRegs.First().SchoolClasses;
            var resultType = firstClass?.Resulttype ?? ResultType.Primary;
            MasterSheet.AnnualReportUrlPrefix = resultType switch
            {
                ResultType.Nursery => "/result-manager/annual-result/nursery-annual-report",
                ResultType.Primary => "/result-manager/annual-result/primary-annual-report",
                ResultType.Jss => "/result-manager/annual-result/junior-annual-report",
                ResultType.SSS => "/result-manager/annual-result/senior-annual-report",
                _ => "/result-manager/annual-result/primary-annual-report"
            };

            var resultsByReg = resultTables
                .GroupBy(rt => rt.TermRegId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var regsByStudent = allRegs.GroupBy(tr => tr.StudentId);
            var rows = new List<MasterSheetRow>();
            var studentAverages = new Dictionary<int, decimal>();
            var totalSubjects = subjects.Count;

            foreach (var group in regsByStudent)
            {
                var studentId = group.Key;
                var studentRegs = group.ToList();
                var student = studentRegs.First().StudentsTable;

                var regByTerm = studentRegs.ToDictionary(tr => tr.Term);
                var subjectData = new Dictionary<int, MasterSheetSubjectData>();
                decimal cumulativeTotal = 0;
                int subjectsTaken = 0;

                foreach (var subject in subjects)
                {
                    var term1Reg = regByTerm.GetValueOrDefault(Term.First);
                    var term2Reg = regByTerm.GetValueOrDefault(Term.Second);
                    var term3Reg = regByTerm.GetValueOrDefault(Term.Third);

                    var term1Res = term1Reg != null && resultsByReg.TryGetValue(term1Reg.Id, out var r1List)
                        ? r1List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;
                    var term2Res = term2Reg != null && resultsByReg.TryGetValue(term2Reg.Id, out var r2List)
                        ? r2List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;
                    var term3Res = term3Reg != null && resultsByReg.TryGetValue(term3Reg.Id, out var r3List)
                        ? r3List.FirstOrDefault(r => r.SubjectId == subject.Id) : null;

                    double? t1 = term1Res != null ? SumScores(term1Res) : null;
                    double? t2 = term2Res != null ? SumScores(term2Res) : null;
                    double? t3 = term3Res != null ? SumScores(term3Res) : null;

                    var isTaken = t1.HasValue || t2.HasValue || t3.HasValue;

                    if (isTaken)
                    {
                        var available = new[] { t1, t2, t3 }
                            .Where(t => t.HasValue).Select(t => t.Value).ToList();
                        var annualTotal = (decimal)available.Average();
                        var (grade, _) = SD.GetGradeAndRemark(annualTotal);

                        cumulativeTotal += (decimal)available.Sum();
                        subjectsTaken++;

                        subjectData[subject.Id] = new MasterSheetSubjectData
                        {
                            IsTaken = true,
                            Term1Total = t1,
                            Term2Total = t2,
                            Term3Total = t3,
                            AnnualTotal = annualTotal,
                            Grade = grade
                        };
                    }
                    else
                    {
                        subjectData[subject.Id] = new MasterSheetSubjectData
                        {
                            IsTaken = false
                        };
                    }
                }

                var avg = subjectsTaken > 0 && maxPerSubject > 0
                    ? Math.Round((decimal)(
                        subjectData.Values
                            .Where(sd => sd.IsTaken)
                            .Average(sd => (double)sd.AnnualTotal / maxPerSubject * 100)
                    ), 2)
                    : 0m;

                studentAverages[studentId] = avg;

                var studentName = student != null
                    ? $"{student.Surname} {student.OtherName} {student.FirstName}".Trim()
                    : "N/A";

                rows.Add(new MasterSheetRow
                {
                    StudentId = studentId,
                    StudentName = studentName,
                    AdmissionNumber = student?.AdmissionNumber ?? "N/A",
                    SubjectData = subjectData,
                    CumulativeTotal = cumulativeTotal,
                    OverallAverage = avg,
                    Grade = avg > 0 ? SD.GetGradeAndRemark(avg).Grade : string.Empty,
                    Remark = avg > 0 ? SD.GetGradeAndRemark(avg).Remark : string.Empty,
                    SubjectsTakenCount = subjectsTaken,
                    SubjectsCount = totalSubjects
                });
            }

            var sorted = studentAverages
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .ToList();

            var positions = new Dictionary<int, int>();
            int? currentPos = null;
            decimal? previousAvg = null;

            for (int i = 0; i < sorted.Count; i++)
            {
                if (previousAvg == null || Math.Abs(sorted[i].Value - previousAvg.Value) > 0.0001m)
                    currentPos = i + 1;
                positions[sorted[i].Key] = currentPos ?? 1;
                previousAvg = sorted[i].Value;
            }

            var sn = 1;
            foreach (var row in rows)
            {
                row.Sn = sn++;
                row.Position = FormatPosition(positions.GetValueOrDefault(row.StudentId, 1));
            }

            MasterSheet = new MasterSheetViewModel
            {
                SessionName = sessionName,
                ClassName = className,
                SubClassName = subClassName,
                Subjects = subjects.Select(s => new SubjectColumnInfo
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                Rows = rows,
                AnnualReportUrlPrefix = MasterSheet.AnnualReportUrlPrefix
            };
        }

        private static double SumScores(ResultTable r)
        {
            return (r.ScoreOne ?? 0) + (r.ScoreTwo ?? 0) + (r.ScoreThree ?? 0)
                + (r.ScoreFour ?? 0) + (r.ScoreFive ?? 0) + (r.ScoreSix ?? 0);
        }

        private async Task LoadSelectedNamesAsync()
        {
            var session = (await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync())
                .FirstOrDefault(s => s.Value == SessionId.ToString());
            var cls = (await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync())
                .FirstOrDefault(c => c.Value == SchoolClassId.ToString());
            var sub = (await _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync())
                .FirstOrDefault(s => s.Value == SubClassId.ToString());

            MasterSheet.SessionName = session?.Text ?? SessionId.ToString();
            MasterSheet.ClassName = cls?.Text ?? SchoolClassId.ToString();
            MasterSheet.SubClassName = sub?.Text ?? SubClassId.ToString();
        }

        private async Task LoadDropdownsAsync()
        {
            SelectionView = new()
            {
                AcademicSession = await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync(),
                SchoolClasses = await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync(),
                SubClass = await _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync()
            };
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
    }
}
