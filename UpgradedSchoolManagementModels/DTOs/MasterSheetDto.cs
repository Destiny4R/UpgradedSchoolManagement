namespace UpgradedSchoolManagementModels.DTOs
{
    public class MasterSheetViewModel
    {
        public string SessionName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubClassName { get; set; } = string.Empty;
        public List<SubjectColumnInfo> Subjects { get; set; } = new();
        public List<MasterSheetRow> Rows { get; set; } = new();
        public string AnnualReportUrlPrefix { get; set; } = string.Empty;
    }

    public class SubjectColumnInfo
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class MasterSheetRow
    {
        public int Sn { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AdmissionNumber { get; set; } = string.Empty;
        public Dictionary<int, MasterSheetSubjectData> SubjectData { get; set; } = new();
        public decimal CumulativeTotal { get; set; }
        public decimal OverallAverage { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public int SubjectsTakenCount { get; set; }
        public int SubjectsCount { get; set; }
        public string SubjectCountDisplay => $"{SubjectsTakenCount}/{SubjectsCount}";
    }

    public class MasterSheetSubjectData
    {
        public bool IsTaken { get; set; }
        public double? Term1Total { get; set; }
        public double? Term2Total { get; set; }
        public double? Term3Total { get; set; }
        public decimal AnnualTotal { get; set; }
        public string Grade { get; set; } = string.Empty;
    }
}
