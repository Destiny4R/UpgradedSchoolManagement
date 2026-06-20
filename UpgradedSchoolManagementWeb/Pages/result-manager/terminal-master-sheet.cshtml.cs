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
    public class terminal_master_sheetModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;

        public terminal_master_sheetModel(ApplicationDbContext db, IUnitOfWork unitOfWork)
        {
            _db = db;
            _unitOfWork = unitOfWork;
        }

        public SelectionViewModal SelectionView { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int TermValue { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SchoolClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SubClassId { get; set; }

        public TerminalMasterSheetViewModel MasterSheet { get; set; } = new();
        public bool HasData => MasterSheet.Rows.Count > 0;

        public async Task OnGetAsync()
        {
            await LoadDropdownsAsync();

            if (SessionId > 0 && TermValue > 0 && SchoolClassId > 0 && SubClassId > 0)
            {
                await LoadSelectedNamesAsync();
                await BuildMasterSheetAsync();
            }
        }

        private async Task BuildMasterSheetAsync()
        {
            var sessionName = MasterSheet.SessionName;
            var termName = MasterSheet.TermName;
            var className = MasterSheet.ClassName;
            var subClassName = MasterSheet.SubClassName;
            var term = (Term)TermValue;

            var configs = await _db.AssessmentConfigurations
                .Where(ac => ac.SchoolClassId == SchoolClassId)
                .OrderBy(ac => ac.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var termRegs = await _db.TermRegistrations
                .Include(tr => tr.StudentsTable)
                .Include(tr => tr.SchoolClasses)
                .Where(tr => tr.SessionId == SessionId
                    && tr.Term == term
                    && tr.SchoolClassId == SchoolClassId
                    && tr.SubClassId == SubClassId)
                .AsNoTracking()
                .ToListAsync();

            if (termRegs.Count == 0) return;

            var termRegIds = termRegs.Select(tr => tr.Id).ToList();
            var resultTables = await _db.ResultTables
                .Where(rt => termRegIds.Contains(rt.TermRegId) && rt.Status)
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

            var maxPerSubject = configs.Sum(c => c.AssessmentScore);
            var configsList = configs.Select(c => new AssessmentConfigDto
            {
                Id = c.Id,
                AssessmentName = c.AssessmentName,
                AssessmentScore = c.AssessmentScore,
                DisplayOrder = c.DisplayOrder
            }).ToList();

            var rows = new List<TerminalMasterSheetRow>();
            var studentAverages = new Dictionary<long, decimal>();
            var totalSubjects = subjects.Count;

            foreach (var tr in termRegs)
            {
                var studentResults = resultTables
                    .Where(r => r.TermRegId == tr.Id)
                    .ToDictionary(r => r.SubjectId);
                var subjectData = new Dictionary<int, TerminalMasterSheetSubjectData>();
                decimal studentTotal = 0;
                int subjectsTaken = 0;

                foreach (var subject in subjects)
                {
                    if (studentResults.TryGetValue(subject.Id, out var result))
                    {
                        var scores = new double?[] { result.ScoreOne, result.ScoreTwo, result.ScoreThree, result.ScoreFour, result.ScoreFive, result.ScoreSix };
                        var total = scores.Sum(s => s ?? 0);
                        var (grade, _) = SD.GetGradeAndRemark((decimal)total);
                        studentTotal += (decimal)total;
                        subjectsTaken++;

                        subjectData[subject.Id] = new TerminalMasterSheetSubjectData
                        {
                            Scores = scores,
                            TotalScore = (decimal)total,
                            Grade = grade,
                            IsTaken = true
                        };
                    }
                    else
                    {
                        subjectData[subject.Id] = new TerminalMasterSheetSubjectData
                        {
                            Scores = new double?[configs.Count],
                            TotalScore = 0,
                            Grade = string.Empty,
                            IsTaken = false
                        };
                    }
                }

                var avg = subjectsTaken > 0 && maxPerSubject > 0
                    ? Math.Round((decimal)((double)studentTotal / (subjectsTaken * maxPerSubject) * 100), 2)
                    : 0m;

                studentAverages[tr.Id] = avg;

                var studentName = tr.StudentsTable != null
                    ? $"{tr.StudentsTable.Surname} {tr.StudentsTable.OtherName} {tr.StudentsTable.FirstName}".Trim()
                    : "N/A";

                rows.Add(new TerminalMasterSheetRow
                {
                    TermRegId = tr.Id,
                    StudentId = tr.StudentId,
                    StudentName = studentName,
                    AdmissionNumber = tr.StudentsTable?.AdmissionNumber ?? "N/A",
                    SubjectData = subjectData,
                    OverallTotal = studentTotal,
                    OverallAverage = avg,
                    Grade = avg > 0 ? SD.GetGradeAndRemark(avg).Grade : string.Empty,
                    Remark = avg > 0 ? SD.GetGradeAndRemark(avg).Remark : string.Empty,
                    ResultType = tr.SchoolClasses?.Resulttype ?? ResultType.Primary,
                    SubjectsTakenCount = subjectsTaken,
                    SubjectsCount = totalSubjects
                });
            }

            var sorted = studentAverages
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .ToList();

            var positions = new Dictionary<long, int>();
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
                row.Position = FormatPosition(positions.GetValueOrDefault(row.TermRegId, 1));
            }

            MasterSheet = new TerminalMasterSheetViewModel
            {
                SessionName = sessionName,
                TermName = termName,
                ClassName = className,
                SubClassName = subClassName,
                AssessmentConfigs = configsList,
                Subjects = subjects.Select(s => new SubjectColumnInfo
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                Rows = rows,
                MaxAssessmentCount = configs.Count
            };
        }

        private async Task LoadSelectedNamesAsync()
        {
            var session = (await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync())
                .FirstOrDefault(s => s.Value == SessionId.ToString());
            var term = _unitOfWork.ViewSelectionService.GetTermForDropdown()
                .FirstOrDefault(t => t.Value == TermValue.ToString());
            var cls = (await _unitOfWork.ViewSelectionService.GetSchoolClassesForDropdownAsync())
                .FirstOrDefault(c => c.Value == SchoolClassId.ToString());
            var sub = (await _unitOfWork.ViewSelectionService.GetSchoolSubclassesForDropdownAsync())
                .FirstOrDefault(s => s.Value == SubClassId.ToString());

            MasterSheet.SessionName = session?.Text ?? SessionId.ToString();
            MasterSheet.TermName = term?.Text ?? TermValue.ToString();
            MasterSheet.ClassName = cls?.Text ?? SchoolClassId.ToString();
            MasterSheet.SubClassName = sub?.Text ?? SubClassId.ToString();
        }

        private async Task LoadDropdownsAsync()
        {
            SelectionView = new()
            {
                AcademicSession = await _unitOfWork.ViewSelectionService.GetSessionsForDropdownAsync(),
                Terms = _unitOfWork.ViewSelectionService.GetTermForDropdown(),
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
