using static UpgradedSchoolManagementModels.Models.ConstantEnums;

namespace UpgradedSchoolManagementModels.DTOs
{
    public class TerminalMasterSheetViewModel
    {
        public string SessionName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubClassName { get; set; } = string.Empty;
        public List<AssessmentConfigDto> AssessmentConfigs { get; set; } = new();
        public List<SubjectColumnInfo> Subjects { get; set; } = new();
        public List<TerminalMasterSheetRow> Rows { get; set; } = new();
        public int MaxAssessmentCount { get; set; }
    }

    public class TerminalMasterSheetRow
    {
        public int Sn { get; set; }
        public long TermRegId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        public Dictionary<int, TerminalMasterSheetSubjectData> SubjectData { get; set; } = new();
        public decimal OverallTotal { get; set; }
        public decimal OverallAverage { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public ResultType ResultType { get; set; }
        public int SubjectsTakenCount { get; set; }
        public int SubjectsCount { get; set; }
        public string SubjectCountDisplay => $"{SubjectsTakenCount}/{SubjectsCount}";
    }

    public class TerminalMasterSheetSubjectData
    {
        public double?[] Scores { get; set; } = Array.Empty<double?>();
        public decimal TotalScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsTaken { get; set; }
    }
}
